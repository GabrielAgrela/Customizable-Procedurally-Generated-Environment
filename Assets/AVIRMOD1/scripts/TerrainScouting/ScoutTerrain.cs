using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoutTerrain : MonoBehaviour
{
    public bool isInWater = false;
    public float yPos = 0f;

    public Vector3 hitPos;

    public bool checkIsInWater()
    {
        RaycastHit hit;
        Ray landingRay = new Ray(transform.position,Vector3.down);
        if (Physics.Raycast(landingRay, out hit, 500f))
        {
            
            if (hit.collider.tag == "water")
            {
                yPos=hit.point.y;
                hitPos=hit.point;
                isInWater = true;
                return true;
            }
            else
            {
                yPos=hit.point.y;
                hitPos=hit.point;
                isInWater = false;
                return false;
            }
            
        }
        return isInWater;
    }

    void Update()
    {
        RaycastHit hit;
        Ray landingRay = new Ray(transform.position,Vector3.down);
        if (Physics.Raycast(landingRay, out hit, 500f))
        {
            if (hit.collider.tag == "water" || hit.collider.tag == "transversable")
            {
                Debug.DrawRay(transform.position, Vector2.down * 500f, Color.blue); // try 
            }
            else
            {
                hitPos=hit.point;
                Debug.DrawRay(transform.position, Vector2.down * 500f, Color.red); // try 
            }
        }
    }
}
