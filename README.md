# Clingies

Sticky‑note–style desktop app for Linux. Each note is a **Clingy**: lightweight, always-on-top (when pinned), quick to create, easy to organize. Built with **GTK3** and **.NET** for speed and simplicity.

> **Status:** actively developed. Linux‑only (tested on Linux Mint 22 Cinnamon). Avalonia code was split off; current front‑end is GTK3.

---

## Features (current)

- Create, view, and edit “clingy” notes
- Custom title bar with pin/lock/roll controls
- Soft delete (`is_deleted`) and hard delete (cascades to children)
- Per‑note properties (position, size, pin/lock/roll/standing)
- Per‑note content: **either** text **or** PNG image (XOR enforced on update)
- SQLite persistence via **Dapper** + **FluentMigrator**
- DI with **Microsoft.Extensions.DependencyInjection**
- Logging via **Serilog**
- System tray indicator + dynamic menu (driven by DB `system_menu`)

## Planned / Roadmap

- Sleeping / recurring notes (scheduler)
- Style presets and theming persistence
- i18n / l10n infrastructure
- Full‑text search (FTS5) for text content
- Import/export of user settings

---

## Tech Stack

- **Language:** C# (targeting .NET 9)
- **UI:** GTK# (GTK3), Linux‑only
- **Persistence:** SQLite, Dapper, FluentMigrator
- **DI:** Microsoft.Extensions.DependencyInjection
- **Logging:** Serilog
- **Dev env:** VS Code on Linux Mint 22

---

## Solution Layout

```
src/
    Clingies.sln
    Clingies/                    # App composition / bootstrap (host)
    Clingies.Domain/             # Core models (DB-mirrors) and enums used across layers
    Clingies.Application/   # DTOs, mapping, services (domain-centric, no UI types)
    Clingies.Infrastructure/     # Repositories, migrations, connection factory
    Clingies.GtkFront/           # GTK3 front-end (widgets, windows, utils)
    Clingies.Utils/         # (Shared) constants, attributes, small enums (UI-agnostic) [optional]
```

**Layering rule of thumb**

- **Infrastructure** deals with **Models** (1:1 with tables).
- **Application** maps Models ⇄ DTOs and enforces invariants.
- **GtkFront** converts DTO bytes to `Gdk.Pixbuf` for rendering (UI types never leak down).

---

## Getting Started

### Prerequisites

- .NET SDK 9.0
- GTK3 runtime (Debian/Ubuntu/Mint): `sudo apt install libgtk-3-0 libgtk-3-dev`
- SQLite (native lib bundled with Microsoft.Data.Sqlite)

### Build & Run

```bash
# restore & build
dotnet restore
dotnet build -c Release

# run GTK front-end
dotnet run --project src/Clingies.GtkFront
```

**Foreign keys:** enable enforcement per connection string or PRAGMA after open.

```csharp
var csb = new SqliteConnectionStringBuilder {
  DataSource = dbPath,
  ForeignKeys = true,
  Mode = SqliteOpenMode.ReadWriteCreate
};
using var conn = new SqliteConnection(csb.ToString());
conn.Open();
```

**Dapper snake_case mapping:** call once at startup.

```csharp
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
```

---

## Repositories (Models only)

- Repos operate strictly on **Models** (`Clingy`, `ClingyProperties`, `ClingyContent`).
- Services map to DTOs and handle UI-facing logic.
- Creation inserts parent, then children (shared PK), all in one transaction.
- Update is **dirty-aware** (loads current aggregate, compares, and updates only changed tables in one TX).
- Soft delete flips `is_deleted`; hard delete removes parent and cascades to children.

---

## Logging

- **Serilog** is used throughout. Configure sinks (console, file) via app settings or programmatic config.

---


## License

    - TO-DO
---

## Notes

- The project intentionally avoids DB triggers and stored procedures. The database stays “dumb”; invariants (like content XOR) live in code.
- Images are stored as PNG **BLOBs** in `clingy_content`. UI converts `byte[]` ⇄ `Gdk.Pixbuf` at the boundary.
- If/when search is added, consider FTS5 for `clingy_content.text`.
