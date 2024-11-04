// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.AI;

// public class Deneme : MonoBehaviour
// {
//     public Transform shelf;
//     public StoreShelfController _StoreShelf;
//     public CaseManager _CaseManager;
//     public Transform casePos;
//     public Transform exitPos;
//     public Transform frontStake;
//     private GameObject heldItem;
//     private NavMeshAgent agent;

//     private enum AgentState { MovingToShelf, PickingItem, WaitingInQueue, MovingToCase, Paying, Exit }

//     private Animator agentAnima;
//     [SerializeField] private AgentState currentState;

//     private void Start()
//     {
//         agent = GetComponent<NavMeshAgent>();
//         agentAnima = GetComponent<Animator>();
//         currentState = AgentState.MovingToShelf;
//         MoveToShelf();
//     }

//     private void Update()
//     {
//         switch (currentState)
//         {
//             case AgentState.MovingToShelf:
//                 if (ReachedDestination())
//                 {
//                     // Stop walking animation when reaching the shelf
//                     _StoreShelf = shelf.parent.GetComponent<StoreShelfController>();
//                     agentAnima.SetBool("Walking", false);

//                     currentState = AgentState.PickingItem;
//                     StartCoroutine(WaitUntilTableFilled());
//                 }
//                 else
//                 {
//                     // Enable walking animation
//                     agentAnima.SetBool("Walking", true);
//                     agentAnima.SetBool("Carry", false);
//                 }
//                 break;

//             case AgentState.MovingToCase:
//                 if (ReachedDestination())
//                 {
//                     if (IsQueueFull()) // If the queue at the case is full
//                     {
//                         // Switch to waiting in queue state
//                         currentState = AgentState.WaitingInQueue;

//                     }
//                     else
//                     {
//                         // Stop carry-walking animation when reaching the case
//                         agentAnima.SetBool("Carry", true);
//                         agentAnima.SetBool("Walking", false);
//                         currentState = AgentState.Paying;
//                         _CaseManager = casePos.parent.GetComponent<CaseManager>();
//                        // PayForItem();
//                     }
//                 }
//                 else
//                 {
//                     // Enable carry-walking animation when carrying an item
//                     agentAnima.SetBool("Carry", true);
//                     agentAnima.SetBool("Walking", true);
//                 }
//                 break;

//             case AgentState.WaitingInQueue:
//                 // Logic to handle the waiting in queue (e.g., check if the agent can move forward in the queue)
//                 if (!IsQueueFull()) // If the agent can now approach the case
//                 {
//                     currentState = AgentState.Paying;
//                 }
//                 break;

//             case AgentState.Paying:
//                 // Logic for paying at the case
//                 //PayForItem();  // Call method to handle payment
//                 //currentState = AgentState.Exit;  // Transition to exit after payment
//                 break;

//             case AgentState.Exit:
//                 // Logic to exit the store
//                 //ExitStore();
//                 break;
//         }
//     }
//     private bool IsQueueFull()
//     {
//         // Example logic to check if the case has a queue
//         //return caseQueue.Count >= maxQueueSize; // maxQueueSize is the maximum number of agents allowed to queue
//         return false;
//     }
//     private void MoveToShelf()
//     {
//         agent.SetDestination(shelf.position);
//     }
//     private IEnumerator WaitUntilTableFilled()
//     {
//         while (_StoreShelf.activeItems.Length == 0)
//         {
//             yield return null;  // Wait for the next frame
//         }

//         PickItem();  // Proceed with picking the item
//     }

//     private void PickItem()
//     {
//         if (_StoreShelf.activeItems.Length > 0)
//         {
//             StartCoroutine(WaitUntilItemPick());
//         }
//     }
//     private IEnumerator WaitUntilItemPick()
//     {
//         _StoreShelf.TransferToAgent(frontStake);
//         agentAnima.SetBool("Walking", false);
//         agentAnima.SetBool("Carry", true);
//         // Wait until the table is filled
//         while (_StoreShelf.isTransferInProgress)
//         {
//             _StoreShelf.TransferToAgent(frontStake);
//             yield return null;
//         }
//         heldItem = frontStake.transform.GetChild(0).gameObject;
//         agentAnima.SetBool("Walking", true);
//         currentState = AgentState.MovingToCase;
//         MoveToCase();
//     }

