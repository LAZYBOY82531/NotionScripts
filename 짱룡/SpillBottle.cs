using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiquidVolumeFX;
using static UnityEngine.ParticleSystem;

public class SpillBottle : MonoBehaviour
{
    public float spillRate = 0.003f;
    GameObject psgo;
    public float pourAngle = 90f;    // ����� ���� ���ذ� (90~120 ���� ����)
    public Transform pourPoint; // ��ü�� ������� ��ġ

    [Header("�⺻ ����")]
    public ParticleSystem ps;
    public string liquidName;
    public float liquidAmount = 0.001f;
    public float MaxAmount = 0.9f;

    [Header("������ ���� ����")]
    public Transform origin;           // Ray ���� ��ġ
    public float initialSpeed = 0.68f;    // �ʱ� �ӵ�
    public float gravity = -9.81f;     // �߷� ��
    public float stepTime = 0.05f;     // �� �ܰ� �ùķ��̼� ����
    public int maxSteps = 50;          // �ùķ��̼� Ƚ��
    public LayerMask hitLayers;        // �浹 ��� Layer

    [Header("���۷���")]
    public LiquidVolume lv;
    public WaterType wt;
    bool isHavingLiquid = false;
    int layerIndex = -1;
    private List<Vector3> trajectoryPoints = new List<Vector3>();
    ParticleSystemRenderer psr;
    static MaterialPropertyBlock _block;

    [Header("���Ҽ�(���Ҽ��� ����ҰŸ� lv�� null�� �ؾߵ�)")]
    [SerializeField] Color liquidColor1;
    [SerializeField] Color liquidColor2;
    [SerializeField] [Range(0, 1)] float murkiness;
    [SerializeField] ParticleSystem dispencerParticle;
    [Header("Raycast ���� (���Ҽ���)")]
    Ray ray;
    RaycastHit hit;
    [SerializeField] float rayDistance = 20f;
    [SerializeField] LayerMask layerMask;
    public bool isAction =false;

    private void Start()
    {
        if (_block == null)
        {
            _block = new MaterialPropertyBlock();
        }
        lv = GetComponent<LiquidVolume>();
        ray = new Ray(pourPoint.position, Vector3.down);
        if (dispencerParticle == null)
            dispencerParticle = GetComponentInChildren<ParticleSystem>();
    }
    private void OnDisable()
    {
        if (psgo != null)
        {
            GameManager.Resource.Destroy(psgo);
            psgo = null;
        }
        if (lv != null)
        {
            if (lv.level < 0.8f)
            {
                lv.level = 0.8f;
            }
        }
    }
    private void Update()
    {
        if (lv != null)
        {
            float dot = Vector3.Dot(transform.forward, Vector3.up);
            if (dot > 0) // ���� ���� �̻� �Ʒ��� ���ϸ�
            {
                if (lv.level > 0)
                {
                    if (psgo == null)
                    {
                        psgo = GameManager.Resource.Instantiate<GameObject>("Particle/BottleLiquidParticle", pourPoint.position, pourPoint.rotation, this.transform, true);
                        ps = psgo.GetComponent<ParticleSystem>();
                        psr = psgo.GetComponent<ParticleSystemRenderer>();
                        _block.Clear();
                        _block.SetColor("_BaseColor", lv.liquidColor1);
                        _block.SetColor("_EmissionColor", lv.liquidColor1);
                        psr.SetPropertyBlock(_block);
                        print("��ƼŬ ���� ����");
                    }
                    lv.level -= spillRate * Time.deltaTime;
                    ShootParabolicRay();
                    if (!ps.isPlaying)
                        ps.Play();
                }
                else
                {
                    lv.level = 0;
                    if (ps.isPlaying)
                    {
                        ps.Stop();
                        GameManager.Resource.Destroy(psgo);
                        psgo = null;
                    }
                }
            }
            else
            {
                if (psgo != null)
                    if (ps.isPlaying) ps.Stop();
            }
        }
        else
        {
            if (isAction)
            {
                ShootParabolicRay();
                if (!dispencerParticle.isPlaying)
                    dispencerParticle.Play();
            }
            else
                if (dispencerParticle.isPlaying) dispencerParticle.Stop();
        }
    }
    void ShootParabolicRay()
    {
        trajectoryPoints.Clear();

        Vector3 startPos = origin.position;
        Vector3 velocity = origin.up * initialSpeed;
        Vector3 prevPos = startPos;

        trajectoryPoints.Add(prevPos);

        for (int step = 0; step < maxSteps; step++)
        {
            velocity.y += gravity * stepTime;
            Vector3 nextPos = prevPos + velocity * stepTime;

            trajectoryPoints.Add(nextPos);

            if (Physics.Linecast(prevPos, nextPos, out RaycastHit hit, hitLayers))
            {
                GameObject other = hit.collider.gameObject;
                HandleLiquidInteraction(other);
                break;
            }

            prevPos = nextPos;
        }
    }

