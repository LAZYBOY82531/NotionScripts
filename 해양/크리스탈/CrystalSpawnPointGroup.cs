using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrystalSpawnPointGroup : MonoBehaviour
{
    [SerializeField] CrystalSpawnPoint[] spawnPoints; // ���� ��ġ ���
    [SerializeField] int startSpawnCount = 5; // ������ ���� ����
    [SerializeField] int maxSpawnCount; // �ִ� ���� ����
    public int currentSpawnCount = 0; // ���� ������ ���� ����
    WaitForSeconds spawnTime = new WaitForSeconds(15f); // ���� ���� ����
    public bool isOutline = false; // �ƿ����� ǥ�� ����
    public List<Outline> outlines = new List<Outline>();

    int chestCheck;
    void Start()
    {
        chestCheck = 0;
        spawnPoints = GetComponentsInChildren<CrystalSpawnPoint>();
        maxSpawnCount = spawnPoints.Length; // �ִ� ���� ������ ���� ����Ʈ�� ����
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
                if (spawnPoints[randomChestIndex].isSpawned) continue; // �̹� ������ ��� �ǳʶٱ�
                spawnPoints[randomChestIndex].SpawnChest(); // ���� ����
                currentSpawnCount++;
            }
        }

        while(startSpawnCount > currentSpawnCount)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            if (spawnPoints[randomIndex].isSpawned) continue; // �̹� ������ ��� �ǳʶٱ�
            spawnPoints[randomIndex].SpawnCrystal(); // ���� ����
            currentSpawnCount++;
        }
    }
    public void BreakCrystal(GameObject crystalObj)
    {
        currentSpawnCount--;
        outlines.Remove(crystalObj.GetComponent<Outline>()); // �ı��� ������ �ƿ����� ����
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
            if (maxSpawnCount == currentSpawnCount) // �ִ� ���� ������ ������ ���
            {
                yield return spawnTime;
                Debug.Log("��� ������ �����Ǿ����ϴ�. �߰� ������ �Ұ����մϴ�.");
                continue;
            }
            if (spawnPoints[randomIndex].isSpawned) continue; // �̹� ������ ��� �ǳʶٱ�
            spawnPoints[randomIndex].SpawnCrystal(); // ���� ����
            currentSpawnCount++;
            yield return spawnTime;
        }
    }
    IEnumerator DrawOutlineCrystalGroupsRoutine()
    {
        if (isOutline) yield break; // �̹� �ƿ������� ǥ�õ� ��� �ߴ�
        isOutline = true;
        foreach (Outline outline in outlines)
        {
            if (outline != null)
            {
                outline.enabled = true; // �ƿ����� Ȱ��ȭ
            }
        }
        yield return new WaitForSeconds(5f);
        foreach (Outline outline in outlines)
        {
            if (outline != null)
            {
                outline.enabled = false; // �ƿ����� Ȱ��ȭ
            }
        }
        isOutline = false; // �ƿ����� ǥ�� ���� �ʱ�ȭ
        yield return null;
    }
}
