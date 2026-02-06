# Testing Guide: 3D Orthographic View Feature

## Prerequisites
- Godot 4.6 or later
- .NET 8.0 SDK
- The project should build successfully (`dotnet build`)

## Quick Start Testing

### 1. Open the Project
```bash
# Navigate to project directory
cd /path/to/suri

# Build the project
dotnet build

# Open in Godot Editor
godot4 project.godot
# Or use Godot Editor's "Import" → "Open" menu
```

### 2. Run the Main Scene
1. In Godot Editor, open `scenes/Main.tscn`
2. Press F5 or click the "Play" button
3. The game should start in 2D view by default

## Test Cases

### Test 1: Initial State (2D View)
**Expected Behavior:**
- [x] Game starts in 2D view
- [x] Top bar shows: Money, Population, Happiness, Income
- [x] Right sidebar shows build menu with building buttons
- [x] View button shows "View: 2D"
- [x] Grid lines are visible
- [x] Camera can pan with WASD
- [x] Camera can zoom with mouse wheel

**How to Test:**
1. Launch the game
2. Verify all UI elements are present
3. Press W/A/S/D to pan camera
4. Scroll mouse wheel to zoom

### Test 2: Building Placement in 2D
**Expected Behavior:**
- [x] Clicking a building button selects it
- [x] Green preview tile follows mouse cursor
- [x] Preview is green when affordable and space is empty
- [x] Preview is red when not affordable or space is occupied
- [x] Left-clicking places the building
- [x] Money is deducted
- [x] Colored square appears on grid

**How to Test:**
1. Click "Residential" button ($100)
2. Move mouse over empty grid space
3. Verify green preview appears
4. Left-click to place
5. Verify building appears and money decreases

### Test 3: Building Demolition in 2D
**Expected Behavior:**
- [x] Right-clicking on a building removes it
- [x] 50% refund is given
- [x] Building disappears from grid

**How to Test:**
1. Place a Residential building ($100)
2. Right-click on it
3. Verify building disappears
4. Verify money increases by $50

### Test 4: Switch to 3D View
**Expected Behavior:**
- [x] Clicking "View: 2D" button switches to 3D
- [x] Button text changes to "View: 3D"
- [x] 2D grid lines disappear
- [x] 2D colored squares disappear
- [x] 3D scene appears with:
  - Ground plane (dark green/brown)
  - 3D meshes for all placed buildings
  - Directional lighting with shadows
- [x] Camera is at isometric angle
- [x] HUD remains visible

**How to Test:**
1. Place several buildings in 2D (mix of types)
2. Click the "View: 2D" button in build menu
3. Observe transition to 3D view
4. Verify all buildings have 3D representations
5. Verify ground plane is visible

### Test 5: 3D Camera Controls
**Expected Behavior:**
- [x] WASD moves camera in view direction
- [x] Mouse wheel zooms in/out (changes orthographic size)
- [x] Camera movement is smooth (interpolated)

**How to Test:**
1. In 3D view, press W to move forward
2. Press A/D to strafe left/right
3. Press S to move backward
4. Scroll mouse wheel up to zoom in
5. Scroll mouse wheel down to zoom out
6. Verify all movements are smooth

