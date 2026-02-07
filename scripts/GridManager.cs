using Godot;
using System.Collections.Generic;

namespace Suri
{
    /// <summary>
    /// Manages the game grid, building placement, and tile rendering.
    /// </summary>
    public partial class GridManager : Node2D
    {
        [Signal]
        public delegate void GridChangedEventHandler(int x, int y);

        [Export] public int GridWidth = 40;
        [Export] public int GridHeight = 30;
        [Export] public int TileSize = 32;
        [Export] public bool ShowGrid = true;

        private BuildingType[,] _grid;
        private Dictionary<Vector2I, ColorRect> _tiles = new Dictionary<Vector2I, ColorRect>();
        private Node2D _tileContainer;
        private Node2D _gridLines;

        public override void _Ready()
        {
            _grid = new BuildingType[GridWidth, GridHeight];
            
            // Create containers
            _tileContainer = new Node2D { Name = "TileContainer" };
            AddChild(_tileContainer);
            
            _gridLines = new Node2D { Name = "GridLines" };
            AddChild(_gridLines);
            
            InitializeGrid();
            DrawGridLines();
        }

        private void InitializeGrid()
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    _grid[x, y] = BuildingType.None;
                }
            }
        }

        private void DrawGridLines()
        {
            if (!ShowGrid) return;

            // Draw vertical lines
            for (int x = 0; x <= GridWidth; x++)
            {
                var line = new Line2D();
                line.AddPoint(new Vector2(x * TileSize, 0));
                line.AddPoint(new Vector2(x * TileSize, GridHeight * TileSize));
                line.DefaultColor = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Gray with 0.3 alpha
                line.Width = 1;
                _gridLines.AddChild(line);
            }

            // Draw horizontal lines
            for (int y = 0; y <= GridHeight; y++)
            {
                var line = new Line2D();
                line.AddPoint(new Vector2(0, y * TileSize));
                line.AddPoint(new Vector2(GridWidth * TileSize, y * TileSize));
                line.DefaultColor = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Gray with 0.3 alpha
                line.Width = 1;
                _gridLines.AddChild(line);
            }
        }

        public bool PlaceBuilding(Vector2I gridPos, BuildingType buildingType)
        {
            if (!IsValidPosition(gridPos)) return false;
            if (_grid[gridPos.X, gridPos.Y] != BuildingType.None) return false;

            _grid[gridPos.X, gridPos.Y] = buildingType;
            UpdateTile(gridPos);
            EmitSignal(SignalName.GridChanged, gridPos.X, gridPos.Y);
            return true;
        }

        public bool RemoveBuilding(Vector2I gridPos)
        {
            if (!IsValidPosition(gridPos)) return false;
            if (_grid[gridPos.X, gridPos.Y] == BuildingType.None) return false;

            _grid[gridPos.X, gridPos.Y] = BuildingType.None;
            UpdateTile(gridPos);
            EmitSignal(SignalName.GridChanged, gridPos.X, gridPos.Y);
            return true;
        }

        private void UpdateTile(Vector2I gridPos)
        {
            // Remove existing tile if present
            if (_tiles.ContainsKey(gridPos))
            {
                _tiles[gridPos].QueueFree();
                _tiles.Remove(gridPos);
            }

            // Create new tile if not empty
            var buildingType = _grid[gridPos.X, gridPos.Y];
            if (buildingType != BuildingType.None)
            {
                var data = BuildingRegistry.GetBuildingData(buildingType);
                var tile = new ColorRect
                {
                    Position = new Vector2(gridPos.X * TileSize, gridPos.Y * TileSize),
                    Size = new Vector2(TileSize - 2, TileSize - 2),
                    Color = data.Color
                };
                _tileContainer.AddChild(tile);
                _tiles[gridPos] = tile;
            }
        }

        public Vector2I WorldToGrid(Vector2 worldPos)
        {
            var localPos = worldPos - GlobalPosition;
            return new Vector2I(
                Mathf.FloorToInt(localPos.X / TileSize),
                Mathf.FloorToInt(localPos.Y / TileSize)
            );
        }

        public Vector2 GridToWorld(Vector2I gridPos)
        {
            return GlobalPosition + new Vector2(gridPos.X * TileSize, gridPos.Y * TileSize);
        }

        public bool IsValidPosition(Vector2I gridPos)
        {
            return gridPos.X >= 0 && gridPos.X < GridWidth && 
                   gridPos.Y >= 0 && gridPos.Y < GridHeight;
        }

        public BuildingType GetBuildingAt(Vector2I gridPos)
        {
            if (!IsValidPosition(gridPos)) return BuildingType.None;
            return _grid[gridPos.X, gridPos.Y];
        }

        public int CountBuildings(BuildingType type)
        {
            int count = 0;
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    if (_grid[x, y] == type) count++;
                }
            }
            return count;
        }

        public void ToggleGrid(bool visible)
        {
            ShowGrid = visible;
            _gridLines.Visible = visible;
        }

        /// <summary>
        /// Gets the entire grid data for reading. Used by 3D view for initial sync.
        /// </summary>
        public BuildingType[,] GetGrid()
        {
            return _grid;
        }

        /// <summary>
        /// Sets the visibility of the 2D tile rendering.
        /// Used when switching between 2D and 3D views.
        /// </summary>
        public void SetTilesVisible(bool visible)
        {
            _tileContainer.Visible = visible;
        }
    }
}
