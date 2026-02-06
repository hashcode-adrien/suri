using Godot;
using System;

namespace Suri
{
    /// <summary>
    /// Manages population growth and happiness.
    /// </summary>
    public partial class PopulationManager : Node
    {
        [Export] public float GrowthTickInterval = 3.0f; // seconds
        [Export] public float GrowthRate = 0.1f; // Population grows by 10% per tick

        private int _currentPopulation;
        private float _happiness = 0.5f; // 0-1 scale
        private float _growthTimer;

        [Signal]
        public delegate void PopulationChangedEventHandler(int newPopulation);

        [Signal]
        public delegate void HappinessChangedEventHandler(float newHappiness);

        public int CurrentPopulation => _currentPopulation;
        public float Happiness => _happiness;

        public override void _Ready()
        {
            _currentPopulation = 0;
            _growthTimer = 0;
        }

        public override void _Process(double delta)
        {
            _growthTimer += (float)delta;
            if (_growthTimer >= GrowthTickInterval)
            {
                _growthTimer = 0;
                ProcessGrowthTick();
            }
        }

        private void ProcessGrowthTick()
        {
            var gameManager = GetNode<GameManager>("/root/Main/GameManager");
            var gridManager = GetNode<GridManager>("/root/Main/GridManager");

            if (gameManager == null || gridManager == null) return;
            if (gameManager.IsPaused) return;

            // Calculate maximum population capacity
            int residentialCount = gridManager.CountBuildings(BuildingType.Residential);
            var residentialData = BuildingRegistry.GetBuildingData(BuildingType.Residential);
            int maxPopulation = residentialCount * residentialData.PopulationCapacity;

            // Calculate happiness
            CalculateHappiness(gridManager);

            // Grow population if below capacity
            if (_currentPopulation < maxPopulation && _happiness > 0.3f)
            {
                int growth = Mathf.Max(1, Mathf.CeilToInt((maxPopulation - _currentPopulation) * GrowthRate * _happiness));
                _currentPopulation = Mathf.Min(_currentPopulation + growth, maxPopulation);
                EmitSignal(SignalName.PopulationChanged, _currentPopulation);
            }
            // Shrink population if unhappy or over capacity
            else if (_currentPopulation > maxPopulation || _happiness < 0.3f)
            {
                int shrink = Mathf.Max(1, Mathf.CeilToInt(_currentPopulation * 0.05f));
                _currentPopulation = Mathf.Max(0, _currentPopulation - shrink);
                EmitSignal(SignalName.PopulationChanged, _currentPopulation);
            }
        }

        private void CalculateHappiness(GridManager gridManager)
        {
            float happinessModifier = 0;
            int totalBuildings = 0;

            // Check all building types for happiness modifiers
            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
            {
                if (type == BuildingType.None) continue;

                int count = gridManager.CountBuildings(type);
                if (count > 0)
                {
                    var data = BuildingRegistry.GetBuildingData(type);
                    happinessModifier += count * data.HappinessModifier;
                    totalBuildings += count;
                }
            }

            // Base happiness is 0.5, modified by buildings
            if (totalBuildings > 0)
            {
                _happiness = Mathf.Clamp(0.5f + happinessModifier / totalBuildings, 0f, 1f);
            }
            else
            {
                _happiness = 0.5f;
            }

            EmitSignal(SignalName.HappinessChanged, _happiness);
        }
    }
}
