using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MilkCapsInventory
{
    public class MilkCapsInventoryUI : MonoBehaviour
    {
        [Header("UI Text Displays")]
        [SerializeField] private TextMeshProUGUI slammersDisplayText;
        [SerializeField] private TextMeshProUGUI milkCapsDisplayText;

        [Header("Optional Panel Testing Suite")]
        [SerializeField] private TMP_InputField debugInputID;
        [SerializeField] private Button debugAddButton;
        [SerializeField] private Button debugRemoveButton;

        private void Start()
        {
            if (MilkCapsInventoryManager.Instance != null)
            {
                MilkCapsInventoryManager.Instance.OnInventoryUpdated += RefreshUI;
                RefreshUI();
            }

            // Setup internal testing controls
            if (debugAddButton != null) debugAddButton.onClick.AddListener(() => ChangeQuantityTest(true));
            if (debugRemoveButton != null) debugRemoveButton.onClick.AddListener(() => ChangeQuantityTest(false));
        }

        private void OnDestroy()
        {
            if (MilkCapsInventoryManager.Instance != null)
            {
                MilkCapsInventoryManager.Instance.OnInventoryUpdated -= RefreshUI;
            }
        }

        public void RefreshUI()
        {
            if (MilkCapsInventoryManager.Instance == null) return;

            var ownedItems = MilkCapsInventoryManager.Instance.GetAllOwnedItems();

            string slammerTextBuffer = "<b>OWNED SLAMMERS:</b>\n";
            string milkCapTextBuffer = "<b>OWNED MILK CAPS:</b>\n";

            int slammerCount = 0;
            int milkCapCount = 0;

            // Process matches against asset types
            foreach (var kvp in ownedItems)
            {
                string itemId = kvp.Key;
                int qty = kvp.Value;

                // Check if it's a registered Slammer Scriptable Object
                SlammerData slammer = MilkCapsInventoryManager.Instance.availableSlammers.Find(s => s.slammerName == itemId);
                if (slammer != null)
                {
                    slammerTextBuffer += $"- {slammer.slammerName} (ID: {itemId}) x{qty}\n";
                    slammerCount++;
                    continue;
                }

                // Check if it exists as an instantiation Prefab tracking structure
                GameObject capPrefab = MilkCapsInventoryManager.Instance.availableMilkCapPrefabs.Find(p => p.name == itemId);
                if (capPrefab != null)
                {
                    milkCapTextBuffer += $"- {capPrefab.name} x{qty}\n";
                    milkCapCount++;
                    continue;
                }

                // Fallback rendering structure for generic items or unassigned data
                milkCapTextBuffer += $"- Unregistered Item [{itemId}] x{qty}\n";
            }

            if (slammerCount == 0) slammerTextBuffer += "<i>No Slammers Unlocked</i>";
            if (milkCapCount == 0) milkCapTextBuffer += "<i>No MilkCaps Collected</i>";

            if (slammersDisplayText != null) slammersDisplayText.text = slammerTextBuffer;
            if (milkCapsDisplayText != null) milkCapsDisplayText.text = milkCapTextBuffer;
        }

        private void ChangeQuantityTest(bool isAdding)
        {
            if (debugInputID == null || string.IsNullOrEmpty(debugInputID.text)) return;

            string id = debugInputID.text;
            if (isAdding)
            {
                MilkCapsInventoryManager.Instance.AddItem(id, 1);
            }
            else
            {
                MilkCapsInventoryManager.Instance.RemoveItem(id, 1);
            }
        }
    }
}
