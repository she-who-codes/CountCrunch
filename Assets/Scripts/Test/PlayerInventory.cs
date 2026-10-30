using System;
using System.Collections.Generic;
using UnityEngine;

namespace MilkCaps.Inventory
{
    /// <summary>
    /// Runtime inventory. Stores item IDs and quantities, not Unity object
    /// references, so save data can be serialized. Configure the catalog in
    /// the Inspector before using item definitions.
    /// </summary>
    public sealed class PlayerInventory : MonoBehaviour
    {
        [Serializable]
        private sealed class CatalogEntry
        {
            public InventoryItemType type;
            public SlammerData slammer;
            public MilkcapDefinition milkcapPrefab;
        }

        [SerializeField] private List<CatalogEntry> catalog = new List<CatalogEntry>();
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool saveOnChange = true;

        private readonly Dictionary<string, SlammerData> slammers =
            new Dictionary<string, SlammerData>();
        private readonly Dictionary<string, MilkcapDefinition> milkcaps =
            new Dictionary<string, MilkcapDefinition>();
        private readonly Dictionary<string, InventoryStackSaveData> quantities =
            new Dictionary<string, InventoryStackSaveData>();

        public event Action InventoryChanged;

        private const string SaveKey = "MilkCaps.Inventory.v1";

        private void Awake()
        {
            BuildCatalog();
            if (loadOnStart)
                LoadLocal();
        }

        private void BuildCatalog()
        {
            slammers.Clear();
            milkcaps.Clear();

            foreach (CatalogEntry entry in catalog)
            {
                if (entry == null) continue;

                if (entry.type == InventoryItemType.Slammer)
                {
                    if (entry.slammer == null || string.IsNullOrWhiteSpace(entry.slammer.name))
                    {
                        Debug.LogWarning("PlayerInventory: Missing slammer or item ID in catalog.", this);
                        continue;
                    }

                    if (!slammers.ContainsKey(entry.slammer.name))
                        slammers.Add(entry.slammer.name, entry.slammer);
                }
                else
                {
                    if (entry.milkcapPrefab == null || string.IsNullOrWhiteSpace(entry.milkcapPrefab.ItemId))
                    {
                        Debug.LogWarning("PlayerInventory: Missing milkcap prefab or item ID in catalog.", this);
                        continue;
                    }

                    // Milkcap prefab metadata is held by the prefab's component.
                    if (!milkcaps.ContainsKey(entry.milkcapPrefab.ItemId))
                        milkcaps.Add(entry.milkcapPrefab.ItemId, entry.milkcapPrefab);
                }
            }
        }

        public bool Add(InventoryItemDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0 || !slammers.ContainsKey(item.ItemId))
                return false;

            return ChangeQuantity(item.ItemId, InventoryItemType.Slammer, amount);
        }

        public bool Add(MilkcapDefinition item, int amount = 1)
        {
            if (item == null || amount <= 0 || !milkcaps.ContainsKey(item.ItemId))
                return false;

            return ChangeQuantity(item.ItemId, InventoryItemType.Milkcap, amount);
        }

        public bool Remove(InventoryItemDefinition item, int amount = 1)
        {
            return item != null && RemoveById(item.ItemId, InventoryItemType.Slammer, amount);
        }

        public bool Remove(MilkcapDefinition item, int amount = 1)
        {
            return item != null && RemoveById(item.ItemId, InventoryItemType.Milkcap, amount);
        }

        public bool Has(string itemId, InventoryItemType type, int amount = 1)
        {
            return amount >= 0 &&
                   quantities.TryGetValue(Key(itemId, type), out InventoryStackSaveData stack) &&
                   stack.quantity >= amount;
        }

        public int GetQuantity(string itemId, InventoryItemType type)
        {
            return quantities.TryGetValue(Key(itemId, type), out InventoryStackSaveData stack)
                ? stack.quantity : 0;
        }

        public List<InventoryStackSaveData> GetAllStacks()
        {
            var result = new List<InventoryStackSaveData>();
            foreach (InventoryStackSaveData stack in quantities.Values)
            {
                if (stack.quantity > 0)
                    result.Add(new InventoryStackSaveData
                    {
                        itemId = stack.itemId,
                        itemType = stack.itemType,
                        quantity = stack.quantity
                    });
            }
            return result;
        }

        public bool TryGetSlammer(string itemId, out SlammerData item) =>
            slammers.TryGetValue(itemId, out item);

        public bool TryGetMilkcap(string itemId, out MilkcapDefinition item) =>
            milkcaps.TryGetValue(itemId, out item);

        public void SaveLocal()
        {
            var data = new InventorySaveData { items = GetAllStacks() };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public void LoadLocal()
        {
            quantities.Clear();

            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey, string.Empty);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);
                        if (data != null && data.items != null)
                        {
                            foreach (InventoryStackSaveData saved in data.items)
                            {
                                if (saved == null || saved.quantity <= 0 || string.IsNullOrWhiteSpace(saved.itemId))
                                    continue;

                                InventoryItemType type = (InventoryItemType)saved.itemType;
                                bool known = type == InventoryItemType.Slammer
                                    ? slammers.ContainsKey(saved.itemId)
                                    : type == InventoryItemType.Milkcap && milkcaps.ContainsKey(saved.itemId);

                                // Ignore save entries no longer present in the catalog.
                                if (!known) continue;

                                quantities[Key(saved.itemId, type)] = new InventoryStackSaveData
                                {
                                    itemId = saved.itemId,
                                    itemType = (int)type,
                                    quantity = saved.quantity
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("PlayerInventory: Could not parse local inventory save: " + ex.Message, this);
                    }
                }
            }

            InventoryChanged?.Invoke();
        }

        /// <summary>
        /// Placeholder integration point. Replace with PlayFab UserData/Inventory
        /// calls after adding the PlayFab SDK and deciding the server data schema.
        /// Do not treat client PlayerPrefs as authoritative for paid/premium items.
        /// </summary>
        public void SaveToPlayFabPlaceholder()
        {
            // TODO: Serialize GetAllStacks() and send through PlayFab Client API.
            Debug.Log("PlayFab inventory save placeholder: integrate PlayFab here.", this);
        }

        private bool ChangeQuantity(string itemId, InventoryItemType type, int delta)
        {
            string key = Key(itemId, type);
            if (!quantities.TryGetValue(key, out InventoryStackSaveData stack))
            {
                stack = new InventoryStackSaveData
                {
                    itemId = itemId,
                    itemType = (int)type,
                    quantity = 0
                };
                quantities.Add(key, stack);
            }

            checked { stack.quantity += delta; }
            if (saveOnChange) SaveLocal();
            InventoryChanged?.Invoke();
            return true;
        }

        private bool RemoveById(string itemId, InventoryItemType type, int amount)
        {
            if (amount <= 0) return false;

            string key = Key(itemId, type);
            if (!quantities.TryGetValue(key, out InventoryStackSaveData stack) || stack.quantity < amount)
                return false;

            stack.quantity -= amount;
            if (stack.quantity == 0)
                quantities.Remove(key);

            if (saveOnChange) SaveLocal();
            InventoryChanged?.Invoke();
            return true;
        }

        private static string Key(string itemId, InventoryItemType type) => ((int)type) + ":" + itemId;
    }
}
