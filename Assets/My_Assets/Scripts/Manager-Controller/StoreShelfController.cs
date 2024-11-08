using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class StoreShelfController : MonoBehaviour
{
    public PlayerStackController _PlayerStack;

    public GameObject itemsGameobject;
    public GameObject[] items;
    public GameObject currentItem;

    public float moveDuration = 0.1f;

    public int[] activeItems;
    public int[] inActiveItems;
    private int itemsMoved = 0;
    public int currentMovingIndex;

    public bool canTableTransfer;
    public bool processShipTableItems;

    [SerializeField] public bool isMovingItem = false;

    public Transform inactivePosition;
    public string itemName;

    public GameObject StoreActor;

    void Start()
    {
        // PlayerPrefs.DeleteAll();
        GetItemsList();
        ItemsListUpdate(items);
        PopulateQueuePositions();

    }

    void Update()
    {
        if (_PlayerStack == null)
        {
            // Debug.LogError("_PlayerStack is not assigned.");
            return;
        }

        // Ensure inActiveItems is properly initialized
        if (inActiveItems == null || inActiveItems.Length == 0)
        {

            //Debug.LogError("inActiveItems is not initialized or empty.");
            return;
        }
        if (isTransferInProgress)
        {
            return;
        }

        // Only proceed if the player can carry more and an item isn't already moving
        if (currentMovingIndex < _PlayerStack.maxCarry && currentMovingIndex < inActiveItems.Length && !isMovingItem)
        {
            int itemIndex = inActiveItems[currentMovingIndex];
            if (canTableTransfer && _PlayerStack.itemName == itemName)
            {
                //Debug.Log("is item moving: " + isMovingItem);
                isMovingItem = true;
                currentMovingIndex++;

                // Instantiate item from player stack
                currentItem = ItemInstantiate(_PlayerStack.LastItemStack());

                if (currentItem == null)
                {
                    //Debug.LogError("currentItem is null after instantiation.");
                    return;
                }
            }
        }

        // If an item is currently being moved, continue moving it
        if (isMovingItem && currentItem != null)
        {
            MoveItemToInactive(currentItem);
        }
    }

    private void MoveItemToInactive(GameObject selectedItem)
    {
        if (itemsMoved >= inActiveItems.Length)
        {
            // Reset moving status and index if all items are moved
            isMovingItem = false;
            currentItem = null;
            ItemsListUpdate(items);
            currentMovingIndex = 0;
            return;
        }

        int countActive = inActiveItems.Length;
        if (countActive > 0)
        {
            inactivePosition = items[inActiveItems[itemsMoved]].transform;
            Vector3 worldTargetPosition = inactivePosition.position;
            float moveSpeed = 1.0f / moveDuration;

            selectedItem.transform.position = Vector3.Lerp(selectedItem.transform.position, worldTargetPosition, moveSpeed * Time.deltaTime);
            selectedItem.transform.rotation = Quaternion.Slerp(selectedItem.transform.rotation, inactivePosition.rotation, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(selectedItem.transform.position, worldTargetPosition) < 0.35f)
            {
                selectedItem.transform.position = worldTargetPosition;
                selectedItem.transform.rotation = inactivePosition.rotation;
                isMovingItem = false;
                Destroy(selectedItem);
                items[inActiveItems[itemsMoved]].SetActive(true);
                currentItem = null;
                itemsMoved++;
            }
        }
        else
        {
            Debug.Log("No active items to move.");
        }

        // Reset item if player carries none or all inactive items are moved
        if (_PlayerStack.carryingAmount == 0 || itemsMoved >= inActiveItems.Length)
        {
            ItemsListUpdate(items);
            currentMovingIndex = 0;
            itemsMoved = 0;
            _PlayerStack.PlayerCarryAnimation(false);

            if (_PlayerStack.carryingAmount == 0)
            {
                _PlayerStack.itemName = "";
                _PlayerStack.isCarry = false;
                _PlayerStack.PlayerCarryAnimation(false);
            }
        }
    }

    public void ItemsListUpdate(GameObject[] items)
    {
        if (items == null || items.Length == 0)
        {
            Debug.LogWarning("The items array is either null or empty.");
            return;
        }

        List<int> activeIndices = new List<int>();
        List<int> inactiveIndices = new List<int>();

        // Loop through the items array to recalculate active and inactive items
        for (int i = 0; i < items.Length; i++)
        {
            bool isActive = CheckIfActive(items[i]);

            if (isActive)
            {
                activeIndices.Add(i);
            }
            else
            {
                inactiveIndices.Add(i);
            }
        }

        // Update the global lists with the new calculated values
        activeItems = activeIndices.ToArray();
        inActiveItems = inactiveIndices.ToArray();

        // Check ShipTable items for special condition only if processShipTableItems is true
        if (processShipTableItems)
        {
            HandleShipTableItems();
        }
    }

    private void HandleShipTableItems()
    {
        // Assuming ShipTable is a specific GameObject that contains items
        GameObject shipTable = itemsGameobject; // Or however you define the ShipTable
        List<int> activeIndices = new List<int>();
        List<int> inactiveIndices = new List<int>();

        // Iterate through the children of the ShipTable
        for (int i = 0; i < shipTable.transform.childCount; i++)
        {
            Transform item = shipTable.transform.GetChild(i);

            if (item.gameObject.activeSelf)
            {
                // If the item is active, add its index to activeIndices
                activeIndices.Add(i);

                // Check and activate its inactive children
                for (int j = 0; j < item.childCount; j++)
                {
                    Transform child = item.GetChild(j);
                    if (!child.gameObject.activeSelf)
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                // If the item is inactive, add its index to inactiveIndices
                inactiveIndices.Add(i);
            }
        }

        // Update global lists with new values
        activeItems = activeIndices.ToArray();
        inActiveItems = inactiveIndices.ToArray();

        Debug.Log("Active Items: " + string.Join(", ", activeItems));
        Debug.Log("Inactive Items: " + string.Join(", ", inActiveItems));
    }
    private bool CheckIfActive(GameObject item)
    {
        if (item.activeSelf)
            return true;

        // Check if any child of the item is active
        for (int i = 0; i < item.transform.childCount; i++)
        {
            if (item.transform.GetChild(i).gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }


    private void GetItemsList()
    {
        int childCount = itemsGameobject.transform.childCount;
        items = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            items[i] = itemsGameobject.transform.GetChild(i).gameObject;
            //Debug.Log("Child GameObject: " + items[i].name + " has been added to the items array.");
        }
    }

    private GameObject ItemInstantiate(GameObject item)
    {
        if (item == null)
        {
            Debug.LogWarning("ItemInstantiate: The 'item' parameter is null.");
            isMovingItem = false;
            return null;
        }

        var itemTransform = item.gameObject.transform;
        var newItem = Instantiate(item, itemTransform.position, itemTransform.rotation);

        newItem.transform.SetParent(itemsGameobject.transform);
        newItem.transform.localScale = itemTransform.localScale;

        item.SetActive(false);

        return newItem;
    }

    #region Agent
    [SerializeField] private GameObject heldItem;
    public float moveSpeedAgent = 0.1f;
    public bool isTransferInProgress = false;

    public void TransferToAgent(Transform handPoint)
    {
        // Check if there is an agent in the queue and it has an Agent component
        if (currentQueueIndex[0] == null || currentQueueIndex[0].GetComponent<Agent>() == null)
        {
            Debug.LogWarning("No agent at the front of the queue to transfer item to.");
            return;
        }

        Agent agentAtFront = currentQueueIndex[0].GetComponent<Agent>();

        // Check if there is an active item to be transferred
        if (activeItems.Length == 0 || activeItems[activeItems.Length - 1] < 0)
        {
            Debug.LogWarning("No active item to transfer.");
            return;
        }

        if (heldItem == null)
        {
            int lastActiveIndex = activeItems[activeItems.Length - 1];
            heldItem = Instantiate(StoreActor, items[lastActiveIndex].transform.position, items[lastActiveIndex].transform.rotation);
            heldItem.name = itemName;
            items[lastActiveIndex].SetActive(false);
            ItemsListUpdate(items);
            isTransferInProgress = true;

            // Parent to agent's hand and move item to the target using DoTween
            heldItem.transform.SetParent(handPoint); // Attach to hand immediately

            // Move item smoothly to the hand position with DoTween
            heldItem.transform.DOMove(handPoint.position, moveSpeedAgent)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    // Finalize the transfer to the agent's hand
                    heldItem.transform.localPosition = Vector3.zero;
                    heldItem.transform.localRotation = Quaternion.identity;

                    RemoveAgentFromQueue(agentAtFront);
                    UpdateQueueAfterPick();
                    heldItem = null;
                    isTransferInProgress = false;
                });

            // Rotate item smoothly to the hand rotation with DoTween
            heldItem.transform.DORotateQuaternion(handPoint.rotation, moveSpeedAgent)
                .SetEase(Ease.Linear);
        }
    }

    // Remove the agent from the queue after they pick the item
    private void RemoveAgentFromQueue(Agent agent)
    {
        if (currentQueueIndex.Contains(agent.gameObject))
        {
            currentQueueIndex.Remove(agent.gameObject);
            currentQueueIndex.Add(null); // Maintain the list size to prevent shifting

            Debug.Log($"Agent {agent.name} removed from queue.");
        }
        else
        {
            Debug.LogWarning($"Agent {agent.name} was not found in the queue.");
        }
    }

    private GameObject Spawn()
    {
        GameObject lastItem = items[activeItems.Length - 1];
        lastItem = Instantiate(StoreActor, lastItem.transform, lastItem.transform);
        items[activeItems.Length - 1].SetActive(false);
        return lastItem;
    }

    public int maxQueueSize = 3;
    [SerializeField] GameObject queuePosHolder;
    [SerializeField] public Transform[] agentQueuePos;
    public List<GameObject> currentQueueIndex;
    public void PopulateQueuePositions()
    {

        if (queuePosHolder == null)
        {
            Debug.LogWarning("queuePosHolder has not been assigned in the inspector.");
            return;
        }

        Transform[] children = queuePosHolder.GetComponentsInChildren<Transform>();
        agentQueuePos = new Transform[children.Length - 1];

        int index = 0;
        foreach (Transform child in children)
        {
            if (child != queuePosHolder.transform)
            {
                agentQueuePos[index] = child;
                index++;
            }
        }

        maxQueueSize = agentQueuePos.Length;
        currentQueueIndex = new List<GameObject>(new GameObject[maxQueueSize]);

        Debug.Log("Queue positions populated: " + agentQueuePos.Length);
    }

    public List<GameObject> tempQueue = new List<GameObject>();
    private void UpdateQueueAfterPick()
    {
        if (currentQueueIndex.Count == 0)
        {
            Debug.LogWarning("Queue is empty, no agents to update.");
            return;
        }

        tempQueue.Clear();
        tempQueue.AddRange(currentQueueIndex);  // Maintain agents in original order

        // Clear and reassign agents to correct queue positions
        currentQueueIndex.Clear();

        for (int i = 0; i < tempQueue.Count; i++)
        {
            currentQueueIndex.Add(tempQueue[i]);
            if (currentQueueIndex[i] != null)
            {
                Agent agentScript = currentQueueIndex[i].GetComponent<Agent>();
                if (agentScript != null)
                {
                    // Assign updated target position
                    agentScript.targetPos = agentQueuePos[i];
                    agentScript.MoveToQueueTable(agentQueuePos[i].position);
                }
                else
                {
                    Debug.LogWarning($"Agent script not found on object at index {i}.");
                }
            }
        }

        // Fill remaining positions in queue with null if fewer agents
        while (currentQueueIndex.Count < agentQueuePos.Length)
        {
            currentQueueIndex.Add(null);
        }

        Debug.Log($"Queue updated after agent picked an item: {string.Join(", ", currentQueueIndex)}");
    }
    #endregion
}
