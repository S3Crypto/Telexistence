# TelexistenceAPI

Telexistence Remote Monitoring and Control API - A robust .NET 8 based API for remote robot operation and monitoring.

## 📝 Project Overview

TelexistenceAPI is a comprehensive solution for remote robot operation, providing a secure and scalable API that enables:

- Remote control of robots through commands (move, rotate)
- Real-time monitoring of robot status and position
- Authentication and authorization for secure access
- Command history tracking
- Containerized deployment with Docker
- CI/CD pipeline with GitHub Actions
- Infrastructure as Code with Terraform

## 🛠️ Technology Stack

- **Backend**: .NET 8 Web API
- **Database**: MongoDB
- **Authentication**: JWT-based authentication
- **Infrastructure**: Azure App Service, Azure Cosmos DB with MongoDB API
- **CI/CD**: GitHub Actions
- **Infrastructure as Code**: Terraform
- **Containerization**: Docker
- **Testing**: xUnit, Moq, TestContainers
- **Logging**: Serilog with Azure Application Insights

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Docker Desktop
- MongoDB (local instance or Docker)
- Azure CLI (for deployment)
- Terraform (for infrastructure provisioning)

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/TelexistenceAPI.git
   cd TelexistenceAPI
   ```

2. **Run with Docker Compose** (recommended)
   ```bash
   docker-compose up --build
   ```
   This will start the API on http://localhost:5000 and a MongoDB instance.

3. **Run locally without Docker**
   ```bash
   # Start a MongoDB instance
   docker run -d -p 27017:27017 --name mongodb mongo:latest

   # Build and run the API
   dotnet restore
   dotnet build
   cd src/TelexistenceAPI
   dotnet run
   ```

4. **Access the API**
   
   The API will be available at:
   - HTTP: http://localhost:5000
   - HTTPS: https://localhost:5001

### Authentication

To use the authenticated endpoints, first obtain a JWT token:

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

Use the returned token in the Authorization header for subsequent requests:

```bash
curl -X GET http://localhost:5000/api/status \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## 🧪 Running Tests

### Running Unit Tests

```bash
dotnet test tests/TelexistenceAPI.Tests
```

### Running Integration Tests

```bash
dotnet test tests/TelexistenceAPI.IntegrationTests
```

Note: Integration tests use TestContainers to spin up a MongoDB container, so Docker must be running.

## 🔄 CI/CD Pipeline

The project includes GitHub Actions workflows for CI/CD:

### CI Workflow (.github/workflows/ci.yml)

Triggered on:
- Push to `main` and `develop` branches
- Pull requests to `main` and `develop` branches

Steps:
1. Checkout code
2. Setup .NET 8
3. Restore dependencies
4. Build the solution
5. Run tests
6. Publish the API
7. Upload artifact

### CD Workflow (.github/workflows/cd.yml)

Triggered when the CI workflow completes successfully on the `main` branch.

Steps:
1. Checkout code
2. Download artifact
3. Setup Terraform
4. Azure Login
5. Terraform Init
6. Terraform Plan
7. Terraform Apply (only on `main` branch)
8. Deploy to Azure App Service

## 🏗️ Infrastructure as Code

The project uses Terraform to provision Azure resources:

- Azure Resource Group
- Azure App Service Plan
- Azure Linux Web App
- Azure Cosmos DB (MongoDB API)
- Azure Application Insights

To apply the Terraform configuration:

```bash
cd terraform
terraform init
terraform plan -var-file="dev.tfvars"
terraform apply -var-file="dev.tfvars"
```

## 📚 API Documentation

### Authentication

**POST /api/auth/login**
```json
{
  "username": "admin",
  "password": "admin123"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2023-01-01T12:00:00Z",
  "username": "admin"
}
```

### Robot Status

**GET /api/status**

Returns all robots and their current status.

Response:
```json
[
  {
    "id": "1",
    "name": "TX-010",
    "position": {
      "x": 0,
      "y": 0,
      "z": 0,
      "rotation": 0
    },
    "status": "Idle",
    "currentTask": null,
    "lastUpdated": "2023-01-01T12:00:00Z"
  }
]
```

**GET /api/status/{robotId}**

Returns a specific robot's status.

### Robot Commands

**POST /api/command**
```json
{
  "command": "move",
  "robot": "1",
  "parameters": {
    "direction": "forward",
    "distance": 2.0
  }
}
```

