using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    public bool isPointerInRange;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "pointer")
        {
            isPointerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "pointer")
        {
            isPointerInRange = false;
        }
    }
}
