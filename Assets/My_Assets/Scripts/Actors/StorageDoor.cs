using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageDoor : MonoBehaviour
{
    private Animation doorAnim;
    [SerializeField] private string doorAnimation = "StorageOutSideDoor";

    void Start()
    {
        doorAnim = GetComponentInParent<Animation>();

        if (doorAnim == null)
        {
            Debug.LogError("No Animation component found on the parent object!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("CargoMan") && doorAnim != null)
        {
            PlayAnimation(1f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
        if (other.CompareTag("CargoMan") && doorAnim != null)
        {
            PlayAnimation(-1f);
        }
    }


    private void PlayAnimation(float playbackSpeed)
    {
        AnimationState animState = doorAnim[doorAnimation];
        animState.speed = playbackSpeed;

        if (playbackSpeed < 0)
        {
            animState.time = animState.length;
        }
        else
        {
            animState.time = 0f;
        }
        doorAnim.Play(doorAnimation);
    }
}
