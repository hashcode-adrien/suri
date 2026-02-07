using Godot;
using System.Collections.Generic;

namespace Suri
{
	/// <summary>
	/// Handles player input for placing and removing buildings.
	/// Uses polling-based input in _Process to avoid event ordering issues
	/// with other nodes using _UnhandledInput.
	/// NEW: Uses "draw and release" mode - accumulate tiles during drag, place on release.
	/// </summary>
	public partial class BuildingPlacer : Node2D
	{
		private GridManager _gridManager;
		private GameManager _gameManager;
		private EconomyManager _economyManager;
		private Camera2D _camera;
		private Camera3D _camera3D;
		private ColorRect _previewTile;
		private ViewManager _viewManager;

		private static readonly Vector2I InvalidGridPosition = new Vector2I(-1, -1);

		// New drag-and-release state
		private bool _isDraggingPlace = false;
		private bool _isDraggingDemolish = false;
		private List<Vector2I> _pendingPlacements = new List<Vector2I>();
		private List<Vector2I> _pendingDemolitions = new List<Vector2I>();
		private Dictionary<Vector2I, ColorRect> _previewTiles = new Dictionary<Vector2I, ColorRect>();

		public override void _Ready()
		{
			_gridManager = GetNode<GridManager>("/root/Main/GridManager");
			_gameManager = GetNode<GameManager>("/root/Main/GameManager");
			_economyManager = GetNode<EconomyManager>("/root/Main/EconomyManager");
			_camera = GetNode<Camera2D>("/root/Main/Camera2D");
			
			// ViewManager and Camera3D might not be ready immediately, get them when scene is ready
			CallDeferred(nameof(InitializeDeferredReferences));

			// Create preview tile for cursor
			_previewTile = new ColorRect
			{
				Size = new Vector2(_gridManager.TileSize - 2, _gridManager.TileSize - 2),
				Color = new Color(1, 1, 1, 0.5f),
				Visible = false,
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			AddChild(_previewTile);
		}

		private void InitializeDeferredReferences()
		{
			if (HasNode("/root/Main/ViewManager"))
			{
				_viewManager = GetNode<ViewManager>("/root/Main/ViewManager");
			}
			
			if (HasNode("/root/Main/SubViewportContainer/SubViewport/Camera3D"))
			{
				_camera3D = GetNode<Camera3D>("/root/Main/SubViewportContainer/SubViewport/Camera3D");
			}
		}

		public override void _Process(double delta)
		{
			// Handle Escape key to cancel drag operations
			if (Input.IsActionJustPressed("ui_cancel"))
			{
				if (_isDraggingPlace || _isDraggingDemolish)
				{
					CancelDragOperation();
				}
			}
			
			UpdatePreview();
			HandleMouseInput();
		}

		private void HandleMouseInput()
		{
			bool leftPressed = Input.IsMouseButtonPressed(MouseButton.Left);
			bool rightPressed = Input.IsMouseButtonPressed(MouseButton.Right);

			// Check if mouse is over a GUI element (HUD buttons etc.)
			bool mouseOverGui = IsMouseOverInteractiveGui();

			var gridPos = GetGridPositionFromMouse();
			bool validPos = _gridManager.IsValidPosition(gridPos);

			// --- LEFT CLICK: Place buildings (drag and release mode) ---
			if (leftPressed)
			{
				if (!_isDraggingPlace && !mouseOverGui && validPos)
				{
					// Start dragging
					_isDraggingPlace = true;
					_pendingPlacements.Clear();
					_pendingPlacements.Add(gridPos);
					UpdatePendingPreview();
				}
				else if (_isDraggingPlace && validPos)
				{
					// Continue dragging - add to pending if not already there
					if (!_pendingPlacements.Contains(gridPos))
					{
						_pendingPlacements.Add(gridPos);
						UpdatePendingPreview();
					}
				}
			}
			else if (_isDraggingPlace)
			{
				// Released - place all pending tiles
				ExecutePendingPlacements();
				_isDraggingPlace = false;
			}

			// --- RIGHT CLICK: Demolish buildings (drag and release mode) ---
			if (rightPressed)
			{
				if (!_isDraggingDemolish && !mouseOverGui && validPos)
				{
					// Start dragging
					_isDraggingDemolish = true;
					_pendingDemolitions.Clear();
					_pendingDemolitions.Add(gridPos);
					UpdatePendingPreview();
				}
				else if (_isDraggingDemolish && validPos)
				{
					// Continue dragging - add to pending if not already there
					if (!_pendingDemolitions.Contains(gridPos))
					{
						_pendingDemolitions.Add(gridPos);
						UpdatePendingPreview();
					}
				}
			}
			else if (_isDraggingDemolish)
			{
				// Released - demolish all pending tiles
				ExecutePendingDemolitions();
				_isDraggingDemolish = false;
			}
		}

		private void UpdatePreview()
		{
			// Only show single-tile preview when not dragging
			if (_isDraggingPlace || _isDraggingDemolish)
			{
				_previewTile.Visible = false;
				return;
			}
			
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

		private void UpdatePendingPreview()
		{
			// Clear old preview tiles
			foreach (var tile in _previewTiles.Values)
			{
				tile.QueueFree();
			}
			_previewTiles.Clear();

			if (_isDraggingPlace)
			{
				var data = BuildingRegistry.GetBuildingData(_gameManager.SelectedBuildingType);
				
				foreach (var gridPos in _pendingPlacements)
				{
					var canAfford = _economyManager.CanAfford(data.Cost * _pendingPlacements.Count);
					var isOccupied = _gridManager.GetBuildingAt(gridPos) != BuildingType.None;
					
					var preview = new ColorRect
					{
						Position = _gridManager.GridToWorld(gridPos),
						Size = new Vector2(_gridManager.TileSize - 2, _gridManager.TileSize - 2),
						MouseFilter = Control.MouseFilterEnum.Ignore
					};

					// Show red for invalid, building color for valid
					if (canAfford && !isOccupied)
					{
						preview.Color = new Color(data.Color, 0.6f); // More saturated during drag
					}
					else
					{
						preview.Color = new Color(Colors.Red, 0.6f);
					}

					AddChild(preview);
					_previewTiles[gridPos] = preview;
				}
			}
			else if (_isDraggingDemolish)
			{
				foreach (var gridPos in _pendingDemolitions)
				{
					var buildingType = _gridManager.GetBuildingAt(gridPos);
					
					var preview = new ColorRect
					{
						Position = _gridManager.GridToWorld(gridPos),
						Size = new Vector2(_gridManager.TileSize - 2, _gridManager.TileSize - 2),
						MouseFilter = Control.MouseFilterEnum.Ignore
					};

					// Show red X pattern or just red tint for demolition
					if (buildingType != BuildingType.None)
					{
						preview.Color = new Color(Colors.Red, 0.6f);
					}
					else
					{
						preview.Color = new Color(Colors.DarkRed, 0.4f); // Darker if nothing to demolish
					}

					AddChild(preview);
					_previewTiles[gridPos] = preview;
				}
			}
		}

		private void ExecutePendingPlacements()
		{
			if (_gameManager.SelectedBuildingType == BuildingType.None)
			{
				ClearPendingPreview();
				return;
			}

			var data = BuildingRegistry.GetBuildingData(_gameManager.SelectedBuildingType);
			int totalCost = 0;
			var validPlacements = new List<Vector2I>();

			// First pass: validate all placements
			foreach (var gridPos in _pendingPlacements)
			{
				if (_gridManager.GetBuildingAt(gridPos) == BuildingType.None)
				{
					totalCost += data.Cost;
					validPlacements.Add(gridPos);
				}
			}

			// Check if can afford all
			if (!_economyManager.CanAfford(totalCost))
			{
				GD.Print($"Not enough money! Need ${totalCost}");
				ClearPendingPreview();
				return;
			}

			// Second pass: place all valid tiles
			int placedCount = 0;
			foreach (var gridPos in validPlacements)
			{
				if (_gridManager.PlaceBuilding(gridPos, _gameManager.SelectedBuildingType))
				{
					placedCount++;
				}
			}

			// Deduct money for all placed buildings
			if (placedCount > 0)
			{
				_economyManager.SpendMoney(data.Cost * placedCount);
				GD.Print($"Placed {placedCount} {data.Name}(s), total cost: ${data.Cost * placedCount}");
			}

			ClearPendingPreview();
		}

		private void ExecutePendingDemolitions()
		{
			int totalRefund = 0;
			int demolishedCount = 0;

			foreach (var gridPos in _pendingDemolitions)
			{
				var buildingType = _gridManager.GetBuildingAt(gridPos);
				if (buildingType == BuildingType.None) continue;

				if (_gridManager.RemoveBuilding(gridPos))
				{
					var data = BuildingRegistry.GetBuildingData(buildingType);
					totalRefund += data.Cost / 2;
					demolishedCount++;
				}
			}

			if (demolishedCount > 0)
			{
				_economyManager.AddMoney(totalRefund);
				GD.Print($"Demolished {demolishedCount} building(s), total refund: ${totalRefund}");
			}

			ClearPendingPreview();
		}

		private void CancelDragOperation()
		{
			GD.Print("Drag operation cancelled");
			_isDraggingPlace = false;
			_isDraggingDemolish = false;
			ClearPendingPreview();
		}

		private void ClearPendingPreview()
		{
			_pendingPlacements.Clear();
			_pendingDemolitions.Clear();
			
			foreach (var tile in _previewTiles.Values)
			{
				tile.QueueFree();
			}
			_previewTiles.Clear();
		}

		private bool IsMouseOverInteractiveGui()
		{
			var hoveredControl = GetViewport().GuiGetHoveredControl();
			if (hoveredControl == null) return false;
			if (hoveredControl == _previewTile) return false;
			// Ignore our own preview tiles
			if (_previewTiles.ContainsValue(hoveredControl as ColorRect)) return false;
			
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
			if (_camera3D == null) return InvalidGridPosition;

			var viewport = GetViewport();
			var mousePos = viewport.GetMousePosition();

			// Get ray from camera
			var from = _camera3D.ProjectRayOrigin(mousePos);
			var dir = _camera3D.ProjectRayNormal(mousePos);

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
