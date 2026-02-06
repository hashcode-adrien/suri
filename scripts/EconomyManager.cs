using Godot;
using System;

namespace Suri
{
    /// <summary>
    /// Manages the game economy including money, income, and expenses.
    /// </summary>
    public partial class EconomyManager : Node
    {
        [Export] public int StartingMoney = 10000;
        [Export] public float IncomeTickInterval = 5.0f; // seconds

        private int _currentMoney;
        private int _lastIncome;
        private int _lastExpenses;
        private float _incomeTimer;

        [Signal]
        public delegate void MoneyChangedEventHandler(int newAmount);

        public int CurrentMoney => _currentMoney;
        public int LastIncome => _lastIncome;
        public int LastExpenses => _lastExpenses;

        public override void _Ready()
        {
            _currentMoney = StartingMoney;
            _incomeTimer = 0;
        }

        public override void _Process(double delta)
        {
            _incomeTimer += (float)delta;
            if (_incomeTimer >= IncomeTickInterval)
            {
                _incomeTimer = 0;
                ProcessIncomeTick();
            }
        }

        private void ProcessIncomeTick()
        {
            var gameManager = GetNode<GameManager>("/root/Main/GameManager");
            var gridManager = GetNode<GridManager>("/root/Main/GridManager");

            if (gameManager == null || gridManager == null) return;
            if (gameManager.IsPaused) return;

            _lastIncome = 0;
            _lastExpenses = 0;

            // Calculate income and expenses from each building type
            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
            {
                if (type == BuildingType.None) continue;

                int count = gridManager.CountBuildings(type);
                var data = BuildingRegistry.GetBuildingData(type);

                _lastIncome += count * data.IncomePerTick;
                _lastExpenses += count * data.MaintenanceCost;
            }

            int netIncome = _lastIncome - _lastExpenses;
            AddMoney(netIncome);
        }

        public bool CanAfford(int amount)
        {
            return _currentMoney >= amount;
        }

        public bool SpendMoney(int amount)
        {
            if (!CanAfford(amount)) return false;
            
            _currentMoney -= amount;
            EmitSignal(SignalName.MoneyChanged, _currentMoney);
            return true;
        }

        public void AddMoney(int amount)
        {
            _currentMoney += amount;
            EmitSignal(SignalName.MoneyChanged, _currentMoney);
        }
    }
}
