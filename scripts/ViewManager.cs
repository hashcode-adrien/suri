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
        private GridManager _gridManager;
        private GridManager3D _gridManager3D;
        private BuildingPlacer _buildingPlacer;
        private SubViewportContainer _viewportContainer3D;

        private bool _is3DView = false;
        public bool Is3DView => _is3DView;

        public override void _Ready()
        {
            _camera2D = GetNode<Camera2D>("/root/Main/Camera2D");
            _camera3D = GetNode<Camera3D>("/root/Main/SubViewportContainer/SubViewport/Camera3D");
            _gridManager = GetNode<GridManager>("/root/Main/GridManager");
            _gridManager3D = GetNode<GridManager3D>("/root/Main/SubViewportContainer/SubViewport/GridManager3D");
            _buildingPlacer = GetNode<BuildingPlacer>("/root/Main/BuildingPlacer");
            _viewportContainer3D = GetNode<SubViewportContainer>("/root/Main/SubViewportContainer");

            // Start in 2D view
            SetView2D();
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
