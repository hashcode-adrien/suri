using Godot;

namespace Suri
{
    /// <summary>
    /// Loading screen with HashCode logo, asset preloading, and fade-out transition.
    /// </summary>
    public partial class LoadingScreen : CanvasLayer
    {
        private ColorRect _background;
        private Label _logoLabel;
        private Label _subtitleLabel;
        private Label _loadingLabel;
        private bool _isLoading = true;
        private double _minDisplayTime = 2.0;
        private double _elapsedTime = 0.0;

        // Cached assets
        public PackedScene CottageScene { get; private set; }

        public override void _Ready()
        {
            // Set high layer so it's on top of everything
            Layer = 100;

            CreateUI();
            StartPreloading();
        }

        private void CreateUI()
        {
            // Full-screen background
            _background = new ColorRect
            {
                Color = new Color(0.08f, 0.08f, 0.12f),
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f
            };
            AddChild(_background);

            // HashCode logo (large centered label)
            _logoLabel = new Label
            {
                Text = "HashCode",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AnchorLeft = 0.5f,
                AnchorTop = 0.4f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.4f,
                OffsetLeft = -200,
                OffsetTop = -50,
                OffsetRight = 200,
                OffsetBottom = 50
            };
            
            // Create large font for logo
            var logoFont = new LabelSettings
            {
                FontSize = 72,
                FontColor = Colors.White
            };
            _logoLabel.LabelSettings = logoFont;
            
            AddChild(_logoLabel);

            // Subtitle
            _subtitleLabel = new Label
            {
                Text = "Suri City Builder",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AnchorLeft = 0.5f,
                AnchorTop = 0.5f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -150,
                OffsetTop = 0,
                OffsetRight = 150,
                OffsetBottom = 40
            };
            
            var subtitleFont = new LabelSettings
            {
                FontSize = 24,
                FontColor = new Color(0.8f, 0.8f, 0.8f)
            };
            _subtitleLabel.LabelSettings = subtitleFont;
            
            AddChild(_subtitleLabel);

            // Loading label at bottom
            _loadingLabel = new Label
            {
                Text = "Loading...",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                AnchorLeft = 0.5f,
                AnchorTop = 1.0f,
                AnchorRight = 0.5f,
                AnchorBottom = 1.0f,
                OffsetLeft = -100,
                OffsetTop = -80,
                OffsetRight = 100,
                OffsetBottom = -40
            };
            
            var loadingFont = new LabelSettings
            {
                FontSize = 18,
                FontColor = new Color(0.6f, 0.6f, 0.6f)
            };
            _loadingLabel.LabelSettings = loadingFont;
            
            AddChild(_loadingLabel);
        }

        private void StartPreloading()
        {
            // Preload heavy assets
            try
            {
                CottageScene = GD.Load<PackedScene>("res://assets/Meshy_AI_Enchanted_Cottage_0206220057_texture.glb");
                GD.Print("Preloaded cottage model");
            }
            catch (System.Exception e)
            {
                GD.PrintErr($"Failed to preload cottage: {e.Message}");
            }

            _isLoading = false;
        }

        public override void _Process(double delta)
        {
            if (_isLoading)
                return;

            _elapsedTime += delta;

            if (_elapsedTime >= _minDisplayTime)
            {
                FadeOutAndRemove();
                SetProcess(false); // Stop processing
            }
        }

        private void FadeOutAndRemove()
        {
            // Create tween for fade-out
            var tween = CreateTween();
            tween.SetParallel(true);
            
            // Fade out all UI elements
            tween.TweenProperty(_background, "modulate:a", 0.0f, 0.5);
            tween.TweenProperty(_logoLabel, "modulate:a", 0.0f, 0.5);
            tween.TweenProperty(_subtitleLabel, "modulate:a", 0.0f, 0.5);
            tween.TweenProperty(_loadingLabel, "modulate:a", 0.0f, 0.5);
            
            // Remove this node after fade completes
            tween.Chain().TweenCallback(Callable.From(() => QueueFree()));
            
            GD.Print("Loading screen fade-out complete");
        }
    }
}
