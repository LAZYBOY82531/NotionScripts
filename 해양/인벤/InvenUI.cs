using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InvenUI : MonoBehaviour
{
    public int crystal1Count = 999;
    public int crystal2Count = 999;
    public int crystal3Count = 999;
    public int crystal4Count = 999;
    public TMPro.TextMeshProUGUI crystal1Text;
    public TMPro.TextMeshProUGUI crystal2Text;
    public TMPro.TextMeshProUGUI crystal3Text;
    public TMPro.TextMeshProUGUI crystal4Text;
    public TMPro.TextMeshProUGUI pinUpgradeText;
    public TMPro.TextMeshProUGUI mainQuestText;
    public TMPro.TextMeshProUGUI weightText;
    public Transform crystalSpawnPos;
    public float crystal1Weight = 0.5f;
    public float crystal2Weight = 1f;
    public float crystal3Weight = 1.5f;
    public float crystal4Weight = 2f;
    UpgradeData pinUpgradeData;
    string pintext = "";
    string mainQuestTextString = "";
    int mainQuestLevel = 0;
    int mainQuestNeedCrystal1 = 0;
    int mainQuestNeedCrystal2 = 0;
    int mainQuestNeedCrystal3 = 0;
    int mainQuestNeedCrystal4 = 0;
    public UpgradeData questData;
    int pinLevel = 0;
    int needCrystal1 = 0;
    int needCrystal2 = 0;
    int needCrystal3 = 0;
    int needCrystal4 = 0;
    float currentWeight = 0;
    public float MaxWeigh;
    public GameObject startTurnOff;
    float dropRate;
    public BoxCrystalView boxCrystalView;
    bool isfirst = true;
    int crystalCount;
    float crystalWeight;
    void OnEnable()
    {
        if (!isfirst) return;
        StartCoroutine(StartRoutine());
    }
    IEnumerator StartRoutine()
    {
        yield return  new WaitForEndOfFrame();
        dropRate = GameManager.Instance.PlayerStats.DropRate;
        MaxWeigh = GameManager.Instance.PlayerStats.InventoryWeight;
        UpdateCrystalUICount();
        startTurnOff.SetActive(false);
        isfirst = false;
    }
    private void UpdateCrystalUICount()
    {
        crystal1Text.text = crystal1Count.ToString();
        crystal2Text.text = crystal2Count.ToString();
        crystal3Text.text = crystal3Count.ToString();
        crystal4Text.text = crystal4Count.ToString();
        weightText.text = $"{currentWeight} / {MaxWeigh}";
        boxCrystalView.SetCrystalView(crystal1Count, crystal2Count, crystal3Count, crystal4Count);
        PinUpgadeUI();
        MainQuestUI();
    }
    private void MainQuestUI()
    {
        mainQuestTextString = "MAIN\n";
        mainQuestLevel = GameManager.Data.saveData.stageLevel;
        questData = GameManager.Resource.Load<UpgradeData>($"UpgradeData/StageData");
        mainQuestNeedCrystal1 = questData.upgradeStepInfo[mainQuestLevel].crystal1Count;
        mainQuestNeedCrystal2 = questData.upgradeStepInfo[mainQuestLevel].crystal2Count;
        mainQuestNeedCrystal3 = questData.upgradeStepInfo[mainQuestLevel].crystal3Count;
        mainQuestNeedCrystal4 = questData.upgradeStepInfo[mainQuestLevel].crystal4Count;
        if (mainQuestNeedCrystal1 != 0)
        {
            mainQuestTextString += $"Crystal : {GameManager.Data.saveData.crystal1Count + crystal1Count} / {mainQuestNeedCrystal1}    ";
        }
        if (mainQuestNeedCrystal2 != 0)
        {
            mainQuestTextString += $"Peridot : {GameManager.Data.saveData.crystal2Count + crystal2Count} / {mainQuestNeedCrystal2}    ";
        }
        if (mainQuestNeedCrystal3 != 0)
        {
            mainQuestTextString += $"Amethyst : {GameManager.Data.saveData.crystal3Count + crystal3Count} / {mainQuestNeedCrystal3}    ";
        }
        if (mainQuestNeedCrystal4 != 0)
        {
            mainQuestTextString += $"Topaz : {GameManager.Data.saveData.crystal4Count + crystal4Count} / {mainQuestNeedCrystal4}";
        }
        mainQuestText.text = mainQuestTextString;
    }
    private void PinUpgadeUI()
    {
        pintext = "";
        pinUpgradeData = GameManager.Resource.Load<UpgradeData>($"UpgradeData/{GameManager.Data.saveData.pinUpgradeDataName}");
        if (pinUpgradeData != null)
        {
            pinLevel = GameManager.Data.UpgradeDataCompareNameForLevel(pinUpgradeData.upgradeName);
            needCrystal1 = pinUpgradeData.upgradeStepInfo[pinLevel].crystal1Count;
            needCrystal2 = pinUpgradeData.upgradeStepInfo[pinLevel].crystal2Count;
            needCrystal3 = pinUpgradeData.upgradeStepInfo[pinLevel].crystal3Count;
            needCrystal4 = pinUpgradeData.upgradeStepInfo[pinLevel].crystal4Count;
            pintext += $"{pinUpgradeData.upgradeName}\n";
        }
        if (needCrystal1 != 0)
        {
            pintext += $"Crystal : {GameManager.Data.saveData.crystal1Count + crystal1Count} / {needCrystal1}    ";
        }
        if (needCrystal2 != 0)
        {
            pintext += $"Peridot : {GameManager.Data.saveData.crystal2Count + crystal2Count} / {needCrystal2}    ";
        }
        if (needCrystal3 != 0)
        {
            pintext += $"Amethyst : {GameManager.Data.saveData.crystal3Count + crystal3Count} / {needCrystal3}    ";
        }
        if (needCrystal4 != 0)
        {
            pintext += $"Topaz : {GameManager.Data.saveData.crystal4Count + crystal4Count} / {needCrystal4}";
        }
        pinUpgradeText.text = pintext;
    }
    public bool GetCrystal(Crystal crystal)
    {
        switch (crystal.crystalData.crystalInfo.name)
        {
            case "Crystal1":
                crystalCount = (int)(crystal.count * dropRate);
                crystalWeight = crystalCount * crystal1Weight;
                if (currentWeight + crystalWeight > MaxWeigh) return false;
                crystal1Count += crystalCount;
                currentWeight += crystalWeight;
                break;
            case "Crystal2":
                crystalCount = (int)(crystal.count * dropRate);
                crystalWeight = crystalCount * crystal2Weight;
                if (currentWeight + crystalWeight > MaxWeigh) return false;
                crystal2Count += crystalCount;
                currentWeight += crystalWeight;
                break;
            case "Crystal3":
                crystalCount = (int)(crystal.count * dropRate);
                crystalWeight = crystalCount * crystal3Weight;
                if (currentWeight + crystalWeight > MaxWeigh) return false;
                crystal3Count += crystalCount;
                currentWeight += crystalWeight;
                break;
            case "Crystal4":
                crystalCount = (int)(crystal.count * dropRate);
                crystalWeight = crystalCount * crystal4Weight;
                if (currentWeight + crystalWeight > MaxWeigh) return false;
                crystal4Count += crystalCount;
                currentWeight += crystalWeight;
                break;
            default:
                Debug.LogWarning("Unknown crystal name: " + name);
                return false;
        }
        UpdateCrystalUICount();
        return true;
    }
    public void ThrowCrystal(int num)
    {
        switch (num)
        {
            case 1:
                if (crystal1Count > 0)
                {
                    crystal1Count--;
                    GameObject crystal = GameManager.Resource.Instantiate<GameObject>($"Crystal/SmallCrystal{(int)(Random.Range(1, 9))}", crystalSpawnPos.position, crystalSpawnPos.rotation, null, true);
                    crystal.GetComponent<Crystal>().crystalData = GameManager.Resource.Load<CrystalData>($"CrystalData/Crystal1");
                    currentWeight -= crystal1Weight;
                }
                else
                {
                    Debug.LogWarning("No Crystal1 to throw.");
                }
                break;
            case 2:
                if (crystal2Count > 0)
                {
                    crystal2Count--;
                    GameObject crystal = GameManager.Resource.Instantiate<GameObject>($"Crystal/SmallCrystal{(int)(Random.Range(1, 9))}", crystalSpawnPos.position, crystalSpawnPos.rotation, null, true);
                    crystal.GetComponent<Crystal>().crystalData = GameManager.Resource.Load<CrystalData>($"CrystalData/Crystal2");
                    currentWeight -= crystal2Weight;
                }
                else
                {
                    Debug.LogWarning("No Crystal2 to throw.");
                }
                break;
            case 3:
                if (crystal3Count > 0)
                {
                    crystal3Count--;
                    GameObject crystal = GameManager.Resource.Instantiate<GameObject>($"Crystal/SmallCrystal{(int)(Random.Range(1, 9))}", crystalSpawnPos.position, crystalSpawnPos.rotation, null, true);
                    crystal.GetComponent<Crystal>().crystalData = GameManager.Resource.Load<CrystalData>($"CrystalData/Crystal3");
                    currentWeight -= crystal3Weight;
                }
                else
                {
                    Debug.LogWarning("No Crystal3 to throw.");
                }
                break;
            case 4:
                if (crystal4Count > 0)
                {
                    crystal4Count--;
                    GameObject crystal = GameManager.Resource.Instantiate<GameObject>($"Crystal/SmallCrystal{(int)(Random.Range(1, 9))}", crystalSpawnPos.position, crystalSpawnPos.rotation, null, true);
                    crystal.GetComponent<Crystal>().crystalData = GameManager.Resource.Load<CrystalData>($"CrystalData/Crystal4");
                    currentWeight -= crystal4Weight;
                }
                else
                {
                    Debug.LogWarning("No Crystal4 to throw.");
                }
                break;
            default:
                Debug.LogWarning("Invalid crystal number: " + num);
                break;
        }
        UpdateCrystalUICount();
    }
}
