using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fallable : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "grass")
        {
            Destroy (collision.gameObject);
            // 
            Destroy (this.GetComponent<Rigidbody>());
            //Destroy (this.GetComponent<Collider>());
            Destroy (this.GetComponent<Fallable>());
        }
            
        
    }
}
