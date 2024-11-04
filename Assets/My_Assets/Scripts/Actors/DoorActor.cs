using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorActor : MonoBehaviour
{
    private Animation doorAnim;
    [SerializeField] private string doorAnimation = "DoorOpen";


    private int entitiesInTrigger = 0;
    private bool isDoorOpen = false;
    
    void Start()
    {
        // Cache the Animation component in the parent
        doorAnim = GetComponentInParent<Animation>();

        if (doorAnim == null)
        {
            Debug.LogError("No Animation component found on the parent object!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Agent")) && doorAnim != null)
        {
            // Increment the count of entities in the trigger
            entitiesInTrigger++;

            // If the door is not already open, play the open animation
            if (!isDoorOpen)
            {
                PlayAnimation(1f);
                isDoorOpen = true; // Mark the door as open
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Agent")) && doorAnim != null)
        {
            // Decrement the count of entities in the trigger
            entitiesInTrigger--;

            // Only play the close animation if no one is left in the trigger
            if (entitiesInTrigger <= 0)
            {
                PlayAnimation(-1f);
                isDoorOpen = false; // Mark the door as closed
                entitiesInTrigger = 0; // Ensure counter doesn’t go negative
            }
        }
    }

    // Helper method to play the animation with a given speed
    private void PlayAnimation(float playbackSpeed)
    {
        // Get the animation state
        AnimationState animState = doorAnim[doorAnimation];

        // Set the playback speed
        animState.speed = playbackSpeed;

        if (playbackSpeed < 0)
        {
            // Play the animation in reverse from the end
            animState.time = animState.length;
        }
        else
        {
            // Play the animation normally from the start
            animState.time = 0f;
        }

        // Play the animation
        doorAnim.Play(doorAnimation);
    }
}
