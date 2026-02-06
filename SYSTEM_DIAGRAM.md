# Suri - System Interaction Diagram

## Component Relationships

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Main Scene (Main.tscn)                      │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                    ┌───────────────┼───────────────┐
                    │               │               │
         ┌──────────▼─────────┐    │    ┌─────────▼──────────┐
         │   GameManager      │    │    │   GridManager      │
         │   (State Control)  │    │    │   (Grid + Tiles)   │
         └──────────┬─────────┘    │    └─────────┬──────────┘
                    │               │               │
                    │               │               │
         ┌──────────▼─────────┐    │    ┌─────────▼──────────┐
         │  EconomyManager    │◄───┼───►│  BuildingPlacer    │
         │  (Money, Income)   │    │    │  (Mouse Input)     │
         └──────────┬─────────┘    │    └────────────────────┘
                    │               │
                    │               │
         ┌──────────▼─────────┐    │    ┌────────────────────┐
         │ PopulationManager  │    │    │  CameraController  │
         │ (Growth, Happiness)│    │    │  (Pan, Zoom)       │
         └──────────┬─────────┘    │    └────────────────────┘
                    │               │
                    └───────────────┼────────────┐
                                    │            │
                         ┌──────────▼────────┐   │
                         │       HUD         │◄──┘
                         │  (User Interface) │
                         └───────────────────┘
```

## Data Flow

### 1. Building Placement Flow
```
User clicks → BuildingPlacer → Check EconomyManager (can afford?)
                              → Check GridManager (valid position?)
                              → Place building → Deduct money
```

### 2. Economy Update Flow (Every 5 seconds)
```
EconomyManager timer → Query GridManager (count buildings)
                    → Calculate income per building type
                    → Calculate expenses per building type
                    → Update money → Emit signal → HUD updates
```

### 3. Population Update Flow (Every 3 seconds)
```
PopulationManager timer → Query GridManager (count residential)
                       → Calculate max population
                       → Calculate happiness (from buildings)
                       → Grow/shrink population
                       → Emit signal → HUD updates
```

### 4. Camera Control Flow
```
User input (WASD/Scroll) → CameraController → Update camera position/zoom
```

### 5. Game State Flow
```
User presses P → GameManager → Toggle pause
                             → Emit signal → HUD shows "[PAUSED]"
                             → All managers check IsPaused before updates
```

## Signal Communication

```
EconomyManager signals:
  ├─ MoneyChanged(int) ──────────► HUD.OnMoneyChanged()

PopulationManager signals:
  ├─ PopulationChanged(int) ─────► HUD.OnPopulationChanged()
  └─ HappinessChanged(float) ────► HUD.OnHappinessChanged()

GameManager signals:
  ├─ GamePaused(bool) ───────────► HUD.OnGamePaused()
  └─ BuildingTypeSelected(int) ──► (Used by UI for highlighting)
```

## Building Data Registry

```
BuildingRegistry (Static)
    │
    ├─ Residential: Cost=$100, Income=$20, PopCap=10, Maintenance=$5
    ├─ Commercial:  Cost=$150, Income=$50, Maintenance=$8
    ├─ Industrial:  Cost=$200, Income=$80, Happiness=-10%, Maintenance=$10
    ├─ Road:        Cost=$10, Maintenance=$1
    └─ Park:        Cost=$50, Happiness=+20%, Maintenance=$2
```

## Scene Node Hierarchy

```
Main (Node2D)
├─ GameManager (Node) ─────────────── Game state controller
├─ EconomyManager (Node) ──────────── Money and income system
├─ PopulationManager (Node) ────────── Population and happiness
├─ GridManager (Node2D)
│  ├─ TileContainer (Node2D)
│  │  └─ [ColorRect tiles created dynamically]
│  └─ GridLines (Node2D)
│     └─ [Line2D grid created dynamically]
├─ BuildingPlacer (Node2D)
│  └─ Preview Tile (ColorRect) ────── Shows placement preview
├─ Camera2D ───────────────────────── Main camera with controls
└─ HUD (CanvasLayer)
   ├─ Top Bar (ColorRect) ─────────── Background for stats
   │  └─ Info Labels ───────────────── Money, Pop, Happiness, Income
   └─ Build Menu (VBoxContainer) ──── Building selection buttons
```

## Update Loop Timing

```
┌────────────────┬──────────────────┬─────────────────────────┐
│   Component    │    Frequency     │      What It Does       │
├────────────────┼──────────────────┼─────────────────────────┤
│ CameraControl  │ Every frame      │ Pan & zoom camera       │
│ BuildingPlacer │ Every frame      │ Update preview tile     │
│ EconomyManager │ Every 5 seconds  │ Calculate income/costs  │
│ PopulationMgr  │ Every 3 seconds  │ Update population       │
│ HUD            │ On signal only   │ Update UI labels        │
└────────────────┴──────────────────┴─────────────────────────┘
```

## File Dependencies

```
BuildingData.cs ◄─────────────┬──────────────┬──────────────┐
                              │              │              │
                      GridManager.cs  EconomyManager.cs  PopulationManager.cs
                              │              │              │
                              └──────────────┴──────────────┤
                                                           │
                              BuildingPlacer.cs ───────────┤
                                      │                    │
                                      └────────────────────┤
                                                           │
                              GameManager.cs ──────────────┤
                                      │                    │
                                      └────────────────────┤
                                                           │
                                  HUD.cs ◄─────────────────┘
                                  
CameraController.cs (Independent, no dependencies)
```

## Key Interactions

1. **Player places building**
   - BuildingPlacer → EconomyManager (check money)
   - BuildingPlacer → GridManager (place tile)
   - EconomyManager (deduct cost, emit signal)
   - HUD (update display)

2. **Economy tick**
   - Timer triggers in EconomyManager
   - Query GridManager for building counts
   - Calculate net income using BuildingRegistry data
   - Update money, emit signal
   - HUD receives signal and updates

3. **Population grows**
   - Timer triggers in PopulationManager
   - Query GridManager for residential count
   - Calculate happiness from all buildings
   - Grow population if happy and space available
   - Emit signal, HUD updates

4. **Game paused**
   - User presses P or clicks button
   - GameManager toggles pause state
   - Emit signal to HUD (show "[PAUSED]")
   - Economy and Population managers check IsPaused flag

## Performance Considerations

- **Building tiles**: ColorRect per building (max ~1,200 buildings on 40×30 grid)
- **Grid lines**: Pre-generated Line2D nodes (70 total lines)
- **Updates**: Timed updates (not every frame) for economy/population
- **Signals**: Efficient communication without tight coupling
- **Preview tile**: Single ColorRect reused, updated each frame