//     private void MoveToCase()
//     {
//         agent.SetDestination(casePos.position);
//     }

//     // private void PayForItem()
//     // {
//     //     if (_CaseManager.isPayment)
//     //     {
//     //        // StartCoroutine(WaitForPayment());
//     //         Debug.Log("Item paid for!");
//     //     }
//     // }

//     // private IEnumerator WaitForPayment()
//     // {
//     //     _CaseManager.a = true;
//     //     agentAnima.SetBool("Carry", false);

//     //     while (_CaseManager.a)
//     //     {
//     //         if (heldItem != null) // Ensure heldItem is still valid before trying to move it
//     //         {
//     //             yield return StartCoroutine(_CaseManager.AgentItemToPos(heldItem.transform));
//     //         }

//     //         yield return null;
//     //     }

//     //     // Proceed after package transfer is complete
//     //     agentAnima.SetBool("Walking", true);
//     //     agentAnima.SetBool("Carry", true);
//     //     currentState = AgentState.Exit;
//     //     MoveToExit();
//     // }

//     private void MoveToExit()
//     {
//         agent.SetDestination(exitPos.position);
//     }
//     private bool ReachedDestination()
//     {
//         if (!agent.pathPending)
//         {
//             if (agent.remainingDistance <= 0.6f)
//             {
//                 if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
//                 {
//                     return true;
//                 }
//             }
//         }
//         return false;
//     }
//     public Transform queuePos;
//     public void SetQueuePosition(Transform queuePosition)
//     {
//         if (queuePosition != null)
//         {
//             queuePos = queuePosition;
//             MoveToQueue();  // Call only when queuePos is valid
//         }
//         else
//         {
//             Debug.Log("No queue position available for agent: " + gameObject.name);
//             currentState = AgentState.WaitingInQueue;
//         }
//     }

//     private void MoveToQueue()
//     {
//         if (queuePos != null)
//         {
//             agent.SetDestination(queuePos.position);
//         }
//         else
//         {
//             Debug.LogError("queuePos is null for agent: " + gameObject.name);
//         }
//     }
// //   using System;
// // using System.Collections;
// // using System.Collections.Generic;
// // using UnityEditor;
// // using UnityEngine;

// // public class AgentManager : MonoBehaviour
// // {
// //     [SerializeField] private CaseManager _CaseManager;
// //     [SerializeField] private StoreManager _StoreManager;

// //     [SerializeField] private List<GameObject> activeShelfs;
// //     [SerializeField] private List<GameObject> casePay;

// //     [SerializeField] private GameObject busStopSpawnPoint;
// //     [SerializeField] private Transform exitPoint;
// //     [SerializeField] private GameObject firstCase;
// //     [SerializeField] private GameObject agentPrefab;
// //     [SerializeField] private int maxAgents = 5;
// //     [SerializeField] private List<GameObject> caseQueue = new List<GameObject>();
// //     public List<Agent> agents = new List<Agent>();

// //     private void Awake()
// //     {
// //         _CaseManager = GetComponent<CaseManager>();
// //         _StoreManager = GetComponent<StoreManager>();
// //     }
// //     private void Start()
// //     {

// //         // Start spawning agents
// //         StartCoroutine(SpawnAgentsRoutine());
// //     }

// //     private void Update()
// //     {
// //         // Logic to process agent actions, if necessary
// //     }


