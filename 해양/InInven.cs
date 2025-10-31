using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InInven : MonoBehaviour
{
    public InvenUI invenUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Crystal"))
        {
            Crystal crystal = other.GetComponent<Crystal>();
            if (crystal != null)
            {
                if(invenUI.GetCrystal(crystal))
                    GameManager.Resource.Destroy(other.gameObject);
            }
        }
    }
}
