using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeInfoUI : MonoBehaviour
{
    public UpgradeData upgradeData;
    public Transform upgradeInfoUIPos;
    int level;
    bool isInfoUIActive = false;
    GameObject upgradeButton;
    GameObject infoUI;
    public GameObject infoUIObj;
    TMP_Text upgradeLevelText;
    int containerCrystal1;
    int containerCrystal2;
    int containerCrystal3;
    int containerCrystal4;
    int useCrystal1;
    int useCrystal2;
    int useCrystal3;
    int useCrystal4;
    public Action crystalused;
    private void Start()
    {
        level = GameManager.Data.UpgradeDataCompareNameForLevel(upgradeData.upgradeName) + 1;
        isInfoUIActive = level <= upgradeData.upgradeStepInfo.Length;
        Button infoButton = GetComponent<Button>();
        upgradeLevelText = transform.GetChild(0).GetComponent<TMP_Text>();
        upgradeLevelText.text = isInfoUIActive ? $"{level}LV" : "MAX";
        infoButton.onClick.AddListener(OnInfoButtonClicked);
    }
    private void OnInfoButtonClicked()
    {
        isInfoUIActive = level <= upgradeData.upgradeStepInfo.Length;
        infoUIObj = GameManager.Resource.Instantiate<GameObject>($"UI/UpgradeInfoUI", upgradeInfoUIPos.position, upgradeInfoUIPos.rotation, true);
        infoUI = infoUIObj.transform.GetChild(0).gameObject;
        infoUI.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(DisableInfoUI);
        infoUI.transform.GetChild(1).GetComponent<TMP_Text>().text = upgradeData.upgradeName;
        infoUI.transform.GetChild(3).GetComponent<TMP_Text>().text = upgradeData.comment;
        upgradeButton = infoUI.transform.GetChild(6).gameObject;
        upgradeButton.GetComponent<Button>().onClick.AddListener(Upgrade);
        infoUI.transform.GetChild(7).GetComponent<Button>().onClick.AddListener(PinUpgrade);
        infoUI.transform.GetChild(8).gameObject.SetActive(upgradeData == GameManager.Data.pinUpgradeData);
        UpdateInfoUI(infoUI);
    }

    private void UpdateInfoUI(GameObject infoUI)
    {
        isInfoUIActive = level < upgradeData.upgradeStepInfo.Length;
        infoUI.transform.GetChild(2).GetComponent<TMP_Text>().text = isInfoUIActive ? $"{level} LV" : "MAX";
        infoUI.transform.GetChild(4).GetComponent<TMP_Text>().text = isInfoUIActive ? $"{upgradeData.upgradeStepInfo[level - 1].value} >> {upgradeData.upgradeStepInfo[level].value}" : $"{upgradeData.upgradeStepInfo[level - 1].value}";
        GameObject crystal1count = infoUI.transform.GetChild(5).GetChild(0).gameObject;
        containerCrystal1 = GameManager.Data.saveData.crystal1Count;
        useCrystal1 = upgradeData.upgradeStepInfo[level - 1].crystal1Count;
        crystal1count.SetActive(useCrystal1 !=0);
        if (crystal1count.activeSelf)
        {
            TMP_Text crystal1text = crystal1count.GetComponent<TMP_Text>();
            crystal1text.text = $"Crystal : {containerCrystal1} / {useCrystal1}";
            if (containerCrystal1 < useCrystal1)
                crystal1text.color = Color.red;
            else
                crystal1text.color = Color.green;
        }
        GameObject crystal2count = infoUI.transform.GetChild(5).GetChild(1).gameObject;
        containerCrystal2 = GameManager.Data.saveData.crystal2Count;
        useCrystal2 = upgradeData.upgradeStepInfo[level - 1].crystal2Count;
        crystal2count.SetActive(useCrystal2 != 0);
        if (crystal2count.activeSelf)
        {
            TMP_Text crystal2text = crystal2count.GetComponent<TMP_Text>();
            crystal2text.text = $"Peridot : {containerCrystal2} / {useCrystal2}";
            if (containerCrystal2 < useCrystal2)
                crystal2text.color = Color.red;
            else
                crystal2text.color = Color.green;
        }
        GameObject crystal3count = infoUI.transform.GetChild(5).GetChild(2).gameObject;
        containerCrystal3 = GameManager.Data.saveData.crystal3Count;
        useCrystal3 = upgradeData.upgradeStepInfo[level - 1].crystal3Count;
        crystal3count.SetActive(useCrystal3 != 0);
        if (crystal3count.activeSelf)
        {
            TMP_Text crystal3text = crystal3count.GetComponent<TMP_Text>();
            crystal3text.text = $"Amethyst : {containerCrystal3} / {useCrystal3}";
            if (containerCrystal3 < useCrystal3)
                crystal3text.color = Color.red;
            else
                crystal3text.color = Color.green;
        }
        GameObject crystal4count = infoUI.transform.GetChild(5).GetChild(3).gameObject;
        containerCrystal4 = GameManager.Data.saveData.crystal4Count;
        useCrystal4 = upgradeData.upgradeStepInfo[level - 1].crystal4Count;
        crystal4count.SetActive(useCrystal4 != 0);
        if (crystal4count.activeSelf)
        {
            TMP_Text crystal4text = crystal4count.GetComponent<TMP_Text>();
            crystal4text.text = $"Topaz : {containerCrystal4} / {useCrystal4}";
            if (containerCrystal4 < useCrystal4)
                crystal4text.color = Color.red;
            else
                crystal4text.color = Color.green;
        }
        upgradeButton.SetActive(isInfoUIActive);
    }
    public void DisableInfoUI()
    {
        upgradeButton.GetComponent<Button>().onClick.RemoveListener(Upgrade);
        infoUI.transform.GetChild(7).GetComponent<Button>().onClick.RemoveListener(PinUpgrade);
        infoUI.transform.GetChild(0).GetComponent<Button>().onClick.RemoveListener(DisableInfoUI);
        GameManager.Resource.Destroy(infoUIObj);
    }

    private void Upgrade()
    {
        if (containerCrystal1 >= useCrystal1 && containerCrystal2 >= useCrystal2 && containerCrystal3 >= useCrystal3 && containerCrystal4 >= useCrystal4)
        {
            level++;
            GameManager.Data.saveData.crystal1Count -= useCrystal1;
            GameManager.Data.saveData.crystal2Count -= useCrystal2;
            GameManager.Data.saveData.crystal3Count -= useCrystal3;
            GameManager.Data.saveData.crystal4Count -= useCrystal4;
            GameManager.Data.UpgradeDataCompareNameForLevel(upgradeData.upgradeName, 1);
            UpdateInfoUI(infoUI);
            upgradeLevelText.text = isInfoUIActive ? $"{level}Lv" : "MAX";
            crystalused?.Invoke();
        }
    }
    private void PinUpgrade()
    {
        GameManager.Data.PinUpgradeData(upgradeData);
        infoUI.transform.GetChild(8).gameObject.SetActive(upgradeData == GameManager.Data.pinUpgradeData);
    }
}
