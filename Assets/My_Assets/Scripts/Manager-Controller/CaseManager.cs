using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
public class CaseManager : MonoBehaviour
{
    [SerializeField] CaseMoneyActor _caseMoneyActor;
    [SerializeField] Transform itemPackPos;
    [SerializeField] Transform packagePos;
    [SerializeField] GameObject packagePrefab;
    private GameObject instantiatedPackage;
    [SerializeField] MeshRenderer digitalFrame;
    [SerializeField] Material digitalFrameNewMat;
    [SerializeField] Material digitalFrameOldMat;
    [SerializeField] int payedMoney;
    [SerializeField] public float moveDuration = 1f;
    [SerializeField] public int maxQueueSize = 5;


    [SerializeField] public Transform agentPos;

    public List<GameObject> currentQueueIndexCase = new List<GameObject>();
    [SerializeField] public Transform[] agentQueuePos;
    [SerializeField] public GameObject queuePosHolder;

    //Bool when player in collider;
    public bool isPayment;
    private bool readOnce;
    private bool packageInstantiated;
    [SerializeField] private GameObject caseWorkerUI;
    [SerializeField] private GameObject casePlayerUI;
    [SerializeField] private GameObject worker;
    bool isworkerActive;
    [SerializeField] private int caseId;
    // Start is called before the first frame update
    private void Start()
    {
        InitializeDigitalFrame();
        // _caseMoneyActor = transform.GetChild(2).GetComponent<CaseMoneyActor>();
        PopulateQueuePositions();
        ActivateWorker();
    }

    private void Update()
    {
        if (isPayment && !readOnce && currentQueueIndexCase[0] != null)
        {
            Agent agent = currentQueueIndexCase[0].GetComponent<Agent>();
            agent.isAgentPaying = true;
            readOnce = true;
        }
    }

    private void InitializeDigitalFrame()
    {
        if (digitalFrame != null && digitalFrame.materials.Length >= 3)
        {
            digitalFrameOldMat = digitalFrame.materials[2];
        }
        else
        {
            Debug.LogWarning("Invalid digital frame or missing materials.");
        }
    }
    private ItemType itemType;
    public IEnumerator AgentItemToPos(Transform item)
    {
        if (item == null || currentQueueIndexCase.Count == 0 || currentQueueIndexCase[0] == null)
        {
            Debug.LogWarning("Invalid item or no agents in queue.");
            yield break;
        }
        // Check if the worker is active and set the "Payment" animation to true
        if (isworkerActive && workerAnimator != null)
        {
            workerAnimator.SetBool("Payment", true); // Set Payment animation to true
            workerAnimator.SetBool("Idle", false); // Set Idle animation to false (if needed)
        }

        itemType = GetItemTypeFromName(item.name);
        Agent currentAgent = currentQueueIndexCase[0].GetComponent<Agent>();
        yield return MoveAgentToQueuePosition(currentAgent, agentQueuePos[0]);
        item.SetParent(itemPackPos);
        Quaternion rotation = item.name.Contains("Ship")
      ? Quaternion.Euler(-90, 90, 0)
      : itemPackPos.rotation;

        yield return MoveObject(item, itemPackPos.position, rotation);

        // Wait for a short duration before destroying the item
        yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds


        if (item != null) // Check if item is still valid before destroying
        {
            Destroy(item.gameObject);
        }
        
        // Start preparing the package after the item is destroyed
        StartCoroutine(PackageGettingReady());
    }

    private IEnumerator MoveAgentToQueuePosition(Agent agent, Transform targetPos)
    {
        while (agent.targetPos != targetPos || Vector3.Distance(agent.transform.position, targetPos.position) > 0.872f)
        {
            yield return null;
        }
    }

    public void MovePackageToAgent() => StartCoroutine(PackageGettingReady());

    public IEnumerator PackageGettingReady()
    {
        if (!packageInstantiated)
        {
            instantiatedPackage = Instantiate(packagePrefab, packagePos.position, packagePos.rotation);
            UpdateDigitalFrameMaterial(digitalFrameNewMat);
            packageInstantiated = true;

            // Use DOTween for some visual effect (optional)
            instantiatedPackage.transform.DOPunchScale(Vector3.one * 0.5f, 1f, 10, 0.5f);
        }

        yield return WaitForDissolveCompletion(instantiatedPackage);
        yield return TransferPackageToAgentHand();
    }

