# Cafe CRM Desktop

Cafe CRM is a WPF (.NET 8) application that helps a cafe team keep track of guests, tables, and POS checks in real time. The solution is split into clean layers (Domain → Infrastructure → Application → App) and ships with a lightweight POS adapter plus an embedded SQLite database.

## Solution layout
| Project | Description |
| --- | --- |
| `CafeCrm.App` | WPF UI with MVVM (CommunityToolkit.Mvvm), navigation service, and toast notifications |
| `CafeCrm.Application` | Use-cases, DTOs, hosted services, and abstractions for POS/event history |
| `CafeCrm.Domain` | Rich entities (`Customer`, `Visit`, `OrderItem`, etc.) + repository contracts |
| `CafeCrm.Infrastructure` | EF Core + SQLite, repositories, migrations, and seeding |
| `CafeCrm.Pos` | Mock POS client that streams tickets for development/demo purposes |

## Highlights
- Touch-friendly dashboard with live customer list plus “Active tables” grid (auto-refresh every 10 seconds).
- Customer visit dialog with validation, loading indicators, and toast-based notifications—no blocking message boxes.
- Visit detail screen that shows guest info, order items, totals, durations, and visit type (dine-in / takeaway).
- Background `PosTicketIngestionService` that consumes POS tickets, opens visits, assigns checks, and writes to the event log.
- Centralized `EventHistoryService` + `EventLog` repository for auditing integrations.
- Database initializer seeds realistic customers, visits, and order items for an instant demo.

## Technology
- .NET 8, WPF, MVVM Toolkit
- Generic Host + dependency injection everywhere (App + BackgroundService)
- Entity Framework Core (SQLite, precision mappings, owned `OrderItems` collection)
- Reactive notifications surfaced via custom `INotificationService` + WPF toast container
- Background integration service talking to a mock POS client

## Getting started
```powershell
# Restore & build everything
.\.dotnet\dotnet build CafeCrm.sln

# Apply pending migrations (creates Data/cafecrm.db under the app folder)
.\.dotnet\dotnet ef database update --project src/CafeCrm.Infrastructure --startup-project src/CafeCrm.App

# Run the WPF app
.\.dotnet\dotnet run --project src/CafeCrm.App
```

When the app boots it will automatically run `DatabaseInitializer`, apply migrations, and seed demo data (customers, visits, order items). The POS background service also starts automatically and listens for mock tickets.

## Database & migrations
- Domain entities expose business behavior (e.g., `Customer.AddVisit`, `Visit.AssignPosCheck`, `Visit.AddOrderItem`) so EF Core stays in sync with the “rich” model.
- `CafeCrmDbContext` configures precision, indexes, cascade behaviors, and the owned `OrderItems` table.
- Use `.dotnet\dotnet ef migrations add <Name> --project src/CafeCrm.Infrastructure --startup-project src/CafeCrm.App` to evolve the schema.

## Event history
- `EventLog` table captures POS-related events (check assignment, visit creation, connection failures, etc.).
- `IEventLogRepository` + `EventLogRepository` store and query the feed.
- `EventHistoryService` exposes `LogAsync`/`GetRecentAsync` so background services and future UI panels can surface integration diagnostics.

## Next steps
- Surface event history inside the dashboard (timeline or log tab).
- Add settlement/close-visit commands so waiters can finalize checks from the detail screen.
- Expand analytics (RFM, loyalty), export flows, and hook into a real POS adapter.
