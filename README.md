# RaceDay Event Management System

## PROG6212 Programming 2B

RaceDay is a web-based event management system designed for South African road running, walking and cycling events.

The system supports two main user roles: Organisers and Participants. Organisers manage events, categories, enrolments and participant results, while Participants can register, browse events, enter events and view their results.

This repository contains the planning and database work completed for Part 1 of the PROG6212 Programming 2B PoE.

## System Purpose

RaceDay is planned as a centralised event management system for road running, walking and cycling events. The system is designed to help Organisers manage events and participant information while allowing Participants to register for events and track their results.

---

## Part 1

Part 1 focuses on system planning, database design and repository configuration.

The main deliverables are:

- Entity Relationship Diagram (ERD)
- API Endpoint Plan
- SQL Database Script
- GitHub repository
- GitHub Actions CI/CD validation

---

## User Roles

## Database Entities

## Database Relationships

The main relationships in the RaceDay database are:

- One User can organise many Events.
- One EventType can be associated with many Events.
- One Location can be associated with many Events.
- One Event can have many Categories.
- One Participant can have many Enrolments.
- One Event can have many Enrolments.
- One Category can be selected for many Enrolments.
- One Enrolment can have one Result.

These relationships are implemented using primary keys and foreign keys.

The RaceDay database consists of seven main entities:

- Users
- EventTypes
- Locations
- Events
- Categories
- Enrolments
- Results

Each entity has a specific purpose within the RaceDay database and supports the functionality required by the system.

## Database Constraints

The SQL database uses several constraints to maintain data integrity:

- Primary keys uniquely identify records.
- Foreign keys maintain relationships between related tables.
- NOT NULL constraints ensure required information is provided.
- UNIQUE constraints prevent duplicate values where appropriate.
- DEFAULT constraints provide suitable default values where required.

## Sample Database Data

The SQL script contains realistic sample data for testing the RaceDay database.

The sample data includes:

- 2 Organisers
- 2 Participants
- 3 Events
- Categories for each Event
- Sample Enrolments
- Sample Results

This sample data allows the relationships between the database entities to be tested before development of the application.

### Organiser

Organisers will be able to:

- Create events
- Edit events
- Delete events
- Manage event categories
- View participant enrolments
- Capture participant results
- View information about events they manage

### Participant

Participants will be able to:

- Register an account
- Log in
- Browse available events
- Enter an event
- Select an event category
- View their own enrolments
- View and track their race results and performance history

---

## Repository Structure

```text
RaceDay/
│
├── .github/
│   └── workflows/
│       └── build.yml
│
├── docs/
│   ├── RaceDay_Database.sql
│   ├── API Endpoint Plan
│   └── RaceDay ERD.pdf
│
└── README.md

## API Endpoint Plan

The planned RaceDay REST API will provide endpoints for the main functions of the system.

The endpoint plan covers:

- Authentication
- User Profiles
- Events
- Categories
- Event Enrolments
- Results

The API will be developed in a later part of the project. Part 1 focuses on planning the endpoints before implementation.

### Authentication Endpoints

The planned authentication endpoints are:

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/auth/register` | Registers a new user. |
| POST | `/api/auth/login` | Authenticates a registered user. |

Authentication will also determine whether the user is an Organiser or Participant.

### Event Endpoints

