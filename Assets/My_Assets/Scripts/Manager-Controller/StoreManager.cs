using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    [SerializeField] private StorageManager _StorageManager;
    [SerializeField] private AgentManager _AgentManager;
    [SerializeField] private GaragaManager _GaragaManager;

    [SerializeField] private GameObject[] shelfs;
    [SerializeField] public GameObject[] tables;
    [SerializeField] private GameObject[] uiColliders;
    [SerializeField] public bool[] count;
    private const string TutorialCompleteKey = "TutorialComplete";

    void Awake()
    {
        //PlayerPrefs.DeleteAll();
        // Check if the StorageManager component exists on this GameObject
        _StorageManager = GetComponent<StorageManager>();
        _AgentManager = GetComponent<AgentManager>();
        _GaragaManager = GetComponent<GaragaManager>();
        isTutorialComplete = PlayerPrefs.GetInt(TutorialCompleteKey, 0) == 1;
        if (_StorageManager == null)
        {
            Debug.LogError("StorageManager component is not attached to the GameObject.");
            return;
        }

        count = new bool[shelfs.Length];
        tables = new GameObject[shelfs.Length];
        uiColliders = new GameObject[shelfs.Length];
        //PlayerPrefs.SetInt("table_0", 1);

        for (int i = 0; i < shelfs.Length; i++)
        {
            if (shelfs[i] != null && shelfs[i].transform.childCount > 0)
            {
                tables[i] = shelfs[i].transform.GetChild(0).gameObject;
            }
            else
            {
                tables[i] = null;
            }

            if (shelfs[i] != null && shelfs[i].transform.childCount > 1)
            {
                uiColliders[i] = shelfs[i].transform.GetChild(1).gameObject;
            }
            else
            {
                uiColliders[i] = null;
            }

            string tableKey = "table_" + i;
            if (PlayerPrefs.GetInt(tableKey, 0) == 1)
            {
                if (tables[i] != null)
                {
                    tables[i].SetActive(true);
                }
                if (uiColliders[i] != null)
                {
                    uiColliders[i].SetActive(false);
                }
            }
            else
            {
                if (tables[i] != null)
                {
                    tables[i].SetActive(false);
                }
            }
        }
    }

    void Start()
    {
        LoadShelfState();
        if (_StorageManager != null)
        {
            _StorageManager.StorageShelfUnlock();
        }
        // Update AgentManager with the new list of active tables
        if (_AgentManager != null)
        {
            _AgentManager.UpdateActiveTables(GetActiveTables());
        }

    }
    void Update()
    {

    }

    public void Unlocked(String name)
    {
        // Only unlock shelves if tutorial is complete or if shelf 0
        for (int i = 0; i < shelfs.Length; i++)
        {
            if (shelfs[i].name == name)
            {
                string tableKey = "table_" + i;
                PlayerPrefs.SetInt(tableKey, true ? 1 : 0);
                PlayerPrefs.Save();
                LoadShelfState();
                _StorageManager.StorageShelfUnlock();
            }
        }
        // Update AgentManager with the new list of active tables
        if (_AgentManager != null)
        {
            _AgentManager.UpdateActiveTables(GetActiveTables());
        }
    }

    public void LoadShelfState()
    {
        for (int i = 0; i < shelfs.Length; i++)
        {
            string tableKey = "table_" + i;
            count[i] = PlayerPrefs.GetInt(tableKey, 0) == 1;

            // Special condition: if table 3 (shelf index 3) is unlocked, activate the garage UI
            bool isGarageUnlocked = PlayerPrefs.GetInt("GarageUnlocked", 0) == 1;
            if (i == 3 && count[i] && _GaragaManager != null&&!isGarageUnlocked)
            {
                _GaragaManager.ActivateGarageUI(); // Calls the method to activate the garage UI
            }
        }
        DisableUIObject();
    }
    public void DisableUIObject()
    {
        // Check if shelf 0 is unlocked
        if (tables[0] != null && tables[0].activeSelf)
        {
            // Disable UI Collider for shelf 0 when it is unlocked
            if (uiColliders[0] != null)
            {
                uiColliders[0].SetActive(false);
            }
        }

        // If the tutorial is not complete, do not activate any other UI colliders
        if (!isTutorialComplete)
        {
            // Ensure all UI colliders are disabled except for shelf 0
            for (int i = 1; i < uiColliders.Length; i++)
            {
                if (uiColliders[i] != null)
                {
                    uiColliders[i].SetActive(false); // Disable all other UI colliders
                }
            }
            return; // Exit the method early since we do not want to activate any UI
        }
        int lastActiveTableIndex = -1;

        for (int i = 0; i < tables.Length; i++)
        {
            if (tables[i] != null && tables[i].activeSelf)
            {
                lastActiveTableIndex = i;
            }
        }
        if (lastActiveTableIndex == -1)
        {

            if (uiColliders.Length > 1 && uiColliders[1] != null)
            {
                uiColliders[1].SetActive(true);
                //Debug.Log("No active tables. Activated UI Collider 1.");
            }
        }
        else
        {
            int nextIndex = lastActiveTableIndex + 1;
            if (nextIndex < uiColliders.Length && uiColliders[nextIndex] != null)
            {
                uiColliders[nextIndex].SetActive(true);
                //Debug.Log($"Activated UI Collider {nextIndex} as the next of active table {lastActiveTableIndex}.");
            }
        }
        for (int i = 0; i < uiColliders.Length; i++)
        {
            if (uiColliders[i] != null && i != lastActiveTableIndex + 1)
            {
                uiColliders[i].SetActive(false);
                //Debug.Log($"Disabled UI Collider {i}.");
            }
        }
    }
    public List<GameObject> GetActiveTables()
    {
        List<GameObject> activeTables = new List<GameObject>();

        // Get the list of active tables
        for (int i = 0; i < tables.Length; i++)
        {
            if (tables[i] != null && tables[i].activeSelf)
            {
                activeTables.Add(tables[i]);
            }
        }

        return activeTables; // Return the list of active tables
    }
    [SerializeField] private bool isTutorialComplete = false;
    public void SetTutorialComplete()
    {
        isTutorialComplete = true;
        LoadShelfState(); // Re-check all shelf states once tutorial is done
    }
}