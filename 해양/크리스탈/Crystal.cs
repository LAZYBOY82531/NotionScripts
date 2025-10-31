using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Crystal : MonoBehaviour
{
    [SerializeField] public CrystalData crystalData;
    MeshRenderer meshRenderer;
    public bool isNoDelete = true;
    XRGrabInteractable grabInteractable;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        grabInteractable = GetComponent<XRGrabInteractable>();
    }
    private void OnEnable()
    {
        StartCoroutine(MaterialSetting());
        StartCoroutine(DisableObject());
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OffGrab);
    }
    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OffGrab);
        StopAllCoroutines();
    }
    private void OnGrab(SelectEnterEventArgs args)
    {
        isNoDelete = false;
        gameObject.GetComponent<BoxCollider>().isTrigger = true;
    }
    private void OffGrab(SelectExitEventArgs args)
    {
        isNoDelete = true;
        gameObject.GetComponent<BoxCollider>().isTrigger = false;
    }
    public int count;
    public string Name
    {
        get { return crystalData.crystalInfo.name; }
    }
    IEnumerator MaterialSetting()
    {
        yield return new WaitForEndOfFrame();
        meshRenderer.material = crystalData.crystalInfo.material;
    }
    IEnumerator DisableObject()
    {
        yield return new WaitForSeconds(30f);
        if (isNoDelete)
            GameManager.Resource.Destroy(gameObject);
    }
}
