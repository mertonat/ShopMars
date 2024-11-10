using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StorageCollider : MonoBehaviour
{
    public PlayerStackController _PlayerStackController;
    public StorageShelfController _StoreShelf;

    private Vector3 initialSize;
    private Vector3 targetScale;


    private bool enableScale;
    private bool scalingUp;

    [SerializeField]
    private float scalePercentage = 10f;

    [SerializeField]
    private float lerpSpeed = 100.0f; // Adjusted for smoother scaling

    public string itemName;
    // Start is called before the first frame update
    void Awake()
    {
        initialSize = transform.localScale;
        targetScale = initialSize * (1 + scalePercentage / 100f);
        itemName = gameObject.name;
        _PlayerStackController = GameObject.Find("Player").GetComponent<PlayerStackController>();
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
            isTutorial = true;
            enableScale = true;
            scalingUp = true;

            if (_PlayerStackController.isCarry)
            {
                if (itemName == _PlayerStackController.itemName)
                {
                    _StoreShelf.canTableTransfer = true;
                    _PlayerStackController.itemName = gameObject.name;
                }
                else
                {
                    Debug.Log("Cannot carry a different item while already carrying one.");
                }
            }
            else
            {
                // If not carrying, allow picking up the new item
                _StoreShelf.canTableTransfer = true;
                _PlayerStackController.itemName = gameObject.name;
                _PlayerStackController.isCarry = true; // Mark as carrying an item
            }
        }
    }
    private bool isExiting = false;

    // Method that triggers on exit
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isExiting)
        {
            isExiting = true;
            scalingUp = false;
            _StoreShelf.canTableTransfer = false;

            // Start the delayed check for the player's stack
            StartCoroutine(CheckPlayerStackAfterDelay(0.2f));

            isExiting = false;
        }
    }

    // Coroutine to wait and then check the stack
    private IEnumerator CheckPlayerStackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_PlayerStackController.frontStack.transform.childCount > 0)
        {
            // Get the last item in the player's stack without removing it
            Transform lastChild = _PlayerStackController.frontStack.transform.GetChild(
                _PlayerStackController.frontStack.transform.childCount - 1
            );
            GameObject lastItem = lastChild.gameObject;

            // Check if the item is one of the known types and update itemName
            if (lastItem != null)
            {
                string lastItemName = lastItem.name;

                if (lastItemName.Contains("gearActor"))
                {
                    _PlayerStackController.itemName = "Gear";
                }
                else if (lastItemName.Contains("circuitActor"))
                {
                    _PlayerStackController.itemName = "Circuit";
                }
                else if (lastItemName.Contains("conductiveActor"))
                {
                    _PlayerStackController.itemName = "Conductive";
                }
                else
                {
                    Debug.LogWarning("Unknown item type in player stack.");
                    _PlayerStackController.itemName = ""; // Reset if unknown
                }
            }
        }
        else
        {
            _PlayerStackController.itemName = ""; // No items in stack, reset name
            _PlayerStackController.isCarry = false;
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
