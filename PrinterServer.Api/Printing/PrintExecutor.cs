using System.Drawing;
using System.Drawing.Printing;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using PrinterServer.Api.Models;
using PrinterServer.Api.Services;

namespace PrinterServer.Api.Printing;

public sealed class PrintExecutor : IPrintExecutor
{
    private readonly ISettingsService _settingsService;

    public PrintExecutor(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public Task ExecuteAsync(PrintJob job, CancellationToken cancellationToken)
    {
        var type = job.Type.Trim().ToLowerInvariant();
        return type switch
        {
            "text" => PrintTextAsync(job),
            "image" => PrintImageAsync(job),
            "raw" or "zpl" => PrintRawAsync(job, cancellationToken),
            "pdf" => PrintPdfAsync(job),
            _ => throw new NotSupportedException($"Unsupported print type: {job.Type}")
        };
    }

    private Task PrintTextAsync(PrintJob job)
    {
        if (string.IsNullOrWhiteSpace(job.Text))
        {
            throw new InvalidOperationException("Text content is empty.");
        }

        try
        {
            using var document = new PrintDocument();
            document.PrinterSettings.PrinterName = job.Printer;
            
            // Crucial for Servers: Silence the print dialog/progress
            document.PrintController = new StandardPrintController();

            if (!document.PrinterSettings.IsValid)
            {
                throw new InvalidOperationException($"Printer '{job.Printer}' is not valid or not accessible.");
            }

            document.DocumentName = $"ServerPrint_{job.JobId}";

            var lines = job.Text.Replace("\r\n", "\n").Split('\n');
            var lineIndex = 0;

            document.PrintPage += (_, args) =>
            {
                try
                {
                    using var font = new Font("Arial", 10f);
                    var lineHeight = font.GetHeight(args.Graphics);
                    float y = args.MarginBounds.Top;

                    while (lineIndex < lines.Length)
                    {
                        var line = lines[lineIndex];
                        args.Graphics.DrawString(line, font, Brushes.Black, args.MarginBounds.Left, y);
                        y += lineHeight;
                        lineIndex++;

                        if (y + lineHeight > args.MarginBounds.Bottom)
                        {
                            args.HasMorePages = true;
                            return;
                        }
                    }

                    args.HasMorePages = false;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Rendering error: {ex.Message}", ex);
                }
            };

            document.Print();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Print failed: {ex.Message}", ex);
        }
    }

    private Task PrintImageAsync(PrintJob job)
    {
        if (job.FileBytes is null || job.FileBytes.Length == 0)
        {
            throw new InvalidOperationException("Image file is empty.");
        }

        using var stream = new MemoryStream(job.FileBytes);
        using var image = Image.FromStream(stream);
        using var document = new PrintDocument();

        document.PrinterSettings.PrinterName = job.Printer;
        document.PrintController = new StandardPrintController();
        document.DocumentName = $"ImagePrint_{job.JobId}";

        document.PrintPage += (_, args) =>
        {
            var bounds = args.MarginBounds;
            var ratioX = (double)bounds.Width / image.Width;
            var ratioY = (double)bounds.Height / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var width = (int)(image.Width * ratio);
            var height = (int)(image.Height * ratio);
            var x = bounds.Left + (bounds.Width - width) / 2;
            var y = bounds.Top + (bounds.Height - height) / 2;

            args.Graphics.DrawImage(image, x, y, width, height);
            args.HasMorePages = false;
        };

        document.Print();
        return Task.CompletedTask;
    }

    private Task PrintRawAsync(PrintJob job, CancellationToken cancellationToken)
    {
        var settings = _settingsService.GetSettings();
        var rawMode = job.RawMode.Trim().ToLowerInvariant();
        var encoding = EncodingFromName(settings.RawEncoding);

        byte[] payload;
        if (job.FileBytes is { Length: > 0 })
        {
            payload = job.FileBytes;
        }
        else if (!string.IsNullOrWhiteSpace(job.Text))
        {
            payload = encoding.GetBytes(job.Text);
        }
        else
        {
            throw new InvalidOperationException("RAW/ZPL payload is empty.");
        }

        if (!string.IsNullOrWhiteSpace(settings.RawTerminator))
        {
            var terminator = encoding.GetBytes(settings.RawTerminator);
            payload = payload.Concat(terminator).ToArray();
        }

        if (rawMode == "tcp")
        {
            return SendTcpAsync(settings, payload, cancellationToken);
        }

        RawPrinterHelper.SendBytes(job.Printer, payload);
        return Task.CompletedTask;
    }

    private async Task PrintPdfAsync(PrintJob job)
    {
        if (job.FileBytes is null || job.FileBytes.Length == 0)
        {
            throw new InvalidOperationException("PDF file is empty.");
        }

        var tempPath = Path.Combine(Path.GetTempPath(), $"print_{job.JobId}.pdf");
        await File.WriteAllBytesAsync(tempPath, job.FileBytes);

        try
        {
            // 1. Try SumatraPDF (Best Solution: Tiny, 0% idle RAM, Fast)
            var sumatraPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SumatraPDF.exe");
            if (File.Exists(sumatraPath))
            {
                if (!File.Exists(tempPath)) 
                {
                    throw new FileNotFoundException("Generated PDF file not found on disk.", tempPath);
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = sumatraPath,
                    // Using -print-to with quotes for printer name and file path
                    // Added -silent and -exit-on-print for background usage
                    Arguments = $"-print-to \"{job.Printer}\" -silent -exit-on-print \"{tempPath}\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false
                };
                
                using (var p = Process.Start(startInfo))
                {
                    if (p == null) throw new Exception("Failed to start SumatraPDF process.");
                    p.WaitForExit(20000); // Give it up to 20 seconds
                }
                return;
            }

            throw new InvalidOperationException("No PDF printing application found. Please place 'SumatraPDF.exe' in the app folder for best performance.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"PDF Printing failed: {ex.Message}", ex);
        }
        finally
        {
            // Immediate cleanup of any hanging edge processes from THIS print job
            _ = Task.Run(() => CleanupStrayProcesses());
            // Delay deletion of temp file
            _ = Task.Run(async () => { await Task.Delay(15000); try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch {} });
        }
    }

    private void CleanupStrayProcesses()
    {
        try
        {
            // Only kill processes that were likely started for printing (background/no window)
            foreach (var p in Process.GetProcessesByName("msedge"))
            {
                try { if (p.MainWindowHandle == IntPtr.Zero) p.Kill(); } catch { }
            }
        }
        catch { }
    }

    private static async Task SendTcpAsync(Settings settings, byte[] payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.RawTcpHost))
        {
            throw new InvalidOperationException("RAW TCP host is not configured.");
        }

        using var client = new TcpClient();
        await client.ConnectAsync(settings.RawTcpHost, settings.RawTcpPort, cancellationToken);
        using var stream = client.GetStream();
        await stream.WriteAsync(payload, cancellationToken);
    }

    private static Encoding EncodingFromName(string name)
    {
        return name.Trim().ToLowerInvariant() switch
        {
            "utf-8" => Encoding.UTF8,
            "utf8" => Encoding.UTF8,
            "ansi" => Encoding.Default,
            _ => Encoding.UTF8
        };
    }
}
