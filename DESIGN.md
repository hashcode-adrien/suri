# Suri - Visual Design Reference

## Color Scheme

This document describes the visual design of Suri's buildings and UI elements.

### Building Types and Colors

All buildings are represented by colored rectangles (placeholder graphics):

1. **Residential Zones** (Green)
   - RGB: (51, 204, 51) or (0.2, 0.8, 0.2)
   - Purpose: Houses for citizens
   - Population: 10 per building
   - Income: $10/tick
   - Expenses: $2/tick
   - Cost: $100

2. **Commercial Zones** (Blue)
   - RGB: (51, 51, 204) or (0.2, 0.2, 0.8)
   - Purpose: Shops and businesses
   - Income: $20/tick
   - Expenses: $3/tick
   - Cost: $150

3. **Industrial Zones** (Yellow)
   - RGB: (230, 230, 51) or (0.9, 0.9, 0.2)
   - Purpose: Factories and production
   - Income: $15/tick
   - Expenses: $4/tick
   - Happiness impact: -5 per building
   - Cost: $200

4. **Roads** (Gray)
   - RGB: (128, 128, 128) or (0.5, 0.5, 0.5)
   - Purpose: Connects zones
   - Expenses: $1/tick
   - Cost: $50

5. **Parks** (Light Green)
   - RGB: (128, 255, 128) or (0.5, 1.0, 0.5)
   - Purpose: Boosts citizen happiness
   - Happiness impact: +10 per building
   - Expenses: $2/tick
   - Cost: $75

### UI Colors

- **Top Bar Background**: Dark gray (0.1, 0.1, 0.1, 0.9)
- **Build Menu Background**: Medium dark gray (0.15, 0.15, 0.15, 0.9)
- **Grid Overlay**: Light gray (0.3, 0.3, 0.3, 0.3)
- **Text Labels**: White (default)

### Layout

- **Window Size**: 1280x720
- **Tile Size**: 64x64 pixels
- **Grid Size**: 30x20 tiles
- **Top Bar Height**: 50 pixels
- **Build Menu**: 200x400 pixels, positioned at (1080, 60)

### Building Visuals

- Each building is rendered as a ColorRect
- Building size: 60x60 pixels (64 - 4 pixels border)
- Position offset: +2 pixels from tile origin (to create visual spacing)

## Future Enhancements

When adding proper graphics, consider:
- Sprite sheets for each building type
- Animated buildings (smoke from factories, people in residential)
- Road connection sprites (corners, intersections)
- UI theme with custom fonts and icons
- Particle effects (construction, demolition)
- Background terrain texture
