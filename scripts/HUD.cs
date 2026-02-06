using Godot;

namespace Suri
{
    /// <summary>
    /// Main HUD displaying game information and build menu.
    /// </summary>
    public partial class HUD : CanvasLayer
    {
        private Label _moneyLabel;
        private Label _populationLabel;
        private Label _happinessLabel;
        private Label _incomeLabel;
        private Label _pauseLabel;
        private VBoxContainer _buildMenu;
        private Button _viewToggleButton;

        private GameManager _gameManager;
        private EconomyManager _economyManager;
        private PopulationManager _populationManager;
        private ViewManager _viewManager;

        public override void _Ready()
        {
            _gameManager = GetNode<GameManager>("/root/Main/GameManager");
            _economyManager = GetNode<EconomyManager>("/root/Main/EconomyManager");
            _populationManager = GetNode<PopulationManager>("/root/Main/PopulationManager");
            
            // ViewManager might not be ready immediately, will connect later
            _viewManager = null;

            CreateHUD();
            ConnectSignals();
            UpdateAllLabels();
        }

        private void CreateHUD()
        {
            // Top bar background
            var topBar = new ColorRect
            {
                Color = new Color(0.1f, 0.1f, 0.1f, 0.9f),
                CustomMinimumSize = new Vector2(1280, 60),
                Position = new Vector2(0, 0)
            };
            AddChild(topBar);

            // Info labels
            var infoContainer = new HBoxContainer
            {
                Position = new Vector2(10, 10)
            };
            topBar.AddChild(infoContainer);

            _moneyLabel = CreateLabel("Money: $0", 200);
            _populationLabel = CreateLabel("Population: 0", 150);
            _happinessLabel = CreateLabel("Happiness: 50%", 150);
            _incomeLabel = CreateLabel("Income: $0/tick", 200);
            _pauseLabel = CreateLabel("", 100);

            infoContainer.AddChild(_moneyLabel);
            infoContainer.AddChild(_populationLabel);
            infoContainer.AddChild(_happinessLabel);
            infoContainer.AddChild(_incomeLabel);
            infoContainer.AddChild(_pauseLabel);

            // Build menu on the right side
            var menuBg = new ColorRect
            {
                Color = new Color(0.1f, 0.1f, 0.1f, 0.9f),
                Position = new Vector2(1100, 70),
                CustomMinimumSize = new Vector2(180, 400)
            };
            AddChild(menuBg);

            _buildMenu = new VBoxContainer
            {
                Position = new Vector2(1110, 80)
            };
            AddChild(_buildMenu);

            CreateBuildButtons();
        }

        private Label CreateLabel(string text, float minWidth)
        {
            var label = new Label
            {
                Text = text,
                CustomMinimumSize = new Vector2(minWidth, 40),
                VerticalAlignment = VerticalAlignment.Center
            };
            return label;
        }

        private void CreateBuildButtons()
        {
            var title = new Label
            {
                Text = "Build Menu",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(160, 30)
            };
            _buildMenu.AddChild(title);

            // Create button for each building type
            foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
            {
                if (type == BuildingType.None) continue;

                var data = BuildingRegistry.GetBuildingData(type);
                var button = new Button
                {
                    Text = $"{data.Name}\n${data.Cost}",
                    CustomMinimumSize = new Vector2(160, 50)
                };

                // Store the type in metadata for the callback
                button.SetMeta("BuildingType", (int)type);
                button.Pressed += () => OnBuildButtonPressed(button);
                
                _buildMenu.AddChild(button);
            }

            // Add clear selection button
            var clearButton = new Button
            {
                Text = "Clear\nSelection",
                CustomMinimumSize = new Vector2(160, 50)
            };
            clearButton.Pressed += () => _gameManager.SelectBuildingType(BuildingType.None);
            _buildMenu.AddChild(clearButton);

            // Add pause button
            var pauseButton = new Button
            {
                Text = "Pause (P)",
                CustomMinimumSize = new Vector2(160, 50)
            };
            pauseButton.Pressed += () => _gameManager.TogglePause();
            _buildMenu.AddChild(pauseButton);

            // Add view toggle button
            _viewToggleButton = new Button
            {
                Text = "View: 2D",
                CustomMinimumSize = new Vector2(160, 50)
            };
            _viewToggleButton.Pressed += OnViewTogglePressed;
            _buildMenu.AddChild(_viewToggleButton);
        }

        private void OnBuildButtonPressed(Button button)
        {
            var typeInt = button.GetMeta("BuildingType").AsInt32();
            var type = (BuildingType)typeInt;
            _gameManager.SelectBuildingType(type);
        }

        private void OnViewTogglePressed()
        {
            // Lazy load ViewManager
            if (_viewManager == null && HasNode("/root/Main/ViewManager"))
            {
                _viewManager = GetNode<ViewManager>("/root/Main/ViewManager");
                _viewManager.ViewChanged += OnViewChanged;
            }

            if (_viewManager != null)
            {
                _viewManager.ToggleView();
            }
        }

        private void ConnectSignals()
        {
            _economyManager.MoneyChanged += OnMoneyChanged;
            _populationManager.PopulationChanged += OnPopulationChanged;
            _populationManager.HappinessChanged += OnHappinessChanged;
            _gameManager.GamePaused += OnGamePaused;
        }

        private void OnMoneyChanged(int amount)
        {
            _moneyLabel.Text = $"Money: ${amount}";
            var income = _economyManager.LastIncome - _economyManager.LastExpenses;
            _incomeLabel.Text = $"Income: ${income}/tick";
        }

        private void OnPopulationChanged(int population)
        {
            _populationLabel.Text = $"Population: {population}";
        }

        private void OnHappinessChanged(float happiness)
        {
            int percentage = Mathf.RoundToInt(happiness * 100);
            _happinessLabel.Text = $"Happiness: {percentage}%";
        }

        private void OnGamePaused(bool paused)
        {
            _pauseLabel.Text = paused ? "[PAUSED]" : "";
        }

        private void OnViewChanged(bool is3D)
        {
            _viewToggleButton.Text = is3D ? "View: 3D" : "View: 2D";
        }

        public override void _ExitTree()
        {
            // Cleanup signal connections
            if (_viewManager != null)
            {
                _viewManager.ViewChanged -= OnViewChanged;
            }
        }

        private void UpdateAllLabels()
        {
            OnMoneyChanged(_economyManager.CurrentMoney);
            OnPopulationChanged(_populationManager.CurrentPopulation);
            OnHappinessChanged(_populationManager.Happiness);
            OnGamePaused(_gameManager.IsPaused);
        }
    }
}
