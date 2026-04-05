# Vacation Manager

Vacation Manager is an ASP.NET Core MVC web application for managing employees, roles, teams, projects, and leave requests inside an organization.

## Overview

The system supports:

- user authentication
- role-based access
- user, role, team, and project management
- leave request creation and approval
- paid, unpaid, and sick leave flows
- file upload and download for sick leave documents
- pagination and filtering in the main listings

## Roles

The application includes these roles:

- `CEO`
- `Team Lead`
- `Developer`
- `Unassigned`

Access is role-aware:

- `CEO` can manage users, roles, teams, and projects
- `Team Lead` can approve leave requests for members of the team they lead
- `Developer` can create, edit, and delete only their own non-approved leave requests

## Main Features

### Users

- paginated users list
- filtering by username, first name, last name, and role
- user details view
- team assignment
- create, edit, and delete user actions for `CEO`

### Roles

- list of all roles
- count of users in each role
- details view with users assigned to the role
- create, edit, and delete role actions for `CEO`

### Teams

- paginated teams list
- filtering by team name and project name
- team details with leader and members
- add and remove members
- create, edit, and delete team actions for `CEO`

### Projects

- paginated projects list
- filtering by project name and description
- project details with assigned teams
- create, edit, and delete project actions for `CEO`

### Leave Requests

- create paid, unpaid, and sick leave requests
- edit and delete only before approval
- approval flow for `Team Lead` and `CEO`
- filter by creation date
- sick leave file upload and download

## Technology Stack

- ASP.NET Core MVC
- Entity Framework Core
- PostgreSQL
- Bootstrap
- Render

## Demo Accounts

- `ceo / ceo123`
- `lead1 / lead123`
- `lead2 / lead123`
- `dev1 / dev123`

## Local Development

Project path:

```text
C:\Users\teodo\Desktop\Vacation_Manager\Vacation_Manager
```

Run locally with:

```powershell
dotnet restore
dotnet run
```

If no database connection string is configured, the application falls back to an in-memory database for local testing.

## Database

The project is configured to work with PostgreSQL through Entity Framework Core.

Environment variable used in production:

```text
ConnectionStrings__DefaultConnection
```

On Render, this should point to the PostgreSQL internal database URL.

## Render Deployment

The project includes:

- `render.yaml`
- `Dockerfile`

The deployed application is hosted on Render and uses PostgreSQL for persistent data storage.

## Important Deployment Note

Database records are persistent because the application is connected to PostgreSQL.

Uploaded sick leave files are currently **not guaranteed to persist** on Render free plan, because free web services use an ephemeral filesystem and do not support persistent disks.

This means:

- users, teams, projects, roles, and leave requests stay saved in the database
- uploaded sick leave files may be lost after redeploy or restart

This is an infrastructure limitation of Render free plan, not a database issue in the application.

## Project Structure

- `Controllers/` - MVC controllers
- `Models/` - domain models and view models
- `Data/` - EF Core DbContext and seed logic
- `Services/` - repository and helper services
- `Views/` - Razor views
- `wwwroot/` - static files

## Validation and Business Rules

The application includes validation for:

- required fields
- maximum text lengths
- unique usernames
- valid leave date ranges
- sick leave without half-day option
- required sick leave attachment
- delete and edit only before leave approval
- approval only by allowed higher-level users

## Future Improvements

Possible future improvements:

- persistent file storage for sick leave documents
- EF Core migrations workflow instead of automatic creation
- automated tests
- stronger password storage with hashing

