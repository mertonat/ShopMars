using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public StoreShelfController _StoreShelf;
    public CaseManager _CaseManager;
    public AgentManager _AgentManager; // Reference to the AgentManager
    public Transform targetPos;
    public Transform exitPosition;
    public Transform frontStake;
    private GameObject heldItem;
    private NavMeshAgent agent;

    public bool isAgentPaying;

    public enum AgentState { MovingToShelf, WaitingInQueueForItem, MovingToCase, WaitingInQueue, Exit }

    public Animator agentAnima;
    [SerializeField] private AgentState currentState;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agentAnima = GetComponent<Animator>();
        currentState = AgentState.MovingToShelf;
        MoveToShelf();

    }
    public bool read = false;
    private void Update()
    {
        if (targetPos == null)
        {
            ReassignShelf();
        }
        switch (currentState)
        {
            case AgentState.MovingToShelf:
                if (ReachedDestination())
                {
                    agentAnima.SetBool("Walking", false);
                    currentState = AgentState.WaitingInQueueForItem;
                    StartCoroutine(WaitUntilTableFilled());
                }
                else
                {
                    agentAnima.SetBool("Walking", true);
                    agentAnima.SetBool("Carry", false);
                }
                break;

            case AgentState.MovingToCase:
                if (ReachedDestination())
                {
                    currentState = AgentState.WaitingInQueue;
                    agentAnima.SetBool("Carry", true);
                    agentAnima.SetBool("Walking", false);
                    // Now check if it's the agent's turn to pay
                    StartCoroutine(WaitForTurnToPay());
                }
                else
                {
                    agentAnima.SetBool("Carry", true);
                    agentAnima.SetBool("Walking", true);
                }
                break;

            case AgentState.Exit:
                // Logic to exit the store
                //ExitStore();
                break;
        }
    }
    public void ReassignShelf()
    {
        if (_AgentManager != null)
        {
            _AgentManager.AssignRandomShelfToAgent(this); // Call the new ReassignShelf method in AgentManager
            TableQueueCheck();
        }
    }
    public void MoveToShelf()
    {
        currentState = AgentState.MovingToShelf;
        TableQueueCheck();
    }
    private IEnumerator WaitUntilTableFilled()
    {
        while (_StoreShelf.activeItems.Length == 0)
        {
            yield return null;
        }

        PickItem();
    }

    private void PickItem()
    {
        // Check if the agent is at the front of the queue
        if (_StoreShelf.currentQueueIndex.Count > 0 && _StoreShelf.currentQueueIndex[0] == this.gameObject)
        {
            if (_StoreShelf.activeItems.Length > 0)
            {
                StartCoroutine(WaitUntilItemPick());
            }
            else
            {
                Debug.Log("No active items available to pick.");
            }
        }
        else
        {
            Debug.LogWarning($"Agent {agent.name} is not at the front of the queue.");
        }
    }
    private IEnumerator WaitUntilItemPick()
    {
        _StoreShelf.TransferToAgent(frontStake);
        agentAnima.SetBool("Walking", false);
        agentAnima.SetBool("Carry", true);
        // Wait until the table is filled
        while (_StoreShelf.isTransferInProgress)
        {
            _StoreShelf.TransferToAgent(frontStake);
            yield return null;
        }
        heldItem = frontStake.transform.GetChild(0).gameObject;
        if (heldItem.name.Contains("Ship"))
        {
            print("---------Ship");

            // Reset local rotation first
            heldItem.transform.localRotation = Quaternion.identity;

            // Apply the specific rotation you want
            heldItem.transform.localRotation = Quaternion.Euler(-90, 90, 0);

            // Ensure scale is as expected
            heldItem.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        }
        agentAnima.SetBool("Walking", true);
        currentState = AgentState.MovingToCase;
        MoveToCase();
    }

    private void MoveToCase()
    {
        _AgentManager.AssignCaseWithQueueSpace(this);
        CaseQueueCheck();
    }
    private IEnumerator WaitForTurnToPay()
    {

        while (_CaseManager.currentQueueIndexCase[0] != this.gameObject)
        {
            // Keep waiting until it's this agent's turn
            yield return null;
        }

        // Now the agent is at the front of the queue. Wait for player to enable payment.
        Debug.Log($"Agent {name} is at the front of the queue, waiting for player to trigger payment...");

        // Wait until the player triggers the payment (via collider or other means)
        while (!_CaseManager.isPayment)
        {
            yield return null;  // Keep waiting for the player to enable payment
        }

        // Once payment is enabled by the player, proceed with the payment process
        PayForItem();
    }
    private void PayForItem()
    {

        StartCoroutine(WaitForPayment());
        Debug.Log("Starting payment process...");

    }


    private IEnumerator WaitForPayment()
    {
        while (!isAgentPaying)
        {
            yield return null;
        }

        agentAnima.SetBool("Carry", false);

        while (isAgentPaying)
        {
            if (heldItem != null)
            {
                yield return StartCoroutine(_CaseManager.AgentItemToPos(heldItem.transform));
            }

            yield return null;
        }

    }

    public void MoveToExit()
    {
        agentAnima.SetBool("Carry", true);
        agentAnima.SetBool("Walking", true);
        currentState = AgentState.Exit;
        Debug.Log($"{name} is moving to exit.");
        agent.SetDestination(exitPosition.position);
        StartCoroutine(CheckIfReachedExit());
    }

    private IEnumerator CheckIfReachedExit()
    {
        while (!ReachedExitDestination())
        {
            yield return null; // Wait until the agent reaches the destination
        }

        AgentManager agentManager = FindObjectOfType<AgentManager>();
        agentManager.RemoveAgent(this); // Remove the agent from the manager
    }
    private bool ReachedExitDestination()
    {
        // Check if the agent is placed on a valid NavMesh and is active
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= 0.6f)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Agent is not placed on a NavMesh or not active.");
        }

        return false;
    }
    private bool ReachedDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= 0.6f)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    #region TableQueueManager
    public void TableQueueCheck()
    {
        // Loop through the currentQueueIndex to find an empty slot
        for (int i = 0; i < _StoreShelf.currentQueueIndex.Count; i++)
        {
            if (_StoreShelf.currentQueueIndex[i] == null) // Check for an empty slot
            {
                // Assign this agent's GameObject to the empty slot
                _StoreShelf.currentQueueIndex[i] = this.gameObject;

                // Instead of setting targetPos to currentQueueIndex[i].transform, use the queue position directly
                targetPos = _StoreShelf.agentQueuePos[i];
                // Move the agent to the corresponding queue position
                MoveToQueueTable(targetPos.position);
                Debug.Log($"Agent {name} added to queue at index {i} and moved to position {targetPos.position}.");

                return; // Exit once the agent is assigned
            }
        }
        Debug.LogWarning("No available slots in the queue.");
    }

    // Method to move the agent using NavMeshAgent
    public void MoveToQueueTable(Vector3 position)
    {
        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            // Set the destination directly without modifying targetPos
            navMeshAgent.SetDestination(position);
            currentState = AgentState.MovingToShelf;
            Debug.Log($"Agent {name} is moving to {position}");
        }
        else
        {
            Debug.LogError("NavMeshAgent not found on this agent.");
        }
    }
    public void CaseQueueCheck()
    {
        // Loop through the currentQueueIndex to find an empty slot
        for (int i = 0; i < _CaseManager.currentQueueIndexCase.Count; i++)
        {
            if (_CaseManager.currentQueueIndexCase[i] == null) // Check for an empty slot
            {
                // Assign this agent's GameObject to the empty slot
                _CaseManager.currentQueueIndexCase[i] = this.gameObject;

                // Instead of setting targetPos to currentQueueIndex[i].transform, use the queue position directly
                targetPos = _CaseManager.agentQueuePos[i];

                // Move the agent to the corresponding queue position
                MoveToQueueCase(targetPos.position);
                Debug.Log($"Agent {name} added to queue at index {i} and moved to position {targetPos.position}.");

                return; // Exit once the agent is assigned
            }
        }

        Debug.LogWarning("No available slots in the queue.");
    }
    public void MoveToQueueCase(Vector3 position)
    {
        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            // Set the destination directly without modifying targetPos
            navMeshAgent.SetDestination(position);
            currentState = AgentState.MovingToCase;
            Debug.Log($"Agent {name} is moving to {position}");
        }
        else
        {
            Debug.LogError("NavMeshAgent not found on this agent.");
        }
    }
    #endregion
}
