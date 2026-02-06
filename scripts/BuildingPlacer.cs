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
        private ViewManager _viewManager;

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
            
            // ViewManager might not be ready immediately, so we'll get it on first use
            _viewManager = null;

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
            // Lazy load ViewManager
            if (_viewManager == null && HasNode("/root/Main/ViewManager"))
            {
                _viewManager = GetNode<ViewManager>("/root/Main/ViewManager");
            }

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

            var gridPos = GetGridPositionFromMouse();
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

            var gridPos = GetGridPositionFromMouse();

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

        /// <summary>
        /// Gets the grid position from the mouse cursor, handling both 2D and 3D views.
        /// </summary>
        private Vector2I GetGridPositionFromMouse()
        {
            if (_viewManager != null && _viewManager.Is3DView)
            {
                // 3D mode: raycast from camera to ground plane
                return GetGridPositionFrom3D();
            }
            else
            {
                // 2D mode: use normal 2D world position
                var mousePos = GetGlobalMousePosition();
                return _gridManager.WorldToGrid(mousePos);
            }
        }

        /// <summary>
        /// Raycast from 3D camera to ground plane (Y=0) to get grid position.
        /// </summary>
        private Vector2I GetGridPositionFrom3D()
        {
            var camera3D = GetNodeOrNull<Camera3D>("/root/Main/SubViewportContainer/SubViewport/Camera3D");
            if (camera3D == null) return InvalidGridPosition;

            var viewport = GetViewport();
            var mousePos = viewport.GetMousePosition();
            
            // Adjust mouse position for the SubViewport
            var subViewportContainer = GetNodeOrNull<SubViewportContainer>("/root/Main/SubViewportContainer");
            if (subViewportContainer == null) return InvalidGridPosition;
            
            var subViewport = GetNodeOrNull<SubViewport>("/root/Main/SubViewportContainer/SubViewport");
            if (subViewport == null) return InvalidGridPosition;

            // Get ray from camera
            var from = camera3D.ProjectRayOrigin(mousePos);
            var dir = camera3D.ProjectRayNormal(mousePos);

            // Intersect with ground plane (Y = 0)
            if (Mathf.Abs(dir.Y) < 0.0001f) return InvalidGridPosition; // Ray parallel to ground

            float t = -from.Y / dir.Y;
            if (t < 0) return InvalidGridPosition; // Ray pointing away from ground

            var intersection = from + dir * t;
            
            // Convert 3D world position to grid coordinates
            // 3D grid mapping: grid cell (x, y) → 3D position (x * cellSize, 0, y * cellSize)
            const float cellSize = 1.0f;
            int gridX = Mathf.FloorToInt(intersection.X / cellSize);
            int gridZ = Mathf.FloorToInt(intersection.Z / cellSize);
            
            return new Vector2I(gridX, gridZ);
        }

        /// <summary>
        /// Sets the visibility of the preview tile (used by ViewManager).
        /// </summary>
        public void SetPreviewVisible(bool visible)
        {
            if (_gameManager.SelectedBuildingType == BuildingType.None)
            {
                _previewTile.Visible = false;
            }
            else
            {
                _previewTile.Visible = visible;
            }
        }
    }
}
