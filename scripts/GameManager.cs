using Godot;

namespace Suri
{
    /// <summary>
    /// Main game manager controlling game state and coordinating systems.
    /// </summary>
    public partial class GameManager : Node
    {
        private bool _isPaused = false;
        private BuildingType _selectedBuildingType = BuildingType.None;

        [Signal]
        public delegate void GamePausedEventHandler(bool paused);

        [Signal]
        public delegate void BuildingTypeSelectedEventHandler(BuildingType buildingType);

        public bool IsPaused => _isPaused;
        public BuildingType SelectedBuildingType => _selectedBuildingType;

        public override void _Ready()
        {
            GD.Print("Game Manager Ready");
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("pause"))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            EmitSignal(SignalName.GamePaused, _isPaused);
            GD.Print($"Game {(_isPaused ? "Paused" : "Resumed")}");
        }

        public void SetPause(bool paused)
        {
            if (_isPaused != paused)
            {
                _isPaused = paused;
                EmitSignal(SignalName.GamePaused, _isPaused);
            }
        }

        public void SelectBuildingType(BuildingType type)
        {
            _selectedBuildingType = type;
            EmitSignal(SignalName.BuildingTypeSelected, (int)type);
            GD.Print($"Selected building type: {type}");
        }
    }
}
