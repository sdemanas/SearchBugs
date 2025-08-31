# SearchBugs Documentation

Welcome to the SearchBugs documentation. This directory contains comprehensive guides and documentation for the SearchBugs application.

## 📖 Documentation Overview

### Core Features

- [**User Impersonation**](./IMPERSONATION.md) - Complete guide for the user impersonation feature
- [**API Documentation**](./Api.md) - API endpoints and usage
- [**Role Management**](./Role.md) - User roles and permissions system

### Development Guides

- [**Repository Features**](./REPOSITORY_FEATURE_IMPLEMENTATION.md) - Git repository integration features
- [**Bug Management**](./BUG_ADD_PAGE_FIX_SUMMARY.md) - Bug tracking and management features
- [**Notification System**](./NOTIFICATION_TESTING_GUIDE.md) - Real-time notification testing guide

### Project Information

- [**Main README**](../README.md) - Project overview and setup instructions
- [**Contributing Guidelines**](../CONTRIBUTING.md) - How to contribute to the project
- [**Code of Conduct**](../CODE_OF_CONDUCT.md) - Community guidelines

## 🏗️ Architecture Overview

SearchBugs is built using:

- **Backend**: .NET Core Web API with Clean Architecture
- **Frontend**: React with TypeScript and Vite
- **Database**: Entity Framework Core with SQL Server/SQLite
- **Authentication**: JWT tokens with custom claims
- **Real-time**: SignalR for notifications

## 🚀 Quick Start

1. **Clone the repository**

   ```bash
   git clone https://github.com/vicheanath/SearchBugs.git
   cd SearchBugs
   ```

2. **Setup Backend**

   ```bash
   cd src/SearchBugs.Api
   dotnet restore
   dotnet run
   ```

3. **Setup Frontend**

   ```bash
   cd src/SearchBugs.Ui
   npm install
   npm run dev
   ```

4. **Access the Application**
   - Backend API: `https://localhost:7071`
   - Frontend UI: `http://localhost:5173`

## 📁 Project Structure

```
SearchBugs/
├── docs/                           # Documentation (this folder)
├── src/
│   ├── SearchBugs.Api/            # Web API project
│   ├── SearchBugs.Application/    # Application layer
│   ├── SearchBugs.Domain/         # Domain layer
│   ├── SearchBugs.Infrastructure/ # Infrastructure layer
│   ├── SearchBugs.Persistence/    # Data persistence layer
│   ├── SearchBugs.Ui/            # React frontend
│   └── Shared/                    # Shared utilities
├── test/                          # Test projects
└── docker-compose.yml             # Docker configuration
```

## 🔧 Key Features

- **Bug Tracking**: Comprehensive bug management system
- **Project Management**: Multi-project support
- **User Management**: Role-based access control with impersonation
- **Git Integration**: Repository browsing and file management
- **Real-time Notifications**: Live updates using SignalR
- **Audit Logging**: Complete activity tracking
- **RESTful API**: Well-documented REST endpoints

## 📱 User Roles

### Admin

- Full system access
- User impersonation capabilities
- System configuration
- Audit log access

### Project Manager

- Project creation and management
- Team member assignment
- Bug assignment and tracking

### Developer

- Bug fixing and updates
- Code repository access
- Time tracking

### Tester

- Bug reporting
- Test case management
- Bug verification

## 🛠️ Development

### Prerequisites

- .NET 9.0 SDK
- Node.js 18+
- SQL Server or SQLite
- Git

### Environment Setup

1. Copy `appsettings.Development.json.example` to `appsettings.Development.json`
2. Update connection strings and configuration
3. Run database migrations: `dotnet ef database update`
4. Start the development servers

### Testing

```bash
# Backend tests
dotnet test

# Frontend tests
cd src/SearchBugs.Ui
npm run test
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](../CONTRIBUTING.md) for details.

### Pull Request Process

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Update documentation
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

## 🔍 Additional Resources

- **Issues**: Report bugs or request features on [GitHub Issues](https://github.com/vicheanath/SearchBugs/issues)
- **Discussions**: Community discussions and Q&A
- **Wiki**: Additional documentation and guides

---

_Last Updated: August 30, 2025_
