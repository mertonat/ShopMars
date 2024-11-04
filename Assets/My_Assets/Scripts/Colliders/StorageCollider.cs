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
            _StoreShelf.canTableTransfer = false;
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
