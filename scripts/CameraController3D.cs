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
            // Calculate where the camera is looking on the ground
            var groundTarget = GetGroundTarget(_targetPosition);
            
            // Calculate visible half-extents on the ground plane
            var viewportSize = GetViewport().GetVisibleRect().Size;
            float aspectRatio = viewportSize.X / viewportSize.Y;
            float halfWidth = Size * aspectRatio / 2.0f;  // ground half-width (camera right is projected on XZ)
            float halfDepth = Size / 2.0f / Mathf.Cos(Mathf.DegToRad(45f)); // stretched by tilt
            
            // Map boundaries with 10-cell margin
            const float margin = 10f;
            float mapWidth = _gridManager.GridWidth * CellSize;
            float mapHeight = _gridManager.GridHeight * CellSize;
            
            float minGroundX = -margin + halfWidth;
            float maxGroundX = mapWidth + margin - halfWidth;
            float minGroundZ = -margin + halfDepth;
            float maxGroundZ = mapHeight + margin - halfDepth;
            
            // Handle case where visible area is larger than allowed range
            if (minGroundX > maxGroundX) { minGroundX = maxGroundX = mapWidth / 2.0f; }
            if (minGroundZ > maxGroundZ) { minGroundZ = maxGroundZ = mapHeight / 2.0f; }
            
            // Clamp the ground target
            var clampedGround = new Vector3(
                Mathf.Clamp(groundTarget.X, minGroundX, maxGroundX),
                0,
                Mathf.Clamp(groundTarget.Z, minGroundZ, maxGroundZ)
            );
            
            // Move camera position by the same delta
            var delta = clampedGround - groundTarget;
            _targetPosition += delta;
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

        /// <summary>
        /// Gets the camera's target position in world coordinates (the point it's looking at).
        /// </summary>
        public Vector3 GetTargetPosition()
        {
            return _targetPosition;
        }

        /// <summary>
        /// Sets the camera's target position in world coordinates.
        /// </summary>
        public void SetTargetPosition(Vector3 position)
        {
            _targetPosition = position;
        }

        /// <summary>
        /// Calculates the ground target point (Y=0) that the camera is looking at.
        /// </summary>
        private Vector3 GetGroundTarget(Vector3 camPos)
        {
            var forward = -Transform.Basis.Z;
            if (Mathf.Abs(forward.Y) < 0.001f) return new Vector3(camPos.X, 0, camPos.Z);
            float t = -camPos.Y / forward.Y;
            return camPos + forward * t;
        }

        /// <summary>
        /// Gets the ground target position that the camera is currently looking at.
        /// </summary>
        public Vector3 GetGroundTargetPosition()
        {
            return GetGroundTarget(Position);
        }

        /// <summary>
        /// Sets the camera position so that it looks at the specified ground position.
        /// </summary>
        public void SetGroundTargetPosition(Vector3 groundPos)
        {
            var currentGround = GetGroundTarget(_targetPosition);
            var delta = groundPos - currentGround;
            _targetPosition += delta;
        }
    }
}
