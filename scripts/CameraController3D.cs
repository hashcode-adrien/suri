using Godot;

namespace Suri
{
    /// <summary>
    /// Controls orthographic 3D camera with WASD panning and scroll zoom.
    /// </summary>
    public partial class CameraController3D : Camera3D
    {
        [Export] public float PanSpeed = 10f;
        [Export] public float ZoomSpeed = 1.0f;
        [Export] public float MinSize = 5f;
        [Export] public float MaxSize = 30f;

        private Vector3 _targetPosition;
        private float _targetSize;
        private GridManager _gridManager;
        private const float CellSize = 1.0f;

        public override void _Ready()
        {
            _gridManager = GetNode<GridManager>("/root/Main/GridManager");
            
            // Set up orthographic projection
            Projection = ProjectionType.Orthogonal;
            Size = 18f; // Default orthographic size - adjusted for better framing
            _targetSize = Size;
            
            // CRITICAL: 45° isometric angle (classic city builder camera)
            // X rotation: -45 degrees (pitch down)
            // Y rotation: -45 degrees (rotated around Y for isometric view)
            // This gives the classic SimCity / Cities Skylines / Anno perspective
            Rotation = new Vector3(Mathf.DegToRad(-45f), Mathf.DegToRad(-45f), 0);
            
            // Position camera above the center of the grid (40x30 grid, cell size 1.0)
            // Center the camera over the grid center (20, 15) in world XZ
            // Y position lowered from 30 to 18 for better initial view
            Position = new Vector3(20, 18, 15);
            
            _targetPosition = Position;
        }

        public override void _Process(double delta)
        {
            HandlePanning(delta);
            
            // Apply boundary clamping after movement calculation
            ApplyCameraClamping();
            
            // Smooth camera movement
            Position = Position.Lerp(_targetPosition, 10f * (float)delta);
            Size = Mathf.Lerp(Size, _targetSize, 10f * (float)delta);
        }

        private void HandlePanning(double delta)
        {
            var moveDirection = Vector3.Zero;

            // Get camera's local axes for movement
            var forward = -Transform.Basis.Z; // Camera's forward direction
            var right = Transform.Basis.X;    // Camera's right direction
            
            // Project onto XZ plane (ignore Y component for horizontal movement)
            forward.Y = 0;
            right.Y = 0;
            forward = forward.Normalized();
            right = right.Normalized();

            if (Input.IsActionPressed("move_up"))
                moveDirection += forward;
            if (Input.IsActionPressed("move_down"))
                moveDirection -= forward;
            if (Input.IsActionPressed("move_left"))
                moveDirection -= right;
            if (Input.IsActionPressed("move_right"))
                moveDirection += right;

            if (moveDirection != Vector3.Zero)
            {
                moveDirection = moveDirection.Normalized();
                _targetPosition += moveDirection * PanSpeed * (float)delta;
            }
        }

        private void ApplyCameraClamping()
        {
            // Clamp camera position to map bounds with 10-cell margin (10 units)
            const float cellMargin = 10f; // 10 cells * 1.0 cell size
            float minX = -cellMargin;
            float maxX = _gridManager.GridWidth * CellSize + cellMargin;
            float minZ = -cellMargin;
            float maxZ = _gridManager.GridHeight * CellSize + cellMargin;
            
            _targetPosition = new Vector3(
                Mathf.Clamp(_targetPosition.X, minX, maxX),
                _targetPosition.Y, // Don't clamp Y (height)
                Mathf.Clamp(_targetPosition.Z, minZ, maxZ)
            );
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == MouseButton.WheelUp && mouseButton.Pressed)
                {
                    ZoomIn();
                }
                else if (mouseButton.ButtonIndex == MouseButton.WheelDown && mouseButton.Pressed)
                {
                    ZoomOut();
                }
            }
        }

        private void ZoomIn()
        {
            _targetSize = Mathf.Clamp(_targetSize - ZoomSpeed, MinSize, MaxSize);
        }

        private void ZoomOut()
        {
            _targetSize = Mathf.Clamp(_targetSize + ZoomSpeed, MinSize, MaxSize);
        }
    }
}
