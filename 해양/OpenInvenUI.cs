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
                OnReturnCancle?.Invoke(); // 이벤트 호출
            }
            _isReturnCancle = value;
        }
    }

    // 버튼
    public void OnReturn()
    {
        if(isReturnClicked) return;
        StopAllCoroutines(); // 중복 실행 방지
        returnUI.SetActive(true);
        isReturnClicked = true;
        LocomotionSystem.gameObject.SetActive(false);
        PlayerStateManager.Instance.SetMovementEnabled(false);
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        // 카운트다운 시작 시 Chase 강제
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

                    // 일정 시간 동안 강제로 플레이어를 타겟으로 유지
                    StartCoroutine(ForceChase(fish, patrol, player.transform, 10f));
                }
            }
        }

        // 카운트다운 진행
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
        GameManager.Scene.GoSeaToBaseScene(); // 씬 이동
    }

    private IEnumerator ForceChase(FishState fish, FishPatrol patrol, Transform player, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (fish != null && patrol != null && player != null)
            {
                patrol.SetTarget(player); // 계속 타겟 유지
                if (fish.currentState != FishAIState.Attack)
                    fish.ChangeState(FishAIState.Chase); // 공격 중이 아닐 때만 Chase 유지
            }
            timer += 0.5f; // 0.5초마다 갱신
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