The planned event endpoints are:

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/events` | Returns available RaceDay events. |
| GET | `/api/events/{id}` | Returns details for a specific event. |
| POST | `/api/events` | Creates a new event for an Organiser. |
| PUT | `/api/events/{id}` | Updates an existing event. |
| DELETE | `/api/events/{id}` | Deletes an event. |

Event creation, updating and deletion will be restricted to Organisers.

### Category Endpoints

The planned category endpoints will allow Organisers to manage categories associated with events.

Categories allow Participants to select the appropriate event category when entering an event.

The endpoint plan includes operations for viewing categories and creating, updating and deleting categories where required.

### Enrolment Endpoints

The planned enrolment endpoints will allow Participants to enter RaceDay events.

An enrolment records the relationship between:

- Participant
- Event
- Category

Organisers will also be able to view enrolments for the events they manage.

### Results Endpoints

The planned results functionality will allow Organisers to capture participant race results.

A result records information such as:

- Finish time
- Finishing position

Participants will be able to view their own race results and performance history.

## Part 1 Status

### Completed

- [x] ERD completed
- [x] API Endpoint Plan completed
- [x] SQL database created
- [x] Database relationships tested
- [x] Sample data inserted
- [x] SQL script added to `/docs`
- [x] GitHub repository configured
- [x] GitHub Actions CI/CD configured
- [x] CI/CD workflow successfully tested
- [x] README documentation completed
- [x] Unlisted video uploaded
- [x] Unlisted youtube video link = https://youtu.be/LAioX0J0bvE


## CI/CD

GitHub Actions is used to validate the Part 1 repository.

The workflow checks that the required repository structure and database files are present.

The latest `RaceDay Part 1 CI` workflow completed successfully.

## Demonstration Video

The Part 1 demonstration video link will be added after the video has been recorded and uploaded to YouTube as an Unlisted video.


## Running RaceDay API

### Prerequisites
- .NET 8 SDK
- SQL Server
- Visual Studio 2022 or another compatible .NET IDE

### Database setup
1. Create the RaceDay database using the SQL script in the `docs` folder.
2. Configure the SQL Server connection string in the API configuration.
3. Configure the JWT signing key using .NET user secrets or another secure configuration provider.

### Build the solution

```powershell
dotnet restore RaceDay.slnx
dotnet build RaceDay.slnx --configuration Release
```

### Run the automated tests

```powershell
dotnet test RaceDay.API.Tests/RaceDay.API.Tests.csproj --configuration Release
```

### API documentation
Run the API and open its Swagger UI to inspect and test the available endpoints.

### Continuous integration
GitHub Actions builds the API and runs the automated tests when changes are pushed or pull requests are opened against the configured branches.

Improve the README
Document setup, SQL Server configuration, JWT secrets, Swagger, endpoints, and test commands.
Commit message: Document API setup and automated testing

Add authentication validation tests
Test duplicate email registration, invalid login credentials, and password verification. If any of these are already covered, add only genuinely missing cases.

Improve event validation
Verify that invalid event dates or missing required event details are rejected, and that an organiser cannot modify another organiser's event.

Add enrolment validation tests
Cover invalid event IDs, a category belonging to another event, and duplicate enrolments. Test the actual endpoint behaviour where practical.

Improve API endpoint documentation
Ensure Swagger descriptions and request/response examples accurately document the endpoints, authentication requirements, and role restrictions.

Improve CI validation
Confirm the workflow builds the exact solution, runs the test project, and fails if tests fail. Document the successful workflow in the README and capture the actual green result.

## Part 2 — RaceDay REST API

### Overview

The RaceDay REST API is an ASP.NET Core Web API developed in C# using .NET 8 and Entity Framework Core. It supports road running, walking, and cycling event management, including participant enrolments and race results.

The API uses SQL Server for persistent data storage and JWT bearer tokens for authentication and role-based access control.

### Technologies Used

- C#
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- Microsoft SQL Server
- JWT bearer authentication
- Swagger / OpenAPI
- xUnit automated testing
- GitHub Actions continuous integration

### Prerequisites

Before running the application, install:

- .NET 8 SDK
- Microsoft SQL Server
- Visual Studio 2022 or another compatible .NET development environment

### Database Configuration

1. Open SQL Server Management Studio (SSMS).
2. Connect to your SQL Server instance.
3. Create and configure the RaceDay database using the SQL script in the `docs` folder.
4. Configure the database connection string in the API configuration.
5. Verify that the connection string matches your SQL Server instance and database name.

Do not commit passwords, private credentials, or production secrets to GitHub.

### JWT Configuration

Configure the JWT signing key using .NET user secrets or another secure configuration provider.

The application expects the signing key under the `Jwt:Key` configuration setting. The issuer and audience must match the values configured by the API.

### Restoring and Building the Solution

Open a terminal in the repository root and run:

```powershell
dotnet restore .\RaceDay.slnx
dotnet build .\RaceDay.slnx --configuration Release
```

### Running Automated Tests

Run the automated test project with:

```powershell
dotnet test .\RaceDay.API.Tests\RaceDay.API.Tests.csproj --configuration Release
```

The test suite includes password hashing, authentication validation, authorization configuration, and enrolment validation scenarios.

### Running the API

Set `RaceDay.Api` as the startup project in Visual Studio and run the application.

When the API is running, open its Swagger UI using the HTTPS address shown in the application output, followed by `/swagger`.

### Authentication and API Testing

1. Register a participant using the registration endpoint.
2. Log in using the login endpoint to obtain a JWT bearer token.
3. Select **Authorize** in Swagger and enter the token.
4. Test protected endpoints using an account with the appropriate role.
5. Verify that unauthorized or invalid requests are rejected.

### Continuous Integration

GitHub Actions is configured to restore, build, and test the solution when supported branches are updated or pull requests are opened. Check the **Actions** tab in the GitHub repository to review workflow results.

### Repository

Source code and development history are available at:

https://github.com/ShaunBuff/RaceDay



