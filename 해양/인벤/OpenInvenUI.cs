using UnityEngine;
using System;
using TMPro;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class OpenInvenUI : MonoBehaviour
{
    public GameObject invenUI;
    public GameObject optionUI;
    public GameObject closeUI;

    public bool _isReturnCancle = false;
    public event Action OnReturnCancle;
    public InvenUI invenUIScript;
    public GameObject returnUI;
    public TextMeshProUGUI countTxt;
    WaitForSeconds countDown = new WaitForSeconds(1f);
    WaitForSeconds cancleBoolWs = new WaitForSeconds(10f);

    public LocomotionSystem LocomotionSystem;
    bool isReturnClicked = false;

    private void Start()
    {
        if(returnUI != null) returnUI.SetActive(false);
    }
    public bool IsReturnCancle
    {
        get { return _isReturnCancle; }
        set
        {
            if (!_isReturnCancle && value)
            {
                OnReturnCancle?.Invoke(); // �̺�Ʈ ȣ��
            }
            _isReturnCancle = value;
        }
    }

    // ��ư
    public void OnReturn()
    {
        if(isReturnClicked) return;
        StopAllCoroutines(); // �ߺ� ���� ����
        returnUI.SetActive(true);
        isReturnClicked = true;
        LocomotionSystem.gameObject.SetActive(false);
        PlayerStateManager.Instance.SetMovementEnabled(false);
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        // ī��Ʈ�ٿ� ���� �� Chase ����
        PlayerCheck player = FindObjectOfType<PlayerCheck>();
        if (player != null)
        {
            Collider[] colliders = Physics.OverlapSphere(
                player.transform.position,
                10f,
                LayerMask.GetMask("FISH")
            );

            foreach (Collider col in colliders)
            {
                FishState fish = col.GetComponent<FishState>();
                FishD fishD = col.GetComponent<FishD>();
                FishPatrol patrol = col.GetComponent<FishPatrol>();

                if (fish != null && fishD == null && patrol != null)
                {
                    patrol.SetTarget(player.transform);
                    fish.ChangeState(FishAIState.Chase);

                    // ���� �ð� ���� ������ �÷��̾ Ÿ������ ����
                    StartCoroutine(ForceChase(fish, patrol, player.transform, 10f));
                }
            }
        }

        // ī��Ʈ�ٿ� ����
        int count = 3;
        while (count > 0)
        {
            countTxt.text = count.ToString();
            yield return countDown;
            count--;
        }

        countTxt.text = "";
        returnUI.SetActive(false);
        GameManager.Data.saveData.crystal1Count += invenUIScript.crystal1Count;
        GameManager.Data.saveData.crystal2Count += invenUIScript.crystal2Count;
        GameManager.Data.saveData.crystal3Count += invenUIScript.crystal3Count;
        GameManager.Data.saveData.crystal4Count += invenUIScript.crystal4Count;
        print(GameManager.Data.saveData.crystal1Count);
        print(GameManager.Data.saveData.crystal2Count);
        print(GameManager.Data.saveData.crystal3Count);
        print(GameManager.Data.saveData.crystal4Count);
        GameManager.Data.saveData.dueDate--;
        GameManager.Data.SaveGameData();
        PlayerStateManager.Instance.SetMovementEnabled(true);
        GameManager.Data.LoadGameData();
        GameManager.Scene.GoSeaToBaseScene(); // �� �̵�
    }

    private IEnumerator ForceChase(FishState fish, FishPatrol patrol, Transform player, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (fish != null && patrol != null && player != null)
            {
                patrol.SetTarget(player); // ��� Ÿ�� ����
                if (fish.currentState != FishAIState.Attack)
                    fish.ChangeState(FishAIState.Chase); // ���� ���� �ƴ� ���� Chase ����
            }
            timer += 0.5f; // 0.5�ʸ��� ����
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnCancleReturn()
    {
        StopAllCoroutines();
        returnUI.SetActive(false);
        isReturnClicked = false;
        LocomotionSystem.gameObject.SetActive(true);
        PlayerStateManager.Instance.SetMovementEnabled(true);
        StartCoroutine(cancleboolCtrl());
    }
    IEnumerator cancleboolCtrl()
    {
        IsReturnCancle = true;
        yield return cancleBoolWs;
        IsReturnCancle = false;
    }

    public void OpenInven()
    {
        if (invenUI != null)
        {
            if (!invenUI.activeSelf && optionUI.activeSelf)
            {
                optionUI.SetActive(false);
                closeUI.SetActive(false);
                return;
            }
            invenUI.SetActive(!invenUI.activeSelf);
            optionUI.SetActive(false);
            closeUI.SetActive(!closeUI.activeSelf);
        }
    }
    public void OpenOption()
    {
        invenUI.SetActive(false);
        optionUI.SetActive(true);
    }
    public void OpenInventory()
    {
        invenUI.SetActive(true);
        optionUI.SetActive(false);
    }
    public void CloseInventory()
    {
        invenUI.SetActive(false);
        optionUI.SetActive(false);
        closeUI.SetActive(false);
    }
}