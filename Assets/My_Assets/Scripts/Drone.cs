using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public float MinDistance = 3;
    public float MaxDistance = 1;
    public float Speed = 3;
    public Transform Player;


    private void FixedUpdate()
    {
        Speed = Player.GetComponent<CharacterController>().velocity.magnitude;
         if (Vector3.Distance(transform.position, Player.position) >= 1)
        {
        transform.rotation = Quaternion.Slerp(transform.rotation, Player.transform.rotation, Speed * Time.deltaTime);
 
        }
        if (Vector3.Distance(transform.position, Player.position) >= MinDistance)
        {
            
            Vector3 follow = Player.position;
          
            follow.y = this.transform.position.y;
           
            this.transform.position = Vector3.MoveTowards(this.transform.position, follow, Speed * Time.deltaTime);
        }
    }
}