    private IEnumerator WaitForDissolveCompletion(GameObject package)
    {
        var dissolveScript = package.GetComponent<DissolveObject>();
        if (dissolveScript != null)
        {
            bool dissolveFinished = false;
            dissolveScript.OnDissolveComplete += () => dissolveFinished = true;
            yield return new WaitUntil(() => dissolveFinished);
        }
    }

    private IEnumerator TransferPackageToAgentHand()
    {
        if (instantiatedPackage == null) yield break;

        Agent agent = currentQueueIndexCase[0].GetComponent<Agent>();
        Transform agentHandPos = agent.frontStake;
        agent.agentAnima.SetBool("Carry", true);
        yield return MoveObject(instantiatedPackage.transform, agentHandPos.position, agentHandPos.rotation);

        instantiatedPackage.transform.SetParent(agentHandPos);
        instantiatedPackage.transform.DOKill();
        instantiatedPackage.transform.localScale = Vector3.one;
        instantiatedPackage.transform.localPosition = Vector3.zero;
        instantiatedPackage.transform.localRotation = Quaternion.identity;
        // After package transfer, reset to "Idle" animation
        if (isworkerActive && workerAnimator != null)
        {
            workerAnimator.SetBool("Payment", false); // Set Payment animation to false
            workerAnimator.SetBool("Idle", true); // Set Idle animation to true
        }
        CompleteTransaction(agent);
    }
    public Vector3 rotate = new Vector3(0, 180, 0);
    private IEnumerator MoveObject(Transform obj, Vector3 targetPosition, Quaternion targetRotation)
    {
        if (obj == null)
        {
            Debug.LogWarning("MoveObject called with a null object.");
            yield break; // Exit the coroutine if the object is null.
        }

        // Using DOTween to move the object
        obj.DOMove(targetPosition, moveDuration).SetEase(Ease.OutQuad);

        // Rotate to the target rotation smoothly
        obj.DORotateQuaternion(targetRotation, moveDuration).SetEase(Ease.OutQuad);

        // Jump parameters
        float jumpHeight = 0.2f; // Adjust the height of the jump
        float jumpDuration = 0.3f; // Duration of the jump

        // Add the jump effect moving to the target location
        obj.DOJump(targetPosition, jumpHeight, 1, jumpDuration).SetEase(Ease.OutQuad);

        // Wait until the movement and jump are completed
        yield return new WaitForSeconds(moveDuration + jumpDuration);
    }
    private bool transactionInProgress = false; // Flag to check if a transaction is ongoing
    public int totalProfit = 0;
    private void CompleteTransaction(Agent agent)
    {
        if (transactionInProgress)
        {
            Debug.LogWarning("Transaction is already in progress. Ignoring subsequent call.");
            return;
        }

        transactionInProgress = true;
        UpdateDigitalFrameMaterial(digitalFrameOldMat);

        // Calculate profit based on the specific item type each agent sold
        if (itemType != ItemType.None && ItemPrice.Prices.TryGetValue(itemType, out int itemPrice))
        {
            totalProfit += itemPrice;
            Debug.Log($"Transaction for item: {itemType}, Price per item: {itemPrice}, Total Profit: {totalProfit}");
        }
        else
        {
            Debug.LogWarning("Invalid item type; no profit added.");
        }

        // Reset after each transaction to avoid incorrect accumulation
        packageInstantiated = false;
        agent.isAgentPaying = false;
        RemoveAgentFromQueue(agent);
        UpdateQueueAfterPick();
        agent.MoveToExit();

        readOnce = false;
        transactionInProgress = false;
        itemType = ItemType.None;  // Reset to avoid stale data
    }
    private int GetMoneySpawnCount(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Circuit: return 1;
            case ItemType.Gear: return 2;
            case ItemType.Conductive: return 3;
            case ItemType.ShipOne: return 4;
            case ItemType.ShipTwo: return 5;
            default: return 0; // No money spawned for 'None' or unrecognized types
        }
    }
    private void UpdateDigitalFrameMaterial(Material newMaterial)
    {
        Material[] materials = digitalFrame.materials;
        materials[2] = newMaterial;
        digitalFrame.materials = materials;
    }

    public void PopulateQueuePositions()
    {
        if (queuePosHolder == null)
        {
            Debug.LogWarning("queuePosHolder is not assigned.");
            return;
        }

        Transform[] children = queuePosHolder.GetComponentsInChildren<Transform>();
        agentQueuePos = new Transform[children.Length - 1];

        for (int i = 0, j = 0; i < children.Length; i++)
        {
            if (children[i] != queuePosHolder.transform)
                agentQueuePos[j++] = children[i];
        }

        maxQueueSize = agentQueuePos.Length;
        currentQueueIndexCase = new List<GameObject>(new GameObject[maxQueueSize]);
    }

    public void RemoveAgentFromQueue(Agent agent)
    {

        int index = currentQueueIndexCase.IndexOf(agent.gameObject);
        if (index == -1) return;

        // Remove agent from queue and add a placeholder for empty position
        currentQueueIndexCase.RemoveAt(index);
        currentQueueIndexCase.Add(null);

        // Determine the number of money instances to spawn based on item type
        int moneySpawnCount = GetMoneySpawnCount(itemType);
        for (int i = 0; i < moneySpawnCount; i++)
        {
            _caseMoneyActor.CreateMoney(agentPos.position);
        }

        Debug.Log(".......Paid Queue Set........");
        UpdateQueueAfterPick();
    }

    private void UpdateQueueAfterPick()
    {
        for (int i = 0; i < currentQueueIndexCase.Count; i++)
        {
            if (currentQueueIndexCase[i] != null)
            {
                Agent agent = currentQueueIndexCase[i].GetComponent<Agent>();
                agent.targetPos = agentQueuePos[i];
                agent.MoveToQueueCase(agentQueuePos[i].position);
            }
        }
    }
    public int GetQueueSize()
    {
        return currentQueueIndexCase.Count(agent => agent != null);
    }
    public int GetValidQueueSize()
    {
        int validCount = 0;

        foreach (var queueElement in currentQueueIndexCase)
        {
            if (queueElement != null) // Only count valid, non-null elements
            {
                validCount++;
            }
        }

        return validCount;
    }

    private ItemType GetItemTypeFromName(string itemName)
    {
        // Logic to parse the item name and return the corresponding ItemType
        switch (itemName)
        {
            case "Circuit":
                return ItemType.Circuit;
            case "Gear":
                return ItemType.Gear;
            case "Conductive":
                return ItemType.Conductive;
            case "ShipOne":
                return ItemType.ShipOne;
            case "ShipTwo":
                return ItemType.ShipTwo;
            default:
                Debug.LogWarning("Unknown item type for item: " + itemName);
                return ItemType.Circuit; // Default or handle as needed
        }
    }
    public int GetTotalProfit()
    {
        return totalProfit;
    }
    public void ReduceTotalProfit(int amount)
    {
        if (totalProfit >= amount)
        {
            totalProfit -= amount;
        }
        else
        {
            totalProfit = 0;
        }
    }

    [SerializeField] private Animator workerAnimator;
    public void ActivateWorker()
    {
        // Retrieve the worker ID, assuming it’s stored or accessible here
        int workerId = caseId;

        // Check if this worker has been paid
        bool isPaid = PlayerPrefs.GetInt("caseWorkerPaid" + workerId, 0) == 1;

        if (isPaid)
        {
            // If the worker has been paid, activate the worker
            caseWorkerUI.SetActive(false);
            casePlayerUI.SetActive(false);
            worker.SetActive(true);
            isPayment = true;
            isworkerActive=true;
        }
        else
        {
            Debug.Log("Worker not paid yet, cannot activate.");
        }
    }

}
public enum ItemType
{
    None,        // Add this default type to represent an uninitialized state
    Circuit,
    Gear,
    Conductive,
    ShipOne,
    ShipTwo
}
public class ItemPrice
{
    public static Dictionary<ItemType, int> Prices = new Dictionary<ItemType, int>
    {
        { ItemType.Circuit, 10 },
        { ItemType.Gear, 15 },
        { ItemType.Conductive, 20 },
        { ItemType.ShipOne, 50 },
        { ItemType.ShipTwo, 75 }
    };
}


