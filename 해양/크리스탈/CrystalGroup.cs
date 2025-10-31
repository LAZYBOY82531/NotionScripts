using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using static Unity.VisualScripting.Member;

public class CrystalGroup : MonoBehaviour
{
    public int crystalType;
    public int crystalPieceCount = 5;
    public CrystalSpawnPoint thisCrystalSpawnPoint;

    List<MeshRenderer> crystalMaterials = new List<MeshRenderer>();
    float hitTime = 0.0f; // ���������� ���� �ð�
    float hitTimeThreshold = 0.2f; // ���� �� �ٽ� �����ϱ������ �ð� ����
    public float crystalHP = 5;
    float currentcrystalHP = 0;
    GameObject crystalPieceObj;
    [SerializeField] Transform[] crystalsSpawnPos;
    Light light; // ���� ������Ʈ
    Color sky;
    Color green;
    Color purple;
    Color yellow;

    AudioSource source;
    [SerializeField] AudioClip hitClip;
    [SerializeField] AudioClip brokenClip;
    

    private void Awake()
    {
        MeshRenderer[] materials = GetComponentsInChildren<MeshRenderer>();
        source = GetComponent<AudioSource>();
        foreach (MeshRenderer meshRenderer in materials)
        {
            if (meshRenderer != null)
            {
                crystalMaterials.Add(meshRenderer);
            }
        }
        light = GetComponentInChildren<Light>();
    }
    private void OnEnable()
    {
        StartCoroutine(MaterialSetting());
        currentcrystalHP = crystalHP;
    }
    public void OnHit(float damage, Vector3 attackPoint)
    {
        if (Time.time - hitTime > hitTimeThreshold)
        {
            Debug.Log("������ ����");
            float pitchRandom = Random.Range(0.5f, 1);
            source.pitch = pitchRandom;
            source.PlayOneShot(hitClip);
            currentcrystalHP -= damage;
            hitTime = Time.time;
            if (currentcrystalHP <= 0)
            {
                AudioSource.PlayClipAtPoint(brokenClip, transform.position, 1.0f);
                BreakCristal();
            }
        }
        ParticleSystem hitEffect1 = GameManager.Resource.Instantiate<ParticleSystem>($"Particle/CrystalHit", attackPoint, Quaternion.identity, null, true);
        GameManager.Resource.Destroy(hitEffect1.gameObject, 2f); // ��ƼŬ ���� �ð� + 1�� �� ����
        ParticleSystem hitEffect2 = hitEffect1.transform.GetChild(0).GetComponent<ParticleSystem>();
        switch (crystalType)
        {
            case 1:
                var main1 = hitEffect1.main;
                main1.startColor = new Color(0.2706f, 0.7608f, 0.8784f, 1f);
                var main2_1 = hitEffect2.main;
                main2_1.startColor = new Color(0.1137f, 0.3490f, 0.6235f, 1f); // �Ķ���
                break;
            case 2:
                var main2 = hitEffect1.main;
                main2.startColor = new Color(0.2980f, 0.8392f, 0.4078f, 1f); // �ʷϻ�
                var main2_2 = hitEffect2.main;
                main2_2.startColor = new Color(0.0980f, 0.4902f, 0.2118f, 1f);
                break;
            case 3:
                var main3 = hitEffect1.main;
                main3.startColor = new Color(0.3922f, 0.1373f, 0.7176f, 1f); // �����
                var main2_3 = hitEffect2.main;
                main2_3.startColor = new Color(0.3176f, 0f, 0.4235f, 1f);
                break;
            case 4:
                var main4 = hitEffect1.main;
                main4.startColor = new Color(0.9686f, 1f, 0.4941f, 1f); // �����
                var main2_4 = hitEffect2.main;
                main2_4.startColor = new Color(0.7255f, 0.6392f, 0.1608f, 1f);
                break;
            default:
                Debug.LogWarning("Unknown crystal type: " + crystalType);
                break;
        }
    }
    void BreakCristal()
    {
        for (int i = 0; i < crystalPieceCount; i++)
        {
            int crystalsize = Random.Range(1, 11);
            if (crystalsize == 10)
            {
                crystalPieceObj = GameManager.Resource.Instantiate<GameObject>($"Crystal/LargeCrystal{(int)(Random.Range(1, 11))}", crystalsSpawnPos[i].position, crystalsSpawnPos[i].rotation, null, true);
                crystalPieceObj.GetComponent<Crystal>().crystalData = GameManager.Resource.Load<CrystalData>($"CrystalData/Crystal{crystalType}");
                crystalPieceObj.GetComponent<Crystal>().count = Random.Range(5, 10);
            }
            else if (crystalsize >= 7)
            {
                crystalPieceObj = GameManager.Resource.Instantiate<GameObject>($"Crystal/MiddleCrystal{(int)(Random.Range(1, 10))}", crystalsSpawnPos[i].position, crystalsSpawnPos[i].rotation, null, true);
                crystalPieceObj.GetComponent<Crystal>().crystalData = GameManager.Resource.Load<CrystalData>($"CrystalData/Crystal{crystalType}");
                crystalPieceObj.GetComponent<Crystal>().count = Random.Range(3, 5);
            }
            else
            {
                crystalPieceObj = GameManager.Resource.Instantiate<GameObject>($"Crystal/SmallCrystal{(int)(Random.Range(1, 9))}", crystalsSpawnPos[i].position, crystalsSpawnPos[i].rotation, null, true);
                crystalPieceObj.GetComponent<Crystal>().crystalData = GameManager.Resource.Load<CrystalData>($"CrystalData/Crystal{crystalType}");
                crystalPieceObj.GetComponent<Crystal>().count = Random.Range(1, 3);
            }
        }
        thisCrystalSpawnPoint.BreakCrystal(this.gameObject); // ���� ����Ʈ�� �ı��� ���� �˸�
        ParticleSystem dust = GameManager.Resource.Instantiate<ParticleSystem>($"Particle/CrystalBreakDust", transform.position, Quaternion.identity, null, true);
        GameManager.Resource.Destroy(dust.gameObject, dust.main.duration + 1f); // ��ƼŬ ���� �ð� + 1�� �� ����
        GameManager.Resource.Destroy(this.gameObject);
    }
    IEnumerator MaterialSetting()
    {
        yield return new WaitForEndOfFrame();
        foreach (MeshRenderer meshRenderer in crystalMaterials)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material = GameManager.Resource.Load<Material>($"CrystalMaterial/Crystal{crystalType}");
            }
        }
        switch (crystalType)
        {
            case 1:
                light.color = sky; // �Ķ���
                break;
            case 2:
                light.color = green; // �ʷϻ�
                break;
            case 3:
                light.color = purple; // �����
                break;
            case 4:
                light.color = yellow; // �����
                break;
            default:
                Debug.LogWarning("Unknown crystal type: " + crystalType);
                break;
        }
        Outline crystalOutline = GetComponent<Outline>();
        if (crystalOutline == null)
        {
            crystalOutline = this.gameObject.AddComponent<Outline>(); // ������ �ƿ����� ������Ʈ �߰�
            crystalOutline.OutlineMode = Outline.Mode.OutlineAll; // �ƿ����� ��� ����
            crystalOutline.OutlineColor = Color.red; // �ƿ����� ���� ����
            crystalOutline.OutlineWidth = 5f; // �ƿ����� �β� ����
        }
        crystalOutline.enabled = thisCrystalSpawnPoint.crystalSpawnPointGroup.isOutline; // �ƿ����� Ȱ��ȭ ���� ����
        thisCrystalSpawnPoint.crystalSpawnPointGroup.outlines.Add(crystalOutline); // ���� ����Ʈ �׷��� �ƿ����� ��Ͽ� �߰�
    }
}
