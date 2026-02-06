# Suri - City Builder Game

A simple SimCity-style city builder game built with Godot 4.

## Description

Suri is a 2D top-down city builder where you place buildings on a grid-based map, manage your city's economy, grow your population, and keep your citizens happy. Build residential, commercial, and industrial zones, connect them with roads, and add parks to boost happiness!

## Features

- **Grid-based building placement** - Place buildings on a tile-based grid
- **Multiple zone types**:
  - Residential (Green) - Houses for citizens
  - Commercial (Blue) - Shops and businesses
  - Industrial (Yellow) - Factories and production
  - Roads (Gray) - Connect zones
  - Parks (Light Green) - Boost happiness
- **Economy system** - Manage your budget with income from taxes and maintenance expenses
- **Population management** - Grow your population with residential zones
- **Happiness tracking** - Keep citizens happy with parks and smart zoning
- **Camera controls** - Pan and zoom to view your city

## How to Run

1. Install [Godot 4](https://godotengine.org/download) (version 4.0 or later)
2. Open Godot Engine
3. Click "Import" and select the `project.godot` file from this repository
4. Click "Import & Edit"
5. Press F5 or click the Play button to run the game

## Controls

### Camera
- **WASD** or **Arrow Keys** - Pan camera
- **Mouse Wheel** - Zoom in/out

### Building
- **Left Click** - Place selected building
- **Right Click** - Demolish building
- **Build Menu** - Click buttons on the right side to select building type

### Game
- **P Key** - Pause/Unpause game

## Game Mechanics

### Economy
- Starting funds: $10,000
- Each building has a placement cost
- Buildings generate income and incur maintenance costs every 5 seconds
- Income sources:
  - Residential zones: $10 (cost $2 maintenance)
  - Commercial zones: $20 (cost $3 maintenance)
  - Industrial zones: $15 (cost $4 maintenance)

### Population
- Population grows based on available residential zones
- Each residential zone supports up to 10 citizens
- Population generates demand for commercial and industrial zones

### Happiness
- Base happiness: 50%
- Parks boost happiness (+10 per park)
- Industrial zones reduce happiness (-5 per industrial zone)
- Happiness is clamped between 0% and 100%

## Project Structure

```
suri/
├── project.godot          # Godot project configuration
├── icon.svg              # Project icon
├── scenes/               # Game scenes
│   └── main.tscn        # Main game scene
├── scripts/              # GDScript files
│   ├── game_manager.gd  # Economy, population, and game state
│   ├── camera_controller.gd  # Camera panning and zooming
│   ├── building_system.gd    # Building placement/demolition
│   ├── grid_overlay.gd       # Visual grid display
│   ├── hud.gd               # UI and HUD
│   └── pause_handler.gd     # Pause functionality
├── assets/               # Textures and sprites (placeholder)
└── ui/                   # UI scenes and scripts
```

## Development

This is a basic implementation using simple colored rectangles as placeholder graphics. The game is fully functional and can be extended with:
- Better graphics and sprites
- Sound effects and music
- More building types
- Advanced city services (police, fire, healthcare)
- Natural disasters
- More complex economy simulation
- Save/load functionality

## License

This project is open source and available for educational purposes.
