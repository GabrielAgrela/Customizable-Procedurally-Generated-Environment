using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    public GameObject ground; 
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "grass")
        {
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.tag == "terrain")
        {
            // 
            
            
            Destroy (this.GetComponent<Rigidbody>());
            Destroy (this.GetComponent<Collider>());

            ground.SetActive(true);
            Destroy (this.GetComponent<Grass>());
        }
            
        
    }
}


    