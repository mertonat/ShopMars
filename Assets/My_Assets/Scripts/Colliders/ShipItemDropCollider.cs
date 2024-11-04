using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ShipItemDropCollider : MonoBehaviour
{
    [SerializeField] private PlayerStackController _PlayerStackController;
    [SerializeField] private ShipCraftManager _ShipCraftManager;

    public GameObject currentItem;
    [SerializeField] private List<GameObject> itemsToTransfer = new List<GameObject>();
    public float moveDuration = 0.1f;
    public bool isItemTransfer = false;
    public int itemsMoved = 0;
    public int itemsRemovedFromPlayer = 0;
    [SerializeField] private bool isMovingItem = false;
    //public Transform shipItemPosition;
    [SerializeField] private Transform conductiveItem;
    [SerializeField] private Transform gearItem;
    [SerializeField] private Transform circuitItem;


    void Start()
    {

    }

    void Update()
    {
        if (isItemTransfer && itemsToTransfer.Count > 0 && !isMovingItem)
        {
            MoveNextItemToPos();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && _PlayerStackController.isCarry)
        {
            string itemName = _PlayerStackController.itemName;
            int requiredAmount = GetRequiredAmountForItem(itemName);

            // Check if the required amount is already met
            if (requiredAmount <= 0)
            {
                Debug.Log("No more items needed for " + itemName);
                isItemTransfer = false;
                return;
            }

            isItemTransfer = true;

            // Populate itemsToTransfer from _PlayerStackController.frontStack in reverse order
            itemsToTransfer = new List<GameObject>();
            for (int i = _PlayerStackController.frontStack.transform.childCount - 1; i >= 0; i--)
            {
                itemsToTransfer.Add(_PlayerStackController.frontStack.transform.GetChild(i).gameObject);
            }
            Debug.Log("Items to transfer count: " + itemsToTransfer.Count);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isItemTransfer = false;
        }
    }

    private void MoveNextItemToPos()
    {
        if (itemsToTransfer.Count == 0)
        {
            Debug.LogWarning("No items to move.");
            return;
        }

        string itemName = _PlayerStackController.itemName;
        int requiredAmount = GetRequiredAmountForItem(itemName);
        int maxTransferable = Mathf.Min(itemsToTransfer.Count, requiredAmount);

        if (maxTransferable <= 0)
        {
            Debug.Log("No more items required for crafting.");
            isItemTransfer = false;
            return;
        }

        isMovingItem = true;
        currentItem = ItemInstantiate(itemsToTransfer[0]);  // Instantiate the first item in the list

        Debug.Log("Moving item to bin: " + currentItem.name);
        MoveItemToPos(currentItem);
    }

    private void MoveItemToPos(GameObject selectedItem)
    {
        Vector3 worldTargetPosition = GetTargetPosition(_PlayerStackController.itemName);

        selectedItem.transform.DOMove(worldTargetPosition, moveDuration)
            .OnComplete(() =>
            {
                Destroy(selectedItem);
                Destroy(itemsToTransfer[0]);
                _ShipCraftManager.UpdateTransferredAmount(_PlayerStackController.itemName);
                _PlayerStackController.carryingAmount--;
                itemsRemovedFromPlayer++;
                itemsMoved++;

                itemsToTransfer.RemoveAt(0);
                isMovingItem = false;
                currentItem = null;

                if (itemsToTransfer.Count == 0)
                {
                    Debug.Log("All items have been moved. Resetting.");
                    itemsMoved = 0;
                    _PlayerStackController.isCarry = false;
                    _PlayerStackController.PlayerCarryAnimation(false);
                    _PlayerStackController.itemName = "";
                    itemsRemovedFromPlayer = 0;
                }
            });
    }

    private Vector3 GetTargetPosition(string itemName)
    {
        switch (itemName)
        {
            case "Circuit":
                return circuitItem ? circuitItem.position : Vector3.zero;
            case "Gear":
                return gearItem ? gearItem.position : Vector3.zero;
            case "Conductive":
                return conductiveItem ? conductiveItem.position : Vector3.zero;
            default:
                Debug.LogWarning("Unknown item name or unassigned transform: " + itemName);
                return Vector3.zero;
        }
    }

    private int GetRequiredAmountForItem(string itemName)
    {
        return _ShipCraftManager.GetRequiredAmount(itemName);
    }

    private GameObject ItemInstantiate(GameObject item)
    {
        var newItem = Instantiate(item, item.transform.position, item.transform.rotation);
        newItem.transform.localScale = item.transform.localScale;
        item.SetActive(false);
        return newItem;
    }

}
