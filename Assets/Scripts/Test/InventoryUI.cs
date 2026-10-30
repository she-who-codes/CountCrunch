using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MilkCaps.Inventory
{
    /// <summary>
    /// Minimal uGUI inventory display. Assign a row prefab containing an Image,
    /// TMP_Text (or legacy Text), and optionally a quantity label.
    /// </summary>
    public sealed class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameObject itemRowPrefab;
        [SerializeField] private TMP_Text statusText;

        private void OnEnable()
        {
            if (inventory != null)
                inventory.InventoryChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (inventory != null)
                inventory.InventoryChanged -= Refresh;
        }

        public void Refresh()
        {
            if (contentRoot == null || itemRowPrefab == null || inventory == null)
            {
                if (statusText != null) statusText.text = "Inventory UI is not configured.";
                return;
            }

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);

            int displayed = 0;
            foreach (InventoryStackSaveData stack in inventory.GetAllStacks())
            {
                InventoryItemType type = (InventoryItemType)stack.itemType;
                Sprite icon = null;
                string displayName = stack.itemId;

                if (type == InventoryItemType.Slammer &&
                    inventory.TryGetSlammer(stack.itemId, out SlammerData slammer))
                {
                    icon = slammer.slammerSprite;
                    displayName = slammer.name;
                }
                else if (type == InventoryItemType.Milkcap &&
                         inventory.TryGetMilkcap(stack.itemId, out MilkcapDefinition milkcap))
                {
                    icon = milkcap.Icon;
                    displayName = milkcap.DisplayName;
                }
                else
                {
                    continue;
                }

                GameObject row = Instantiate(itemRowPrefab, contentRoot);
                Image image = row.GetComponentInChildren<Image>();
                if (image != null)
                {
                    image.sprite = icon;
                    image.enabled = icon != null;
                    image.preserveAspect = true;
                }

                TMP_Text[] labels = row.GetComponentsInChildren<TMP_Text>(true);
                if (labels.Length > 0)
                    labels[0].text = displayName;
                if (labels.Length > 1)
                    labels[1].text = "x" + stack.quantity;

                // If a row uses legacy UnityEngine.UI.Text instead of TMP:
                Text[] legacyLabels = row.GetComponentsInChildren<Text>(true);
                if (labels.Length == 0 && legacyLabels.Length > 0)
                    legacyLabels[0].text = displayName;
                if (labels.Length == 0 && legacyLabels.Length > 1)
                    legacyLabels[1].text = "x" + stack.quantity;

                displayed++;
            }

            if (statusText != null)
                statusText.text = displayed == 0 ? "Your inventory is empty." : "Inventory";
        }
    }
}
