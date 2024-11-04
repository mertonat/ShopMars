using System.Collections;
using TMPro;
using UnityEngine;

public class ShipCraftManager : MonoBehaviour
{
    [SerializeField] private PlayerStackController _PlayerStack;
    [SerializeField] private ShipItemDropCollider _ShipItemDropCollider;
    [SerializeField] private StoreShelfController _StoreShelfController;
    [SerializeField] private DissolveObject _dissolve;

    [SerializeField] private GameObject shipActorItem; // Ship hologram gameobject for table
    [SerializeField] private GameObject Ship;
    [SerializeField] private GameObject[] electric; // Particle systems for visual effects

    [SerializeField] private TextMeshProUGUI conductiveAmountText;
    [SerializeField] private TextMeshProUGUI circuitAmountText;
    [SerializeField] private TextMeshProUGUI gearAmountText;

    // Define max amounts for each item type
    [SerializeField] private int maxConductiveAmount = 12;
    [SerializeField] private int maxCircuitAmount = 10;
    [SerializeField] private int maxGearAmount = 8;

    [SerializeField] private int conductiveTransferred = 0;
    [SerializeField] private int circuitTransferred = 0;
    [SerializeField] private int gearTransferred = 0;

    private bool isConductiveComplete = false;
    private bool isCircuitComplete = false;
    private bool isGearComplete = false;
    [SerializeField] private bool isCraftProcessReadyDebug; // Set this manually in the Inspector for testing

    public bool IsCraftProcessReady => isCraftProcessReadyDebug || AllItemsCollected();
    private void Start()
    {
        LoadCraftingProgress();
        UpdateUI();
        foreach (var effect in electric)
        {
            if (effect != null)
                effect.SetActive(false);
        }
    }
    private bool isCraftingInProgress = false;

    private void Update()
    {
        // if (IsCraftProcessReady && !isCraftingInProgress)
        // {
        //     Debug.Log("All items are collected. Craft process can now start!");
        //     isCraftingInProgress = true; // Set the flag to true to prevent re-entry
        //     StartCoroutine(ShipCraftSequence());
        // }
    }

    public void UpdateTransferredAmount(string itemName)
    {
        if (isCraftingInProgress) return; // Prevent updates while crafting

        switch (itemName)
        {
            case "Conductive":
                if (conductiveTransferred < maxConductiveAmount) conductiveTransferred++;
                break;
            case "Circuit":
                if (circuitTransferred < maxCircuitAmount) circuitTransferred++;
                break;
            case "Gear":
                if (gearTransferred < maxGearAmount) gearTransferred++;
                break;
            default:
                Debug.LogWarning("Unknown item name: " + itemName);
                break;
        }

        UpdateUI();

        // Check both if all items are collected and if there's an inactive item on the table
        if (IsCraftProcessReady && !isCraftingInProgress && _StoreShelfController != null && _StoreShelfController.inActiveItems.Length > 0)
        {
            Debug.Log("Conditions met for crafting. Starting ShipCraftSequence.");
            StartCoroutine(ShipCraftSequence());
        }
        else if (!IsCraftProcessReady)
        {
            Debug.Log("Not all items collected yet.");
        }
        else if (_StoreShelfController == null || _StoreShelfController.inActiveItems.Length == 0)
        {
            Debug.Log("No inactive items available on the table.");
        }
    }

    private void UpdateUI(int carryingAmount = -1)
    {
        conductiveAmountText.text = $"{conductiveTransferred}/{maxConductiveAmount}";
        circuitAmountText.text = $"{circuitTransferred}/{maxCircuitAmount}";
        gearAmountText.text = $"{gearTransferred}/{maxGearAmount}";

        if (carryingAmount >= 0)
        {
            Debug.Log($"Carrying amount updated in UI: {carryingAmount}");
        }
    }

    private bool AllItemsCollected()
    {
        return conductiveTransferred >= maxConductiveAmount &&
               circuitTransferred >= maxCircuitAmount &&
               gearTransferred >= maxGearAmount;
    }


    private IEnumerator ShipCraftSequence()
    {
        ActivateCompletionEffects(); // Activate ship and effects

        yield return StartCoroutine(WaitForDissolveCompletion()); // Wait for dissolve to complete

        DeactivateShipAndEffects(); // Deactivate ship and effects after dissolve
        AddShipToTable();
        ResetCraftingItems();
        isCraftingInProgress = false; // Reset the flag after crafting is complete
    }

