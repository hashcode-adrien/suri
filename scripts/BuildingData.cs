using Godot;

namespace Suri
{
    /// <summary>
    /// Defines the types of buildings that can be placed in the game.
    /// </summary>
    public enum BuildingType
    {
        None,
        Residential,
        Commercial,
        Industrial,
        Road,
        Park
    }

    /// <summary>
    /// Data structure containing properties for each building type.
    /// </summary>
    public struct BuildingData
    {
        public BuildingType Type;
        public string Name;
        public int Cost;
        public int MaintenanceCost;
        public Color Color;
        public int PopulationCapacity;
        public int IncomePerTick;
        public float HappinessModifier;

        public BuildingData(BuildingType type, string name, int cost, int maintenance, Color color, 
                          int popCapacity = 0, int income = 0, float happiness = 0)
        {
            Type = type;
            Name = name;
            Cost = cost;
            MaintenanceCost = maintenance;
            Color = color;
            PopulationCapacity = popCapacity;
            IncomePerTick = income;
            HappinessModifier = happiness;
        }
    }

    /// <summary>
    /// Static registry of all building types and their properties.
    /// </summary>
    public static class BuildingRegistry
    {
        public static readonly BuildingData[] Buildings = new BuildingData[]
        {
            new BuildingData(BuildingType.None, "None", 0, 0, Colors.Transparent),
            new BuildingData(BuildingType.Residential, "Residential", 100, 5, Colors.Green, popCapacity: 10, income: 20),
            new BuildingData(BuildingType.Commercial, "Commercial", 150, 8, Colors.Blue, income: 50),
            new BuildingData(BuildingType.Industrial, "Industrial", 200, 10, Colors.Yellow, income: 80, happiness: -0.1f),
            new BuildingData(BuildingType.Road, "Road", 10, 1, Colors.Gray),
            new BuildingData(BuildingType.Park, "Park", 50, 2, Colors.LightGreen, happiness: 0.2f)
        };

        public static BuildingData GetBuildingData(BuildingType type)
        {
            return Buildings[(int)type];
        }
    }
}
