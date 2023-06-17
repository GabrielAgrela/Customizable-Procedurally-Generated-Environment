using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCheck : MonoBehaviour
{
    public GameObject[] scouts;
    public bool Placable = false;
    public bool CheckIsInWater()
    {
        foreach (GameObject scout in scouts)
        {
            if (scout.GetComponent<ScoutTerrain>().checkIsInWater() == false)
            {
                Placable=false; 
                return Placable;
            }
            else
            {
                Placable=true;
            }
        }
        return Placable;
    }
}