Response:
```json
{
  "id": "command-id",
  "command": "move",
  "robot": "1",
  "status": "Completed",
  "user": "user-id",
  "createdAt": "2023-01-01T12:00:00Z",
  "executedAt": "2023-01-01T12:00:01Z"
}
```

**GET /api/command/{commandId}**

Returns details of a specific command.

**PUT /api/command/{commandId}**
```json
{
  "command": "move",
  "parameters": {
    "direction": "left",
    "distance": 1.0
  }
}
```

### Command History

**GET /api/history/{robotId}?limit=10**

Returns the command history for a specific robot.

Response:
```json
[
  {
    "id": "command-id",
    "command": "move",
    "robot": "1",
    "status": "Completed",
    "user": "user-id",
    "createdAt": "2023-01-01T12:00:00Z",
    "executedAt": "2023-01-01T12:00:01Z"
  }
]
```

### Health Check

**GET /health**

Returns the health status of the API.

Response:
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "API",
      "status": "Healthy",
      "description": "API is healthy",
      "duration": 0.0123
    }
  ],
  "totalDuration": 0.0123
}
```

## 🌐 Frontend Integration Guide

### Real-time Status Updates

For real-time robot status updates, frontend applications should:

1. **Poll the status endpoint periodically**:
   ```javascript
   // Example using JavaScript fetch API
   async function pollRobotStatus(robotId) {
     try {
       const response = await fetch(`/api/status/${robotId}`, {
         headers: {
           'Authorization': `Bearer ${token}`
         }
       });
       const data = await response.json();
       updateUI(data);
     } catch (error) {
       console.error('Error fetching robot status:', error);
     }
   }

   // Poll every 2 seconds
   setInterval(() => pollRobotStatus('1'), 2000);
   ```

2. **Send commands**:
   ```javascript
   // Example of sending a move command
   async function sendMoveCommand(robotId, direction, distance) {
     try {
       const response = await fetch('/api/command', {
         method: 'POST',
         headers: {
           'Content-Type': 'application/json',
           'Authorization': `Bearer ${token}`
         },
         body: JSON.stringify({
           command: 'move',
           robot: robotId,
           parameters: {
             direction: direction,
             distance: distance
           }
         })
       });
       const data = await response.json();
       return data;
     } catch (error) {
       console.error('Error sending command:', error);
       throw error;
     }
   }
   ```

3. **Check command status**:
   ```javascript
   async function checkCommandStatus(commandId) {
     try {
       const response = await fetch(`/api/command/${commandId}`, {
         headers: {
           'Authorization': `Bearer ${token}`
         }
       });
       const data = await response.json();
       return data;
     } catch (error) {
       console.error('Error checking command status:', error);
       throw error;
     }
   }
   ```

4. **View command history**:
   ```javascript
   async function getCommandHistory(robotId, limit = 10) {
     try {
       const response = await fetch(`/api/history/${robotId}?limit=${limit}`, {
         headers: {
           'Authorization': `Bearer ${token}`
         }
       });
       const data = await response.json();
       return data;
     } catch (error) {
       console.error('Error fetching command history:', error);
       throw error;
     }
   }
   ```

### Command Parameters

#### Move Command

Parameters:
- `direction`: string ("forward", "backward", "left", "right")
- `distance`: double (optional, defaults to 1.0)

#### Rotate Command

Parameters:
- `degrees`: double (positive for clockwise, negative for counter-clockwise)

## 📊 Time Breakdown

- Reading and understanding the requirements: 15 minutes
- Initial project setup and architecture: 15 minutes
- Implementing core entities and interfaces: 30 minutes
- Implementing controllers and services: 1 hour
- Setting up repositories and database context: 30 minutes
- Configuring authentication and middleware: 30 minutes
- Testing and debugging: 30 minutes
- Setting up CI/CD and Terraform: 15 minutes
- Documentation: 15 minutes

## 📦 Future Enhancements

1. **WebSocket Support**: Implement WebSocket for real-time communication between the server and clients
2. **Enhanced Command Types**: Add more command types for complex robot operations
3. **Multi-robot Orchestration**: Support for coordinating multiple robots
4. **Role-based Access Control**: More granular access control based on user roles
5. **Robot Firmware Updates**: Support for remote firmware updates
6. **Analytics Dashboard**: Real-time analytics and monitoring
