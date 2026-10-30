using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorldMapManager : MonoBehaviour
{
    public MapNode currentNode;

    [Header("Input Actions")]
    [SerializeField] private PlayerInput playerInput;
    // Make sure your Input Action Asset has a "Navigate" Vector2 action (WASD/Left Stick)
    // and a "Submit" Button action (Space/A Button)

    private Vector2 navigationInput;

    private void Start()
    {
        // Ensure your Input Action map is active
        playerInput.currentActionMap.Enable();
    }

    public void SetStartingNode(MapNode setNode) 
    {
        if (setNode != null)
        {
            currentNode = setNode;
            currentNode.SetSelected(true);
        }
    }
    // Hook this to your Navigate action's "Performed" and "Canceled" events via PlayerInput component
    public void OnNavigate(InputAction.CallbackContext context)
    {
        navigationInput = context.ReadValue<Vector2>();

        // Only trigger movement when the stick/key is pushed past a threshold, not held continuously
        if (context.performed && navigationInput.magnitude > 0.5f)
        {
            TryMoveSelection(navigationInput);
        }
    }

    // Hook this to your Submit action's "Performed" event
    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.performed && currentNode != null)
        {
            if (currentNode.state == MapNodeState.Open)
            {
                Debug.Log($"Entering level: {currentNode.levelName}");
                // Load your level scene here
            }
            else
            {
                Debug.Log("Cannot enter a locked or completed level!");
            }
        }
    }

    private void TryMoveSelection(Vector2 direction)
    {
        MapNode bestChoice = null;
        float highestDot = -1f;

        // Vector3 conversion to compare positions in 2D space
        Vector3 inputDir = new Vector3(direction.x, direction.y, 0).normalized;
        Debug.Log(inputDir);
        foreach (MapNode neighbor in currentNode.adjacentNodes)
        {
            // Rule 1 & 2: Cannot select Locked nodes or Completed nodes
            if (neighbor.state == MapNodeState.Locked)// || neighbor.state == MapNodeState.Completed)
                continue;

            // Rule 3: Cannot select points too far up the map 
            // (Handled implicitly by only checking immediate 'adjacentNodes' connections)

            // Determine directional intent using Vector math (Dot Product)
            Vector3 dirToNeighbor = (neighbor.transform.position - currentNode.transform.position).normalized;
            float dotProduct = Vector3.Dot(inputDir, dirToNeighbor);

            // Check if the neighbor lies roughly in the direction the player pushed
            if (dotProduct > 0.5f) // Roughly a 60-degree cone
            {
                if (dotProduct > highestDot)
                {
                    highestDot = dotProduct;
                    bestChoice = neighbor;
                }
            }
        }

        // If a valid open node is found in that direction, swap selection
        if (bestChoice != null)
        {
            currentNode.SetSelected(false);
            currentNode = bestChoice;
            currentNode.SetSelected(true);
        }
    }

    // Call this method whenever the player beats a level
    public void UnlockNextNodes(MapNode finishedNode)
    {
        finishedNode.state = MapNodeState.Completed;
        finishedNode.UpdateVisuals();

        foreach (MapNode neighbor in finishedNode.adjacentNodes)
        {
            // If the adjacent node was locked, it is now accessible
            if (neighbor.state == MapNodeState.Locked)
            {
                neighbor.state = MapNodeState.Open;
                neighbor.UpdateVisuals();
            }
        }
    }
}
