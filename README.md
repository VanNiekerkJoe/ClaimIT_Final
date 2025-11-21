# ClaimIT - Contract Monthly Claim System

![ClaimIT Banner](https://via.placeholder.com/800x200/2C3E50/FFFFFF?text=ClaimIT+-+Contract+Monthly+Claim+System)

## 📋 Project Overview

ClaimIT is a comprehensive web application designed to automate the monthly claim submission and approval process for contract lecturers in educational institutions. The system streamlines the entire workflow from claim creation to final approval and payment processing.

**Course:** PROG6212 
**Institution:** Emiris  
**Student:** Joe Leo Van Niekerk  
**Student Code:** ST10445055  
**Year:** 2025

## ✨ Features

### 🎯 Role-Based Automation

#### **Lecturer View**
- 📝 Multi-step claim submission form
- ⚡ Real-time calculations and validations
- 📎 Document upload with drag & drop
- 📊 Personal dashboard with claim history
- 🔄 Auto-population of lecturer details

#### **Programme Coordinator View**
- ✅ Claim verification workflow
- 🔍 Document review capabilities
- 📋 Approval queue management
- 📈 Status tracking dashboard

#### **Academic Manager View**
- ✅ Final approval authority
- 📊 Comprehensive oversight
- 📋 Batch processing capabilities
- 📈 Performance analytics

#### **HR View**
- 👥 User management system
- 📊 Advanced reporting and analytics
- 🧾 Invoice generation
- 📋 System administration

### 🔧 Technical Features

- **Security**: Role-based access control with session authentication
- **Responsive Design**: Mobile-first approach with Bootstrap 5
- **File Management**: Secure document upload and storage
- **Real-time Validation**: Client and server-side validation
- **Professional UI**: Modern, intuitive interface with animations
- **Data Persistence**: Entity Framework Core with in-memory database

## 🛠 Technologies Used

### Backend
- **ASP.NET Core 9.0** - Web framework
- **Entity Framework Core 9.0** - ORM
- **C# 12** - Programming language
- **Session Authentication** - Security
- **Dependency Injection** - Architecture

### Frontend
- **Bootstrap 5.3** - CSS framework
- **JavaScript ES6+** - Client scripting
- **jQuery** - DOM manipulation
- **CSS3** - Styling and animations
- **HTML5** - Markup

### Libraries & Tools
- **Particles.js** - Animated backgrounds
- **AOS Library** - Scroll animations
- **Bootstrap Icons** - Icon library
- **Visual Studio 2022** - Development IDE
- **Git/GitHub** - Version control

## 🚀 Installation & Setup

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- Modern web browser

### Step-by-Step Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/ST10445055/claimit-system.git
   cd claimit-system
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the Application**
   ```bash
   dotnet build
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Access the Application**
   - Open your browser and navigate to: `https://localhost:7000`
   - The application will automatically redirect to the login page

### Configuration

The application uses an in-memory database by default. No additional database setup is required for development.

## 👥 Demo Credentials

Use the following credentials to test different user roles:

### **Lecturer Account**
- **Email:** `lecturer@university.com`
- **Password:** `lecturer123`
- **Capabilities:** Submit claims, view personal claim history, upload documents

### **Programme Coordinator Account**
- **Email:** `coordinator@university.com`
- **Password:** `coordinator123`
- **Capabilities:** Verify claims, manage approval queue, review documents

### **Academic Manager Account**
- **Email:** `manager@university.com`
- **Password:** `manager123`
- **Capabilities:** Final approval, oversee all claims, analytics

### **HR Account**
- **Email:** `hr@university.com`
- **Password:** `hr123`
- **Capabilities:** User management, reports, system administration

## 📁 Project Structure

```
ClaimIT/
├── Controllers/
│   ├── AuthController.cs
│   ├── ClaimsController.cs
│   └── HRController.cs
├── Models/
│   ├── Claim.cs
│   ├── User.cs
│   ├── ClaimDocument.cs
│   └── ViewModels/
├── Data/
│   ├── EnhancedContext.cs
│   └── SimpleContext.cs
├── Views/
│   ├── Claims/
│   ├── Auth/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── uploads/
├── Program.cs
└── appsettings.json
```

## 🔧 Key Components

### Models
- **Claim**: Main claim entity with validation attributes
- **User**: User management with role-based properties
- **ClaimDocument**: File upload and document management

### Controllers
- **AuthController**: Handles authentication and session management
- **ClaimsController**: Core business logic for claim processing
- **HRController**: User management and reporting features

### Data Context
- **EnhancedContext**: In-memory database with seed data
- Role-based data access methods
- Automated workflow transitions

## 🎨 UI/UX Features

### Enhanced Animations
- **Particle.js** background effects
- **AOS** scroll animations
- **CSS3** transitions and transforms
- **Real-time** status updates

### Responsive Design
- Mobile-first approach
- Bootstrap 5 grid system
- Touch-friendly interfaces
- Cross-browser compatibility

## 🚀 Getting Started

### Development
1. Open the solution in Visual Studio 2022
2. Set `ClaimIT` as the startup project
3. Press `F5` to run with debugging
4. Use demo credentials to test different roles

### Production Deployment
1. Update `appsettings.json` for production database
2. Configure session timeout settings
3. Set up secure file upload directory
4. Configure SSL certificates

## 📊 API Endpoints

### Authentication
- `GET /Auth/Login` - Login page
- `POST /Auth/Login` - Authenticate user
- `GET /Auth/Logout` - Logout user

### Claims Management
- `GET /Claims/Index` - Dashboard
- `GET /Claims/Create` - New claim form
- `POST /Claims/Create` - Submit claim
- `GET /Claims/ApprovalQueue` - Pending claims
- `GET /Claims/Approve/{id}` - Approve claim
- `GET /Claims/Verify/{id}` - Verify claim

### File Management
- `GET /Claims/ViewDocument/{fileName}` - View uploaded document
- `GET /Claims/DownloadDocument/{fileName}` - Download document

## 🛡 Security Features

- Session-based authentication
- Role-based authorization
- Anti-forgery tokens
- Secure file upload validation
- Input sanitization
- XSS protection

## 🧪 Testing

The project includes xUnit tests for core functionality:

```bash
dotnet test
```

## 📈 POE Requirements Met

### ✅ Application Enhancement (Automation) - Lecturer View [20 Marks]
- ✅ Lecturer rates automatically pull through when submitting claims
- ✅ Automatic calculation of amounts and validation logic
- ✅ Proper EF Core usage and database integration
- ✅ Multi-step form with real-time calculations

### ✅ Application Enhancement (Automation) - Coordinator & Manager View [20 Marks]
- ✅ Session-based authentication implemented
- ✅ Predefined login credentials (no self-registration)
- ✅ Separate roles with distinct access and views
- ✅ Automated approval workflows

### ✅ Application Enhancement (Automation) - HR View [20 Marks]
- ✅ User management (add, update users)
- ✅ Report generation capabilities
- ✅ Invoice generation functionality
- ✅ Comprehensive system administration

### ✅ PowerPoint Presentation [20 Marks]
- ✅ Comprehensive feature coverage
- ✅ Professional design and layout
- ✅ UI screenshots and explanations
- ✅ Clear value proposition

### ✅ Design and User-Friendliness [10 Marks]
- ✅ Intuitive navigation
- ✅ Clear user flows
- ✅ Professional interface design
- ✅ Responsive across devices

### ✅ Version Control [10 Marks]
- ✅ Regular commits with descriptive messages
- ✅ Proper GitHub repository management
- ✅ Clear commit history demonstrating development process

## 🔮 Future Enhancements

### Planned Features
- Email notifications
- PDF invoice generation
- Mobile application
- Advanced analytics
- API integration
- Cloud deployment

### Technical Improvements
- Database migration to SQL Server
- Caching implementation
- Background job processing
- API versioning
- Microservices architecture

## 📝 Submission Details

**Student Information:**
- **Name:** Joe Leo Van Niekerk
- **Student Code:** ST10445055
- **Institution:** Eduvos
- **Course:** PROG6212 - Advanced Programming
- **Academic Year:** 2025

**Submission Components:**
1. ✅ Source Code (GitHub Repository)
2. ✅ PowerPoint Presentation
3. ✅ Implementation Documentation
4. ✅ Lecturer Feedback Implementation
5. ✅ Version Control History

## 🆘 Support

For support and questions:
- **Student:** Joe Leo Van Niekerk
- **Student Code:** ST10445055
- **Institution:** Emeris
- **Course:** PROG6212

---

<div align="center">

**Built with ❤️ by Joe Leo Van Niekerk (ST10445055)**

**ASP.NET Core 9.0 | Entity Framework Core | Bootstrap 5**

*POE Submission - December 2025*

</div>
