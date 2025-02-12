# Gresham Recordings Management System

## Overview
The Gresham Recordings Management System is an enterprise-grade ASP.NET Core Web API that provides comprehensive management of audio recordings, with features including authentication, audit logging, and integration with Azure services.

## Features
- User Authentication and Authorization
- Audit Logging System
  - Comprehensive audit trail of all system actions
  - Filtering capabilities by:
    - Event Type (e.g., "Recording", "User", "System")
    - Search text (across username, record ID, event name, and details)
    - Date range
  - Paginated results with configurable page size
- Historical Recording Management
- Live Recording Controls
- Azure Blob Storage Integration
- Tag Management
- Comments System
- Comprehensive API Documentation

## Prerequisites
- .NET 9.0 SDK or later
- SQL Server LocalDB
- Visual Studio 2022, JetBrains Rider, or VS Code
- Azure Account (for blob storage)
- Git

## Required Secrets
The following secrets need to be configured. Please contact Mcjoseph.Agbanlog@wizard-ai.com to obtain the actual values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<SQL_CONNECTION_STRING>",
    "AzureBlobStorage": "<AZURE_BLOB_CONNECTION_STRING>"
  },
  "AuthSettings": {
    "Scope": "<AUTH_SCOPE>",
    "ClientSecret": "<CLIENT_SECRET>",
    "ClientId": "<CLIENT_ID>"
  },
  "AdminCredentials": {
    "UserName": "<ADMIN_USERNAME>",
    "Password": "<ADMIN_PASSWORD>"
  }
}
```

## Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/your-org/gresham-recordings.git
cd gresham-recordings/backend
```

### 2. Configure User Secrets
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>"
dotnet user-secrets set "ConnectionStrings:AzureBlobStorage" "<your-blob-connection>"
```

### 3. Setup Database
```bash
dotnet ef database update
```

### 4. Run the Application
```bash
dotnet run
```
Access the API at `https://localhost:7277` or `http://localhost:5277`

## Project Structure
```
backend/
├── Controllers/     # API endpoints
├── Services/        # Business logic
├── Data/           # Database context and models
├── DTOs/           # Data transfer objects
├── Middleware/     # Custom middleware
├── Extensions/     # Extension methods
└── Constants/      # Shared constants
```

## API Documentation
Swagger documentation is available at `/swagger` when running the application.

### Key Endpoints

#### Authentication
- `POST /api/v1.0/Auth/login` - Authenticate user
- `POST /api/v1.0/Auth/logout` - End user session
- `POST /api/v1.0/Auth/refresh` - Refresh authentication token

#### Recordings
- `GET /api/v1.0/Recording` - List all recordings
- `GET /api/v1.0/Recording/download` - Download recording file
- `GET /api/v1.0/Recording/stream` - Stream recording audio
- `POST /api/v1.0/Recording/sync` - Synchronize recordings with storage

#### Audit Logs
- `GET /api/v1.0/Audit` - View audit logs with filtering and pagination

## Development

### Code Style
- Follow Microsoft's C# coding conventions
- Use async/await for asynchronous operations
- Include XML documentation for public APIs

### Testing
```bash
dotnet test
```

### Database Migrations
```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

## Deployment

### Prerequisites
- Azure subscription
- Azure CLI installed
- .NET 9.0 SDK

### Steps
1. Create Azure resources:
   ```bash
   az group create --name gresham-recordings --location eastus
   az webapp create --resource-group gresham-recordings --plan myAppServicePlan --name gresham-api
   ```

2. Deploy the application:
   ```bash
   dotnet publish -c Release
   az webapp deploy --resource-group gresham-recordings --name gresham-api --src-path bin/Release/net9.0/publish
   ```

## Troubleshooting

### Common Issues
1. Database Connection
   - Verify connection string
   - Check SQL Server is running
   - Ensure database exists

2. Azure Blob Storage
   - Verify storage account exists
   - Check connection string
   - Verify container permissions

### Logging
- Logs are stored in `logs/` directory
- Use Azure Application Insights in production
- Set log level in appsettings.json

## Contributing
1. Fork the repository
2. Create feature branch
3. Commit changes
4. Create pull request

## License
Proprietary - All rights reserved

## Support
Contact support at Mcjoseph.Agbanlog@wizard-ai.com


