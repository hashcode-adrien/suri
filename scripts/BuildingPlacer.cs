using Godot;

namespace Suri
{
    /// <summary>
    /// Handles player input for placing and removing buildings.
    /// </summary>
    public partial class BuildingPlacer : Node2D
    {
        private GridManager _gridManager;
        private GameManager _gameManager;
        private EconomyManager _economyManager;
        private Camera2D _camera;
        private ColorRect _previewTile;

        public override void _Ready()
        {
            _gridManager = GetNode<GridManager>("/root/Main/GridManager");
            _gameManager = GetNode<GameManager>("/root/Main/GameManager");
            _economyManager = GetNode<EconomyManager>("/root/Main/EconomyManager");
            _camera = GetNode<Camera2D>("/root/Main/Camera2D");

            // Create preview tile
            _previewTile = new ColorRect
            {
                Size = new Vector2(_gridManager.TileSize - 2, _gridManager.TileSize - 2),
                Color = new Color(1, 1, 1, 0.5f),
                Visible = false
            };
            AddChild(_previewTile);
        }

        public override void _Process(double delta)
        {
            UpdatePreview();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
            {
                var mousePos = GetGlobalMousePosition();
                var gridPos = _gridManager.WorldToGrid(mousePos);

                if (!_gridManager.IsValidPosition(gridPos)) return;

                if (mouseButton.ButtonIndex == MouseButton.Left)
                {
                    PlaceBuilding(gridPos);
                }
                else if (mouseButton.ButtonIndex == MouseButton.Right)
                {
                    DemolishBuilding(gridPos);
                }
            }
        }

        private void UpdatePreview()
        {
            if (_gameManager.SelectedBuildingType == BuildingType.None)
            {
                _previewTile.Visible = false;
                return;
            }

            var mousePos = GetGlobalMousePosition();
            var gridPos = _gridManager.WorldToGrid(mousePos);

            if (!_gridManager.IsValidPosition(gridPos))
            {
                _previewTile.Visible = false;
                return;
            }

            var worldPos = _gridManager.GridToWorld(gridPos);
            _previewTile.Position = worldPos;
            
            var data = BuildingRegistry.GetBuildingData(_gameManager.SelectedBuildingType);
            var canAfford = _economyManager.CanAfford(data.Cost);
            var isOccupied = _gridManager.GetBuildingAt(gridPos) != BuildingType.None;

            if (canAfford && !isOccupied)
            {
                _previewTile.Color = new Color(data.Color, 0.5f);
            }
            else
            {
                _previewTile.Color = new Color(Colors.Red, 0.5f);
            }

            _previewTile.Visible = true;
        }

        private void PlaceBuilding(Vector2I gridPos)
        {
            if (_gameManager.SelectedBuildingType == BuildingType.None) return;

            var data = BuildingRegistry.GetBuildingData(_gameManager.SelectedBuildingType);
            
            if (!_economyManager.CanAfford(data.Cost))
            {
                GD.Print("Not enough money!");
                return;
            }

            if (_gridManager.PlaceBuilding(gridPos, _gameManager.SelectedBuildingType))
            {
                _economyManager.SpendMoney(data.Cost);
                GD.Print($"Placed {data.Name} at {gridPos}");
            }
        }

        private void DemolishBuilding(Vector2I gridPos)
        {
            var buildingType = _gridManager.GetBuildingAt(gridPos);
            if (buildingType == BuildingType.None) return;

            if (_gridManager.RemoveBuilding(gridPos))
            {
                // Get small refund (50% of original cost)
                var data = BuildingRegistry.GetBuildingData(buildingType);
                int refund = data.Cost / 2;
                _economyManager.AddMoney(refund);
                GD.Print($"Demolished {data.Name} at {gridPos}, refund: ${refund}");
            }
        }
    }
}
