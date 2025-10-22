# ClaimIT - Contract Monthly Claim System

![ClaimIT](https://img.shields.io/badge/ClaimIT-Contract%20Claim%20System-blue)
![.NET 9.0](https://img.shields.io/badge/.NET-9.0-purple)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-green)

A modern, web-based claim management system designed for educational institutions to handle contract lecturers' monthly hour claims with seamless approval workflows.

## 🚀 Features

### Core Functionality
- **📋 Claim Submission** - Easy-to-use form for submitting monthly hour claims
- **🔄 Approval Workflow** - Multi-step verification and approval process
- **📊 Dashboard Analytics** - Real-time statistics and visualizations
- **📎 Document Management** - Secure file upload and document handling
- **👥 Role-based Access** - Different actions for submitters and approvers

### Advanced Features
- **🎨 Modern UI/UX** - Particle effects, animations, and responsive design
- **📱 Mobile Responsive** - Works seamlessly on all devices
- **⚡ Real-time Updates** - Live status tracking and activity feeds
- **🔍 Document Preview** - In-browser viewing of uploaded documents
- **📈 Progress Tracking** - Visual claim status progression

## 🛠 Technology Stack

### Backend
- **ASP.NET Core 9.0** - Web framework
- **C# 12** - Programming language
- **MVC Pattern** - Architecture pattern
- **In-Memory Data Store** - Lightweight data persistence

### Frontend
- **Bootstrap 5** - CSS framework
- **JavaScript ES6+** - Client-side scripting
- **Particles.js** - Background animations
- **AOS** - Scroll animations
- **Font Awesome** - Icons

### Testing
- **xUnit** - Testing framework
- **Moq** - Mocking library
- **ASP.NET Core Test Host** - Integration testing

## 📦 Installation & Setup

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A code editor (VS Code, Visual Studio, or Rider)
- Modern web browser

### Quick Start
1. **Clone or Download the Project**
   ```bash
   git clone <repository-url>
   cd ClaimIT
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the Application**
   ```bash
   dotnet run
   ```

4. **Access the Application**
   - Open your browser to: `https://localhost:7000` or `http://localhost:5000`
   - The application will redirect to the Claims dashboard

### Development Setup
1. **Open in VS Code**
   ```bash
   code .
   ```

2. **Build the Project**
   ```bash
   dotnet build
   ```

3. **Run Tests**
   ```bash
   dotnet test
   ```

## 🏗 Project Structure

```
ClaimIT/
├── Controllers/          # MVC Controllers
│   ├── ClaimsController.cs
│   └── HomeController.cs
├── Models/              # Data Models
│   ├── Claim.cs
│   ├── ClaimDocument.cs
│   └── ErrorViewModel.cs
├── Data/               # Data Access Layer
│   ├── SimpleContext.cs
│   └── ApplicationDbContext.cs
├── Views/              # Razor Views
│   ├── Claims/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Details.cshtml
│   └── Shared/
├── wwwroot/           # Static Files
│   └── uploads/       # Document storage
├── Tests/             # Unit Tests
│   ├── ClaimsControllerTests.cs
│   ├── ClaimModelTests.cs
│   └── SimpleContextTests.cs
└── Program.cs         # Application entry point
```

## 🎯 Usage Guide

### For Lecturers (Claim Submitters)

1. **Submit a New Claim**
   - Click "Add New Claim" from dashboard
   - Fill in lecturer details, hours worked, and hourly rate
   - Upload supporting documents (timesheets, contracts)
   - Submit for approval

2. **Track Claim Status**
   - View the status tracker on dashboard
   - See real-time progress through approval stages
   - Check recent activity feed

### For Coordinators (Approvers)

1. **Review Pending Claims**
   - Access Approval Queue from dashboard
   - View all claims awaiting verification/approval
   - See claim details and supporting documents

2. **Approve/Verify Claims**
   - **Verify**: Confirm claim details are accurate
   - **Approve**: Final approval for payment processing
   - **Reject**: Return claim for corrections

### Document Management

- **Supported Formats**: PDF, Word, Excel, Images (JPG, PNG)
- **File Size Limit**: 5MB per file
- **Security**: Unique file naming and validation
- **Preview**: In-browser viewing for images and PDFs

## 🔧 Configuration

### AppSettings (appsettings.json)
```json
{
  "FileUpload": {
    "MaxFileSize": 5242880,
    "AllowedExtensions": [".pdf", ".docx", ".xlsx", ".jpg", ".jpeg", ".png"],
    "UploadsPath": "wwwroot/uploads",
    "MaxFilesPerClaim": 10
  },
  "Application": {
    "Name": "ClaimIT",
    "Version": "1.0.0",
    "EnableTutorial": true,
    "AutoBackup": true
  }
}
```

## 🧪 Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test project
dotnet test Tests/ClaimIT.Tests.csproj
```

### Test Coverage
- **Controller Tests**: Action methods, redirects, view results
- **Model Tests**: Business logic, calculations, validations
- **Context Tests**: Data operations, state management
- **Integration Tests**: End-to-end workflow validation

## 🚀 Deployment

### Development
```bash
dotnet run --environment Development
```

### Production
```bash
dotnet publish -c Release
cd bin/Release/net9.0/publish
dotnet ClaimIT.dll
```

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ClaimIT.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClaimIT.dll"]
```

## 📊 Sample Data

The application comes with sample claims:
- **Dr. Sarah Smith**: 40 hours @ R150/hour (Pending)
- **Prof. James Johnson**: 35 hours @ R180/hour (Verified)  
- **Dr. Maria Brown**: 45 hours @ R160/hour (Approved)

## 🛡 Security Features

- **Input Validation** - Server and client-side validation
- **File Type Restrictions** - Whitelisted file extensions
- **Size Limits** - Prevent large file uploads
- **XSS Protection** - Built-in ASP.NET Core protections
- **CSRF Protection** - Anti-forgery tokens

## 🔄 Workflow

```
Submit Claim → Pending → Verified → Approved → Paid
     ↓          ↓         ↓         ↓
   Create    Verify    Approve   Process
   Claim    Details   Payment   Payment
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Write unit tests for new features
- Follow ASP.NET Core best practices
- Maintain responsive design
- Ensure cross-browser compatibility

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

### Common Issues
1. **File Upload Fails**
   - Check file size (max 5MB)
   - Verify file type is supported
   - Ensure uploads directory exists

2. **Tests Fail**
   - Run `dotnet clean` and `dotnet restore`
   - Check for shared state issues in SimpleContext

3. **Application Won't Start**
   - Verify .NET 9.0 SDK is installed
   - Check port availability (5000, 7000)

### Getting Help
- Check the application logs in console output
- Review test results for specific failures
- Ensure all dependencies are restored

## 🎉 Acknowledgments

- **Bootstrap** - For the responsive UI components
- **Particles.js** - For the animated background effects
- **Font Awesome** - For the comprehensive icon set
- **ASP.NET Core Team** - For the excellent web framework

---

**Built with ❤️ using ASP.NET Core 9.0**

