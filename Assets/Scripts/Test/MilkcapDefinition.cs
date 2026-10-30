using UnityEngine;

namespace MilkCaps.Inventory
{
    /// <summary>
    /// Attach this component to each milkcap prefab. The prefab itself remains
    /// the source asset; this component supplies a stable ID and UI metadata.
    /// </summary>
    public sealed class MilkcapDefinition : MonoBehaviour
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;

        public string ItemId => itemId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;
        public Sprite Icon => icon;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
                itemId = System.Guid.NewGuid().ToString("N");
        }
#endif
    }
}
