# Clingies Commands

## SDK
- Preferred SDK: `.NET SDK 9.0.115` via `global.json`.
- Check active SDK: `dotnet --version`

## Restore and build
- Restore solution: `dotnet restore Clingies.sln`
- Build solution: `dotnet build Clingies.sln`
- Build host only: `dotnet build Clingies/Clingies.csproj`

## Run
- Run the executable host: `dotnet run --project Clingies/Clingies.csproj`

Do not use `Clingies.GtkFront` as the run target. It is a library project, not the executable entry point.

## Environment prerequisites
- GTK packages on Debian/Ubuntu/Mint: `sudo apt install libgtk-3-0 libgtk-3-dev`
- The app expects a Linux desktop session for GTK and tray behavior.

## Useful file locations
- Solution: `Clingies.sln`
- Entry point: `Clingies/Program.cs`
- Migrations: `Clingies.Infrastructure/Migrations`
- GTK assets: `Clingies.GtkFront/Assets`
- Package versions: `Directory.Packages.props`

## Manual smoke test checklist
1. Start the app and confirm it reaches GTK initialization.
2. Create a clingy and confirm it appears on screen.
3. Edit title/content and confirm updates persist after restart.
4. Toggle pin, lock, and roll, then restart and confirm state persists.
5. Start from an empty database and confirm migrations seed the required schema and menu/style data.
