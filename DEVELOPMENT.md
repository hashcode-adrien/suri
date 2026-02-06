# Development Guide

## Prerequisites

1. **Godot 4.x .NET Edition**
   - Download from: https://godotengine.org/download/
   - Make sure to download the .NET/Mono version, not the standard version
   
2. **.NET 8.0 SDK**
   - Usually included with Godot .NET edition
   - Can be downloaded separately from: https://dotnet.microsoft.com/download

## Opening the Project

1. Launch Godot 4 .NET Edition
2. Click "Import"
3. Browse to the project folder and select `project.godot`
4. Click "Import & Edit"
5. Wait for the project to load and compile C# scripts

## Project Structure

```
suri/
├── scenes/              # Game scenes
│   └── Main.tscn       # Main game scene (entry point)
├── scripts/            # All C# scripts
│   ├── BuildingData.cs
│   ├── BuildingPlacer.cs
│   ├── CameraController.cs
│   ├── EconomyManager.cs
│   ├── GameManager.cs
│   ├── GridManager.cs
│   ├── HUD.cs
│   └── PopulationManager.cs
├── assets/             # For future assets (textures, sounds, etc.)
├── ui/                 # For future UI scenes
├── project.godot       # Godot project configuration
├── Suri.csproj        # C# project file
├── Suri.sln           # Visual Studio solution
└── icon.svg           # Project icon
```

## Running the Game

### In Godot Editor
- Press **F5** or click the **Play** button
- To run the current scene: Press **F6**

### Controls During Play
- **WASD/Arrow Keys**: Pan camera
- **Mouse Wheel**: Zoom in/out
- **Left Click**: Place building (after selecting from menu)
- **Right Click**: Demolish building
- **P**: Pause/Resume

## Making Changes

### Adding a New Building Type

1. Open `scripts/BuildingData.cs`
2. Add new enum value to `BuildingType`
3. Add new entry to `BuildingRegistry.Buildings` array
4. The building will automatically appear in the build menu

### Modifying Game Balance

Edit the export variables in `scenes/Main.tscn` or directly in the scripts:
- Starting money: `EconomyManager.StartingMoney`
- Income tick rate: `EconomyManager.IncomeTickInterval`
- Population growth rate: `PopulationManager.GrowthRate`
- Grid size: `GridManager.GridWidth/GridHeight`
- Camera speed: `CameraController.PanSpeed`

### Debugging

1. Use `GD.Print()` for console logging
2. Enable debugger in Godot (debugger tab at bottom)
3. Set breakpoints in your IDE (Visual Studio, VS Code, or Rider)
4. Run with debugger attached

## Common Issues

### "C# scripts not compiling"
- Make sure you have .NET 8.0 SDK installed
- Go to Project → Tools → C# → Regenerate C# project files
- Restart Godot

### "Main scene not found"
- Make sure `scenes/Main.tscn` exists
- Check `project.godot` has the correct path to Main.tscn

### "Scripts not found in scene"
- The scene file uses relative paths (res://)
- Make sure all scripts are in the `scripts/` folder

## Code Style

This project follows standard C# conventions:
- PascalCase for public members and types
- camelCase with underscore prefix for private fields (_field)
- Descriptive variable names
- XML documentation comments for public APIs
- Namespace: `Suri`

## Architecture

See `ARCHITECTURE.md` for detailed system architecture and design decisions.

## Building for Distribution

1. In Godot: Project → Export
2. Add export templates if not already installed
3. Configure platform (Windows, Linux, macOS)
4. Export the project
5. The exported build will include the .NET runtime

## Testing

Currently, the project doesn't have automated tests. To test:
1. Run the game
2. Try placing each building type
3. Verify economy updates every 5 seconds
4. Verify population updates every 3 seconds
5. Test pause functionality
6. Test camera controls
7. Test building demolition

## Future Enhancements

Ideas for expanding the game:
- Save/Load system
- More building types (police, fire, hospital, school)
- Natural disasters
- Better graphics (sprites instead of colored rectangles)
- Sound effects and music
- Multiple city zones/regions
- Citizens with individual needs
- Traffic simulation
- Resource management (power, water)
- Day/night cycle
- Seasons