// //     #region AgentSpawn-Region
// //     private float spawnInterval = 2f;
// //     private IEnumerator SpawnAgentsRoutine()
// //     {
// //         while (true) // Infinite loop to keep spawning
// //         {
// //             SpawnAgent(); // Spawn an agent
// //             yield return new WaitForSeconds(spawnInterval); // Wait for the specified interval
// //         }
// //     }

// //     public void SpawnAgent()
// //     {
// //         if (agents.Count < maxAgents)
// //         {
// //             // Create a new agent at the bus stop position
// //             GameObject newAgent = Instantiate(agentPrefab, busStopSpawnPoint.transform.position, Quaternion.identity);
// //             newAgent.SetActive(true);
// //             newAgent.name = "Agent" + agents.Count;
       
       
// //             Agent agentComponent = newAgent.GetComponent<Agent>();
// //             agentComponent.exitPosition = exitPoint;
           
         
// //             agents.Add(agentComponent);

// //             // Assign the _StoreShelf and _CaseManager references from AgentManager to the Agent
// //             if (activeShelfs.Count > 0)
// //             {
// //                 agentComponent._StoreShelf = activeShelfs[0].GetComponent<StoreShelfController>(); // Set StoreShelfController
// //             }

// //             if (casePay.Count > 0)
// //             {
// //                 agentComponent._CaseManager = casePay[0].GetComponent<CaseManager>(); // Set CaseManager
// //             }

// //             Debug.Log($"Spawned agent: {agentComponent.name}. Total agents: {agents.Count}");
// //         }
// //         else
// //         {
// //             Debug.Log("Maximum number of agents reached. Cannot spawn more.");
// //         }
// //     }

// //     public void RemoveAgent(Agent agent)
// //     {
// //         if (agents.Contains(agent))
// //         {
// //             agents.Remove(agent);
// //             Destroy(agent.gameObject); // Destroy the agent GameObject
// //             Debug.Log($"Agent {agent.name} has exited the store. Total agents: {agents.Count}");
// //         }
// //     }
// //     #endregion


// //     #region TableIndex
// //     public void UpdateActiveTables(List<GameObject> newActiveTables)
// //     {
// //         // Clear the activeShelfs to avoid duplicates
// //         activeShelfs.Clear();
// //         casePay.Clear(); // Clear the casePay list to reset it

// //         // Ensure firstCase is the first element in the casePay list
// //         if (firstCase != null)
// //         {
// //             casePay.Add(firstCase);
// //             Debug.Log($"Added {firstCase.name} as the first case in casePay.");
// //         }

// //         // Iterate through the new active tables and update the activeShelfs list with their parent GameObjects
// //         foreach (GameObject table in newActiveTables)
// //         {
// //             if (table != null)
// //             {
// //                 GameObject parentObject = table.transform.parent?.gameObject;

// //                 if (parentObject != null)
// //                 {
// //                     // Add the parent to activeShelfs if it's a valid GameObject
// //                     activeShelfs.Add(parentObject);
// //                     Debug.Log($"Added parent {parentObject.name} of {table.name} to activeShelfs.");
// //                 }
// //             }
// //         }

// //         // Handle cases specifically: Add the parent to casePay list if the name contains "case"
// //         foreach (GameObject table in newActiveTables)
// //         {
// //             if (table != null && table.name.Contains("case", StringComparison.OrdinalIgnoreCase))
// //             {
// //                 GameObject parentObject = table.transform.parent?.gameObject;

// //                 if (parentObject != null && parentObject != firstCase)
// //                 {
// //                     casePay.Add(parentObject); // Add the parent to casePay (ignoring firstCase which is already added)
// //                     Debug.Log($"Added parent {parentObject.name} of {table.name} to casePay.");
// //                 }
// //             }
// //         }

// //         Debug.Log("Active tables updated. Total active shelves: " + activeShelfs.Count);
// //         Debug.Log("Total active payment cases: " + casePay.Count);
// //     }

// //     #endregion
// // }

