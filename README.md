# Bikuida CS2

Bikuida is a external cs2 menu project built with **C# and .NET 10**. It uses WPF for its menu and ImGui.NET for the overlay.

## Features

- WPF configuration menu.
- Transparent overlay rendered with ImGui.NET.
- Entity data models and world-to-screen coordinate conversion.
- Periodic data updates and keyboard/mouse input helpers.
- Remote offsets loaded from JSONBin.io.
- Windows x64, self-contained, single-file publishing configuration.

> Use only in authorized test environments and follow the rules of the software you interact with.

## Technologies

- C# / .NET 10 (`net10.0-windows`)
- WPF and Windows Forms
- ImGui.NET 1.90.0.1
- ClickableTransparentOverlay 9.1.0
- `System.Numerics` for vectors and matrices
- P/Invoke (`user32.dll`) for Windows input APIs
- `HttpClient` and `System.Text.Json` for remote JSON data

## Project structure

```text
Config/       Application options and colors
Game/         Periodic update loop
Helpers/      Input, item names, and coordinate helpers
MemAccess/    Process memory reading helpers
Menu/         WPF window and view model
Models/       Entity, game data, and offsets models
Overlay/      Overlay window and renderer
Services/     Startup, input, and offsets services
Resources/    Application icon
Program.cs    Application entry point
*.csproj      Build and publish settings
*.slnx       Visual Studio solution
```

## Application flow

1. `Program.cs` starts the WPF application and menu.
2. The menu updates settings through `EspMenuViewModel` and `EspConfig`.
3. `BikuidaBootstrap` coordinates startup and initializes services.
4. `GameLoop` periodically updates entity data and the view matrix.
5. `MainOverlay` and `EspRenderer` draw the enabled visual elements.
6. Other services handle keyboard state, offsets, and input helpers.

The update loop targets an interval of about **4.10 ms** (roughly 244 cycles per second). This is a configured target, not a measured or guaranteed rate.

## Build and publish

### Requirements

- Windows x64
- .NET 10 SDK
- Visual Studio with the .NET desktop development workload, or the .NET CLI

Restore and build:

```powershell
dotnet restore .\bikuidacs2.csproj
dotnet build .\bikuidacs2.csproj -c Release
```

Publish a release build:

```powershell
dotnet publish -c Release
```

The project is configured for Windows x64, self-contained and single-file publishing. The default output folder is usually:

```text
bin\Release\net10.0-windows\win-x64\publish\
```

To place the published files in `release/publish/`, run:

```powershell
dotnet publish -c Release -o .\release\publish
```

Self-contained publishing includes the .NET runtime, but the application still requires a compatible Windows system.

## Remote offsets (JSONBin.io)

Offsets are loaded from [JSONBin.io](https://jsonbin.io/). When an update changes the required offsets, update the JSON document manually in JSONBin.io instead of changing the values in the source code each time.

1. Check which offsets changed.
2. Update the values in the JSON document while keeping the expected field names and structure.
3. Save the document in JSONBin.io.
4. Run the application in an authorized test environment and verify that the response loads correctly.

The endpoint is configured in `Services/OffsetsService.cs`. Keep the JSON structure compatible with the models in `Models/Offsets.cs`.

**Security note:** the current `OffsetsService.cs` accepts any TLS certificate. Remove that override and keep standard certificate validation enabled. Do not store private API credentials in source control; values embedded in a desktop application can be extracted.

## Notes for maintenance

- Offsets and memory layouts may change when the target software is updated; validate them before use.
- Shared data is accessed by the update loop and overlay, so thread synchronization should be reviewed.
- Profile a Release build before making performance claims. No benchmark results are included here.
- Consider adding automated tests, structured logging, and clean cancellation/shutdown for background loops.

## References

- [.NET 10 documentation](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview)
- [.NET support policy](https://dotnet.microsoft.com/platform/support/policy/dotnet-core)
- [Publishing .NET applications](https://learn.microsoft.com/dotnet/core/deploying/)
- [Single-file deployment](https://learn.microsoft.com/dotnet/core/deploying/single-file/)
- [WPF documentation](https://learn.microsoft.com/dotnet/desktop/wpf/)
- [JSONBin.io](https://jsonbin.io/)
