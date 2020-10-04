using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchScript : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Touchy the chicken!");
    }
}