// // using System.Collections;
// // using System.Collections.Generic;
// // using UnityEngine;
// // using DG.Tweening;
// // public class CaseManager : MonoBehaviour
// // {
// //     [SerializeField] CaseMoneyActor _CaseMoneyActor;
// //     [SerializeField] Transform itemPackPos;
// //     [SerializeField] Transform packagePos;
// //     [SerializeField] GameObject Package;
// //     [SerializeField] MeshRenderer digitalFrame;
// //     [SerializeField] Material digitalFrameNewMat;
// //     [SerializeField] Material digitalFrameOldMat;
// //     [SerializeField] int payedMoney;
// //     [SerializeField] public Transform agentPos;

// //     //Bool when player in collider;
// //     public bool isPayment;

// //     // Start is called before the first frame update
// //     void Start()
// //     {
// //         _CaseMoneyActor = transform.GetChild(2).GetComponent<CaseMoneyActor>();

// //         if (digitalFrame != null && digitalFrame.materials.Length >= 3)
// //         {
// //             // Access the third material (index 2)
// //             digitalFrameOldMat = digitalFrame.materials[2];

// //             // Now you can manipulate or use the thirdMaterial as needed
// //             Debug.Log("Third material name: " + digitalFrameOldMat.name);
// //         }
// //         else
// //         {
// //             Debug.LogWarning("Either the MeshRenderer is missing or it doesn't have at least 3 materials.");
// //         }
// //         PopulateQueuePositions();
// //     }
// //     bool readOnce;
// //     private void Update()
// //     {
// //         if (isPayment && !readOnce)
// //         {
// //             if (currentQueueIndexCase[0] != null)
// //             {
// //                 Agent agent = currentQueueIndexCase[0].GetComponent<Agent>();
// //                 agent.isAgentPaying = true;
// //                 readOnce = true;
// //             }
// //         }
// //     }

// //     [SerializeField] private float moveDuration = 0.1f;  // Adjust the move duration

// //     public System.Action OnDissolveComplete;

// //     #region Agent 

// //     //public float moveSpeed = 10f;
// //     public IEnumerator AgentItemToPos(Transform item)
// //     {
// //         if (item != null)
// //         {
// //             print("Agent item gameobject " + item.gameObject.name);

// //             // Check if there are any agents in the queue
// //             if (currentQueueIndexCase.Count == 0 || currentQueueIndexCase[0] == null)
// //             {
// //                 Debug.LogWarning("No agents in the queue to process.");
// //                 yield break; // Exit the coroutine if there's no agent to work with
// //             }

// //             // Get the agent at the front of the queue (position 0)
// //             Agent currentAgent = currentQueueIndexCase[0].GetComponent<Agent>();

// //             // Check if currentAgent is null
// //             if (currentAgent == null)
// //             {
// //                 Debug.LogWarning("Current agent is null. Cannot proceed.");
// //                 yield break; // Exit the coroutine if the agent is null
// //             }

// //             Transform agentPos = currentAgent.transform; // Get the agent's transform

// //             // Ensure the agent reaches the first queue position (agentQueuePos[0])
// //             while (currentAgent != null &&
// //                    (currentAgent.targetPos != agentQueuePos[0] ||
// //                     Vector3.Distance(agentPos.position, agentQueuePos[0].position) > 0.872f))
// //             {
// //                 yield return null; // Wait until the agent reaches position 0 in the queue
// //             }

// //             print("Agent reached queue pos 0");

// //             // Move the item to the target position (itemPackPos)
// //             float moveSpeed = 10f / moveDuration;
// //             Vector3 targetPosition = itemPackPos.position;
// //             item.transform.SetParent(null);  // Unparent the item first, so it doesn't follow any other objects

// //             while (item != null && Vector3.Distance(item.transform.position, targetPosition) > 0.01f)
// //             {
// //                 if (item == null) yield break;

// //                 item.transform.position = Vector3.Lerp(item.transform.position, targetPosition, moveSpeed * Time.deltaTime);
// //                 item.transform.rotation = Quaternion.Slerp(item.transform.rotation, itemPackPos.rotation, moveSpeed * Time.deltaTime);

