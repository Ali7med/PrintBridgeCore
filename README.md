# 🖨️ PrintBridge Core

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**PrintBridge Core** is a lightweight, high-performance Print Server built with .NET 10. It provides a modern Web Dashboard and a robust REST API to enable seamless printing from any device (Desktop, Mobile, or IoT) across your network.

---

## ✨ Key Features

*   🚀 **Lightweight & Fast:** Minimal resource footprint, optimized for background execution.
*   📄 **PDF Printing:** Silent printing using a dedicated lightweight engine (SumatraPDF).
*   🖼️ **Image Support:** Print JPEG, PNG, and BMP directly to any system printer.
*   📝 **Text & RAW:** Supports plain text and RAW/ZPL printing for label printers.
*   🔒 **Secure:** Token-based authentication (`X-Print-Token`) to prevent unauthorized access.
*   🌐 **Web Dashboard:** UI for managing settings, generating tokens, and viewing print history.
*   📺 **Live History:** Track every print job with status, client IP, and error logs using SQLite.
*   📡 **Network Ready:** Easily accessible across your local network.

---

## 🛠️ Tech Stack

*   **Backend:** ASP.NET Core (.NET 10)
*   **Database:** SQLite (Lightweight history & settings tracking)
*   **Frontend:** Vanilla HTML5, CSS3, and JavaScript (No heavy frameworks)
*   **Printing Engine:** Native Winspool & SumatraPDF for PDFs.

---

## 🚀 Getting Started

### 1. Prerequisites
*   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   A Windows environment (for printer driver access).

### 2. Installation
1.  Clone the repository:
    ```bash
    git clone https://github.com/your-username/PrintBridge-Core.git
    cd PrintBridge-Core
    ```
2.  Ensure `SumatraPDF.exe` is in the root or `bin` folder for PDF support.

### 3. Running the Server
To enable access from other devices on the network:
```bash
dotnet run --project PrinterServer.Api --urls "http://0.0.0.0:5166"
```

---

## �️ API Route Map

The API is fully documented and can be interacted with using the following endpoints:

### **Print Operations**
*   `POST /print` - Submit a print job (Content-Type: `application/json` or `multipart/form-data`).
    *   Supports: `pdf`, `image`, `text`, `raw`.

### **Printers & Configuration**
*   `GET /printers` - List all installed printers on the server.
*   `GET /settings` - Retrieve current server/printer settings.
*   `POST /settings` - Update global printing settings.

### **History & Logs**
*   `GET /history` - Access the print audit trail.
    *   Query Params: `status`, `printer`, `from`, `to`, `limit`.

### **Security & Tokens**
*   `GET /token` - Retrieve token status and local access settings.
*   `POST /token/generate` - Generate a new secure API token.
*   `POST /token/disable` - Toggle token-free access for local network devices.

---

## �🔌 API Usage Example (JSON)

### Listing Printers
```bash
curl -X GET http://localhost:5166/printers -H "X-Print-Token: YOUR_TOKEN"
```

### Printing a PDF (via Base64)
```json
POST /print
{
  "type": "pdf",
  "printer": "Canon MF272",
  "base64File": "JVBERi0xLjQKJ..."
}
```

---

## 📸 Dashboard Preview
The dashboard is available at the root URL (e.g., `http://localhost:5166`).
*   **Dashboard:** View stats and available printers.
*   **History:** Audit trail of all print jobs.
*   **Token Manager:** Securely manage API access.

---

## 📜 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙌 Contributing
Contributions are welcome! Feel free to open an issue or submit a pull request.
