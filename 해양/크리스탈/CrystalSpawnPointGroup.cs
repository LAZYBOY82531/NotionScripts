using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrystalSpawnPointGroup : MonoBehaviour
{
    [SerializeField] CrystalSpawnPoint[] spawnPoints; // 생성 위치 목록
    [SerializeField] int startSpawnCount = 5; // 생성할 광물 개수
    [SerializeField] int maxSpawnCount; // 최대 생성 개수
    public int currentSpawnCount = 0; // 현재 생성된 광물 개수
    WaitForSeconds spawnTime = new WaitForSeconds(15f); // 광물 생성 간격
    public bool isOutline = false; // 아웃라인 표시 여부
    public List<Outline> outlines = new List<Outline>();

    int chestCheck;
    void Start()
    {
        chestCheck = 0;
        spawnPoints = GetComponentsInChildren<CrystalSpawnPoint>();
        maxSpawnCount = spawnPoints.Length; // 최대 생성 개수는 스폰 포인트의 개수
        SceneStartSpawn();
        StartCoroutine(SpawnCrystalRoutine());
    }
    void SceneStartSpawn()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        while (chestCheck < 2 && currentScene != "UnderSeaLv0")
        {
            chestCheck++;
            if (Random.Range(0,2) == 1 && GameManager.Data.saveData.stageLevel != 0)
            {
                int randomChestIndex = Random.Range(0, spawnPoints.Length);
                if (spawnPoints[randomChestIndex].isSpawned) continue; // 이미 생성된 경우 건너뛰기
                spawnPoints[randomChestIndex].SpawnChest(); // 상자 생성
                currentSpawnCount++;
            }
        }

        while(startSpawnCount > currentSpawnCount)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            if (spawnPoints[randomIndex].isSpawned) continue; // 이미 생성된 경우 건너뛰기
            spawnPoints[randomIndex].SpawnCrystal(); // 광물 생성
            currentSpawnCount++;
        }
    }
    public void BreakCrystal(GameObject crystalObj)
    {
        currentSpawnCount--;
        outlines.Remove(crystalObj.GetComponent<Outline>()); // 파괴된 광물의 아웃라인 제거
    }
    public void DrawOutlineCrystalGroups()
    {
        StartCoroutine(DrawOutlineCrystalGroupsRoutine());
    }
    IEnumerator SpawnCrystalRoutine()
    {
        yield return spawnTime;
        while (true)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            if (maxSpawnCount == currentSpawnCount) // 최대 생성 개수에 도달한 경우
            {
                yield return spawnTime;
                Debug.Log("모든 광물이 생성되었습니다. 추가 생성이 불가능합니다.");
                continue;
            }
            if (spawnPoints[randomIndex].isSpawned) continue; // 이미 생성된 경우 건너뛰기
            spawnPoints[randomIndex].SpawnCrystal(); // 광물 생성
            currentSpawnCount++;
            yield return spawnTime;
        }
    }
    IEnumerator DrawOutlineCrystalGroupsRoutine()
    {
        if (isOutline) yield break; // 이미 아웃라인이 표시된 경우 중단
        isOutline = true;
        foreach (Outline outline in outlines)
        {
            if (outline != null)
            {
                outline.enabled = true; // 아웃라인 활성화
            }
        }
        yield return new WaitForSeconds(5f);
        foreach (Outline outline in outlines)
        {
            if (outline != null)
            {
                outline.enabled = false; // 아웃라인 활성화
            }
        }
        isOutline = false; // 아웃라인 표시 상태 초기화
        yield return null;
    }
}
