
using System;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
using Cinemachine;
using JetBrains.Annotations;

public class BaseCharControl : MonoBehaviour
{
    [SerializeField] private GameObject Finger;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private VariableJoystick joystick;

    [SerializeField] public float moveSpeed;
    [SerializeField] private float rotationSpeed;
    Vector3 moveDirection;
    Vector3 targetDirection;

    float walkspeed = 0.6f;
    [SerializeField] private Animator _animator;
    private bool isMoving;
    private float inactivityTimer = 0f;
    private float inactivityLimit = 5f; // 5 seconds inactivity limit
    void Start()
    {
        // Set initial animation state to idle
        _animator.SetBool("Idle", true);
        _animator.SetBool("Running", false);
        _animator.SetBool("Walking", false);
    }

    private void FixedUpdate()
    {
        PlayerMove();
        TrackInactivity();
    }

    private void PlayerMove()
    {
        // Get joystick input values
        float hoz = joystick.Horizontal;
        float ver = joystick.Vertical;

        // Check if there's any joystick input (movement)
        isMoving = Mathf.Abs(hoz) > 0.15f || Mathf.Abs(ver) > 0.15f;

        if (isMoving)
        {
            // Reset the inactivity timer since the player is active
            inactivityTimer = 0f;

            // Deactivate the Finger GameObject
            if (Finger.activeSelf)
            {
                Finger.SetActive(false);
            }

            // Calculate movement direction based on joystick input
            moveDirection = new Vector3(hoz, 0, ver).normalized;

            // Move the character
            _characterController.SimpleMove(moveDirection * moveSpeed);

            // Rotate the character smoothly towards the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            _characterController.transform.rotation = Quaternion.Slerp(
                _characterController.transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime
            );
        }
        else
        {
            // Stop the character immediately if no joystick input
            moveDirection = Vector3.zero;
            _characterController.SimpleMove(Vector3.zero);
        }

        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (!isMoving) // No movement input
        {
            _animator.SetBool("Running", false);
            _animator.SetBool("Walking", false);
            _animator.SetBool("Idle", true);
        }
        else // Movement input detected
        {
            _animator.SetBool("Idle", false);

            // Determine if the player is running or walking based on moveSpeed
            float currentSpeed = _characterController.velocity.magnitude / moveSpeed;

            if (currentSpeed >= 0.5f) // Running threshold
            {
                _animator.SetBool("Walking", false);
                _animator.SetBool("Running", true);
                _animator.SetFloat("RunSpeed", currentSpeed);
            }
            else // Walking animation if below running threshold
            {
                _animator.SetBool("Running", false);
                _animator.SetBool("Walking", true);
                _animator.SetFloat("WalkSpeed", walkspeed);
            }
        }
    }

    private void TrackInactivity()
    {
        // Increment inactivity timer
        inactivityTimer += Time.deltaTime;

        // If 5 seconds pass with no interaction, reactivate the Finger GameObject
        if (inactivityTimer >= inactivityLimit && !Finger.activeSelf)
        {
            Finger.SetActive(true);
        }
    }

    public void PlayerCarryAnimation(bool isCarry)
    {
        _animator.SetBool("Carry", isCarry);
    }
}
