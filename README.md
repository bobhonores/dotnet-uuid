# Playground with UUIDv7

This project tries to use different NuGet packages which implements UUIDv7. To do so, a simple web api will be used. The API should generate the identifier and the persist it into a SQL database.

NuGet packages to test:
- UUIDNext (https://github.com/mareek/UUIDNext)
- Medo.Uuid7 (https://github.com/medo64/Medo.Uuid7/)
- UuidExtensions (https://github.com/stevesimmons/uuid7-csharp)
- Olivier Martinet (https://medium.com/@oliviermartinet/creating-a-uuid-v7-generator-in-c-a95b23cf6a99)
- Tanner Gooding (https://github.com/dotnet/runtime/pull/104124/files)
- Nguid (https://github.com/bgrainger/NGuid)

## GitHub issues

- Extend System.Guid with a new creation API for v7 https://github.com/dotnet/runtime/issues/103658
- Improve our SQL Server client-generated GUIDs https://github.com/dotnet/efcore/issues/33579
