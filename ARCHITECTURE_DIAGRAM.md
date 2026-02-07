# Architecture Diagram: 3D View Feature

## Scene Hierarchy

```
Main (Node2D)
│
├── GameManager (Node)
│   └── Manages game state, selected building type, pause
│
├── EconomyManager (Node)
│   └── Manages money, income, expenses
│
├── PopulationManager (Node)
│   └── Manages population, happiness
│
├── GridManager (Node2D) ⚡ MODIFIED
│   ├── _grid: BuildingType[40,30] (SOURCE OF TRUTH)
│   ├── _tileContainer (2D tiles)
│   ├── _gridLines (2D grid)
│   └── Signals: GridChanged(x, y)
│
├── BuildingPlacer (Node2D) ⚡ MODIFIED
│   ├── Handles mouse input
│   ├── 2D mode: WorldToGrid()
│   └── 3D mode: Raycast to ground plane
│
├── Camera2D (CameraController)
│   └── 2D view camera (WASD + zoom)
│
├── ViewManager (Node) 🆕 NEW
│   ├── Controls view switching
│   ├── Toggles camera states
│   └── Signals: ViewChanged(is3D)
│
├── SubViewportContainer (NEW) 🆕
│   ├── offset_right: 1280
│   ├── offset_bottom: 720
│   ├── stretch: true
│   │
│   └── SubViewport (NEW) 🆕
│       ├── size: 1280x720
│       ├── render_target_update_mode: ALWAYS
│       │
│       ├── GridManager3D (Node3D) 🆕 NEW
│       │   ├── Subscribes to GridManager.GridChanged
│       │   ├── _buildingMeshes: Dict<Vector2I, Node3D>
│       │   ├── _groundPlane: MeshInstance3D
│       │   └── Procedurally generates building meshes
│       │
│       ├── Camera3D (CameraController3D) 🆕 NEW
│       │   ├── Orthographic projection
│       │   ├── Position: (20, 20, 20)
│       │   ├── LookAt: (20, 0, 15)
│       │   └── WASD + zoom controls
│       │
│       └── DirectionalLight3D (NEW) 🆕
│           ├── Transform: 45° angle
│           ├── light_energy: 0.8
│           └── shadow_enabled: true
│
└── HUD (CanvasLayer) ⚡ MODIFIED
	├── Top bar (money, population, etc.)
	├── Build menu (building buttons)
	└── View toggle button 🆕 NEW
```

## Data Flow Diagram

### Building Placement Flow

```
┌─────────────────────────────────────────────────┐
│ 1. USER CLICKS TO PLACE BUILDING               │
└─────────────────────────────────────────────────┘
					  ↓
		┌─────────────────────────┐
		│  Is 3D View Active?     │
		└─────────────────────────┘
				↙            ↘
		   YES                   NO
			↓                     ↓
┌─────────────────────────┐   ┌─────────────────────────┐
│ BuildingPlacer          │   │ BuildingPlacer          │
│ GetGridPositionFrom3D() │   │ Get mouse world pos     │
│  • ProjectRayOrigin()   │   │ GridManager.WorldToGrid │
│  • ProjectRayNormal()   │   └─────────────────────────┘
│  • Intersect Y=0 plane  │                ↓
│  • Convert to grid pos  │                │
└─────────────────────────┘                │
			↓                               │
			└───────────┬───────────────────┘
						↓
			┌─────────────────────────┐
			│ GridManager             │
			│ PlaceBuilding(x, y)     │
			│  • Update _grid[x,y]    │
			│  • Update 2D tile       │
			│  • Emit GridChanged(x,y)│
			└─────────────────────────┘
						↓
			┌─────────────────────────┐
			│ GridManager3D           │
			│ OnGridChanged(x, y)     │
			│  • Remove old mesh      │
			│  • Create new mesh      │
			│  • Add to scene         │
			└─────────────────────────┘
						↓
		┌─────────────────────────────┐
		│ BOTH VIEWS NOW IN SYNC      │
		└─────────────────────────────┘
```

### View Switching Flow

```
┌────────────────────────────────────────────────┐
│ USER CLICKS "VIEW: 2D" BUTTON IN HUD           │
└────────────────────────────────────────────────┘
					  ↓
			┌─────────────────────┐
			│ HUD                 │
			│ OnViewTogglePressed │
			└─────────────────────┘
					  ↓
			┌─────────────────────┐
			│ ViewManager         │
			│ ToggleView()        │
			└─────────────────────┘
					  ↓
		┌─────────────────────────┐
		│ Current view is 2D?     │
		└─────────────────────────┘
			NO ↙        ↘ YES
			  ↓           ↓
	┌────────────────┐  ┌────────────────┐
	│ SetView3D()    │  │ SetView2D()    │
	└────────────────┘  └────────────────┘
			  ↓                   ↓
	┌────────────────┐  ┌────────────────┐
	│ • Camera2D     │  │ • Camera2D     │
	│   OFF          │  │   ON           │
	│ • 2D Tiles     │  │ • 2D Tiles     │
	│   HIDDEN       │  │   VISIBLE      │
	│ • Grid Lines   │  │ • Grid Lines   │
	│   HIDDEN       │  │   VISIBLE      │
	│ • SubViewport  │  │ • SubViewport  │
	│   VISIBLE      │  │   HIDDEN       │
	│ • Camera3D     │  │ • Camera3D     │
	│   ON (Current) │  │   OFF          │
	│ • Preview      │  │ • Preview      │
	│   HIDDEN       │  │   VISIBLE      │
	└────────────────┘  └────────────────┘
			  ↓                   ↓
			  └─────────┬─────────┘
						↓
			┌─────────────────────┐
			│ Emit ViewChanged    │
			└─────────────────────┘
						↓
			┌─────────────────────┐
			│ HUD                 │
			│ OnViewChanged()     │
			│ Update button text  │
			└─────────────────────┘
```

