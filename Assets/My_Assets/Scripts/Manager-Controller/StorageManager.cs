using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StorageManager : MonoBehaviour
{
    public SplineTech _SP;
    public StorageShelfController[] _StorageShelfController;
    public StoreManager _StoreManager;
    public GameObject CargoMan;
    public GameObject FrontStackPoint;
    public GameObject[] storageShelfs;
    public Animator cargoManAnima;

    public float shipArrivalInterval = 60f; // 1 minute interval
    [SerializeField] private float timeSinceLastShip = 0f;
    private bool isTimerActive = false; // To check if the timer is active

    #region Agent

    [SerializeField] private Transform[] target;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    public float walkSpeed = 2.0f;
    public float animationSpeedMultiplier = 1.0f;
    private float distance;
    [SerializeField] private int currentTargetIndex = 0;
    private bool isWaiting = false;
    private bool movingForward = true;

    #endregion

    void Awake()
    {
        InitializeStorageControllers();
        _StoreManager = GetComponent<StoreManager>();
        if (_StoreManager == null)
        {
            Debug.LogError("StoreManager component is missing.");
        }
    }
    bool CargoFlag;
    void Update()
    {
        if (_StoreManager.tables.Length > 0 && _StoreManager.tables[0] != null && _StoreManager.tables[0].activeSelf && !CargoFlag)
        {
            _SP._follower.follow = true;
            CargoFlag = true;
        }
        if (_SP.isCargo)
        {
            StartCargoProcess();
        }

        if (!isWaiting && CargoMan.activeSelf)
        {
            ProcessAgentMovement();
        }

        // Handle ship timer logic
        if (isTimerActive)
        {
            timeSinceLastShip += Time.deltaTime;

            // Check if the 1-minute interval has passed
            if (timeSinceLastShip >= shipArrivalInterval)
            {
                // Trigger the next cargo process (ship arrives)
                _SP.CargoMove();

                // Reset the timer and stop it until the ship leaves again
                timeSinceLastShip = 0f;
                isTimerActive = false;
            }
        }

    }

    #region Initialization

    private void InitializeStorageControllers()
    {
        _StorageShelfController = new StorageShelfController[storageShelfs.Length];
        for (int i = 0; i < storageShelfs.Length; i++)
        {
            _StorageShelfController[i] = storageShelfs[i].GetComponent<StorageShelfController>();
            if (_StorageShelfController[i] == null)
            {
                Debug.LogError($"StorageShelfController not found on {storageShelfs[i].name}");
            }
        }
    }

    private void StartCargoProcess()
    {
        hasFlagged = false;
        movingForward = true;
        currentTargetIndex = 0;
        CargoManActive(true);
        MoveToNextTarget();
        _SP.isCargo = false;
    }

    #endregion

    #region Agent Movement Logic

    private void ProcessAgentMovement()
    {
        distance = agent.remainingDistance;
        if (IsTargetReached())
        {
            HandleStorageLoadingLogic();
        }
        else
        {
            UpdateAgentMovement();
        }
    }

    private bool IsTargetReached()
    {
        Vector3 targetPosition = target[currentTargetIndex].position;
        return Vector3.Distance(agent.transform.position, targetPosition) <= 1.22f;
    }

    private void HandleStorageLoadingLogic()
    {
        // When at target index 1, check only storageShelfs[1]
        if (currentTargetIndex == 1)
        {
            ProcessStorageLoading(1);
        }
        // When at target index 2, check both storageShelfs[0] and storageShelfs[2]
        else if (currentTargetIndex == 2)
        {
            ProcessMultipleStorageLoading(new int[] { 0, 2 });
        }
        else
        {
            AllowMovementToNextTarget();
        }
    }

    private void ProcessStorageLoading(int index)
    {
        if (CheckStorage(index))
        {
            _StorageShelfController[index].isStorageLoading = true;
            StartCoroutine(WaitUntilStorageIsLoaded(index));
        }
        else
        {
            AllowMovementToNextTarget();
        }
    }
    private void ProcessMultipleStorageLoading(int[] indexes)
    {
        bool anyStorageLoading = false;

        foreach (int index in indexes)
        {
            if (CheckStorage(index))
            {
                _StorageShelfController[index].isStorageLoading = true;
                StartCoroutine(WaitUntilStorageIsLoaded(index));
                anyStorageLoading = true;
            }
        }

        if (!anyStorageLoading)
        {
            AllowMovementToNextTarget();
        }
    }

    private void UpdateAgentMovement()
    {
        agent.speed = walkSpeed;
        animator.SetFloat("Speed", agent.velocity.magnitude * animationSpeedMultiplier);
    }
    private bool hasFlagged = false;
    private void AllowMovementToNextTarget()
    {
        if (hasFlagged)
        {
            return; // If already flagged, do nothing.
        }

        if (movingForward)
        {
            currentTargetIndex++;
            if (currentTargetIndex >= target.Length)
            {
                movingForward = false;
                currentTargetIndex--;
            }
        }
        else
        {
            currentTargetIndex--;
            if (currentTargetIndex < 0)
            {
                currentTargetIndex = 0;
                if (!hasFlagged)
                {
                    agent.isStopped = true;
                    CargoManActive(false);
                    _SP.CargoMove();
                    hasFlagged = true; // Set the flag to prevent further calls
                    isTimerActive = true;
                    return;
                }
            }
        }
        MoveToNextTarget();
    }

    private void MoveToNextTarget()
    {
        if (currentTargetIndex >= 0 && currentTargetIndex < target.Length)
        {
            MoveToTarget(target[currentTargetIndex]);
        }
        else
        {
            Debug.LogError("Target index out of bounds!");
        }
    }

    private void MoveToTarget(Transform targetTransform)
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            cargoManAnima.SetBool("Walk", true);
            agent.SetDestination(targetTransform.position);
        }
        else
        {
            Debug.LogError("NavMeshAgent is not placed on a NavMesh!");
        }
    }

    private void CargoManActive(bool isActive)
    {
        CargoMan.SetActive(isActive);
    }

    #endregion

    #region StorageControl

    private bool CheckStorage(int index)
    {
        return storageShelfs[index].activeSelf && _StorageShelfController[index].inActiveItems.Length > 0;
    }

    IEnumerator WaitUntilStorageIsLoaded(int shelfIndex)
    {
        isWaiting = true;
        agent.isStopped = true;
        cargoManAnima.SetBool("Idle", true);

        while (_StorageShelfController[shelfIndex].isStorageLoading)
        {
            LookAtTarget(_StorageShelfController[shelfIndex].transform);
            _StorageShelfController[shelfIndex].LoadStorage(FrontStackPoint);
            yield return null;
        }

        cargoManAnima.SetBool("Idle", false);
        isWaiting = false;

        // After finishing storage loading, allow movement to the next target
        AllowMovementToNextTarget();
    }

    private void LookAtTarget(Transform target)
    {
        Vector3 direction = new Vector3(target.position.x - agent.transform.position.x, 0, target.position.z - agent.transform.position.z).normalized;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    #endregion

    #region StorageShelf Unlock

    public void StorageShelfUnlock()
    {
        for (int i = 0; i < storageShelfs.Length; i++)
        {
            if (_StoreManager.tables[i].activeSelf && !storageShelfs[i].gameObject.activeSelf)
            {
                storageShelfs[i].gameObject.SetActive(true);
                Debug.Log($"Activated storage shelf {i}.");
            }
        }
    }

    #endregion
}
