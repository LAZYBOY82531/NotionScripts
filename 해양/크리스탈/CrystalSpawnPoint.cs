using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalSpawnPoint : MonoBehaviour
{
    public int crystalType; // ������ ���� Ÿ��
    public int crystalCount; // ������ ���� ����
    public int crystalHP; // ������ ü��
    public bool isSpawned = false; // ������ �����Ǿ����� ����
    public CrystalSpawnPointGroup crystalSpawnPointGroup; // ���� ���� ����Ʈ �׷�
    [SerializeField] int crystal2probability = 70; // �ʷϻ� ���� ���� Ȯ��
    [SerializeField] int crystal3probability = 90; // ����� ���� ���� Ȯ��
    [SerializeField] int crystal4probability = 98; // ����� ���� ���� Ȯ��
    

    public GameObject goldChest;

    private void Start()
    {
        crystalSpawnPointGroup = GetComponentInParent<CrystalSpawnPointGroup>();
    }
    public void SpawnCrystal()
    {
        if (isSpawned) return; // �̹� ������ ��� �ߺ� ���� ����
        int i = Random.Range(1,101);
        if (i > crystal4probability)
        {
            crystalType = 4; // 2% Ȯ���� ����� ���� ����
        }
        else if (i > crystal3probability)
        {
            crystalType = 3; // 8% Ȯ���� ����� ���� ����
        }
        else if (i > crystal2probability)
        {
            crystalType = 2; // 20% Ȯ���� �ʷϻ� ���� ����
        }
        else
        {
            crystalType = 1; // 70% Ȯ���� �Ķ��� ���� ����
        }
        crystalCount = Random.Range(1, 6); // 1~5�� ������ ���� ���� ���� ����
        crystalHP = Random.Range(5, 11); // 5 10 ������ ���� ü�� ���� ����
        int num = Random.Range(1, 4); // 1, 2, 3 �� �ϳ��� ���ڸ� �������� ����
        GameObject crystalPrefab = GameManager.Resource.Instantiate<GameObject>($"Crystal/CrystalGroup{num}",transform.position, transform.rotation, true); // ���� ������ �ε�
        if (crystalPrefab == null)
        {
            Debug.LogError($"���� �������� ã�� �� �����ϴ�: CrystalGroup{num}");
            return;
        }
        CrystalGroup crystalGroup = crystalPrefab.GetComponent<CrystalGroup>();
        if (crystalGroup == null)
        {
            Debug.LogError("CrystalGroup ������Ʈ�� ã�� �� �����ϴ�.");
            return;
        }
        crystalGroup.crystalType = crystalType; // ���� Ÿ�� ����
        crystalGroup.crystalPieceCount = crystalCount; // ���� ���� ����
        crystalGroup.crystalHP = crystalHP; // ���� ü�� ����
        crystalGroup.thisCrystalSpawnPoint = this; // ���� ���� ����Ʈ ����
        isSpawned = true; // ������ �����Ǿ����� ǥ��
    }
    public void BreakCrystal(GameObject CrystalObj)
    {
        isSpawned = false; // ������ �ı��Ǿ����� ǥ��
        crystalSpawnPointGroup.BreakCrystal(CrystalObj); // ���� ����Ʈ �׷쿡 �ı��� ���� �˸�
    }
    public void SpawnChest()
    {
        if (isSpawned) return; // �̹� ������ ��� �ߺ� ���� ����

        GameObject goldlPrefab = GameManager.Resource.Instantiate<GameObject>($"GoldChest", transform.position, transform.rotation, true);
        isSpawned = true; // ������ �����Ǿ����� ǥ��
    }
}
