using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalSpawnPoint : MonoBehaviour
{
    public int crystalType; // 생성할 광물 타입
    public int crystalCount; // 생성할 광물 개수
    public int crystalHP; // 광물의 체력
    public bool isSpawned = false; // 광물이 생성되었는지 여부
    public CrystalSpawnPointGroup crystalSpawnPointGroup; // 광물 스폰 포인트 그룹
    [SerializeField] int crystal2probability = 70; // 초록색 광물 생성 확률
    [SerializeField] int crystal3probability = 90; // 보라색 광물 생성 확률
    [SerializeField] int crystal4probability = 98; // 노란색 광물 생성 확률
    

    public GameObject goldChest;

    private void Start()
    {
        crystalSpawnPointGroup = GetComponentInParent<CrystalSpawnPointGroup>();
    }
    public void SpawnCrystal()
    {
        if (isSpawned) return; // 이미 생성된 경우 중복 생성 방지
        int i = Random.Range(1,101);
        if (i > crystal4probability)
        {
            crystalType = 4; // 2% 확률로 노란색 광물 생성
        }
        else if (i > crystal3probability)
        {
            crystalType = 3; // 8% 확률로 보라색 광물 생성
        }
        else if (i > crystal2probability)
        {
            crystalType = 2; // 20% 확률로 초록색 광물 생성
        }
        else
        {
            crystalType = 1; // 70% 확률로 파란색 광물 생성
        }
        crystalCount = Random.Range(1, 6); // 1~5개 사이의 광물 개수 랜덤 선택
        crystalHP = Random.Range(5, 11); // 5 10 사이의 광물 체력 랜덤 선택
        int num = Random.Range(1, 4); // 1, 2, 3 중 하나의 숫자를 랜덤으로 선택
        GameObject crystalPrefab = GameManager.Resource.Instantiate<GameObject>($"Crystal/CrystalGroup{num}",transform.position, transform.rotation, true); // 광물 프리팹 로드
        if (crystalPrefab == null)
        {
            Debug.LogError($"광물 프리팹을 찾을 수 없습니다: CrystalGroup{num}");
            return;
        }
        CrystalGroup crystalGroup = crystalPrefab.GetComponent<CrystalGroup>();
        if (crystalGroup == null)
        {
            Debug.LogError("CrystalGroup 컴포넌트를 찾을 수 없습니다.");
            return;
        }
        crystalGroup.crystalType = crystalType; // 광물 타입 설정
        crystalGroup.crystalPieceCount = crystalCount; // 광물 개수 설정
        crystalGroup.crystalHP = crystalHP; // 광물 체력 설정
        crystalGroup.thisCrystalSpawnPoint = this; // 현재 스폰 포인트 설정
        isSpawned = true; // 광물이 생성되었음을 표시
    }
    public void BreakCrystal(GameObject CrystalObj)
    {
        isSpawned = false; // 광물이 파괴되었음을 표시
        crystalSpawnPointGroup.BreakCrystal(CrystalObj); // 스폰 포인트 그룹에 파괴된 광물 알림
    }
    public void SpawnChest()
    {
        if (isSpawned) return; // 이미 생성된 경우 중복 생성 방지

        GameObject goldlPrefab = GameManager.Resource.Instantiate<GameObject>($"GoldChest", transform.position, transform.rotation, true);
        isSpawned = true; // 광물이 생성되었음을 표시
    }
}
