using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{

    private StoreManager _storeManager;

    [SerializeField] private GameObject playerTutorialArrow;

    [SerializeField] private GameObject circuitTablePayArrow;
    [SerializeField] private GameObject circuitStorageArrow;
    [SerializeField] private GameObject tableDropArrow;
    [SerializeField] private GameObject casePositionArrow;


    [SerializeField] private PayCollider circuitTableCollider; // PayCollider script
    [SerializeField] private StorageCollider circuitStorageCollider; // StorageCollider script
    [SerializeField] private ShelfCollider tableDropCollider; // ShelfCollider script
    [SerializeField] private CaseCollider casePositionCollider; // CaseCollider script

    private Transform currentTarget; // Variable for the current step's transform target

    [SerializeField] private float arrowMovementSpeed = 2f; // Speed of movement

    private enum TutorialStep
    {
        CircuitTable,
        CircuitStorage,
        TableDrop,
        CasePosition,
        Completed
    }

    [SerializeField] private TutorialStep currentStep;
    // Circular movement variables
    [SerializeField] private float radius = 1f; // 1 pi unit away (you can adjust the radius value)
    [SerializeField] private float distanceTo = 3f;

    private const string TutorialStepKey = "TutorialStep";

    void Start()
    {
        _storeManager = GetComponent<StoreManager>();
        // Make sure all arrows are initially deactivated
        DeactivateAllArrows();

        // Load the saved tutorial step
        LoadTutorialProgress();
        ActivateCurrentStepArrow();
    }

    void Update()
    {
        if (currentStep == TutorialStep.Completed)
        {
            playerTutorialArrow.SetActive(false);
            return; // Exit Update if the tutorial is completed
        }

        if (currentTarget == null) return;

        // Always check the distance to the target
        float distanceToTarget = Vector3.Distance(playerTutorialArrow.transform.parent.position, currentTarget.position);

        if (distanceToTarget > distanceTo)
        {
            playerTutorialArrow.SetActive(true);
            UpdatePlayerArrowPositionAndDirection();
        }
        else
        {
            playerTutorialArrow.SetActive(false);
        }

        // Check progress and move the active arrow
        CheckCurrentStepProgress();
        MoveActiveArrow();
    }

    [SerializeField] private float movementOffset = 0f; // Stores the movement offset
    [SerializeField] private float maxMovement = 0.5f; // Maximum movement offset (0.5 units)

    // Update the MoveActiveArrow method
    private void MoveActiveArrow()
    {
        // Calculate the movement based on a sine wave
        movementOffset = Mathf.Sin(Time.time * arrowMovementSpeed) * maxMovement;

        // Move arrows for the active step
        switch (currentStep)
        {
            case TutorialStep.CircuitTable:
                circuitTablePayArrow.transform.localPosition += new Vector3(0, movementOffset, 0);
                break;
            case TutorialStep.CircuitStorage:
                circuitStorageArrow.transform.localPosition += new Vector3(0, movementOffset, 0);
                break;
            case TutorialStep.TableDrop:
                tableDropArrow.transform.localPosition += new Vector3(0, movementOffset, 0);
                break;
            case TutorialStep.CasePosition:
                casePositionArrow.transform.localPosition += new Vector3(0, movementOffset, 0);
                break;
        }
    }

    private void CheckCurrentStepProgress()
    {
        switch (currentStep)
        {
            case TutorialStep.CircuitTable:
                if (circuitTableCollider.IsUnlocked())
                {
                    ProgressToNextStep(TutorialStep.CircuitStorage);
                }
                break;

            case TutorialStep.CircuitStorage:
                if (circuitStorageCollider.IsUnlocked())
                {
                    ProgressToNextStep(TutorialStep.TableDrop);
                }
                break;

            case TutorialStep.TableDrop:
                if (tableDropCollider.IsUnlocked())
                {
                    ProgressToNextStep(TutorialStep.CasePosition);
                }
                break;

            case TutorialStep.CasePosition:
                if (casePositionCollider.IsUnlocked())
                {
                    CompleteTutorial();
                }
                break;
        }
    }

    // Progress to the next tutorial step and save the progress
    private void ProgressToNextStep(TutorialStep nextStep)
    {
        DeactivateCurrentStepArrow();
        currentStep = nextStep;
        ActivateCurrentStepArrow();
        SaveTutorialProgress();
    }
    private const string TutorialCompleteKey = "TutorialComplete";
    // Complete the entire tutorial sequence and save the progress
    private void CompleteTutorial()
    {
        DeactivateCurrentStepArrow(); // Deactivate the arrow for the last step
        currentStep = TutorialStep.Completed; // Update the current step to Completed
        playerTutorialArrow.SetActive(false); // Ensure player arrow is disabled
        PlayerPrefs.SetInt(TutorialCompleteKey, 1);
        PlayerPrefs.Save();
        _storeManager?.SetTutorialComplete();
        SaveTutorialProgress();
        Debug.Log("Tutorial completed!");
    }

    // Activate the arrow for the current tutorial step and set the target
    private void ActivateCurrentStepArrow()
    {
        switch (currentStep)
        {
            case TutorialStep.CircuitTable:
                circuitTablePayArrow.SetActive(true);
                currentTarget = circuitTableCollider.transform; // Set the target transform for the player arrow
                playerTutorialArrow.SetActive(true); // Enable player arrow
                break;
            case TutorialStep.CircuitStorage:
                circuitStorageArrow.SetActive(true);
                currentTarget = circuitStorageCollider.transform;
                playerTutorialArrow.SetActive(true); // Enable player arrow
                break;
            case TutorialStep.TableDrop:
                tableDropArrow.SetActive(true);
                currentTarget = tableDropCollider.transform;
                playerTutorialArrow.SetActive(true); // Enable player arrow
                break;
            case TutorialStep.CasePosition:
                casePositionArrow.SetActive(true);
                currentTarget = casePositionCollider.transform;
                playerTutorialArrow.SetActive(true); // Enable player arrow
                break;
            case TutorialStep.Completed:
                playerTutorialArrow.SetActive(false); // Disable player arrow after tutorial
                break;
        }
    }

    // Deactivate the arrow for the previous tutorial step
    private void DeactivateCurrentStepArrow()
    {
        switch (currentStep)
        {
            case TutorialStep.CircuitTable:
                circuitTablePayArrow.SetActive(false);
                break;
            case TutorialStep.CircuitStorage:
                circuitStorageArrow.SetActive(false);
                break;
            case TutorialStep.TableDrop:
                tableDropArrow.SetActive(false);
                break;
            case TutorialStep.CasePosition:
                casePositionArrow.SetActive(false);
                break;
        }

        playerTutorialArrow.SetActive(false); // Disable player arrow
    }

    // Deactivate all arrows at the start of the tutorial
    private void DeactivateAllArrows()
    {
        circuitTablePayArrow.SetActive(false);
        circuitStorageArrow.SetActive(false);
        tableDropArrow.SetActive(false);
        casePositionArrow.SetActive(false);
        playerTutorialArrow.SetActive(false);
    }

    private void UpdatePlayerArrowPositionAndDirection()
    {

        if (currentTarget == null) return;

        Transform playerTransform = playerTutorialArrow.transform.parent;

        Vector3 playerPosition = playerTransform.position;

        Vector3 directionToTarget = (currentTarget.position - playerPosition).normalized;
        Vector3 newArrowPosition = playerPosition + directionToTarget * radius;

        playerTutorialArrow.transform.position = new Vector3(newArrowPosition.x, playerTutorialArrow.transform.position.y, newArrowPosition.z);
        playerTutorialArrow.transform.LookAt(currentTarget);

    }

    private void SaveTutorialProgress()
    {
        PlayerPrefs.SetInt(TutorialStepKey, (int)currentStep);
        PlayerPrefs.Save();
        Debug.Log("Tutorial progress saved: " + currentStep);
    }

    // Load the tutorial progress from PlayerPrefs
    private void LoadTutorialProgress()
    {
        if (PlayerPrefs.HasKey(TutorialStepKey))
        {
            currentStep = (TutorialStep)PlayerPrefs.GetInt(TutorialStepKey);
        }
        else
        {
            currentStep = TutorialStep.CircuitTable; // Start from the first step if no saved data exists
        }

        Debug.Log("Loaded tutorial progress: " + currentStep);
    }

    // Clear the tutorial progress if needed (optional reset method)
    public void ResetTutorialProgress()
    {
        PlayerPrefs.DeleteKey(TutorialStepKey);
        currentStep = TutorialStep.CircuitTable;
        ActivateCurrentStepArrow();
        Debug.Log("Tutorial progress reset.");
    }
}


