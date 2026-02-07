using Godot;

namespace Suri
{
    /// <summary>
    /// Controls camera panning and zooming.
    /// </summary>
    public partial class CameraController : Camera2D
    {
        [Export] public float PanSpeed = 400f;
        [Export] public float ZoomSpeed = 0.1f;
        [Export] public float MinZoom = 0.5f;
        [Export] public float MaxZoom = 2.0f;

        private Vector2 _targetPosition;
        private Vector2 _targetZoom;

        public override void _Ready()
        {
            // Grid center in 2D: (GridWidth * TileSize / 2, GridHeight * TileSize / 2)
            // = (40 * 32 / 2, 30 * 32 / 2) = (640, 480)
            Position = new Vector2(640, 480);
            _targetPosition = Position;
            _targetZoom = Zoom;
        }

        public override void _Process(double delta)
        {
            HandlePanning(delta);
            HandleZoom(delta);
            
            // Smooth camera movement
            Position = Position.Lerp(_targetPosition, 10f * (float)delta);
            Zoom = Zoom.Lerp(_targetZoom, 10f * (float)delta);
        }

        private void HandlePanning(double delta)
        {
            var moveDirection = Vector2.Zero;

            if (Input.IsActionPressed("move_up"))
                moveDirection.Y += 1;
            if (Input.IsActionPressed("move_down"))
                moveDirection.Y -= 1;
            if (Input.IsActionPressed("move_left"))
                moveDirection.X -= 1;
            if (Input.IsActionPressed("move_right"))
                moveDirection.X += 1;

            if (moveDirection != Vector2.Zero)
            {
                moveDirection = moveDirection.Normalized();
                _targetPosition += moveDirection * PanSpeed * (float)delta / Zoom.X;
            }
        }

        private void HandleZoom(double delta)
        {
            // Zoom with mouse wheel is handled in _UnhandledInput
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
            var newZoom = _targetZoom + Vector2.One * ZoomSpeed;
            _targetZoom = newZoom.Clamp(new Vector2(MinZoom, MinZoom), new Vector2(MaxZoom, MaxZoom));
        }

        private void ZoomOut()
        {
            var newZoom = _targetZoom - Vector2.One * ZoomSpeed;
            _targetZoom = newZoom.Clamp(new Vector2(MinZoom, MinZoom), new Vector2(MaxZoom, MaxZoom));
        }
    }
}
