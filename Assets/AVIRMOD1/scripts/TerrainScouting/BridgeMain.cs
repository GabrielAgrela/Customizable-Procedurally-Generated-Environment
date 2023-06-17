using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeMain : MonoBehaviour
{
    public bool Placable = false;
    public GameObject LandCheck;
    public GameObject WaterCheck;
    public GameObject YPosCheck;

    public bool checkPlacable()
    {
        if (LandCheck.GetComponent<LandCheck>().CheckIsInWater() == true && WaterCheck.GetComponent<WaterCheck>().CheckIsInWater() == true)
            return true;
        else
            return false;
    }
}
