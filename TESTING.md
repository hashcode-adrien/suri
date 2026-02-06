# Testing Guide for Suri

This guide provides instructions for testing the Suri city builder game.

## Prerequisites

- Godot 4.0 or later installed
- Project opened in Godot Editor

## Testing Steps

### 1. Project Launch Test
- [ ] Open the project in Godot 4
- [ ] Verify no errors in the Output panel
- [ ] Press F5 to run the game
- [ ] Verify the game window opens at 1280x720 resolution

### 2. Initial State Test
- [ ] Verify HUD displays at the top:
  - Money: $10,000
  - Population: 0
  - Happiness: 50%
- [ ] Verify build menu appears on the right side with 5 buttons:
  - Residential ($100)
  - Commercial ($150)
  - Industrial ($200)
  - Road ($50)
  - Park ($75)
- [ ] Verify grid overlay is visible on the game world

### 3. Camera Controls Test
- [ ] Press W/Up Arrow - camera should pan up
- [ ] Press S/Down Arrow - camera should pan down
- [ ] Press A/Left Arrow - camera should pan left
- [ ] Press D/Right Arrow - camera should pan right
- [ ] Scroll mouse wheel up - camera should zoom in
- [ ] Scroll mouse wheel down - camera should zoom out

### 4. Building Placement Test
- [ ] Click "Residential" button in build menu
- [ ] Click on the grid - a green square should appear
- [ ] Verify money decreased by $100
- [ ] Click "Commercial" button
- [ ] Click on a different grid cell - a blue square should appear
- [ ] Verify money decreased by $150
- [ ] Click "Industrial" button
- [ ] Click on a different grid cell - a yellow square should appear
- [ ] Click "Road" button
- [ ] Click on a different grid cell - a gray square should appear
- [ ] Click "Park" button
- [ ] Click on a different grid cell - a light green square should appear

### 5. Building Demolition Test
- [ ] Right-click on any placed building
- [ ] Verify the building is removed
- [ ] Verify no money is refunded (as per game design)

### 6. Economy System Test
- [ ] Place several buildings
- [ ] Wait for 5 seconds (one game tick)
- [ ] Verify money updates based on income and expenses
- [ ] Expected behavior:
  - Residential: +$10 income, -$2 expenses = +$8 net
  - Commercial: +$20 income, -$3 expenses = +$17 net
  - Industrial: +$15 income, -$4 expenses = +$11 net
  - Road: -$1 expenses
  - Park: -$2 expenses

### 7. Population System Test
- [ ] Place 1 residential building
- [ ] Wait for 5 seconds (one game tick)
- [ ] Verify population starts increasing (max 10 per residential)
- [ ] Place additional residential buildings
- [ ] Verify population cap increases

### 8. Happiness System Test
- [ ] Place 2-3 industrial buildings
- [ ] Wait 5 seconds
- [ ] Verify happiness decreases (shown in HUD)
- [ ] Place 2-3 parks
- [ ] Wait 5 seconds
- [ ] Verify happiness increases

### 9. Pause Functionality Test
- [ ] Press P key
- [ ] Verify "PAUSED" text appears in center of screen
- [ ] Verify game timer stops (population/money no longer update)
- [ ] Press P key again
- [ ] Verify "PAUSED" text disappears
- [ ] Verify game resumes

### 10. Budget Management Test
- [ ] Spend money until balance is low
- [ ] Try to place a building you can't afford
- [ ] Verify building is not placed
- [ ] Check console for "Not enough money!" message

### 11. Collision Test
- [ ] Place a building on a grid cell
- [ ] Try to place another building on the same cell
- [ ] Verify second building is not placed

## Expected Results

All tests should pass without errors. The game should be fully playable with:
- Smooth camera movement
- Accurate building placement and removal
- Correct money calculations
- Growing population with residential zones
- Dynamic happiness based on parks and industrial zones
- Working pause functionality

## Known Limitations

- Buildings use simple colored rectangles (placeholder graphics)
- No sound effects or music
- No save/load functionality
- Basic economy model (can be expanded)

## Reporting Issues

If any tests fail or unexpected behavior occurs, please document:
1. Steps to reproduce
2. Expected behavior
3. Actual behavior
4. Any error messages in Godot's Output panel