## Component Communication

```
┌──────────────────────────────────────────────────────────────┐
│                         ViewManager                          │
│  • Coordinates all view-related state                        │
│  • No direct data access (just enables/disables nodes)       │
└──────────────────────────────────────────────────────────────┘
					↓ controls     ↓ controls
	  ┌─────────────────┐    ┌──────────────────┐
	  │ 2D Subsystem    │    │ 3D Subsystem     │
	  └─────────────────┘    └──────────────────┘
	  │                      │
	  │ • Camera2D          │ • Camera3D
	  │ • GridManager       │ • GridManager3D
	  │   (tiles visible)   │ • SubViewport
	  │ • Grid lines        │ • Light3D
	  │ • Preview tile      │
	  │                      │
	  └──────────┬───────────┘
				 │
				 │ Both read from
				 ↓
	  ┌─────────────────────┐
	  │   GridManager       │
	  │   _grid[40,30]      │
	  │  (Single Source of  │
	  │      Truth)         │
	  └─────────────────────┘
				 ↑
				 │ Signals changes
				 │
	  ┌──────────────────────┐
	  │   BuildingPlacer     │
	  │ (Works in both modes)│
	  └──────────────────────┘
```

## Building Type → 3D Mesh Mapping

```
┌─────────────┬──────────────────────────────────────────────┐
│ Building    │ 3D Representation                            │
├─────────────┼──────────────────────────────────────────────┤
│ Residential │ BoxMesh (0.8 x 1.5 x 0.8)                   │
│             │ Color: Green                                 │
│             │ Position: (x+0.5, 0.75, y+0.5)              │
│             │ Represents: Simple house                     │
├─────────────┼──────────────────────────────────────────────┤
│ Commercial  │ BoxMesh (0.8 x 2.0 x 0.8)                   │
│             │ Color: Blue                                  │
│             │ Position: (x+0.5, 1.0, y+0.5)               │
│             │ Represents: Tall building/store              │
├─────────────┼──────────────────────────────────────────────┤
│ Industrial  │ BoxMesh (0.9 x 1.0 x 0.9) - Base            │
│             │ + CylinderMesh (0.1r x 0.6h) - Chimney      │
│             │ Colors: Yellow base, Dark gray chimney       │
│             │ Position: Base (x+0.5, 0.5, y+0.5)          │
│             │           Chimney (x+0.75, 1.3, y+0.75)     │
│             │ Represents: Factory with smokestack          │
├─────────────┼──────────────────────────────────────────────┤
│ Road        │ BoxMesh (0.9 x 0.1 x 0.9)                   │
│             │ Color: Dark gray                             │
│             │ Position: (x+0.5, 0.05, y+0.5)              │
│             │ Represents: Flat road surface                │
├─────────────┼──────────────────────────────────────────────┤
│ Park        │ BoxMesh (0.9 x 0.2 x 0.9) - Ground          │
│             │ + SphereMesh (0.2r x 0.4h) - Tree           │
│             │ Colors: Light green ground, Dark green tree  │
│             │ Position: Ground (x+0.5, 0.1, y+0.5)        │
│             │           Tree (x+0.5, 0.4, y+0.5)          │
│             │ Represents: Park with tree                   │
└─────────────┴──────────────────────────────────────────────┘

Note: All positions use cellSize = 1.0
	  Grid coordinates (x, y) → 3D position (x*1.0, height, y*1.0)
```

## Camera Setup

### 2D Camera (Camera2D)
```
Type: Orthographic (2D)
Position: (640, 360) - center of viewport
Zoom: 1.0 (adjustable 0.5 - 2.0)
Controls:
  • WASD: Pan
  • Mouse Wheel: Zoom
```

### 3D Camera (Camera3D)
```
Type: Orthographic
Position: (20, 20, 20)
LookAt: (20, 0, 15)
Rotation: ~45° angle (isometric-like)
Size: 20 (adjustable 5 - 30)
Controls:
  • WASD: Pan along view direction
  • Mouse Wheel: Adjust orthographic size

Ray Projection:
  • ProjectRayOrigin(mousePos) → Vector3
  • ProjectRayNormal(mousePos) → Vector3
  • Used for ground plane intersection
```

## Signal Flow

```
GridManager.PlaceBuilding(x, y)
	↓
EmitSignal("GridChanged", x, y)
	↓
GridManager3D.OnGridChanged(x, y)
	↓
Update 3D mesh at position


ViewManager.ToggleView()
	↓
EmitSignal("ViewChanged", is3D)
	↓
HUD.OnViewChanged(is3D)
	↓
Update button text
```

## Performance Characteristics

### Memory
- **2D Mode**: ~40x30 ColorRect nodes + lines
- **3D Mode**: ~40x30 Node3D containers + meshes
- **Peak**: Both views in memory simultaneously
- **Optimization**: SubViewport only renders when visible

### CPU
- **Building Updates**: O(1) - only affected cell
- **View Switch**: O(1) - just toggle visibility
- **Initial 3D Sync**: O(n*m) - scans entire grid once
- **Per Frame**: Minimal - no continuous updates

### GPU
- **2D Mode**: Software rendering of colored rectangles
- **3D Mode**: Hardware rendering of ~N meshes + shadows
- **Lighting**: 1 directional light with shadows
- **Materials**: Simple StandardMaterial3D (no textures)
```
