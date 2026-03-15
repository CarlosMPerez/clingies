# Clingies Project Notes

## Purpose
Clingies is a Linux-only GTK3 desktop sticky-notes application built on .NET 9. The executable entry point is `Clingies/Program.cs`; the GTK UI code lives in `Clingies.GtkFront`.

## Solution layout
- `Clingies/`: app bootstrap, DI registration, startup logging, migration run, single-instance lock.
- `Clingies.Domain/`: domain models and shared domain concepts.
- `Clingies.Application/`: interfaces, services, command providers, and application orchestration.
- `Clingies.Infrastructure/`: SQLite access, Dapper repositories, entities, mappers, and FluentMigrator migrations.
- `Clingies.GtkFront/`: GTK windows, dialogs, tray/menu code, UI services, and assets.
- `Clingies.Utils/`: shared constants, enums, attributes, and small helpers.

## Runtime facts
- Linux only.
- GTK3 runtime is required.
- SQLite database path is built from `Environment.SpecialFolder.ApplicationData/Clingies/clingies.db`.
- A single-instance lock file is created in `Environment.SpecialFolder.ApplicationData/Clingies/clingies.lock`.
- Migrations run automatically during startup before GTK host initialization.
- Logs are written under `AppContext.BaseDirectory/logs/clingies.log`.

## Data model invariants
- A clingy aggregate spans `clingies`, `clingy_properties`, and `clingy_content`.
- New clingies are created with null content; update logic enforces text XOR png content.
- Soft delete sets `is_deleted = 1` and changes `type_id`; hard delete removes the root row and cascades children.
- Existing migrations should be treated as immutable once applied. Add new migrations for schema changes.

## Editing guidance
- Keep GTK-specific concerns in `Clingies.GtkFront`; do not move UI logic into Application or Domain.
- Keep raw SQL and Dapper mapping in Infrastructure.
- If you change a model shape, check corresponding entities, mappers, repositories, services, and GTK bindings.
- If you change assets, confirm `Assets/**` still copies to output from `Clingies.GtkFront.csproj`.

## Current repo constraints
- There are no automated test projects in the solution.
- Verification is primarily restore/build plus targeted manual smoke testing.
- The repo is pinned to SDK 9 because SDK 10 on this machine causes `dotnet build` to exit early without diagnostics.
