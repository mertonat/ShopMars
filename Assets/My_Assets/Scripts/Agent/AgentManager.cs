using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField] private StoreManager _StoreManager;
    [SerializeField] private List<GameObject> GarageShelfs;
    [SerializeField] private List<GameObject> activeShelfs;
    [SerializeField] private List<GameObject> casePay;

    [SerializeField] private GameObject busStopSpawnPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private GameObject firstCase;
    [SerializeField] private GameObject[] agentPrefab;
    [SerializeField] private int maxAgents; // Dynamically updated
    private List<int> availableNumbers = new List<int>(); // Track available numbers
    private int nextAgentNumber = 1; // Start with agent number 1 


    public List<Agent> agents = new List<Agent>();

    private void Awake()
    {

        _StoreManager = GetComponent<StoreManager>();
    }

    private void Start()
    {
        StartCoroutine(SpawnAgentsRoutine());

    }

    private void Update()
    {
        // Continuously update max agents based on active shelves
        UpdateMaxAgents();

    }

    #region AgentSpawn-Region
    private float spawnInterval = 2f;

    private IEnumerator SpawnAgentsRoutine()
    {
        while (true)
        {
            if (agents.Count < maxAgents)
            {
                SpawnAgent();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SpawnAgent()
    {
        if (agents.Count < maxAgents)
        {
            // Select a random agent prefab
            GameObject randomAgentPrefab = agentPrefab[UnityEngine.Random.Range(0, agentPrefab.Length)];

            // Instantiate the selected random agent prefab
            GameObject newAgent = Instantiate(randomAgentPrefab, busStopSpawnPoint.transform.position, Quaternion.identity);
            newAgent.SetActive(true);

            // Assign a unique name based on available numbers
            int agentNumber = GetAvailableAgentNumber();
            newAgent.name = "Agent" + agentNumber;

            Agent agentComponent = newAgent.GetComponent<Agent>();
            agentComponent.exitPosition = exitPoint;
            agentComponent._AgentManager = this;

            // Assign a random shelf to the agent
            AssignRandomShelfToAgent(agentComponent);

            // Assign the first case if available
            if (casePay.Count > 0)
            {
                agentComponent._CaseManager = casePay[0].GetComponent<CaseManager>();
            }

            agents.Add(agentComponent);
            Debug.Log($"Spawned agent: {newAgent.name}. Total agents: {agents.Count}");
        }
        else
        {
            Debug.Log("Maximum number of agents reached. Cannot spawn more.");
        }
    }
    private int GetAvailableAgentNumber()
    {
        if (availableNumbers.Count > 0)
        {
            // Reuse a number from the available pool
            int numberToReuse = availableNumbers[0];
            availableNumbers.RemoveAt(0);
            return numberToReuse;
        }
        else
        {
            // Assign a new number if no numbers are available for reuse
            return nextAgentNumber++;
        }
    }

    public List<StoreShelfController> availableShelves = new List<StoreShelfController>();

    public void AssignRandomShelfToAgent(Agent agent)
    {

        // Filter out shelves that are both active and not full (max queued agents)
        foreach (var shelf in activeShelfs)
        {
            StoreShelfController shelfController = shelf.GetComponent<StoreShelfController>();

            // Ensure the shelf is active, enabled, and has available space for more agents
            if (shelfController != null && shelfController.isActiveAndEnabled &&
                shelfController.currentQueueIndex.Count < shelfController.maxQueueSize)
            {
                availableShelves.Add(shelfController);
            }
        }

        // Now, check if there are any available shelves left
        if (availableShelves.Count > 0)
        {
            // Randomly select a shelf from the available ones
            int randomIndex = UnityEngine.Random.Range(0, availableShelves.Count);
            agent._StoreShelf = availableShelves[randomIndex];

            // Ensure that the agent's shelf has been assigned before they move to it
            if (agent._StoreShelf != null)
            {
                Debug.Log($"Assigned shelf {agent._StoreShelf.name} to agent {agent.name}.");
            }
            else
            {
                Debug.LogError($"Failed to assign shelf to agent {agent.name}");
            }
        }
        else
        {
            Debug.LogWarning("No available shelves to assign to agent (all shelves are full or inactive).");
        }
    }
    public void AssignRandomGarageShelfToAgent(Agent agent)
    {
        // Get available garage shelves based on unlock status
        List<GameObject> availableGarageShelves = GetAvailableGarageShelves();

        // Ensure there are available shelves to assign
        if (availableGarageShelves.Count > 0)
        {
            // Randomly select a shelf from the available ones
            int randomIndex = UnityEngine.Random.Range(0, availableGarageShelves.Count);
            GameObject selectedShelf = availableGarageShelves[randomIndex];

            // Assign the shelf to the agent's _StoreShelf (or you could create a separate property if needed)
            agent._StoreShelf = selectedShelf.GetComponent<StoreShelfController>();

            // Ensure that the agent's shelf has been assigned before they move to it
            if (agent._StoreShelf != null)
            {
                Debug.Log($"Assigned garage shelf {agent._StoreShelf.name} to agent {agent.name}.");
            }
            else
            {
                Debug.LogError($"Failed to assign garage shelf to agent {agent.name}");
            }
        }
        else
        {
            Debug.LogWarning("No available garage shelves to assign to agent (all shelves are locked or inactive).");
        }
    }

    public void RemoveAgent(Agent agent)
    {
        if (agents.Contains(agent))
        {
            string agentName = agent.name;
            if (agentName.StartsWith("Agent"))
            {
                string numberPart = agentName.Substring(5);
                if (int.TryParse(numberPart, out int agentNumber))
                {
                    availableNumbers.Add(agentNumber);
                }
            }

            agents.Remove(agent);
            Destroy(agent.gameObject);
            Debug.Log($"Agent {agent.name} has exited the store. Total agents: {agents.Count}");

            if (agents.Count < maxAgents)
            {
                SpawnAgent();
            }
        }
    }
    #endregion
    [SerializeField] List<GameObject> availableGarageShelves;
    #region TableIndex
    public void UpdateActiveTables(List<GameObject> newActiveTables)
    {
        activeShelfs.Clear();
        availableShelves.Clear(); // Keep availableShelves in sync with activeShelfs
        casePay.Clear();

        if (firstCase != null)
        {
            casePay.Add(firstCase);
            Debug.Log($"Added {firstCase.name} as the first case in casePay.");
        }

        foreach (GameObject table in newActiveTables)
        {
            if (table != null)
            {
                GameObject parentObject = table.transform.parent?.gameObject;
                if (parentObject != null)
                {
                    // Add to activeShelfs only if not already present
                    if (!activeShelfs.Contains(parentObject))
                    {
                        activeShelfs.Add(parentObject);
                        Debug.Log($"Added parent {parentObject.name} of {table.name} to activeShelfs.");
                    }

                    // Get the StoreShelfController from the parentObject and add it to availableShelves
                    StoreShelfController shelfController = parentObject.GetComponent<StoreShelfController>();
                    if (shelfController != null && !availableShelves.Contains(shelfController))
                    {
                        availableShelves.Add(shelfController);
                        Debug.Log($"Added shelf controller {parentObject.name} to availableShelves.");
                    }
                }
            }
        }

        foreach (GameObject table in newActiveTables)
        {
            if (table != null && table.name.Contains("case", StringComparison.OrdinalIgnoreCase))
            {
                GameObject parentObject = table.transform.parent?.gameObject;
                if (parentObject != null && parentObject != firstCase)
                {
                    if (!casePay.Contains(parentObject))
                    {
                        casePay.Add(parentObject);
                        Debug.Log($"Added parent {parentObject.name} of {table.name} to casePay.");
                    }
                }
            }
        }
        availableGarageShelves = GetAvailableGarageShelves();
        foreach (var garageShelf in availableGarageShelves)
        {
            // Add garage shelves to activeShelfs if they are not already present
            if (!activeShelfs.Contains(garageShelf))
            {
                activeShelfs.Add(garageShelf);
                Debug.Log($"Added {garageShelf.name} to activeShelfs.");
            }

            // Add to availableShelves if the StoreShelfController is available and not already added
            StoreShelfController shelfController = garageShelf.GetComponent<StoreShelfController>();
            if (shelfController != null && !availableShelves.Contains(shelfController))
            {
                availableShelves.Add(shelfController);
                Debug.Log($"Added garage shelf controller {garageShelf.name} to availableShelves.");
            }
        }

        Debug.Log("Active tables updated. Total active shelves: " + activeShelfs.Count);
        Debug.Log("Total active payment cases: " + casePay.Count);

        // Update max agents whenever active tables are updated
        UpdateMaxAgents();
    }
    private void UpdateMaxAgents()
    {
        case1 = casePay[0].transform.GetComponent<CaseManager>();
        // Check for the number of active shelves
        if (activeShelfs.Count == 1)
        {
            maxAgents = 3; // Max 3 agents for 1 active shelf
        }
        else if (activeShelfs.Count >= 2)
        {
            maxAgents = 5; // Max 5 agents for 2 active shelves

            // Increase max agents to 9 if there are 2 cases available
            if (casePay.Count == 2)
            {
                maxAgents = 9; // Max 9 agents for 2 active shelves if there are 2 cases
                Debug.Log($"Increased max agents to: {maxAgents} because there are 2 cases in casePay.");
                case2 = casePay[1].transform.GetComponent<CaseManager>();
            }
        }

        Debug.Log($"Max agents updated to: {maxAgents} based on {activeShelfs.Count} active shelves and {casePay.Count} cases.");
    }
    #endregion

    #region Case
    public void AssignCaseWithQueueSpace(Agent agent)
    {
        availableCase = GetRandomAvailableCase();
        if (availableCase != null)
        {
            agent._CaseManager = availableCase;
            Debug.Log($"Assigned case {availableCase.name} to agent {agent.name}.");
        }
        else
        {
            Debug.LogWarning("No available case with space in queue for agent.");
        }
    }
    [SerializeField] CaseManager case1;
    [SerializeField] CaseManager case2;
    [SerializeField] CaseManager availableCase;
    private CaseManager GetRandomAvailableCase()
    {
        if (casePay.Count < 2)
        {
            Debug.LogWarning("Less than two cases available. Ensure both cases are unlocked.");
            return null;
        }

        if (case1 == null || case2 == null)
        {
            Debug.LogError("One or both CaseManagers are null. Check the case objects.");
            return null;
        }

        // Log the current queue sizes
        Debug.Log($"Case1 Queue Size: {case1.currentQueueIndexCase.Count}/{case1.maxQueueSize}, Case2 Queue Size: {case2.currentQueueIndexCase.Count}/{case2.maxQueueSize}");

        // Get the valid queue sizes (non-null elements)
        int case1ValidQueueSize = case1.GetValidQueueSize();
        int case2ValidQueueSize = case2.GetValidQueueSize();

        // Log the valid queue sizes
        Debug.Log($"Case1 Valid Queue Size: {case1ValidQueueSize}/{case1.maxQueueSize}, Case2 Valid Queue Size: {case2ValidQueueSize}/{case2.maxQueueSize}");

        // Rule 1: If both cases have space, randomly assign an agent to either case
        if (case1ValidQueueSize < case1.maxQueueSize && case2ValidQueueSize < case2.maxQueueSize)
        {
            int randomChoice = UnityEngine.Random.Range(0, 2); // Randomly choose between case1 and case2
            Debug.Log($"Both cases have space. Randomly assigning agent to {(randomChoice == 0 ? "Case1" : "Case2")}.");
            return (randomChoice == 0) ? case1 : case2;
        }

        // Rule 2: If case1 is full (based on valid elements), assign the agent to case2
        if (case1ValidQueueSize >= case1.maxQueueSize)
        {
            if (case2ValidQueueSize < case2.maxQueueSize)
            {
                Debug.Log("Case1 is full. Assigning agent to Case2.");
                return case2;
            }
        }

        // Rule 3: If case2 is full (based on valid elements), assign the agent to case1
        if (case2ValidQueueSize >= case2.maxQueueSize)
        {
            if (case1ValidQueueSize < case1.maxQueueSize)
            {
                Debug.Log("Case2 is full. Assigning agent to Case1.");
                return case1;
            }
        }

        // Rule 4: If both queues are full, no cases are available
        Debug.LogWarning("Both case queues are full. No available case for the agent.");
        return null;
    }


    #endregion

    public List<GameObject> GetAvailableGarageShelves()
    {
        availableGarageShelves = new List<GameObject>();

        // Check if HeliPad1 and HeliPad2 are unlocked
        bool pad1Unlocked = PlayerPrefs.GetInt("HeliPad1Unlocked", 0) == 1;
        bool pad2Unlocked = PlayerPrefs.GetInt("HeliPad2Unlocked", 0) == 1;

        foreach (var garageShelf in GarageShelfs)
        {
            string shelfName = garageShelf.name;

            // Add only HeliPad1 if it's unlocked
            if (pad1Unlocked && shelfName == "HeliPad1")
            {
                availableGarageShelves.Add(garageShelf);
                Debug.Log("Added HeliPad1 shelf to availableGarageShelves.");
            }

            // Add HeliPad2 only if both HeliPad1 and HeliPad2 are unlocked
            if (pad2Unlocked && shelfName == "HeliPad2" && pad1Unlocked)
            {
                availableGarageShelves.Add(garageShelf);
                Debug.Log("Added HeliPad2 shelf to availableGarageShelves.");
            }
        }

        return availableGarageShelves;
    }
}
