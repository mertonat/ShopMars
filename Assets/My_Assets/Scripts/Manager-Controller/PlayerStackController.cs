using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerStackController : MonoBehaviour
{
    [SerializeField] private BaseCharControl _BaseCharController;

    public int maxCarry;
    public int carryingAmount;
    public bool isCarry;
    public bool isBin;
    public string itemName = "";

    [SerializeField] public GameObject frontStack;
    [SerializeField] private GameObject circuitActor;
    [SerializeField] private GameObject gearActor;
    [SerializeField] private GameObject conductiveActor;
    //[SerializeField] private GameObject[] carryingItems;

    [SerializeField] private float circuitYOffset = 0.12f;
    [SerializeField] private float gearYOffset = 0.135f;
    [SerializeField] private float conductiveYOffset = 0.25f;
    public Vector3 lastStackPosition;


    void Start()
    {
        _BaseCharController = GameObject.FindWithTag("Player").GetComponent<BaseCharControl>();
        lastStackPosition = new Vector3(0, 0, 0);
        carryingAmount = 0;
        itemName = "";
    }

    public void IncreaseStack(string typeOfObject)
    {
        //Debug.Log("Last Pos: " + GetLastPos());
        _BaseCharController.PlayerCarryAnimation(true);
        if (carryingAmount >= maxCarry)
        {
            return;
        }

        // Get object and Y offset for the type
        float yOffset;
        GameObject objectToStack = GetObjectToStack(typeOfObject, out yOffset);

        if (objectToStack == null)
        {
            //Debug.LogWarning($"Unknown object type: {typeOfObject}");
            return;
        }

        Vector3 newPosition = lastStackPosition;
        Transform lastChild = GetLastChildTransform(frontStack.transform);
        if (lastChild != null)
        {
            newPosition.y += yOffset;
        }
        else
        {
            newPosition.y = 0;
        }

        GameObject newStackedObject = Instantiate(objectToStack, newPosition, Quaternion.identity);
        newStackedObject.transform.SetParent(frontStack.transform);
        newStackedObject.transform.localPosition = newPosition;
        newStackedObject.transform.localRotation = Quaternion.identity;

        lastStackPosition = newPosition;
        carryingAmount++;

        Debug.Log($"Stacked {typeOfObject}, now carrying {carryingAmount} items.");

        if (isBin)
        {

        }

    }

    public GameObject LastItemStack()
    {
        Transform lastChild = GetLastChildTransform(frontStack.transform);
        if (lastChild != null)
        {
            GameObject lastItem = lastChild.gameObject;

            // Update lastStackPosition before removing the item
            lastStackPosition = (frontStack.transform.childCount > 1)
                ? frontStack.transform.GetChild(frontStack.transform.childCount - 2).localPosition
                : Vector3.zero;

            carryingAmount--;

            // Destroy the last item in the stack
            Destroy(lastItem);

            Debug.Log($"Removed {lastItem.name} from stack. Now carrying {carryingAmount} items.");

            return lastItem;
        }
        else
        {
            Debug.LogWarning("No items to remove from the stack.");
            _BaseCharController.PlayerCarryAnimation(false);
            return null; // No item to remove, return null
        }
    }
    private GameObject GetObjectToStack(string typeOfObject, out float yOffset)
    {
        yOffset = 0; // Default Y offset

        switch (typeOfObject)
        {
            case "Circuit":
                yOffset = circuitYOffset;
                return circuitActor;
            case "Gear":
                yOffset = gearYOffset;
                return gearActor;
            case "Conductive":
                yOffset = conductiveYOffset;
                return conductiveActor;
            default:
                return null;
        }
    }

    public Vector3 addToLastPos()
    {
        Transform lastChild = GetLastChildTransform(frontStack.transform);
        if (lastChild != null)
        {
            float itemHeight = lastChild.transform.localPosition.y;
            return lastChild.localPosition + new Vector3(0, itemHeight, 0);
        }
        return new Vector3(0, 0, 0);
    }

    private Transform GetLastChildTransform(Transform parent)
    {
        return parent.childCount > 0 ? parent.GetChild(parent.childCount - 1) : null;
    }
    public bool CanCarryMore()
    {
        return carryingAmount < maxCarry;
    }

    public void PlayerCarryAnimation(bool iscarry)
    {
        _BaseCharController.PlayerCarryAnimation(isCarry);
    }

    public GameObject RemoveLastItem()
    {
        Transform lastChild = GetLastChildTransform(frontStack.transform);
        if (lastChild != null)
        {
            GameObject lastItem = lastChild.gameObject;

            // Update lastStackPosition before removing the item
            lastStackPosition = (frontStack.transform.childCount > 1)
                ? frontStack.transform.GetChild(frontStack.transform.childCount - 2).localPosition
                : Vector3.zero;

            carryingAmount--;

            // Destroy the last item in the stack
            Destroy(lastItem);

            Debug.Log($"Removed {lastItem.name} from stack. Now carrying {carryingAmount} items.");

            return lastItem;
        }
        else
        {
            Debug.LogWarning("No items to remove from the stack.");
            _BaseCharController.PlayerCarryAnimation(false);
            return null; // No item to remove, return null
        }
    }

}