    private void ResetCraftingItems()
    {
        // Reset transferred counts
        conductiveTransferred = 0;
        circuitTransferred = 0;
        gearTransferred = 0;

        // Clear PlayerPrefs data for item counts
        PlayerPrefs.DeleteKey("ConductiveTransferred");
        PlayerPrefs.DeleteKey("CircuitTransferred");
        PlayerPrefs.DeleteKey("GearTransferred");
        PlayerPrefs.Save();

        UpdateUI(); // Refresh UI to reflect reset values
        Debug.Log("Crafting items reset and saved data cleared. Ready for next crafting process.");
    }
    private IEnumerator WaitForDissolveCompletion()
    {
        Debug.Log("Waiting for dissolve to complete...");

        if (_dissolve != null)
        {
            bool dissolveFinished = false;

            // Subscribe to the event
            _dissolve.OnDissolveComplete += () => dissolveFinished = true;

            // Wait until the dissolve is complete
            yield return new WaitUntil(() => dissolveFinished);

            Debug.Log("Dissolve completed.");
        }
        else
        {
            Debug.LogWarning("DissolveObject reference is missing.");
        }
    }
    private void ActivateCompletionEffects()
    {
        if (Ship != null)
            Ship.SetActive(true);

        foreach (var effect in electric)
        {
            if (effect != null)
                effect.SetActive(true);
        }

        Debug.Log("All items collected! Completion effects activated.");

        // Start dissolve effect on the ship
        if (_dissolve != null)
        {
            _dissolve.StartCoroutine("AdjustHeightAndNoiseScaleOverTime");
        }
    }


    private void DeactivateShipAndEffects()
    {
        if (Ship != null)
            Ship.SetActive(false);

        foreach (var effect in electric)
        {
            if (effect != null)
                effect.SetActive(false);
        }

        Debug.Log("Dissolve complete. Ship and effects deactivated.");
    }
    private void AddShipToTable()
    {
        if (_StoreShelfController != null)
        {
            _StoreShelfController.gameObject.SetActive(true); // Activate the StoreShelfController
            _StoreShelfController.ItemsListUpdate(_StoreShelfController.items); // Update items to refresh active and inactive lists

            // Check for currently active items
            if (_StoreShelfController.inActiveItems.Length > 0)
            {
                // Get the first inactive item
                int firstInactiveIndex = _StoreShelfController.inActiveItems[0];
                GameObject firstInactiveItem = _StoreShelfController.items[firstInactiveIndex];

                // Activate and add the first inactive item
                firstInactiveItem.SetActive(true);
                _StoreShelfController.ItemsListUpdate(_StoreShelfController.items); // Update the shelf with the newly activated item
                Debug.Log($"First inactive item added to the table: {firstInactiveItem.name}");
            }
            else
            {
                // If no inactive items, log a message
                Debug.Log("No inactive items to add.");
            }
        }
        else
        {
            Debug.LogWarning("StoreShelfController reference is missing.");
        }
    }

    public int GetRequiredAmount(string itemName)
    {
        switch (itemName)
        {
            case "Conductive":
                return maxConductiveAmount - conductiveTransferred;
            case "Circuit":
                return maxCircuitAmount - circuitTransferred;
            case "Gear":
                return maxGearAmount - gearTransferred;
            default:
                Debug.LogWarning("Unknown item name: " + itemName);
                return 0;
        }
    }
    private void SaveCraftingProgress()
    {
        PlayerPrefs.SetInt("ConductiveTransferred", conductiveTransferred);
        PlayerPrefs.SetInt("CircuitTransferred", circuitTransferred);
        PlayerPrefs.SetInt("GearTransferred", gearTransferred);
        PlayerPrefs.Save();
        Debug.Log("Crafting progress saved.");
    }

    private void LoadCraftingProgress()
    {
        conductiveTransferred = PlayerPrefs.GetInt("ConductiveTransferred", 0);
        circuitTransferred = PlayerPrefs.GetInt("CircuitTransferred", 0);
        gearTransferred = PlayerPrefs.GetInt("GearTransferred", 0);
        Debug.Log("Crafting progress loaded.");
    }
    private void OnApplicationQuit()
    {
        SaveCraftingProgress();
    }
}
