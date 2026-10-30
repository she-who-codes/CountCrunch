//TODO: Gemini Code Failed Due to Save Issue...See remarks in Task List
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MilkCapsInventory
{
    public class MilkCapsInventoryManager : MonoBehaviour
    {
        public static MilkCapsInventoryManager Instance { get; private set; }

        [Header("Asset Databases")]
        [Tooltip("Assign all available Slammer Scriptable Objects here.")]
        public List<SlammerData> availableSlammers = new List<SlammerData>();

        [Tooltip("Assign all available MilkCap Prefabs here. Prefab names should match item IDs.")]
        public List<GameObject> availableMilkCapPrefabs = new List<GameObject>();

        // Runtime inventory state (Item ID -> Quantity)
        private Dictionary<string, int> inventory = new Dictionary<string, int>();

        public System.Action OnInventoryUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadInventory();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // --- Core Inventory API ---

        public void AddItem(string itemId, int amount = 1)
        {
            if (inventory.ContainsKey(itemId))
                inventory[itemId] += amount;
            else
                inventory[itemId] = amount;

            SaveInventory();
            OnInventoryUpdated?.Invoke();
        }

        public bool RemoveItem(string itemId, int amount = 1)
        {
            if (!inventory.ContainsKey(itemId) || inventory[itemId] < amount)
                return false;

            inventory[itemId] -= amount;
            if (inventory[itemId] <= 0)
                inventory.Remove(itemId);

            SaveInventory();
            OnInventoryUpdated?.Invoke();
            return true;
        }

        public int GetQuantity(string itemId)
        {
            return inventory.ContainsKey(itemId) ? inventory[itemId] : 0;
        }

        public Dictionary<string, int> GetAllOwnedItems()
        {
            return new Dictionary<string, int>(inventory);
        }

        // --- Save / Load & PlayFab Architecture Implementation ---

        private void SaveInventory()
        {
            // 1. Local Fallback/Primary Save State
            List<InventoryItemData> saveData = new List<InventoryItemData>();
            foreach (var kvp in inventory)
            {
                saveData.Add(new InventoryItemData { itemId = kvp.Key, quantity = kvp.Value });
                Debug.Log(kvp.Key + "   " + kvp.Value);
            }
            string json = JsonUtility.ToJson(new SerializationWrapper<InventoryItemData>(saveData));
            // UNDONE -- FAIL ISSUE #1A: Saving Slammers to "MilkCaps_Inventory" instead of "Slammers_Inventory"
            PlayerPrefs.SetString("MilkCaps_Inventory", json);
            PlayerPrefs.Save();

            // 2. PlayFab Integration Placeholder Implementation
            UpdateInventoryToPlayFab(json);
        }

        private void LoadInventory()
        {
            inventory.Clear();
            //UNDONE -- FAIL ISSUE #1B: Only loading MilkCaps Inventory
            // Check local state first
            if (PlayerPrefs.HasKey("MilkCaps_Inventory"))
            {
                string json = PlayerPrefs.GetString("MilkCaps_Inventory");
                try
                {
                    var wrapper = JsonUtility.FromJson<SerializationWrapper<InventoryItemData>>(json);
                    foreach (var item in wrapper.items)
                    {
                        inventory[item.itemId] = item.quantity;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse local inventory data: {e.Message}");
                }
            }
            else
            {
                // Give starter items if empty for testing tracking
                InitializeDefaultInventory();
            }

            // PlayFab Integration Placeholder Hook
            LoadInventoryFromPlayFab();
        }

        private void InitializeDefaultInventory()
        {
            // Add initial mock items using first found entries if available
            if (availableSlammers.Count > 0) AddItem(availableSlammers[0].slammerName, 1);
            if (availableMilkCapPrefabs.Count > 0) AddItem(availableMilkCapPrefabs[0].name, 5);
        }

        // --- PlayFab Connection Layer Placeholders ---

        private void UpdateInventoryToPlayFab(string jsonInventoryData)
        {
            /* 
             * PLAYFAB MIGRATION CODE:
             * var request = new PlayFab.ClientModels.UpdateUserDataRequest {
             *     Data = new Dictionary<string, string> { { "Inventory", jsonInventoryData } }
             * };
             * PlayFab.PlayFabClientAPI.UpdateUserData(request, 
             *     result => Debug.Log("Successfully synced inventory to PlayFab cloud storage."),
             *     error => Debug.LogError($"PlayFab synchronization failure: {error.GenerateErrorReport()}")
             * );
             */
            Debug.Log("[PlayFab Sandbox Hook] Inventory state would be updated dynamically to User Data Cloud Platform.");
        }

        private void LoadInventoryFromPlayFab()
        {
            /* 
             * PLAYFAB MIGRATION CODE:
             * PlayFab.PlayFabClientAPI.GetUserData(new PlayFab.ClientModels.GetUserDataRequest(), result => {
             *     if (result.Data != null && result.Data.ContainsKey("Inventory")) {
             *         string json = result.Data["Inventory"].Value;
             *         // Parse and override runtime local dictionary...
             *         OnInventoryUpdated?.Invoke();
             *     }
             * }, error => Debug.LogError(error.GenerateErrorReport()));
             */
            Debug.Log("[PlayFab Sandbox Hook] Cloud data fetch invoked. Falling back smoothly to local PlayerPrefs state system.");
        }

        // Utility class to bypass Unity native JsonUtility root array limitations
        [System.Serializable]
        private class SerializationWrapper<T>
        {
            public List<T> items;
            public SerializationWrapper(List<T> items) { this.items = items; }
        }
    }
}
