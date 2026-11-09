# RaceDay Event Management System

## PROG6212 – Programming 2B

RaceDay is a web-based event management system developed for South African road running, walking and cycling events.

The system supports two main user roles:

- **Organisers:** Manage events, categories, participant enrolments and race results.
- **Participants:** Register accounts, log in, browse events, enter events and view their enrolments and results.

The project is developed using C#, ASP.NET Core, .NET 8, Entity Framework Core and Microsoft SQL Server. It includes a REST API, an MVC web application and automated tests.

GitHub repository: https://github.com/ShaunBuff/RaceDay

---

## Project Structure

The repository contains the following main components:

```text
RaceDay/
├── .github/
│   └── workflows/
├── docs/
│   ├── RaceDay_Database.sql
│   ├── API Endpoint Plan
│   └── RaceDay ERD.pdf
├── RaceDay.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Services/
│   └── Program.cs
├── RaceDay.API.Tests/
│   └── Automated tests
├── RaceDay.Mvc/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Views/
│   ├── wwwroot/
│   ├── Dockerfile
│   └── Program.cs
├── RaceDay.slnx
└── README.md
```

---

# Part 1 – System Planning and Database Design

Part 1 focused on planning the RaceDay system, designing the database and preparing the repository.

### Main deliverables

- Entity Relationship Diagram (ERD)
- API Endpoint Plan
- SQL database script
- Sample database data
- GitHub repository
- GitHub Actions workflow

### Database entities

The database consists of seven main entities:

- Users
- EventTypes
- Locations
- Events
- Categories
- Enrolments
- Results

### Database relationships

The database supports the following relationships:

- One User can organise many Events.
- One EventType can be associated with many Events.
- One Location can be associated with many Events.
- One Event can have many Categories.
- One Participant can have many Enrolments.
- One Event can have many Enrolments.
- One Category can be selected for many Enrolments.
- One Enrolment can have one Result.

Primary keys, foreign keys and other database constraints help maintain data integrity.

### Sample data

The SQL script includes sample organisers, participants, events, categories, enrolments and results.

### Part 1 video

The original Part 1 demonstration video is available here:

https://youtu.be/LAioX0J0bvE

---

# Part 2 – REST API Development

Part 2 introduced the RaceDay REST API using ASP.NET Core Web API and Entity Framework Core.

### Technologies used

- C#
- ASP.NET Core Web API
- .NET 8
- Entity Framework Core
- Microsoft SQL Server
- JWT bearer authentication
- Swagger / OpenAPI
- xUnit
- GitHub Actions

### API functionality

The API provides functionality for:

- User registration and login.
- Event listing and event details.
- Event creation, updating and deletion.
- Organiser ownership validation.
- Participant enrolments.
- Enrolment validation and duplicate prevention.
- Role-based access control.

### Authentication and authorisation

RaceDay uses JWT bearer tokens to authenticate users.

The API distinguishes between Organisers and Participants. Protected endpoints use role-based authorisation, while event ownership checks help prevent one Organiser from modifying another Organiser's events.

### API endpoint documentation

Swagger provides an interface for inspecting and testing the available API endpoints.

To access Swagger:

1. Start the `RaceDay.Api` project.
2. Open the HTTPS address shown in the application output.
3. Add `/swagger` to the address.

For the configured local development profile, the API uses:

`https://localhost:7149/swagger/index.html`

---

# Part 3 – MVC Web Application and Docker

The MVC application provides the web interface for interacting with the RaceDay API.

### Technologies used

- ASP.NET Core MVC
- C#
- .NET 8
- HttpClient
- JWT-based API communication
- Docker

### MVC functionality

The MVC application communicates with the REST API through an API service.

It provides the web interface for user authentication and the available RaceDay application functions.

### Running the MVC application locally

