using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkCollision : MonoBehaviour
{
    public Rigidbody rb;
    public int  frame = 0;
    public void Start ()
    {
        Physics.gravity = new Vector3(0, -50.0F, 0);
        rb = GetComponent<Rigidbody>();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "grass")
            Destroy(collision.gameObject);
    }   
}
