using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.Constants
{
    public static class InventoryLevels
    {
        public const string Good = "Good";
        public const string Low = "Low";
        public const string Critical = "Critical";
    }

    public static class InventoryRules
    {
        public const int GoodFromPercent = 60;       
        public const int CriticalBelowPercent = 30;   
        public const int DefaultCapacity = 100;
        public static readonly Dictionary<string, int> Capacities = new()
        {
            ["A+"] = 200,
            ["A-"] = 80,
            ["B+"] = 150,
            ["B-"] = 60,
            ["AB+"] = 100,
            ["AB-"] = 40,
            ["O+"] = 250,
            ["O-"] = 100
        };

        public static int CapacityFor(string bloodTypeName) =>
            Capacities.TryGetValue(bloodTypeName, out var capacity) ? capacity : DefaultCapacity;

        public static string LevelFor(int percent) =>
            percent >= GoodFromPercent ? InventoryLevels.Good :
            percent < CriticalBelowPercent ? InventoryLevels.Critical :
            InventoryLevels.Low;
    }
}