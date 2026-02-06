# Suri - City Builder Game

A simple SimCity-style city builder game built with **Godot 4 using C# (Mono)**.

## Description

Suri is a 2D top-down city builder where you can:
- Place buildings on a grid-based map (residential, commercial, industrial zones)
- Build roads and infrastructure to connect zones
- Manage your city budget through taxes and expenses
- Grow your population and keep citizens happy
- Balance zoning and services to build a thriving city

## Requirements

- **Godot 4.x .NET/Mono Edition** is required to run this project
- .NET 8.0 SDK (comes with Godot .NET edition)

## How to Run

1. Install [Godot 4.x .NET/Mono Edition](https://godotengine.org/download/)
2. Clone or download this repository
3. Open Godot 4 .NET edition
4. Import the project by selecting the `project.godot` file
5. Press F5 or click the Play button to run the game

## Controls

- **WASD** or **Arrow Keys**: Pan the camera around the map
- **Mouse Scroll Wheel**: Zoom in/out
- **Left Click**: Place selected building (must select from build menu first)
- **Right Click**: Demolish building (get 50% refund)
- **P Key**: Pause/Resume the game

## Game Mechanics

### Building Types

- **Residential (Green)** - $100
  - Houses for citizens
  - Each provides capacity for 10 population
  - Generates $20 income per tick
  - Maintenance: $5/tick

- **Commercial (Blue)** - $150
  - Shops and businesses
  - Generates $50 income per tick
  - Maintenance: $8/tick

- **Industrial (Yellow)** - $200
  - Factories and production facilities
  - Generates $80 income per tick
  - Decreases happiness slightly
  - Maintenance: $10/tick

- **Road (Gray)** - $10
  - Connects zones and infrastructure
  - Maintenance: $1/tick

- **Park (Light Green)** - $50
  - Boosts citizen happiness
  - Maintenance: $2/tick

### Economy System

- **Starting Money**: $10,000
- **Income**: Generated from taxes on zones (calculated every 5 seconds)
- **Expenses**: Maintenance costs for buildings
- **Net Income**: Income - Expenses (displayed in HUD)

### Population System

- Population grows based on available residential zones
- Growth rate affected by happiness level
- Each residential zone can house 10 citizens
- Population updates every 3 seconds

### Happiness System

- Base happiness: 50%
- Parks increase happiness
- Industrial zones decrease happiness
- Low happiness (<30%) causes population decline
- Happiness affects population growth rate

## Project Structure

```
suri/
├── scenes/          # Godot scene files (.tscn)
│   └── Main.tscn   # Main game scene
├── scripts/         # C# script files (.cs)
│   ├── BuildingData.cs
│   ├── GridManager.cs
│   ├── CameraController.cs
│   ├── EconomyManager.cs
│   ├── PopulationManager.cs
│   ├── GameManager.cs
│   ├── BuildingPlacer.cs
│   └── HUD.cs
├── assets/          # Textures and sprites (placeholder graphics)
├── ui/              # UI scenes
├── project.godot    # Godot project file
├── Suri.csproj      # C# project file
├── Suri.sln         # Visual Studio solution file
└── README.md        # This file
```

## Development Notes

- All scripts are written in **C# (.cs files)** - no GDScript
- Uses simple colored rectangles as placeholder graphics
- Target framework: .NET 8.0
- Godot version: 4.3+

## Tips

1. Start by placing residential zones to grow your population
2. Add commercial and industrial zones to generate income
3. Balance your budget - don't build too much at once
4. Place parks near residential areas to keep citizens happy
5. Industrial zones generate good income but reduce happiness
6. Roads are cheap - use them to organize your city layout

## License

This project is open source and available for educational purposes.
