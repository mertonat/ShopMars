using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class StorageShelfController : MonoBehaviour
{
    public PlayerStackController _PlayerStack;
    public GameObject itemsGameobject;
    public GameObject[] items;
    public GameObject currentItem;
    [SerializeField] private GameObject storeItemActor;

    private bool isMovingItem = false;
    public bool canTableTransfer;
    public bool isStorageLoading = false;
    public float moveDuration = 0.5f;
    public int currentMovingIndex;
    public string itemName;

    private Vector3 worldTargetPosition;
    public Transform playerHand;

    public int[] activeItems;   // Stores the indices of active items
    public int[] inActiveItems; // Stores the indices of inactive items
    private int itemsMoved;

    private int loadingMovingIndex = 0;
    private GameObject transferItem;

    private void Start()
    {  
        GetItemsList();
        ItemsCanTransfer(items);
    }

    private void Update()
    {
        if (_PlayerStack.CanCarryMore() && currentMovingIndex < activeItems.Length && !isMovingItem)
        {
            if (canTableTransfer && _PlayerStack.itemName == itemName)
            {
                StartItemTransfer();
            }
        }

        if (isMovingItem && currentItem != null)
        {
            MoveItemToTarget(currentItem, _PlayerStack.addToLastPos(), playerHand.rotation, () =>
            {
                _PlayerStack.IncreaseStack(itemName);
                FinalizeItemTransfer();
            });
        }
    }

    private void StartItemTransfer()
    {
        int itemIndex = activeItems[currentMovingIndex];
        isMovingItem = true;
        currentItem = InstantiateItem(itemIndex);
        currentMovingIndex++;
    }

    private void MoveItemToTarget(GameObject item, Vector3 localTargetPosition, Quaternion targetRotation, System.Action onComplete)
    {
        worldTargetPosition = _PlayerStack.frontStack.transform.TransformPoint(localTargetPosition);

        item.transform.DOMove(worldTargetPosition, moveDuration)
            .OnComplete(() =>
            {
                item.transform.position = worldTargetPosition;
                item.transform.rotation = targetRotation;
                onComplete?.Invoke();
            });

        item.transform.DORotateQuaternion(targetRotation, moveDuration);
    }
    private void MoveItemToTargetStorageAgent(GameObject item, Vector3 worldTargetPosition, Quaternion targetRotation, System.Action onComplete)
    {
        // Directly move to the world position without transforming local to world.
        item.transform.DOMove(worldTargetPosition, moveDuration)
            .OnComplete(() =>
            {
                item.transform.position = worldTargetPosition;
                item.transform.rotation = targetRotation;
                onComplete?.Invoke();
            });

        item.transform.DORotateQuaternion(targetRotation, moveDuration);
    }
    private void FinalizeItemTransfer()
    {
        Destroy(currentItem);
        isMovingItem = false;
        currentItem = null;

        if (currentMovingIndex >= activeItems.Length || currentMovingIndex >= _PlayerStack.maxCarry)
        {
            currentMovingIndex = 0;
            ItemsCanTransfer(items);
        }
    }

    private GameObject InstantiateItem(int itemNumber)
    {
        var itemTransform = items[itemNumber].transform;
        var newItem = Instantiate(storeItemActor, itemTransform.position, itemTransform.rotation);
        items[itemNumber].SetActive(false);
        return newItem;
    }

    private void ItemsCanTransfer(GameObject[] items)
    {
        if (items == null || items.Length == 0)
        {
            Debug.LogWarning("The items array is either null or empty.");
            return;
        }

        List<int> activeIndices = new List<int>();
        List<int> inactiveIndices = new List<int>();

        foreach (var item in items)
        {
            if (CheckIfActive(item))
                activeIndices.Add(System.Array.IndexOf(items, item));
            else
                inactiveIndices.Add(System.Array.IndexOf(items, item));
        }

        activeItems = activeIndices.ToArray();
        inActiveItems = inactiveIndices.ToArray();
    }

    private bool CheckIfActive(GameObject item)
    {
        return item.activeSelf || CheckChildActive(item);
    }

    private bool CheckChildActive(GameObject item)
    {
        for (int i = 0; i < item.transform.childCount; i++)
        {
            if (item.transform.GetChild(i).gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    public void GetItemsList()
    {
        int childCount = itemsGameobject.transform.childCount;
        items = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            items[i] = itemsGameobject.transform.GetChild(i).gameObject;
        }
    }

    #region StorageLoad
    public void LoadStorage(GameObject a)
    {
        if (_PlayerStack == null || inActiveItems == null || inActiveItems.Length == 0 || isMovingItem)
        {
            return;
        }

        if (isStorageLoading && loadingMovingIndex < inActiveItems.Length)
        {
            isMovingItem = true;
            transferItem = InstantiateObject(a);
            loadingMovingIndex++;
        }

        if (isMovingItem && transferItem != null)
        {
            MoveItemToTargetStorageAgent(transferItem, items[inActiveItems[itemsMoved]].transform.position, items[inActiveItems[itemsMoved]].transform.rotation, () =>
            {
                items[inActiveItems[itemsMoved]].SetActive(true);
                FinalizeStorageLoading();
            });
        }
    }

    private void FinalizeStorageLoading()
    {
        Destroy(transferItem);
        isMovingItem = false;
        transferItem = null;
        itemsMoved++;

        if (itemsMoved >= inActiveItems.Length)
        {
            ItemsCanTransfer(items);
            loadingMovingIndex = 0;
            itemsMoved = 0;
            isStorageLoading = false;
        }
    }

    private GameObject InstantiateObject(GameObject obj)
    {
        if (obj == null) return null;

        var itemTransform = obj.transform;
        var newItem = Instantiate(storeItemActor, itemTransform.position, itemTransform.rotation);
        newItem.transform.SetParent(itemsGameobject.transform);
        return newItem;
    }
    #endregion
}
