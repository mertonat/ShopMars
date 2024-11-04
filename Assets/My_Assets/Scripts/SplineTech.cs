using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;

public class SplineTech : MonoBehaviour
{
    public SplineFollower _follower;
    public bool isCargo;

    void Start()
    {
        _follower = GetComponent<SplineFollower>();
        _follower.follow = false;
    }

    void Update()
    {

    }
    public void CargoArrivePoint()
    {
        print("Trigger 1 ");
        _follower.follow = false;
        isCargo = true;
    }

    public void CargoMove()
    {
        _follower.follow = true;


    }
    public void waitForCargo()
    {

        _follower.follow = false;
    }
}
