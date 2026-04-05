# Vacation Manager

Vacation Manager is an ASP.NET Core MVC application for managing employees, teams, projects, roles and leave requests inside an organization.

## Features

- Authentication with demo accounts
- Role-aware access for `CEO`, `Team Lead`, `Developer` and `Unassigned`
- CRUD management for users, roles, teams and projects
- Leave requests for paid, unpaid and sick leave
- Approval flow for `Team Lead` and `CEO`
- File upload/download for sick leave documents
- Pagination and filtering across the main listings
- Render deployment with PostgreSQL support

## Tech Stack

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


## Render Deployment

The project contains:

- [render.yaml](C:\Users\teodo\Desktop\Vacation_Manager\Vacation_Manager\render.yaml)
- [Dockerfile](C:\Users\teodo\Desktop\Vacation_Manager\Vacation_Manager\Dockerfile)

To use PostgreSQL on Render, add this environment variable to the web service:

```text
ConnectionStrings__DefaultConnection
```

Set its value to the Render PostgreSQL internal database URL.

## Persistent Uploads

Sick leave files can be persisted outside the container by setting:

```text
UPLOADS_ROOT
```

Example for a Render persistent disk mounted at `/var/data`:

```text
UPLOADS_ROOT=/var/data/uploads
```

Without `UPLOADS_ROOT`, files are stored in `wwwroot/uploads`, which may be lost after redeploys.

## Project Structure

- `Controllers/` - MVC controllers
- `Models/` - domain models and view models
- `Data/` - EF Core DbContext and seed initializer
- `Services/` - repository, current user access and helpers
- `Views/` - Razor views
- `wwwroot/` - static assets

## Notes

- The database is persistent when PostgreSQL is configured.
- File uploads are only persistent if `UPLOADS_ROOT` points to durable storage.
