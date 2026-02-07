using Godot;

namespace Suri
{
    /// <summary>
    /// Manages switching between 2D and 3D views.
    /// </summary>
    public partial class ViewManager : Node
    {
        [Signal]
        public delegate void ViewChangedEventHandler(bool is3D);

        private Camera2D _camera2D;
        private Camera3D _camera3D;
        private CameraController _cameraController2D;
        private CameraController3D _cameraController3D;
        private GridManager _gridManager;
        private GridManager3D _gridManager3D;
        private BuildingPlacer _buildingPlacer;
        private SubViewportContainer _viewportContainer3D;

        private bool _is3DView = true;
        public bool Is3DView => _is3DView;

        public override void _Ready()
        {
            _camera2D = GetNode<Camera2D>("/root/Main/Camera2D");
            _camera3D = GetNode<Camera3D>("/root/Main/SubViewportContainer/SubViewport/Camera3D");
            
            // Verify camera types
            _cameraController2D = _camera2D as CameraController;
            _cameraController3D = _camera3D as CameraController3D;
            
            if (_cameraController2D == null)
            {
                GD.PrintErr("Camera2D is not of type CameraController - camera synchronization will not work");
            }
            if (_cameraController3D == null)
            {
                GD.PrintErr("Camera3D is not of type CameraController3D - camera synchronization will not work");
            }
            
            _gridManager = GetNode<GridManager>("/root/Main/GridManager");
            _gridManager3D = GetNode<GridManager3D>("/root/Main/SubViewportContainer/SubViewport/GridManager3D");
            _buildingPlacer = GetNode<BuildingPlacer>("/root/Main/BuildingPlacer");
            _viewportContainer3D = GetNode<SubViewportContainer>("/root/Main/SubViewportContainer");

            // Start in 3D view
            SetView3D();
        }

        public void ToggleView()
        {
            if (_is3DView)
                SetView2D();
            else
                SetView3D();
        }

        public void SetView2D()
        {
            _is3DView = false;
            
            // Sync camera position: 3D -> 2D
            if (_cameraController3D != null && _cameraController2D != null)
            {
                // Get the ground target that 3D camera is looking at
                var groundTarget = _cameraController3D.GetGroundTargetPosition();
                // Convert 3D ground position (X, Z) to 2D pixel position (X * TileSize, Z * TileSize)
                var target2D = new Vector2(groundTarget.X * _gridManager.TileSize, groundTarget.Z * _gridManager.TileSize);
                _cameraController2D.SetTargetPosition(target2D);
            }
            
            // Enable 2D
            _camera2D.Enabled = true;
            _gridManager.SetTilesVisible(true);
            _gridManager.ToggleGrid(true);
            
            // Disable 3D
            _camera3D.Current = false;
            _viewportContainer3D.Visible = false;
            
            // Update building placer
            _buildingPlacer.SetPreviewVisible(true);
            
            EmitSignal(SignalName.ViewChanged, false);
            GD.Print("Switched to 2D view");
        }

        public void SetView3D()
        {
            _is3DView = true;
            
            // Sync camera position: 2D -> 3D
            if (_cameraController2D != null && _cameraController3D != null)
            {
                var target2D = _cameraController2D.GetTargetPosition();
                // Convert 2D pixel position to 3D ground position
                var gridCenter = target2D / _gridManager.TileSize;
                var groundTarget = new Vector3(gridCenter.X, 0, gridCenter.Y);
                // Set the 3D camera to look at this ground position
                _cameraController3D.SetGroundTargetPosition(groundTarget);
            }
            
            // Disable 2D rendering (but keep grid data)
            _camera2D.Enabled = false;
            _gridManager.SetTilesVisible(false);
            _gridManager.ToggleGrid(false);
            
            // Enable 3D
            _camera3D.Current = true;
            _viewportContainer3D.Visible = true;
            
            // Update building placer (still works but uses 3D raycast)
            _buildingPlacer.SetPreviewVisible(false);
            
            EmitSignal(SignalName.ViewChanged, true);
            GD.Print("Switched to 3D view");
        }
    }
}