1. Open `RaceDay.slnx` in Visual Studio.
2. Ensure SQL Server and the API configuration are correct.
3. Start `RaceDay.Api`.
4. Set `RaceDay.Mvc` as the startup project.
5. Run the MVC application.

The configured local MVC development address is:

`https://localhost:7099/`

The actual address may vary depending on the selected launch profile.

### Running RaceDay using Docker

Docker is used to package and run the MVC application in a container.

From the repository root, build the Docker image:

```powershell
docker build -f RaceDay.Mvc/Dockerfile -t raceday-mvc .
```

Ensure the API is running on the host machine before starting the MVC container.

Run the container:

```powershell
docker run -d --name raceday-mvc-container `
-p 8080:8080 `
-v raceday-dp-keys:/root/.aspnet/DataProtection-Keys `
-e ApiSettings__BaseUrl="https://host.docker.internal:7149/" `
raceday-mvc
```

Open the Docker-hosted MVC application:

`http://localhost:8080/`

The `host.docker.internal` address allows the MVC container to communicate with the API running on the Windows host.

To stop the container:

```powershell
docker stop raceday-mvc-container
```

To remove it:

```powershell
docker rm raceday-mvc-container
```

**Development security note:** The current local development configuration includes certificate-validation bypass behaviour for API communication. This must be replaced with proper certificate validation before production deployment.

---

# Database and Application Configuration

### Prerequisites

Install the following software:

- .NET 8 SDK
- Microsoft SQL Server
- SQL Server Management Studio (SSMS)
- Visual Studio 2022 or a compatible .NET development environment
- Docker Desktop (for container execution)

### Database setup

1. Open SQL Server Management Studio.
2. Connect to your SQL Server instance.
3. Execute the SQL script located in `docs/RaceDay_Database.sql`.
4. Configure the API connection string.
5. Confirm that the database connection works before running the API.

The API expects the connection string configuration key:

`ConnectionStrings:RaceDayConnection`

### JWT configuration

The API expects the JWT signing key under:

`Jwt:Key`

Use .NET User Secrets or another secure configuration provider to supply the signing key.

Do not commit passwords, signing keys, access tokens or production credentials to GitHub.

---

# Building and Testing

Run the following commands from the repository root.

### Restore dependencies

```powershell
dotnet restore RaceDay.slnx
```

### Build the solution

```powershell
dotnet build RaceDay.slnx --configuration Release
```

### Run automated tests

```powershell
dotnet test RaceDay.API.Tests/RaceDay.API.Tests.csproj --configuration Release
```

### Latest local verification

The latest verified Release build completed successfully for all three projects:

- RaceDay.Api
- RaceDay.Mvc
- RaceDay.API.Tests

The automated test results were:

| Test result | Total |
|---|---:|
| Passed | 20 |
| Failed | 0 |
| Skipped | 0 |

These results reflect the latest local test run and may change as the project develops.

---

# Continuous Integration

GitHub Actions is used to automate repository validation and API build and test checks.

The repository includes workflows for:

- Part 1 repository and database validation.
- .NET solution restoration and building.
- Automated API test execution.

The workflows can be viewed under the **Actions** tab in the GitHub repository:

https://github.com/ShaunBuff/RaceDay/actions

---

# Azure Blob Storage

An initial `BlobStorageService` implementation and the Azure Storage Blobs NuGet package have been added to the API project.

**Current status:** Azure Blob Storage integration is not yet complete. The service still needs to be registered, connected to an Azure Storage Account and container, and integrated with the relevant application functionality before it can be described as a working feature.

---

# Demonstration Video

Add the final Part 3 demonstration video here after recording and uploading it to YouTube as an Unlisted video.

**Final demonstration video:** (https://youtu.be/XAf3ckhbkXs)

The demonstration should show the working application, relevant API functionality, automated testing and Docker execution.

---

# Source Code

The complete RaceDay source code, documentation, database script, automated tests and development history are available at:

https://github.com/ShaunBuff/RaceDay


