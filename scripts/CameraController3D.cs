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

        public override void _Ready()
        {
            // Set up orthographic projection
            Projection = ProjectionType.Orthogonal;
            Size = 20f; // Default orthographic size
            _targetSize = Size;
            
            // Position camera at an angle (isometric-like view)
            // Looking down at 45 degrees from the side
            Position = new Vector3(20, 20, 20);
            LookAt(new Vector3(20, 0, 15), Vector3.Up);
            
            _targetPosition = Position;
        }

        public override void _Process(double delta)
        {
            HandlePanning(delta);
            
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
                moveDirection -= forward;
            if (Input.IsActionPressed("move_down"))
                moveDirection += forward;
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
