//Gemini
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MilkCapsInventory
{
    // Inventory item categories
    public enum ItemType { MilkCap, Slammer }

    // Abstract base definition for inventory items
    [System.Serializable]
    public class InventoryItemData
    {
        public string itemId;
        public string displayName;
        public ItemType type;
        public int quantity;
    }

     
}