### Test 6: Building Placement in 3D
**Expected Behavior:**
- [x] Selecting a building type works
- [x] No preview tile visible (since it's 2D)
- [x] Can still place buildings by left-clicking
- [x] Raycasting determines grid position
- [x] Building appears immediately in 3D
- [x] Money is deducted

**How to Test:**
1. In 3D view, click "Commercial" button
2. Left-click on empty ground space
3. Verify blue tall building appears
4. Verify money decreases by $150
5. Try placing multiple buildings

### Test 7: Building Demolition in 3D
**Expected Behavior:**
- [x] Right-clicking on a building removes it
- [x] 3D mesh disappears immediately
- [x] 50% refund is given

**How to Test:**
1. In 3D view, right-click on any building
2. Verify 3D mesh disappears
3. Verify money increases

### Test 8: Switch Back to 2D View
**Expected Behavior:**
- [x] Clicking "View: 3D" button switches to 2D
- [x] Button text changes to "View: 2D"
- [x] 3D scene disappears
- [x] 2D grid and colored squares reappear
- [x] All buildings placed in 3D are visible in 2D
- [x] Camera returns to 2D controls

**How to Test:**
1. In 3D view, click "View: 3D" button
2. Observe transition back to 2D
3. Verify all buildings are still there
4. Verify 2D camera controls work

### Test 9: Synchronization Test
**Expected Behavior:**
- [x] Buildings placed in 2D appear in 3D
- [x] Buildings placed in 3D appear in 2D
- [x] Buildings removed in 2D disappear from 3D
- [x] Buildings removed in 3D disappear from 2D
- [x] Both views always show identical grid state

**How to Test:**
1. Start in 2D, place 3 buildings
2. Switch to 3D, verify all 3 buildings are visible
3. Place 2 more buildings in 3D
4. Switch to 2D, verify all 5 buildings are visible
5. Remove 1 building in 2D
6. Switch to 3D, verify only 4 buildings remain
7. Remove 1 building in 3D
8. Switch to 2D, verify only 3 buildings remain

### Test 10: All Building Types in 3D
**Expected Behavior:**
- [x] Residential: Green house (box 0.8x1.5x0.8)
- [x] Commercial: Blue tall building (box 0.8x2.0x0.8)
- [x] Industrial: Yellow factory with gray chimney
- [x] Road: Dark gray flat surface
- [x] Park: Light green ground with dark green tree sphere

**How to Test:**
1. Switch to 3D view
2. Place one of each building type:
   - Residential ($100)
   - Commercial ($150)
   - Industrial ($200)
   - Road ($10)
   - Park ($50)
3. Verify each has correct appearance:
   - Residential: Shortest, green
   - Commercial: Tallest, blue
   - Industrial: Yellow with chimney on top
   - Road: Nearly flat, gray
   - Park: Flat with sphere on top

### Test 11: Edge Cases

#### 11a: Building on Grid Boundaries
**How to Test:**
1. Try placing buildings at grid edges (0,0) and (39,29)
2. Verify they work correctly in both views

#### 11b: Rapid View Switching
**How to Test:**
1. Click view toggle button rapidly 10 times
2. Verify no crashes or glitches
3. Verify final view state is consistent

#### 11c: Building While Switching Views
**How to Test:**
1. Select a building type
2. Click to place
3. Immediately switch views
4. Verify building appears correctly

#### 11d: Mouse Over HUD
**How to Test:**
1. Hover mouse over build menu buttons
2. Click on buttons
3. Verify buildings aren't placed behind UI

### Test 12: Performance Check
**Expected Behavior:**
- [x] Smooth 60 FPS in both views
- [x] No lag when placing/removing buildings
- [x] No lag when switching views
- [x] Memory usage stable

**How to Test:**
1. Enable FPS counter in Godot (Debug menu)
2. Place 100+ buildings
3. Switch between views multiple times
4. Verify frame rate stays smooth
5. Check memory usage doesn't grow

## Visual Verification Checklist

### 2D View Appearance
```
┌────────────────────────────────────────────────┐
│ Money: $10000  Population: 0  Happiness: 50%  │ ← Top bar
├────────────────────────────────────────────────┤
│                                           ┌────┤
│  ░░░░░░░░░░░░░░░░░░░░░                   │Bld │
│  ░░░░░░░░░░░░░░░░░░░░░  ← Grid           │Menu│
│  ░░░■■░░░░░░░░░░░░░░░░     with          │    │
│  ░░░■■░░□□░░░░░░░░░░░░     buildings     │Res │
│  ░░░░░░░□□░░░░░░░░░░░░                   │$100│
│  ░░░░░░░░░░░░░░░░░░░░░                   │    │
│  ░░░░░░░░░░░░░░░░░░░░░                   │Com │
│                                           │$150│
│                                           │    │
│                                           │... │
│                                           │    │
│                                           │View│ ← Toggle
│                                           │2D  │   button
└───────────────────────────────────────────────┘
```

### 3D View Appearance
```
┌────────────────────────────────────────────────┐
│ Money: $10000  Population: 0  Happiness: 50%  │ ← Top bar
├────────────────────────────────────────────────┤
│                                           ┌────┤
│         ╱▔╲                              │Bld │
│        │ ■ │← Commercial                 │Menu│
│        ╲__╱                               │    │
│    ╱▔╲                                   │Res │
│   │ ▪ │← Residential                    │$100│
│   ╲__╱                                   │    │
│  ▔▔▔▔▔▔▔▔▔▔▔ ← Ground plane            │Com │
│  Isometric view                          │$150│
│                                           │    │
│                                           │... │
│                                           │    │
│                                           │View│ ← Toggle
│                                           │3D  │   button
└───────────────────────────────────────────────┘
```

## Debugging Tips

### Issue: 3D View is Black/Empty
**Check:**
1. Is DirectionalLight3D present in scene?
2. Is Camera3D Current property set to true?
3. Is SubViewportContainer visible?
4. Are any buildings placed?

**Fix:**
- Verify scene hierarchy in Main.tscn
- Check ViewManager.SetView3D() is being called
- Check console for error messages

### Issue: Mouse Clicks Not Working in 3D
**Check:**
1. Is Camera3D reference cached in BuildingPlacer?
2. Is raycast calculation returning valid positions?
3. Add debug prints to GetGridPositionFrom3D()

**Fix:**
```csharp
GD.Print($"Ray origin: {from}, direction: {dir}");
GD.Print($"Intersection: {intersection}");
GD.Print($"Grid pos: ({gridX}, {gridZ})");
```

### Issue: Buildings Not Syncing Between Views
**Check:**
1. Is GridManager.GridChanged signal being emitted?
2. Is GridManager3D connected to signal?
3. Check OnGridChanged() is being called

**Fix:**
- Add debug prints in signal emission and handler
- Verify signal connection in _Ready()

### Issue: View Toggle Button Not Working
**Check:**
1. Is ViewManager node present in scene?
2. Is button's Pressed signal connected?
3. Check console for null reference errors

**Fix:**
- Verify ViewManager path in HUD.cs
- Check lazy loading logic in OnViewTogglePressed()

## Performance Benchmarks

### Expected Performance
- **2D View**: 60 FPS with 1200 buildings (full grid)
- **3D View**: 60 FPS with 1200 buildings (full grid)
- **View Switch**: < 1 frame delay
- **Building Place**: Immediate (0 frame delay)
- **Memory**: ~100-200 MB for full grid

### Performance Issues?
If performance is poor:
1. Reduce shadow quality on DirectionalLight3D
2. Disable shadows: `shadow_enabled = false`
3. Reduce grid size for testing
4. Check for memory leaks (use Godot profiler)

## Reporting Issues

If you find bugs, please report with:
1. **Steps to reproduce**
2. **Expected behavior**
3. **Actual behavior**
4. **Screenshots** (especially for visual issues)
5. **Console output** (any error messages)
6. **System info** (OS, Godot version, .NET version)

## Success Criteria

All tests pass if:
- ✅ Can switch between 2D and 3D views seamlessly
- ✅ Can place and remove buildings in both views
- ✅ Both views always show the same grid state
- ✅ Camera controls work smoothly in both views
- ✅ All 5 building types render correctly in 3D
- ✅ HUD remains functional in both views
- ✅ No crashes, errors, or performance issues
- ✅ 60 FPS in both views with reasonable number of buildings

## Next Steps After Testing

Once all tests pass:
1. Take screenshots of both views for documentation
2. Record a short video demonstrating the feature
3. Consider additional enhancements:
   - Grid lines in 3D view
   - Building placement animation
   - Rotate camera in 3D mode
   - More detailed building meshes
   - Ambient occlusion