    void HandleLiquidInteraction(GameObject other)
    {
        var liquidVolume = other.GetComponent<LiquidVolume>();
        var sc = other.GetComponent<SpillController>();
        var shakerLiquid = other.GetComponent<ShakerLiquid>();

        if (liquidVolume == null && shakerLiquid == null)
        {
            print($"{other.name}�� ���㼭��");
            return;
        }
        // LiquidVolume ó��
        isHavingLiquid = false;
        layerIndex = -1;


        if (liquidVolume != null)
        {
            for (int j = 0; j < liquidVolume.liquidLayers.Length; j++)
            {
                if (liquidVolume.liquidLayers[j].layerName == liquidName)
                {
                    isHavingLiquid = true;
                    layerIndex = j;
                    break;
                }
            }
            if (!isHavingLiquid)
            {
                print($"{other.name}�� {liquidName}��(��) �����");
                for (int j = 0; j < liquidVolume.liquidLayers.Length; j++)
                {
                    if (liquidVolume.liquidLayers[j].amount == 0)
                    {
                        layerIndex = j;
                        liquidVolume.liquidLayers[j].layerName = liquidName;
                        liquidVolume.liquidLayers[j].color = lv != null ? lv.liquidColor1 : liquidColor1;
                        liquidVolume.liquidLayers[j].murkColor = lv != null ? lv.liquidColor2 : liquidColor2;
                        liquidVolume.liquidLayers[j].murkiness = lv != null ? lv.murkiness : murkiness;
                        break;
                    }
                }
                if (layerIndex <= -1)
                {
                    print($"{other.name}�� {liquidName}��(��) �߰��� ������ �����");
                    return;
                }

                if (liquidVolume.level >= MaxAmount)
                {
                    print($"{other.name}�� ���� ��������");
                    return;
                }

                print($"{other.name}�� {liquidName}��(��) �߰��߾��");
                liquidVolume.liquidLayers[layerIndex].amount += liquidAmount;
                liquidVolume.UpdateMaterialProperties();
                sc.UpdateUI();
                ResetIndex();
            }
            else
            {
                if (liquidVolume.level >= MaxAmount)
                {
                    print($"{other.name}�� ���� ��������");
                }
                else
                {
                    print($"{other.name}�� {liquidName}��(��) ������ �־��");
                    liquidVolume.liquidLayers[layerIndex].amount += liquidAmount;
                    sc.UpdateUI();
                    liquidVolume.UpdateLayers();
                    ResetIndex();
                }
            }
        }
        else // ShakerLiquid ó��
        {
            if (!shakerLiquid.isClosed)
            {
                if (shakerLiquid.TotalAmount >= MaxAmount)
                {
                    print($"{other.name}�� ���� ��������");
                    return;
                }
                wt.liquidName = liquidName;
                wt.liquidColor = lv != null ? lv.liquidColor1 : liquidColor1;
                wt.murkColor = lv != null ? lv.liquidColor2 : liquidColor2;
                wt.murkiness = lv != null ? lv.murkiness : murkiness;

                print($"{other.name}�� {liquidName}��(��) �߰��߾��");
                shakerLiquid.AddLiquid(wt, liquidAmount);
                shakerLiquid.UpdateUI();
            }
            else
            {
                print($"{other.name}��(��) �����־��");
            }
        }
    }

    void ResetIndex()
    {
        isHavingLiquid = false;
        layerIndex = -1;
    }

    //Gizmos ������
    void OnDrawGizmos()
    {
        if (trajectoryPoints == null || trajectoryPoints.Count < 2)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < trajectoryPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1]);
        }

        // ������ ǥ��
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(trajectoryPoints[0], 0.002f);

        // ������ �� ǥ��
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(trajectoryPoints[trajectoryPoints.Count - 1], 0.002f);
    }
    public void ActionButton()
    {
        isAction = !isAction;
    }
}
