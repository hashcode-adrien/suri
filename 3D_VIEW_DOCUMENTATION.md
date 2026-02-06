# 3D Orthographic View Feature

## Overview
This feature adds a 3D orthographic rendering view that shows the same city map as the existing 2D view, but rendered in 3D with procedurally-generated meshes for each building type. Players can toggle between "2D View" and "3D View" using a button in the HUD.

## Architecture

### New Files Created

#### 1. `scripts/ViewManager.cs`
Manages the toggle between 2D and 3D views.
- **Signal**: `ViewChangedEventHandler(bool is3D)` - Emitted when view changes
- **Property**: `Is3DView` - Returns true if currently in 3D mode
- **Methods**:
  - `ToggleView()` - Switches between 2D and 3D
  - `SetView2D()` - Explicitly sets 2D view
  - `SetView3D()` - Explicitly sets 3D view

#### 2. `scripts/GridManager3D.cs`
Renders the grid in 3D using procedurally-generated meshes.
- Subscribes to `GridManager.GridChanged` signal for real-time updates
- Creates procedural 3D meshes for each building type:
  - **Residential**: Green box (0.8 x 1.5 x 0.8 units) - house shape
  - **Commercial**: Blue box (0.8 x 2.0 x 0.8 units) - taller building
  - **Industrial**: Yellow box (0.9 x 1.0 x 0.9 units) with cylinder chimney
  - **Road**: Dark gray flat box (0.9 x 0.1 x 0.9 units)
  - **Park**: Light green box (0.9 x 0.2 x 0.9 units) with dark green sphere (tree)
- Creates a ground plane covering the full 40x30 grid area

#### 3. `scripts/CameraController3D.cs`
Orthographic 3D camera with WASD panning and scroll zoom.
- **Orthographic projection** positioned at an isometric-like angle (45°)
- **WASD** keys for panning (uses same input actions as 2D camera)
- **Mouse scroll** for zooming (adjusts orthographic size)
- **Smooth interpolation** for camera movement
- **Helper method**: `GetRayFromScreen()` for raycasting

### Modified Files

#### 1. `scripts/GridManager.cs`
Added:
- `[Signal] GridChangedEventHandler(int x, int y)` - Emitted when buildings are placed/removed
- `GetGrid()` - Returns the entire grid array for initial 3D sync
- `SetTilesVisible(bool)` - Controls visibility of 2D tile rendering
- Signal emission in `PlaceBuilding()` and `RemoveBuilding()`

#### 2. `scripts/BuildingPlacer.cs`
Added:
- Support for 3D mouse-to-grid conversion via raycasting
- `GetGridPositionFromMouse()` - Determines grid position based on current view mode
- `GetGridPositionFrom3D()` - Raycasts from 3D camera to ground plane (Y=0)
- `SetPreviewVisible(bool)` - Called by ViewManager to hide/show preview tile
- Lazy loading of ViewManager reference

#### 3. `scripts/HUD.cs`
Added:
- "View: 2D" / "View: 3D" toggle button in the build menu
- `OnViewTogglePressed()` - Calls ViewManager.ToggleView()
- `OnViewChanged(bool)` - Updates button text when view changes
- Lazy loading of ViewManager reference

#### 4. `scenes/Main.tscn`
Added scene structure:
```
Main (Node2D)
├── GameManager
├── EconomyManager
├── PopulationManager
├── GridManager
├── BuildingPlacer
├── Camera2D
├── ViewManager (NEW)
├── SubViewportContainer (NEW)
│   └── SubViewport (NEW)
│       ├── GridManager3D (NEW)
│       ├── Camera3D (NEW)
│       └── DirectionalLight3D (NEW)
└── HUD
```

## How It Works

### View Synchronization
Both 2D and 3D views read from the same `GridManager._grid` data:
1. When a building is placed/removed, `GridManager` emits `GridChanged(x, y)`
2. `GridManager3D` subscribes to this signal and updates its 3D meshes
3. Both views always display the exact same grid state

### View Switching
When toggling views:
- **2D → 3D**: 
  - Disable Camera2D
  - Hide 2D tiles (`GridManager.SetTilesVisible(false)`)
  - Hide grid lines
  - Show SubViewportContainer with 3D scene
  - Enable Camera3D
  - Hide 2D preview tile
  
- **3D → 2D**:
  - Enable Camera2D
  - Show 2D tiles
  - Show grid lines
  - Hide SubViewportContainer
  - Disable Camera3D
  - Show 2D preview tile

### Mouse Input in 3D Mode
Building placement works in both views:
- **2D mode**: Uses `GetGlobalMousePosition()` → `WorldToGrid()`
- **3D mode**: 
  1. Gets mouse position from main viewport
  2. Projects ray from Camera3D through mouse position
  3. Calculates intersection with ground plane (Y=0)
  4. Converts 3D world position to grid coordinates
  5. Uses formula: `gridPos = floor(worldPos / cellSize)`

### 3D Grid Mapping
- Grid cell (x, y) → 3D position ((x + 0.5) * cellSize, 0, (y + 0.5) * cellSize)
- CellSize = 1.0 unit
- Buildings are centered on their grid cell
- Ground plane is centered at the middle of the grid area

## Controls

### 2D View
- **WASD / Arrow Keys**: Pan camera
- **Mouse Wheel**: Zoom in/out
- **Left Click**: Place selected building
- **Right Click**: Demolish building
- **View Button**: Switch to 3D view

### 3D View
- **WASD / Arrow Keys**: Pan camera (moves along camera's view direction)
- **Mouse Wheel**: Zoom in/out (changes orthographic size)
- **Left Click**: Place selected building (uses raycasting)
- **Right Click**: Demolish building (uses raycasting)
- **View Button**: Switch back to 2D view

## Technical Details

### Procedural Mesh Generation
All 3D models are generated procedurally using Godot's built-in mesh types:
- `BoxMesh` - for buildings and ground patches
- `CylinderMesh` - for industrial chimneys
- `SphereMesh` - for park trees
- `PlaneMesh` - for ground plane
- `StandardMaterial3D` - for coloring (matches 2D colors from BuildingRegistry)

### Performance Considerations
- Meshes are only created/destroyed when buildings change
- Dictionary lookup for quick mesh access by grid position
- No per-frame updates unless buildings are placed/removed
- SubViewport only renders when visible (3D mode)

### Rendering Pipeline
- Main scene renders 2D content directly
- 3D content renders in a SubViewport (1280x720)
- SubViewportContainer displays the viewport as a texture
- HUD (CanvasLayer) renders on top of both views

## Future Enhancements
Possible improvements:
- Add grid lines to 3D view
- Animate building placement/removal
- Add ambient occlusion or better lighting
- Free camera rotation mode
- Building selection highlights in 3D
- More detailed building meshes
- Particle effects for industrial buildings
