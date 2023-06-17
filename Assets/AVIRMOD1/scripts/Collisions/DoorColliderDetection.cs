using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class DoorColliderDetection : MonoBehaviour
{
    public Vector3 SpawnPos;
    public void Start() 
    {
        //transform.rotation = Quaternion.LookRotation(transform.position - Player.transform.position);
        
       // Vector3 lookAtRotation = Quaternion.LookRotation(GameObject.Find("Rocks").transform.GetChild(GameObject.Find("Rocks").transform.childCount - 1).position - transform.position).eulerAngles;
        //transform.rotation = Quaternion.Euler(Vector3.Scale(lookAtRotation, new Vector3(0,1,0)));
    }
}
