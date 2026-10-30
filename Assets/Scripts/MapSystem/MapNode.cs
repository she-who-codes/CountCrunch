using System.Collections.Generic;
using UnityEngine;

public enum MapNodeState { Locked, Open, Completed }

public class MapNode : MonoBehaviour
{
    [Header("Node Info")]
    public string levelName;
    public MapNodeState state = MapNodeState.Locked;

    [Header("Connections")]
    // Nodes you can reach directly from this node moving forward/sideways
    public List<MapNode> adjacentNodes = new List<MapNode>();
    public WorldMapManager mapManager;

    // Visual indicator (e.g., a selection ring or highlight)
    [SerializeField] private GameObject selectionVisual;

    [SerializeField] private GameObject unavailableVisualPrefab;
    private GameObject unavailableVisualInstance;
    [SerializeField] private GameObject completedVisualPrefab;
    private GameObject completedVisualInstance;

    // Instead of holding a child GameObject in the hierarchy, reference your Prefab Asset directly
    [SerializeField] private GameObject unlockedStateVisualPrefab;
    private GameObject spawnedUnlockedStateVisualInstance;

    private void Start()
    {
        SetSelected(false);
        UpdateVisuals();
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionVisual != null)
        {
            // selectionVisual.SetActive(isSelected);
            if (isSelected) 
            {
                selectionVisual.transform.position = transform.position;
            }
        }
    }

    // Call this whenever a level's state updates to change its color/sprite
    public void UpdateVisuals()
    {
        if (spawnedUnlockedStateVisualInstance != null) Destroy(spawnedUnlockedStateVisualInstance);

        switch (state)
        {
            case MapNodeState.Open:
                if (unlockedStateVisualPrefab != null)
                {
                    // Spawn the graphic dynamically at runtime directly inside this node anchor
                    spawnedUnlockedStateVisualInstance = Instantiate(unlockedStateVisualPrefab, transform.position, Quaternion.identity, transform);
                    spawnedUnlockedStateVisualInstance.transform.position = new Vector3(transform.position.x, transform.position.y +1, transform.position.z);
                }
                mapManager.SetStartingNode(this);
                break;

            case MapNodeState.Completed:
                // Spawn a different completed-state prefab checkmark if desired
                if (completedVisualPrefab != null)
                {
                    // Spawn the graphic dynamically at runtime directly inside this node anchor
                    completedVisualInstance = Instantiate(completedVisualPrefab, transform.position, Quaternion.identity, transform);
                    completedVisualInstance.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
                }
                break;
            case MapNodeState.Locked:
                // e.g., Gray out sprite
                if (unavailableVisualPrefab != null)
                {
                    // Spawn the graphic dynamically at runtime directly inside this node anchor
                    unavailableVisualInstance = Instantiate(unavailableVisualPrefab, transform.position, Quaternion.identity, transform);
                    unavailableVisualInstance.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
                }
                break;
            
        }
        // Add your custom logic here to change sprites or colors based on state
       
    }
}
