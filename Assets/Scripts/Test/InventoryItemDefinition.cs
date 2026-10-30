using UnityEngine;

namespace MilkCaps.Inventory
{
    public enum InventoryItemType
    {
        Milkcap = 0,
        Slammer = 1
    }

    /// <summary>
    /// Shared metadata for inventory entries. Milkcap entries reference a prefab;
    /// slammer entries reference a ScriptableObject asset.
    /// </summary>
    public abstract class InventoryItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;

        public string ItemId => itemId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public Sprite Icon => icon;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
                itemId = System.Guid.NewGuid().ToString("N");
        }
#endif
    }
}
