using System.Runtime.InteropServices;

namespace PrinterServer.Api.Printing;

internal static class RawPrinterHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private sealed class DOCINFO
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDocName = "Raw Document";
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDatatype = "RAW";
    }

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int Level, [In] DOCINFO pDocInfo);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    public static void SendBytes(string printerName, byte[] bytes)
    {
        if (!OpenPrinter(printerName, out var printerHandle, IntPtr.Zero))
        {
            throw new InvalidOperationException("Unable to open printer.");
        }

        try
        {
            var docInfo = new DOCINFO();
            if (!StartDocPrinter(printerHandle, 1, docInfo))
            {
                throw new InvalidOperationException("Unable to start document.");
            }

            try
            {
                if (!StartPagePrinter(printerHandle))
                {
                    throw new InvalidOperationException("Unable to start page.");
                }

                try
                {
                    var unmanagedBytes = Marshal.AllocHGlobal(bytes.Length);
                    try
                    {
                        Marshal.Copy(bytes, 0, unmanagedBytes, bytes.Length);
                        if (!WritePrinter(printerHandle, unmanagedBytes, bytes.Length, out _))
                        {
                            throw new InvalidOperationException("Failed to write to printer.");
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(unmanagedBytes);
                    }
                }
                finally
                {
                    EndPagePrinter(printerHandle);
                }
            }
            finally
            {
                EndDocPrinter(printerHandle);
            }
        }
        finally
        {
            ClosePrinter(printerHandle);
        }
    }
}
