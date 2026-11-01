# Validation record — 30 October 2026

## Executed successfully

- .NET 10.0.403 SDK, Linux, test database provider SQLite.
- Final application/test compilation: **0 warnings, 0 errors** after the cryptography dependency update.
- Standard `dotnet test` with xUnit/VSTest: **96 passed, 0 failed, 0 skipped**. Final run duration: 1 minute 38 seconds.
- Frontend Node test runner with jsdom: **9 passed, 0 failed**.
- `node --check` passed for the new frontend script.
- Final package does not contain generated build directories, Git history, editor caches, or node_modules.

The .NET suite consists of 12 unit cases, 20 service/database integration cases and 64 HTTP integration cases. The latter include 45 role-specific feature-page render cases, registration, real login cookies, antiforgery rejection, access denials and disabled sessions.

## Coverage (final Cobertura result)

| Source group | Covered executable lines | Line coverage |
| --- | --- | --- |
| Services/ | 1039/1297 | 80.1% |
| Controllers/ | 701/1664 | 42.1% |
| Security/ | 17/19 | 89.5% |

Overall instrumented line coverage is 23.37%; the denominator includes migration/snapshot code and generated Razor code. Coverage is not complete, and passing tests do not establish correctness for every input or concurrent execution. Raw measurements are in `validation/coverage.cobertura.xml`.

## Not verified here

- Your existing LocalDB contents, Windows/IIS deployment and SQL Server migration execution.
- SQL Server rowversion, collation and multi-user race conditions. SQLite's test model does not generate SQL Server rowversion values.
- Real-browser visual layout and touch interaction. Browser installation was unavailable; the frontend tests validate DOM behavior rather than pixel rendering.
- Load/performance testing or exhaustive security review.

Use the local acceptance checklist in `../TESTING.md` before your final demonstration/deployment. The retained TRX and frontend output are evidence of the completed automated runs, not a certification that every feature is defect-free.

## Navigation follow-up

The authenticated Home/System Home links were subsequently hidden, leaving Dashboard navigation. This small Razor visibility change was inspected in source; the .NET SDK was no longer available for a repeat build in this session. The test results above refer to the preceding full validation run.

Password styling follow-up: JavaScript syntax and a DOM smoke check verified icon-only password toggling in both directions and accessible state updates. Razor markup was inspected across all six password fields. No .NET rebuild or real-browser visual verification was performed for this small styling change.

Profile footer follow-up: inspected both hostel-assignment branches and removed the overflowing percentage-height rule. Source checks confirm content-sized cards and the shared stack spacing. Real-browser layout verification remains pending.

Room-change error follow-up: inspected GET/POST eligibility guards and message-key usage. Two new HTTP regression cases were added but could not be executed because the .NET SDK is unavailable in this follow-up environment. The previous 96-test pass remains historical evidence; it does not include these two new cases.
