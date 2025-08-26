# iwellDashboard

## Overview

iwellDashboard is a full-stack web application for monitoring and managing battery data.  

- **BatteryDashboard.Server**: The backend API built with .NET Core, responsible for handling data processing and serving endpoints.
- **batterydashboard.client**: The frontend application developed using Angular, providing a user-friendly interface for data visualization and interaction.

## Technologies Used

- **Frontend**: Angular  
- **Backend**: .NET Core Web API  
- **Database**: Azure SQL  
- **Authentication**: Azure Active Directory
- **Hosting**: Azure App Service (for both backend and frontend)
- **Version Control**: GitHub

The application provides a user-friendly dashboard for real-time battery status, power, and capacity information.

---

## Features

- User login and registration  
- Role-based access (User / BatteryOwner)  
- Real-time battery status and power visualization  
- Interactive charts (using ApexCharts)  
- Secure authentication via Azure Active Directory

---
## Setup Instructions


### Prerequisites

- Node.js (for Angular)  
- Angular CLI (`npm install -g @angular/cli`)  
- .NET SDK (for backend)  
- Azure SQL Database (existing instance)  

---

## Local Setup

### Backend (.NET)

1. Navigate to the backend folder:

```bash
cd DoNetBackend
```

2. Restore dependencies:
```
dotnet restore
```

3. Configure the connection string in appsettings.json:
```
"ConnectionStrings": {
  "SqlConnectionString": "Server=tcp:<your-server>.database.windows.net;Database=<your-db>;User ID=<username>;Password=<password>;Encrypt=True;"
}
```

4. Run the backend API:
```
dotnet run
```


### Frontend (Angular)

1. Navigate to the Angular app folder:
```
cd AngularApp
```
2. Install dependencies:
```
npm install
```
3. Serve the app:
```
ng serve
```
4. Open your browser at
```
http://localhost:4200
```

## Deployment

### Backend (Azure App Service)

1. Create an Azure App Service instance.
2. Deploy the .NET backend using Visual Studio, Azure CLI, or GitHub Actions.
3. Configure the connection string in App Service → Configuration → Application Settings.
4. Ensure the Azure SQL firewall allows access from the App Service.

### Frontend (Angular)

Option 1: Deploy Angular to Azure Static Web Apps (recommended).

Option 2: Serve Angular via .NET backend (place dist folder in wwwroot).


## Security Best Practices

- Do not commit credentials (appsettings.json) to GitHub.
- Use environment variables or App Service Application Settings for production secrets.
- Use HTTPS for all API calls.

## GitHub Workflow
1. Initialize your local repo:
```
git init
git add .
git commit -m "Initial commit"
```

2. Add the remote:
```
git remote add origin https://github.com/yourusername/iwellDashboard.git
```

3. Push to Github:
```
git branch -M main
git push -u origin main
```

## Contributing 
- Fork the repository
- Create a feature branch
- Commit your changes
- Open a Pull Request


## Architecture
<img width="1536" height="1024" alt="593e293d-89b7-4ee5-94c0-043d4ee5d821" src="https://github.com/user-attachments/assets/847f0131-3de6-42cb-aa52-c519ed6a4b3f" />