// //                 yield return null;
// //             }

// //             // Once the item reaches the target position, destroy it and move the package
// //             if (item != null)
// //             {
// //                 Destroy(item.gameObject);
// //                 MovePackageToAgent();
// //             }
// //         }
// //         else
// //         {
// //             Debug.LogError("Item is null. Cannot proceed with AgentItemToPos.");
// //         }

// //         yield return null;
// //     }

// //     public void MovePackageToAgent()
// //     {
// //         StartCoroutine(PackageGettingReady());
// //     }

// //     private bool packageInstantiated = false;
// //     private GameObject instantiatedPackage;

// //     public IEnumerator PackageGettingReady()
// //     {
// //         // Check if the package is already instantiated
// //         if (!packageInstantiated)
// //         {
// //             instantiatedPackage = InstantiatePackage();

// //             Material[] materials = digitalFrame.materials;
// //             materials[2] = digitalFrameNewMat;
// //             digitalFrame.materials = materials;

// //             packageInstantiated = true;
// //         }

// //         DissolveObject dissolveScript = instantiatedPackage.GetComponent<DissolveObject>();

// //         if (dissolveScript != null)
// //         {
// //             bool dissolveFinished = false;
// //             dissolveScript.OnDissolveComplete += () => dissolveFinished = true;

// //             // Wait until the dissolve process is complete
// //             yield return new WaitUntil(() => dissolveFinished);
// //         }

// //         // Once dissolve is done, transfer the package to the agent's hand
// //         yield return StartCoroutine(TransferPackageToAgentHand());


// //     }

// //     public IEnumerator TransferPackageToAgentHand()
// //     {
// //         if (instantiatedPackage != null)
// //         {
// //             Agent agent = currentQueueIndexCase[0].GetComponent<Agent>();
// //             Transform agentHandPos = agent.frontStake;

// //             // Move the package to the agent's hand
// //             float moveSpeed = 5f;
// //             while (Vector3.Distance(instantiatedPackage.transform.position, agentHandPos.position) > 0.1f)
// //             {
// //                 instantiatedPackage.transform.position = Vector3.Lerp(
// //                     instantiatedPackage.transform.position,
// //                     agentHandPos.position,
// //                     moveSpeed * Time.deltaTime
// //                 );
// //                 instantiatedPackage.transform.rotation = Quaternion.Slerp(
// //                     instantiatedPackage.transform.rotation,
// //                     agentHandPos.rotation,
// //                     moveSpeed * Time.deltaTime
// //                 );

// //                 yield return null;
// //             }

// //             // Snap the package to the agent's hand position and rotation
// //             instantiatedPackage.transform.position = agentHandPos.position;
// //             instantiatedPackage.transform.rotation = agentHandPos.rotation;

// //             // Set the package as a child of the agent's hand
// //             instantiatedPackage.transform.SetParent(agentHandPos);

// //             // Reset local position and rotation so that the package fits perfectly into the hand
// //             instantiatedPackage.transform.localPosition = Vector3.zero;
// //             instantiatedPackage.transform.localRotation = Quaternion.identity;

// //             // Restore the original material of the digital frame after package transfer
// //             Material[] materials = digitalFrame.materials;
// //             materials[2] = digitalFrameOldMat;
// //             digitalFrame.materials = materials;

// //             // Handle removing agent from queue and spawning money
// //             if (currentQueueIndexCase.Count > 0 && currentQueueIndexCase[0] != null)
// //             {
// //                 Agent agentAtFront = currentQueueIndexCase[0].GetComponent<Agent>();
// //                 if (agentAtFront != null)
// //                 {
// //                     agent.isAgentPaying = false;
// //                     _CaseMoneyActor.CreateMoney(agentPos.position);
// //                     packageInstantiated = false;
// //                     readOnce = false;
// //                     RemoveAgentFromQueue(agentAtFront); 
// //                     UpdateQueueAfterPick();

