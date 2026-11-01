# Final polish changes

## Backend and authorization

- Added a standalone xUnit test project, included in the solution and excluded from the web project's source/content globbing.
- Added database-backed session access checks: deleted accounts, stale roles, inactive maintenance staff and unassigned wardens cannot continue using operational pages. Logout and access-denied pages remain available.
- Super Admin identification now requires a real Admin user with no Warden record. Bootstrap seeding recognizes this distinction too.
- Added global automatic antiforgery validation for unsafe MVC requests while preserving existing tokens.
- Student registration and staff account/profile creation now save their related user and profile in one EF transaction, avoiding orphan accounts if the profile insert fails.
- Normalized new registration/staff email addresses and login lookup.
- Room creation/editing validates room-number length, trims whitespace, and enforces capacity 1–20 at the service boundary.
- Allocation rejects cross-hostel students. Room-change requests reject other hostels, full rooms, empty/overlong reasons and stale allocation state at approval.
- Invalid room-change form redisplay stays scoped to available rooms in the student's hostel.
- Room deletion now explains when request history prevents deletion. Hostel deletion checks linked students, staff, menus and announcements before hitting restrictive foreign keys.
- Complaint assignment enforces staff hostel and active status in the service; invalid priority/status values cannot bypass transition rules.
- Feedback rejects invalid source/rating/comment length and missing students. Meal selection rejects invalid enum values.
- Meal cutoffs use an injectable TimeProvider for deterministic boundary testing; invalid cutoff settings fail explicitly instead of silently becoming midnight.
- Explicitly referenced `System.Security.Cryptography.Pkcs` 10.0.14 to resolve the vulnerable transitive package reported by NuGet. Other existing package versions and production database provider are preserved.

## Frontend

- Live, case-insensitive table search with clear/reset and result counts.
- Numeric-aware ascending/descending sorting with accessible sort state.
- Ten-row pagination, keyboard-operable controls and a clear no-results message.
- Textarea character counters and maxlength enforcement from existing validation metadata.
- Valid POST forms show saving feedback and prevent duplicate submissions; named submit-button values remain intact. Invalid/cancelled forms are not locked. Browser Back restores usable forms.
- Skip-to-content link, visible keyboard focus, alert/status semantics, responsive controls and reduced-motion support.
- Printing includes all matching table rows, not only the current page.
- All enhancements are progressive: existing MVC form submissions and full table content work without JavaScript.

## Delivery

- `TESTING.md` contains setup, commands, test boundaries and the remaining local acceptance checklist.
- `docs/VALIDATION.md` and `docs/validation/` contain the actual test results.
- Added a GitHub Actions test workflow; no commits, pushes, account changes or deployment were performed.
- No production schema changes or new migration are required. Source files already modified in the uploaded ZIP were preserved.
- Build output, editor caches and embedded Git metadata are excluded from the returned source ZIP. Your local user-secrets file and database are not part of this archive.

## Navigation follow-up

- Removed the duplicate Home and System Home links for authenticated users. Each role retains its Dashboard link; visitors retain the public Home page. The brand link and existing Home redirect remain functional.

## Password and Register styling follow-up

- Password toggles now show only the eye icon inside a single outlined input, including confirmation fields and staff/warden account creation. Removed the separate button border and Show/Hide text.
- Kept keyboard interaction, screen-reader labels, tooltips and pressed state for the toggle.
- Register navigation text is white on blue, with sufficient selector specificity to override the shared navbar text color.

## Profile footer overlap fix

- Removed the forced 100% height from profile cards and used a flex column with a 22px gap for the hostel information/services stack. Cards now size to their content, so the footer follows the complete profile content instead of crossing an overflowing card. Applies to assigned and unassigned students.

## Room-change error routing fix

- Both GET and POST room-change requests from a student without a room now redirect to RoomChangeRequests, not Profile/Dashboard.
- A dedicated RoomChangeError TempData key is consumed only by the room-change list alert, so Complaints cannot display or consume it.
- Added two HTTP regression cases checking GET/POST redirects, message isolation, single display and no request creation.
