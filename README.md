# TelexistenceAPI
Telexistence Remote Monitoring and Control API

TelexistenceAPI/
├── src/
│   ├── TelexistenceAPI/                        # Main API project
│   │   ├── Controllers/                        # API controllers
│   │   ├── Models/                             # Data models
│   │   ├── Services/                           # Business logic services
│   │   ├── Repositories/                       # Data access layer
│   │   ├── Middleware/                         # Custom middleware
│   │   ├── Extensions/                         # Extension methods
│   │   ├── DTOs/                               # Data transfer objects
│   │   ├── Program.cs                          # Application entry point
│   │   ├── appsettings.json                    # Configuration
│   │   └── TelexistenceAPI.csproj              # Project file
│   └── TelexistenceAPI.Core/                   # Core business logic
│       ├── Entities/                           # Domain entities
│       ├── Interfaces/                         # Interfaces for dependencies
│       ├── Services/                           # Core business services
│       └── TelexistenceAPI.Core.csproj         # Project file
├── tests/
│   ├── TelexistenceAPI.Tests/                  # Unit tests
│   │   ├── Controllers/                        # Controller tests
│   │   ├── Services/                           # Service tests
│   │   └── TelexistenceAPI.Tests.csproj        # Test project file
│   └── TelexistenceAPI.IntegrationTests/       # Integration tests
│       └── TelexistenceAPI.IntegrationTests.csproj
├── terraform/                                  # IaC with Terraform
│   ├── main.tf                                 # Main configuration
│   ├── variables.tf                            # Variables
│   └── outputs.tf                              # Outputs
├── .github/
│   └── workflows/
│       ├── ci.yml                              # CI workflow
│       └── cd.yml                              # CD workflow
├── Dockerfile                                  # Docker configuration
├── docker-compose.yml                          # Docker Compose for local dev
└── README.md                                   # Project documentation
