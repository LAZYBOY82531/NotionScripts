using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class BoxCrystalView : MonoBehaviour
{
    public GameObject littleCrystals;
    public GameObject middleCrystals;
    public GameObject aLotCrystals;
    int allCrystalCount = 0;
    private void Start()
    {
        littleCrystals.SetActive(false);
        middleCrystals.SetActive(false);
        aLotCrystals.SetActive(false);
    }
    public void SetCrystalView(int crystal1, int crystal2, int crystal3, int crystal4)
    {
        allCrystalCount = crystal1 + crystal2 + crystal3 + crystal4;
        littleCrystals.SetActive(allCrystalCount >= 10);
        middleCrystals.SetActive(allCrystalCount >= 30);
        aLotCrystals.SetActive(allCrystalCount >= 50);
    }
}