// //                 }
// //                 else
// //                 {
// //                     Debug.LogWarning("No Agent component found on the object at the front of the queue.");
// //                 }
// //             }
// //             else
// //             {
// //                 Debug.LogWarning("No agents in the queue or the first queue element is null.");
// //             }
// //         }
// //         yield return null;
// //     }
// //     public GameObject InstantiatePackage()
// //     {
// //         GameObject objectSpawn = Instantiate(Package, packagePos, packagePos);
// //         return objectSpawn;
// //     }
// //     #endregion
// //     public int maxQueueSize = 3;
// //     [SerializeField] GameObject queuePosHolder;
// //     [SerializeField] public Transform[] agentQueuePos;
// //     public List<GameObject> currentQueueIndexCase;
// //     public void PopulateQueuePositions()
// //     {

// //         if (queuePosHolder == null)
// //         {
// //             Debug.LogWarning("queuePosHolder has not been assigned in the inspector.");
// //             return;
// //         }

// //         Transform[] children = queuePosHolder.GetComponentsInChildren<Transform>();
// //         agentQueuePos = new Transform[children.Length - 1];

// //         int index = 0;
// //         foreach (Transform child in children)
// //         {
// //             if (child != queuePosHolder.transform)
// //             {
// //                 agentQueuePos[index] = child;
// //                 index++;
// //             }
// //         }

// //         maxQueueSize = agentQueuePos.Length;
// //         currentQueueIndexCase = new List<GameObject>(new GameObject[maxQueueSize]);

// //         Debug.Log("Queue positions populated: " + agentQueuePos.Length);
// //     }

// //     private void UpdateQueueAfterPick()
// //     {
// //         // Ensure there are agents to update
// //         if (currentQueueIndexCase.Count == 0 || currentQueueIndexCase[0] == null)
// //         {
// //             Debug.LogWarning("Queue is empty or no agent at the front to update.");
// //             return;
// //         }

// //         // Update each agent's queue position
// //         for (int i = 0; i < currentQueueIndexCase.Count; i++)
// //         {
// //             if (currentQueueIndexCase[i] != null)
// //             {
// //                 Agent agentScript = currentQueueIndexCase[i].GetComponent<Agent>();

// //                 if (agentScript != null)
// //                 {
// //                     // Update each agent's target position in the queue
// //                     agentScript.targetPos = agentQueuePos[i];
// //                     agentScript.MoveToQueueCase(agentQueuePos[i].position);
// //                 }
// //                 else
// //                 {
// //                     Debug.LogWarning($"Agent script not found on object at index {i}.");
// //                 }
// //             }
// //         }

// //         Debug.Log($"Queue updated after agent picked an item: {string.Join(", ", currentQueueIndexCase)}");
// //     }
// //     public void RemoveAgentFromQueue(Agent agent)
// //     {
// //         int index = currentQueueIndexCase.IndexOf(agent.gameObject);

// //         if (index != -1)
// //         {
// //             // Remove the agent from the queue at the correct index
// //             currentQueueIndexCase[index] = null;

// //             // Shift all the agents after the removed agent forward in the queue
// //             for (int i = index; i < currentQueueIndexCase.Count - 1; i++)
// //             {
// //                 currentQueueIndexCase[i] = currentQueueIndexCase[i + 1];
// //             }

// //             // Set the last position to null (since we've shifted everything forward)
// //             currentQueueIndexCase[currentQueueIndexCase.Count - 1] = null;

// //             Debug.Log($"Agent {agent.name} removed from queue at index {index}.");
// //         }
// //         else
// //         {
// //             Debug.LogWarning($"Agent {agent.name} was not found in the queue.");
// //         }

// //         UpdateQueueAfterPick(); // Ensure to update the queue after removing an agent
// //     }

// // }



// }
