# HostelHub — Hostel Management and Maintenance System

A hostel management and maintenance system built with ASP.NET Core MVC, Entity Framework Core, and SQL Server.

## Tech Stack
- ASP.NET Core MVC (.NET 10)
- Entity Framework Core + SQL Server (LocalDB for development)
- BCrypt.Net-Next for password hashing
- Cookie-based authentication with role-based authorization
- Bootstrap for UI

## Roles
- **Admin/Warden** — manages students, hostels, rooms, staff, and complaints
- **Student** — files complaints, requests room changes, views mess menu
- **Maintenance Staff** — updates assigned complaint status

## Week 1 Status (Completed)
- Project scaffold and layered architecture (Controllers/Services/Data/ViewModels)
- 9 core domain entities with EF Core relationships and constraints
- Initial database migration applied to SQL Server LocalDB
- Authentication: registration, login, logout with BCrypt password hashing
- Role-based authorization (Admin/Student/MaintenanceStaff)
- Seeded Admin account via configuration (not hardcoded)
- Shared Bootstrap layout with role-aware navigation

## Upcoming
- Week 2: Complaint workflow, room-change workflow
- Week 3: Mess/meal-selection system
- Week 4: Unit and integration testing

## Local Setup
1. Requires .NET 10 SDK and SQL Server LocalDB (bundled with Visual Studio).
2. Update `appsettings.Development.json` with your own `SeedAdmin` credentials if needed.
3. Run `Update-Database` in the Package Manager Console to apply migrations.
4. Run the project (F5) — a database and seeded Admin account will be created automatically on first run.