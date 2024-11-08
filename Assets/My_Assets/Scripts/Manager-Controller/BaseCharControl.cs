
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
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private FloatingJoystick joystick;

    [SerializeField] public float moveSpeed;
    [SerializeField] private float rotationSpeed;
    Vector3 moveDirection;
    Vector3 targetDirection;

    float walkspeed = 0.6f;
    [SerializeField] private Animator _animator;


    // Start is called before the first frame update
    private void FixedUpdate()
    {
        PlayerMove1();
        //playerMove();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void PlayerMove1()
    {
        float hoz = joystick.Horizontal;
        float ver = joystick.Vertical;
        moveDirection = new Vector3(joystick.Direction.x, 0, joystick.Direction.y);

        _characterController.SimpleMove(moveDirection * moveSpeed);

        targetDirection = Vector3.RotateTowards(_characterController.transform.forward,
        moveDirection, rotationSpeed * Time.fixedDeltaTime, 0.0f);

        _characterController.transform.rotation = Quaternion.LookRotation(targetDirection);

        float currentSpeed = _characterController.velocity.magnitude / moveSpeed;

        if (moveDirection.sqrMagnitude <= 0)
        {
            _animator.SetBool("Running", false);
            _animator.SetBool("Walking", false);
            _animator.SetBool("Idle", true);
        }
        else
        {
            bool isRunning = _animator.GetCurrentAnimatorStateInfo(0).IsName("RunForward");
            if (currentSpeed >= 0.5f)
            {
                _animator.SetBool("Idle", false);
                _animator.SetBool("Walking", false);
                _animator.SetBool("Running", true);
                _animator.SetFloat("RunSpeed", currentSpeed);

            }
            else if (moveDirection.sqrMagnitude != 0 || currentSpeed <= 0.5)
            {

                _animator.SetBool("Running", false);
                _animator.SetBool("Idle", false);
                _animator.SetBool("Walking", true);
                _animator.SetFloat("WalkSpeed", walkspeed);

            }
        }
    }

    public void PlayerCarryAnimation(bool isCarry)
    {
        _animator.SetBool("Carry", isCarry);
    }

}
