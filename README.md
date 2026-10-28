# Bikuida CS2

Bikuida is an external CS2 menu application built with **C# and .NET 10**. It uses WPF for the menu and ImGui.NET for the overlay.

## In Game Preview

The screenshot below shows Bikuida running in-game, with its configuration menu and visual overlay.

<img width="1913" height="1077" alt="menu" src="https://github.com/user-attachments/assets/b1716851-f69e-4cc2-8dfd-1c26fdac8866" />

### Available Settings

- **ESP:** Skeleton, Box, Name, Health, Lines and Weapon.
- **Opacity:** Adjusts the overlay transparency.
- **Trigger:** Configurable field of view.
- **Aimbot:** Configurable field of view.

Some features may require additional configuration and compatible offsets.

## Features

- WPF configuration menu.
- Transparent overlay rendered with ImGui.NET.
- Entity data models and world-to-screen conversion.
- Periodic updates and input helpers.
- Remote offsets loaded from JSONBin.io.
- Windows x64, self-contained, single-file publishing.

> Use the software only in environments where you have permission and follow the rules of the software you interact with.

## Requirements

- Windows x64
- .NET 10 SDK
- Visual Studio with the .NET desktop development workload, or the .NET CLI

## Build and Publish

From the project directory, run:

```powershell
dotnet publish -c Release
```

The project is configured for Windows x64, self-contained and single-file publishing. The output is normally created at:

```text
bin\Release\net10.0-windows\win-x64\publish\
```

To publish directly to `release/publish/`, run:

```powershell
dotnet publish -c Release -o .\release\publish
```

Self-contained publishing includes the .NET runtime, but the application still requires a compatible Windows system.

## Remote Offsets (JSONBin.io)

Bikuida loads offsets from [JSONBin.io](https://jsonbin.io/). When an update changes the required offsets, update the JSON document in JSONBin.io instead of changing the values in the source code.

1. Check which offsets have changed.
2. Update the existing JSON document, keeping its field names and structure unchanged.
3. Save the document.
4. Test that the application can load the updated data.

The endpoint is configured in `Services/OffsetsService.cs`. The JSON structure must remain compatible with `Models/Offsets.cs`. Use the project's existing JSON as the template.

**Security:** Do not commit private API credentials to source control. Credentials embedded in a desktop application can be extracted. The application should also use standard TLS certificate validation.

## Project Structure

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

## Application Flow

1. `Program.cs` starts the application.
2. The menu updates settings through `EspMenuViewModel` and `EspConfig`.
3. `BikuidaBootstrap` initializes the services.
4. `GameLoop` updates entity data and the view matrix.
5. `MainOverlay` and `EspRenderer` draw enabled visual elements.

The update loop targets an interval of about **4.10 ms**. This is a configured target, not a measured or guaranteed rate.

## Maintenance

- Validate offsets after updates to the target software.
- Keep the remote JSON compatible with `Models/Offsets.cs`.
- Profile Release builds before making performance claims.
- Review thread synchronization and ensure background loops stop cleanly.

## References

- [.NET 10 documentation](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview)
- [.NET support policy](https://dotnet.microsoft.com/platform/support/policy/dotnet-core)
- [Publishing .NET applications](https://learn.microsoft.com/dotnet/core/deploying/)
- [Single-file deployment](https://learn.microsoft.com/dotnet/core/deploying/single-file/)
- [WPF documentation](https://learn.microsoft.com/dotnet/desktop/wpf/)
- [JSONBin.io](https://jsonbin.io/)
