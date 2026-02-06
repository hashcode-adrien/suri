using Godot;

namespace Suri
{
    /// <summary>
    /// Handles player input for placing and removing buildings.
    /// Uses polling-based input in _Process to avoid event ordering issues
    /// with other nodes using _UnhandledInput.
    /// </summary>
    public partial class BuildingPlacer : Node2D
    {
        private GridManager _gridManager;
        private GameManager _gameManager;
        private EconomyManager _economyManager;
        private Camera2D _camera;
        private ColorRect _previewTile;

        private static readonly Vector2I InvalidGridPosition = new Vector2I(-1, -1);

        // State tracking for click-and-drag functionality
        private bool _wasLeftPressed = false;
        private bool _wasRightPressed = false;
        private bool _isPlacing = false;
        private bool _isDemolishing = false;
        private Vector2I _lastPlacedGridPos = InvalidGridPosition;

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
                Visible = false,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            AddChild(_previewTile);
        }

        public override void _Process(double delta)
        {
            UpdatePreview();
            HandleMouseInput();
        }

        private void HandleMouseInput()
        {
            bool leftPressed = Input.IsMouseButtonPressed(MouseButton.Left);
            bool rightPressed = Input.IsMouseButtonPressed(MouseButton.Right);

            // Check if mouse is over a GUI element (HUD buttons etc.)
            // If so, don't process placement/demolition
            bool mouseOverGui = IsMouseOverInteractiveGui();

            var mousePos = GetGlobalMousePosition();
            var gridPos = _gridManager.WorldToGrid(mousePos);
            bool validPos = _gridManager.IsValidPosition(gridPos);

            // --- LEFT CLICK: Place buildings ---
            // Detect press (transition from not pressed to pressed)
            if (leftPressed && !_wasLeftPressed && !mouseOverGui)
            {
                if (validPos)
                {
                    _isPlacing = true;
                    PlaceBuilding(gridPos);
                    _lastPlacedGridPos = gridPos;
                }
            }
            // While held down, keep placing on new tiles
            else if (leftPressed && _isPlacing)
            {
                if (validPos && gridPos != _lastPlacedGridPos)
                {
                    PlaceBuilding(gridPos);
                    _lastPlacedGridPos = gridPos;
                }
            }
            // Released
            if (!leftPressed && _wasLeftPressed)
            {
                _isPlacing = false;
                _lastPlacedGridPos = InvalidGridPosition;
            }

            // --- RIGHT CLICK: Demolish buildings ---
            if (rightPressed && !_wasRightPressed && !mouseOverGui)
            {
                if (validPos)
                {
                    _isDemolishing = true;
                    DemolishBuilding(gridPos);
                    _lastPlacedGridPos = gridPos;
                }
            }
            else if (rightPressed && _isDemolishing)
            {
                if (validPos && gridPos != _lastPlacedGridPos)
                {
                    DemolishBuilding(gridPos);
                    _lastPlacedGridPos = gridPos;
                }
            }
            if (!rightPressed && _wasRightPressed)
            {
                _isDemolishing = false;
                _lastPlacedGridPos = InvalidGridPosition;
            }

            // Update previous frame state
            _wasLeftPressed = leftPressed;
            _wasRightPressed = rightPressed;
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

        private bool IsMouseOverInteractiveGui()
        {
            var hoveredControl = GetViewport().GuiGetHoveredControl();
            if (hoveredControl == null) return false;
            if (hoveredControl == _previewTile) return false;
            // If hovering over any control that's part of the HUD CanvasLayer, it's GUI
            Node parent = hoveredControl;
            while (parent != null)
            {
                if (parent is CanvasLayer) return true;
                if (parent is Button) return true;
                parent = parent.GetParent();
            }
            return false;
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
