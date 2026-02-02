using System;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private Collider coll = null;
    
    void Start()
    {
        coll = GetComponent<Collider>();
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            Debug.Log("Goal");
        }
    }
}
