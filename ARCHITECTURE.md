# Suri - Architecture Overview

## System Architecture

### Core Systems

1. **GameManager** (`scripts/GameManager.cs`)
   - Central game state controller
   - Handles pause/resume functionality
   - Manages selected building type
   - Coordinates between all other systems

2. **GridManager** (`scripts/GridManager.cs`)
   - Manages the 40x30 tile grid
   - Handles building placement and removal
   - Renders buildings as colored rectangles
   - Provides grid/world coordinate conversion
   - Draws grid overlay

3. **CameraController** (`scripts/CameraController.cs`)
   - Camera panning with WASD/Arrow keys
   - Zoom with mouse wheel
   - Smooth interpolation for camera movement

4. **BuildingPlacer** (`scripts/BuildingPlacer.cs`)
   - Handles mouse input for building placement
   - Shows preview tile at mouse position
   - Left-click to place, right-click to demolish
   - Checks affordability before placement
   - Provides 50% refund on demolition

5. **EconomyManager** (`scripts/EconomyManager.cs`)
   - Tracks current money (starts at $10,000)
   - Calculates income from buildings every 5 seconds
   - Calculates maintenance expenses
   - Emits signals on money changes

6. **PopulationManager** (`scripts/PopulationManager.cs`)
   - Tracks current population
   - Population grows based on residential capacity
   - Calculates happiness from building modifiers
   - Population growth affected by happiness
   - Updates every 3 seconds

7. **HUD** (`scripts/HUD.cs`)
   - Displays game statistics (money, population, happiness)
   - Build menu with buttons for each building type
   - Pause indicator
   - Income/expense information

### Data Structures

**BuildingData** (`scripts/BuildingData.cs`)
- Defines building types enum
- BuildingData struct with properties:
  - Cost, maintenance, color
  - Population capacity
  - Income per tick
  - Happiness modifier
- BuildingRegistry with static building definitions

### Scene Hierarchy

```
Main (Node2D)
├── GameManager (Node)
├── EconomyManager (Node)
├── PopulationManager (Node)
├── GridManager (Node2D)
│   ├── TileContainer (Node2D)
│   │   └── ColorRect tiles (created dynamically)
│   └── GridLines (Node2D)
│       └── Line2D grid lines
├── BuildingPlacer (Node2D)
│   └── Preview tile (ColorRect)
├── Camera2D
└── HUD (CanvasLayer)
    ├── Top bar (ColorRect)
    │   └── Labels (Money, Population, Happiness, etc.)
    └── Build menu (VBoxContainer)
        └── Building buttons
```

## Game Loop

1. **Input Processing**
   - Camera movement (CameraController)
   - Building selection (HUD buttons → GameManager)
   - Building placement (BuildingPlacer)
   - Pause toggle (GameManager)

2. **Update Ticks**
   - Economy tick (every 5 seconds)
     - Calculate income from all buildings
     - Calculate maintenance costs
     - Update money
   - Population tick (every 3 seconds)
     - Calculate happiness from buildings
     - Grow/shrink population based on capacity and happiness

3. **Rendering**
   - GridManager draws building tiles
   - HUD updates labels based on signals
   - BuildingPlacer shows preview tile

## Signal System

The game uses Godot signals for loose coupling:

- `EconomyManager.MoneyChanged` → HUD updates money display
- `PopulationManager.PopulationChanged` → HUD updates population display
- `PopulationManager.HappinessChanged` → HUD updates happiness display
- `GameManager.GamePaused` → HUD shows/hides pause indicator
- `GameManager.BuildingTypeSelected` → Broadcast for UI updates

## Building Types

| Type | Cost | Maintenance | Color | Special Properties |
|------|------|-------------|-------|-------------------|
| Residential | $100 | $5 | Green | +10 pop capacity, +$20 income |
| Commercial | $150 | $8 | Blue | +$50 income |
| Industrial | $200 | $10 | Yellow | +$80 income, -10% happiness |
| Road | $10 | $1 | Gray | Infrastructure |
| Park | $50 | $2 | Light Green | +20% happiness |

## Key Features

- **Grid-based building**: 40x30 tile grid, 32x32 pixel tiles
- **Simple economy**: Income vs expenses, building costs
- **Population mechanics**: Growth based on housing and happiness
- **Visual feedback**: Preview tiles, colored buildings, grid overlay
- **Intuitive controls**: Mouse for building, WASD for camera, scroll for zoom

## Technical Details

- **Framework**: Godot 4.3+ with .NET 8.0
- **Language**: C# only (no GDScript)
- **Graphics**: ColorRect nodes for simple colored rectangles
- **Resolution**: 1280x720 default window
- **No external assets**: All graphics generated at runtime
