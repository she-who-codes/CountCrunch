using System;
using System.Collections.Generic;

namespace MilkCaps.Inventory
{
    [Serializable]
    public sealed class InventorySaveData
    {
        public int schemaVersion = 1;
        public List<InventoryStackSaveData> items = new List<InventoryStackSaveData>();
    }

    [Serializable]
    public sealed class InventoryStackSaveData
    {
        public string itemId;
        public int itemType;
        public int quantity;
    }
}
