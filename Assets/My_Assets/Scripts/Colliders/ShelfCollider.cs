using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfCollider : MonoBehaviour
{
    public PlayerStackController _PlayerStackController;
    public StoreShelfController _StoreShelfController;
    private Vector3 initialSize;
    private Vector3 targetScale;
    private bool enableScale;
    private bool scalingUp;

    [SerializeField]
    private float scalePercentage = 10f;

    [SerializeField]
    private float lerpSpeed = 100.0f; // Adjusted for smoother scaling

    [SerializeField] string itemName;
    // Start is called before the first frame update
    void Awake()
    {
        initialSize = transform.localScale;
        targetScale = initialSize * (1 + scalePercentage / 100f);
        Debug.Log("Initial Size: " + initialSize);
        _PlayerStackController = GameObject.FindWithTag("Player").GetComponent<PlayerStackController>();
        itemName = gameObject.name;
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            enableScale = true;
            scalingUp = true;

            // Check if the player is carrying an item
            if (_PlayerStackController.isCarry)
            {
                // If carrying, only allow the transfer if the item is the same as the one already carried
                if (itemName == _PlayerStackController.itemName)
                {
                    // Allow transfer to store shelf
                    _StoreShelfController.canTableTransfer = true;
                    isTutorial = true;
                    _PlayerStackController.itemName = gameObject.name;
                }
                else
                {
                    // Do not allow picking up a new item while carrying a different one
                    Debug.Log("Cannot carry a different item while already carrying one.");
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Player is still inside the BoxCollider.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            scalingUp = false;
            _StoreShelfController.canTableTransfer = false;
            _StoreShelfController.currentMovingIndex = 0;
            _StoreShelfController.ItemsListUpdate(_StoreShelfController.items);
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
    bool isTutorial;
    public bool IsUnlocked()
    {
        return isTutorial;
    }

}
