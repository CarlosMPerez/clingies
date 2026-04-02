# Clingies Change Checklist

## For any change
- Run `dotnet restore Clingies.sln` if package state may have changed.
- Run `dotnet build Clingies.sln`.
- Summarize anything that could not be verified locally.

## If changing domain or application code
- Check repository contracts and mapper/entity alignment.
- Check GTK event handlers and window state updates for affected properties.

## If changing infrastructure or persistence
- Add a new migration for schema changes; do not rewrite old migrations.
- Verify Dapper SQL aliases still match model/entity names.
- Re-check startup migration flow from `Clingies/Program.cs`.

## If changing GTK/UI behavior
- Smoke-test create, edit, pin, lock, roll, and close flows.
- Check tray/menu wiring and icon paths.
- Confirm assets still copy to output.

## If changing startup or configuration
- Verify the executable target remains `Clingies/Clingies.csproj`.
- Verify database path, lock file path, and log path assumptions still hold.
- Check single-instance behavior.

## Known gaps
- No automated tests currently exist.
- Some verification requires a Linux desktop session with GTK available.
