using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GaragaManager : MonoBehaviour
{
    [SerializeField] private AgentManager _agentManager;
    [SerializeField] private StoreManager _StoreManager;
    [SerializeField] private GameObject garageGateWall;
    [SerializeField] private GameObject garageWalls;

    [SerializeField] private GameObject garageUnlockUI;

    [SerializeField] private GameObject heliPad1;
    [SerializeField] private GameObject heliPad2;

    [SerializeField] private GameObject heliPad1UI;
    [SerializeField] private GameObject heliPad2UI;

    [SerializeField] bool isGarageUnlocked;
    void Awake()
    {
       
    }
    void Start()
    {
        _agentManager = GetComponent<AgentManager>();
        _StoreManager = GetComponent<StoreManager>();

        // Load the saved garage unlock state
        isGarageUnlocked = PlayerPrefs.GetInt("GarageUnlocked", 0) == 1;

        if (isGarageUnlocked)
        {
            SetGarageActive();
        }
        else
        {
            SetGarageDeactive();
        }

        // Load the saved heliPad unlock states and set UI prompts accordingly
        UpdateHeliPadUIStates();
    }

    public void ActivateGarageUI()
    {
        if (garageUnlockUI != null)
        {
            garageUnlockUI.SetActive(true); // Activates the garage UI when called
        }
    }

    public void SetGarageActive()
    {
        garageUnlockUI.SetActive(false);
        garageGateWall.SetActive(false);
        garageWalls.SetActive(true);

        // Activate or deactivate heliPads based on their saved states
        ActivateHeliPad("HeliPad1", PlayerPrefs.GetInt("HeliPad1Unlocked", 0) == 1);
        ActivateHeliPad("HeliPad2", PlayerPrefs.GetInt("HeliPad2Unlocked", 0) == 1);
        UpdateHeliPadUIStates();

    }

    public void SetGarageDeactive()
    {
        if (heliPad1.transform.childCount > 0)
            heliPad1.transform.GetChild(0).gameObject.SetActive(false);

        if (heliPad2.transform.childCount > 0)
            heliPad2.transform.GetChild(0).gameObject.SetActive(false);

        garageGateWall.SetActive(true);
        garageWalls.SetActive(false);
    }

    public void UnlockAndActivateHeliPad(string heliPadName)
    {
        if (heliPadName == "HeliPad1")
        {
            PlayerPrefs.SetInt("HeliPad1Unlocked", 1);
            ActivateHeliPad("HeliPad1", true);
            heliPad1UI.SetActive(false);

            // Enable heliPad2 UI only after heliPad1 is unlocked
            if (PlayerPrefs.GetInt("HeliPad2Unlocked", 0) == 0)
            {
                heliPad2UI.SetActive(true);
            }
        }
        else if (heliPadName == "HeliPad2" && PlayerPrefs.GetInt("HeliPad1Unlocked", 0) == 1)
        {
            PlayerPrefs.SetInt("HeliPad2Unlocked", 1);
            ActivateHeliPad("HeliPad2", true);
            heliPad2UI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("HeliPad2 cannot be unlocked before HeliPad1.");
        }
        _agentManager.GetAvailableGarageShelves();
        _agentManager.UpdateActiveTables(_StoreManager.GetActiveTables());
        PlayerPrefs.Save(); // Ensure all PlayerPrefs changes are saved immediately
    }

    private void ActivateHeliPad(string heliPadName, bool activate)
    {
        GameObject heliPad = null;

        // Determine which heliPad to activate based on the name
        if (heliPadName == "HeliPad1")
        {
            heliPad = heliPad1;
        }
        else if (heliPadName == "HeliPad2")
        {
            heliPad = heliPad2;
        }

        if (heliPad != null && heliPad.transform.childCount > 0)
        {
            heliPad.transform.GetChild(0).gameObject.SetActive(activate);
        }
        else
        {
            Debug.LogError($"HeliPad '{heliPadName}' or its child not found in GaragaManager.");
        }
    }

    private void UpdateHeliPadUIStates()
    {
        // Update UI prompts based on saved unlock states
        heliPad1UI.SetActive(PlayerPrefs.GetInt("HeliPad1Unlocked", 0) == 0);
        heliPad2UI.SetActive(PlayerPrefs.GetInt("HeliPad2Unlocked", 0) == 0 && PlayerPrefs.GetInt("HeliPad1Unlocked", 0) == 1);
    }
}
