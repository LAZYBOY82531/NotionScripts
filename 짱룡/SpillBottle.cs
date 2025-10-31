using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiquidVolumeFX;
using static UnityEngine.ParticleSystem;

public class SpillBottle : MonoBehaviour
{
    public float spillRate = 0.003f;
    GameObject psgo;
    public float pourAngle = 90f;    // 기울임 각도 기준값 (90~120 사이 권장)
    public Transform pourPoint; // 액체가 쏟아지는 위치

    [Header("기본 설정")]
    public ParticleSystem ps;
    public string liquidName;
    public float liquidAmount = 0.001f;
    public float MaxAmount = 0.9f;

    [Header("포물선 레이 설정")]
    public Transform origin;           // Ray 시작 위치
    public float initialSpeed = 0.68f;    // 초기 속도
    public float gravity = -9.81f;     // 중력 값
    public float stepTime = 0.05f;     // 한 단계 시뮬레이션 간격
    public int maxSteps = 50;          // 시뮬레이션 횟수
    public LayerMask hitLayers;        // 충돌 대상 Layer

    [Header("레퍼런스")]
    public LiquidVolume lv;
    public WaterType wt;
    bool isHavingLiquid = false;
    int layerIndex = -1;
    private List<Vector3> trajectoryPoints = new List<Vector3>();
    ParticleSystemRenderer psr;
    static MaterialPropertyBlock _block;

    [Header("디스팬서(디스팬서로 사용할거면 lv를 null로 해야됨)")]
    [SerializeField] Color liquidColor1;
    [SerializeField] Color liquidColor2;
    [SerializeField] [Range(0, 1)] float murkiness;
    [SerializeField] ParticleSystem dispencerParticle;
    [Header("Raycast 설정 (디스팬서용)")]
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
            if (dot > 0) // 일정 수준 이상 아래로 향하면
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
                        print("파티클 색상 설정");
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
            print($"{other.name}가 마잣서오");
            return;
        }
        // LiquidVolume 처리
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
                print($"{other.name}에 {liquidName}이(가) 없어오");
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
                    print($"{other.name}에 {liquidName}을(를) 추가할 공간이 없어오");
                    return;
                }

                if (liquidVolume.level >= MaxAmount)
                {
                    print($"{other.name}의 물이 꽉찻서오");
                    return;
                }

                print($"{other.name}에 {liquidName}을(를) 추가했어오");
                liquidVolume.liquidLayers[layerIndex].amount += liquidAmount;
                liquidVolume.UpdateMaterialProperties();
                sc.UpdateUI();
                ResetIndex();
            }
            else
            {
                if (liquidVolume.level >= MaxAmount)
                {
                    print($"{other.name}의 물이 꽉찻서오");
                }
                else
                {
                    print($"{other.name}에 {liquidName}을(를) 가지고 있어오");
                    liquidVolume.liquidLayers[layerIndex].amount += liquidAmount;
                    sc.UpdateUI();
                    liquidVolume.UpdateLayers();
                    ResetIndex();
                }
            }
        }
        else // ShakerLiquid 처리
        {
            if (!shakerLiquid.isClosed)
            {
                if (shakerLiquid.TotalAmount >= MaxAmount)
                {
                    print($"{other.name}의 물이 꽉찻서오");
                    return;
                }
                wt.liquidName = liquidName;
                wt.liquidColor = lv != null ? lv.liquidColor1 : liquidColor1;
                wt.murkColor = lv != null ? lv.liquidColor2 : liquidColor2;
                wt.murkiness = lv != null ? lv.murkiness : murkiness;

                print($"{other.name}에 {liquidName}을(를) 추가했어오");
                shakerLiquid.AddLiquid(wt, liquidAmount);
                shakerLiquid.UpdateUI();
            }
            else
            {
                print($"{other.name}이(가) 닫혀있어오");
            }
        }
    }

    void ResetIndex()
    {
        isHavingLiquid = false;
        layerIndex = -1;
    }

    //Gizmos 디버깅용
    void OnDrawGizmos()
    {
        if (trajectoryPoints == null || trajectoryPoints.Count < 2)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < trajectoryPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1]);
        }

        // 시작점 표시
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(trajectoryPoints[0], 0.002f);

        // 마지막 점 표시
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(trajectoryPoints[trajectoryPoints.Count - 1], 0.002f);
    }
    public void ActionButton()
    {
        isAction = !isAction;
    }
}
