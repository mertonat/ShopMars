using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinCollider : MonoBehaviour
{
    public PlayerStackController _PlayerStackController;
    private Vector3 initialSize;
    private Vector3 targetScale;
    private bool enableScale;
    private bool scalingUp;

    [SerializeField]
    private float scalePercentage = 10f;
    [SerializeField]
    private float lerpSpeed = 100.0f; // Adjusted for smoother scaling


    // Start is called before the first frame update
    void Awake()
    {
        initialSize = transform.localScale;
        targetScale = initialSize * (1 + scalePercentage / 100f);
        Debug.Log("Initial Size: " + initialSize);
        _PlayerStackController = GameObject.FindWithTag("Player").GetComponent<PlayerStackController>();
    }

    void Update()
    {
        if (enableScale)
        {
            if (scalingUp)
            {
                ScaleObject(targetScale);
            }
            else
            {
                ScaleObject(initialSize);
            }
        }
        if (isBin && _PlayerStackController.carryingAmount > 0 && !isMovingItem)
        {
            MoveNextItemToBin();
        }

        if (isMovingItem && currentItem != null)
        {
            MoveItemToBin(currentItem);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            enableScale = true;
            scalingUp = true;

            if (_PlayerStackController.isCarry)
            {
                isBin = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            scalingUp = false;
            isBin = false;
        }
    }

    private void ScaleObject(Vector3 target)
    {
        transform.localScale = Vector3.Lerp(transform.localScale, target, lerpSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.localScale, target) < 0.01f)
        {
            transform.localScale = target;
        }
    }

    public GameObject currentItem;
    public float moveDuration = 0.1f;
    public bool isBin = false;
    public int itemsMoved = 0;
    [SerializeField] private bool isMovingItem = false;
    public Transform binPosition;
    private void MoveNextItemToBin()
    {
        isMovingItem = true;
        currentItem = ItemInstantiate(_PlayerStackController.LastItemStack());

        if (currentItem == null)
        {
            Debug.LogError("currentItem is null after instantiation.");
            return;
        }

        Debug.Log("Moving item to bin: " + currentItem.name);
    }
    private void MoveItemToBin(GameObject selectedItem)
    {

        Vector3 worldTargetPosition = binPosition.position;
        float moveSpeed = 1.0f / moveDuration;


        selectedItem.transform.position = Vector3.Lerp(selectedItem.transform.position, worldTargetPosition, moveSpeed * Time.deltaTime);


        if (Vector3.Distance(selectedItem.transform.position, worldTargetPosition) < 0.35f)
        {

            selectedItem.transform.position = worldTargetPosition;
            Destroy(selectedItem);


            _PlayerStackController.RemoveLastItem();

            isMovingItem = false;
            currentItem = null;
            itemsMoved++;
            Debug.Log("Item successfully moved to bin.");
        }
        if (_PlayerStackController.carryingAmount == 0)
        {
            Debug.Log("Resetting after all items have been moved or carrying amount is 0.");
            itemsMoved = 0; // Reset the itemsMoved counter
            _PlayerStackController.isCarry = false;
            _PlayerStackController.PlayerCarryAnimation(false);
            _PlayerStackController.itemName="";
        }
    }

    private GameObject ItemInstantiate(GameObject item)
    {
        if (item == null)
        {
            Debug.LogWarning("ItemInstantiate: The 'item' parameter is null.");
            return null;
        }

        var itemTransform = item.transform;
        var newItem = Instantiate(item, itemTransform.position, itemTransform.rotation);
        newItem.transform.localScale = itemTransform.localScale;

        item.SetActive(false);

        return newItem;
    }
}
