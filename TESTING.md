# HostelHub testing and final review

## Start here (Windows / Visual Studio)

1. Extract this ZIP into a new folder. Keep your existing project/database backup.
2. Open `Hostel_hub.slnx` in a Visual Studio version supporting .NET 10, or open a terminal in the extracted `Hostel_hub` folder with the .NET 10 SDK installed.
3. Run the automated backend tests (they use isolated SQLite databases, not your LocalDB):

   ```powershell
   dotnet restore Hostel_hub.slnx
   dotnet test Tests/HostelHub.Tests/HostelHub.Tests.csproj --logger "trx;LogFileName=hostelhub.trx" --collect:"XPlat Code Coverage"
   ```

4. Run the frontend tests with Node.js 18 or newer:

   ```powershell
   cd Tests/frontend
   npm ci
   npm test
   cd ../..
   ```

5. To run the application, retain your existing `DefaultConnection` setting and user secrets. For a new database, apply the existing migrations using Visual Studio Package Manager Console: `Update-Database`. No new schema migration is required by these changes. For first-time Super Admin setup, configure `SeedAdmin:Email` and `SeedAdmin:Password` in **Manage User Secrets**. Do not put real passwords in source control.
6. Trust the development HTTPS certificate if needed: `dotnet dev-certs https --trust`. Run `dotnet run --launch-profile https`, then open `https://localhost:7155`. Use the HTTPS profile because authentication cookies are secure-only.
7. Use **Test > Test Explorer > Run All** in Visual Studio to run the .NET tests individually. `Tests/HostelHub.Tests/TestResults` holds TRX results and coverage when using the command above.

## What the suites cover

| Suite | Purpose |
| --- | --- |
| `UnitTests.cs` | BCrypt salting/password rejection, role redirects, registration validation, feedback rating bounds, deterministic meal cutoff boundaries |
| `ServiceIntegrationTests.cs` | Real relational SQLite transactions, unique keys, CRUD, room allocation and change approval/rejection, complaint lifecycle/history, feedback ownership, menus and cleanup, staff activation, hostel scoping and dashboard counts |
| `WebIntegrationTests.cs` | Real MVC pipeline, Razor rendering, authentication cookies, antiforgery protection, registration persistence, redirects, role and hostel access, deactivated sessions, 404 handling |
| `Tests/frontend/enhancements.test.cjs` | DOM interaction checks for search, sorting, pagination, empty states, counters, submit protection, print and accessibility semantics |

Every .NET database fixture has independent synthetic data. The app skips production admin seeding only in the `Testing` environment. Never configure a deployed app with this environment name. The production provider remains SQL Server. Tests do not connect to your project database.

SQLite cannot reproduce SQL Server's generated `rowversion`, SQL Server collation, migrations, lock/deadlock behavior or concurrent multi-user traffic. Its test-only context overrides rowversion generation. No claim of production concurrency validation is made. DOM tests are not visual browser tests.

## Local acceptance checklist (not executed against your LocalDB)

Use a disposable database copy and separate browser profiles for each role.

- Super Admin: create/edit a hostel with its warden; assign an unassigned student; verify system overview, staff overview and global announcements. Operational editing should remain restricted to the relevant warden.
- Warden: create/edit rooms; allocate a student from the same hostel; submit and approve/reject room changes; try a full room and foreign-hostel IDs; verify occupancy after each operation.
- Two simultaneous sessions: attempt the last available bed at the same time, and attempt allocating the same unallocated student to two different rooms. Verify no overbooking, duplicate allocation or occupancy mismatch. This remains a required SQL Server concurrency check.
- Student: register/login/logout; edit profile; create/edit a pending complaint; make a room-change request; verify rejected submissions never expose other-hostel rooms.
- Complaint workflow: warden assigns staff and priority; assigned staff moves Assigned → InProgress → Resolved; another staff member cannot update it; student submits one resolved-complaint feedback. Confirm status history and counts.
- Mess: create/edit a menu and dishes; select Take then Skip; check participation totals; verify breakfast closes at 23:00 on the previous date, lunch at 11:30 and dinner at 19:00 on the meal date. Cutoffs use the server's local timezone. Set the host timezone to the hostel's timezone (India for this deployment).
- Past-menu cleanup: existing behavior deliberately deletes past menus and their selections when the admin menu list is opened. Confirm that this history-retention behavior is still wanted before running against valuable records.
- Announcements: verify global and hostel-specific visibility, publish/expiry dates, and deactivation.
- Staff: create/edit/deactivate/reactivate; after deactivation, verify a browser already logged in cannot open operational pages.
- UI: test desktop and a 375px mobile viewport; keyboard Tab/Enter, skip link, password toggle, forms, dropdowns, table scrolling, search/sort/paging, empty results and browser Back. Check browser console for errors.
- Startup: test fresh database migrations and existing-database startup separately. Missing/incorrect LocalDB configuration is not tested by the SQLite suite.

## Continuous integration

`.github/workflows/tests.yml` runs the .NET and frontend suites and uploads TRX/coverage files when this project is pushed to GitHub. No repository was pushed or deployed as part of this update.
