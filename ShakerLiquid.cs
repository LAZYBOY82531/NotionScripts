using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiquidVolumeFX;
using static SpillController;
using TMPro;
using System.Text;

public class ShakerLiquid : MonoBehaviour
{
    public List<WaterType> shakerLiquids = new List<WaterType>();
    public WaterType mixedLiquid = new WaterType();
    public bool isClosed = false;
    public ParticleSystem spillParticle;
    ParticleSystemRenderer psr;
    static MaterialPropertyBlock _block;
    public float spillRate;
    public GameObject shakerCap;
    public ShakerCapOpen capOpenScript;
    public ShakeDetector shakeDetector;
    public bool isInIce=false;
    public GameObject ice;
    WaterType Ice = new WaterType()
    {
        liquidName = "Ice",
        liquidAmount = 0f,
        liquidColor = Color.clear,
        murkColor = Color.clear,
        murkiness = 0f
    };
    public float TotalAmount
    {
        get
        {
            float total = 0f;
            foreach (var wt in shakerLiquids)
            {
                total += wt.liquidAmount;
            }
            return total;
        }
    }
    RaycastHit hit;
    private List<Vector3> trajectoryPoints = new List<Vector3>();
    [Header("포물선 레이 설정")]
    public Transform origin;           // Ray 시작 위치
    public float initialSpeed = 0.68f;    // 초기 속도
    public float gravity = -9.81f;     // 중력 값
    public float stepTime = 0.05f;     // 한 단계 시뮬레이션 간격
    public int maxSteps = 50;          // 시뮬레이션 횟수
    public LayerMask hitLayers;        // 충돌 대상 Layer
    public TextMeshProUGUI liquidText;
    private void Awake()
    {
        if (_block == null)
        {
            _block = new MaterialPropertyBlock();
        }
        mixedLiquid.liquidName = "";
        mixedLiquid.liquidAmount = 0f;
        mixedLiquid.liquidColor = Color.clear;
        mixedLiquid.murkColor = Color.clear;
        mixedLiquid.murkiness = 0f;
        psr = spillParticle.GetComponent<ParticleSystemRenderer>();
    }
    private void OnTriggerEnter(Collider other)
    {
        var iceType = other.GetComponent<Ice>();
        if (other.CompareTag("ShakerCap"))
        {
            isClosed = true;
            shakerCap.SetActive(true);
            other.gameObject.SetActive(false);
        }
        if (iceType != null)
        {
            if (iceType.iceType == IceType.Cube)
            {
                isInIce = true;
                ice.SetActive(true);
                AddLiquid(Ice, 0);
                other.gameObject.SetActive(false);
            }
        }
    }
    public void AddLiquid(WaterType waterType, float amount)
    {
        foreach (var wt in shakerLiquids)
        {
            if (wt.liquidName == waterType.liquidName)
            {
                wt.liquidAmount += amount;
                return;
            }
        }
        waterType.liquidAmount = amount;
        shakerLiquids.Add(waterType);
    }
    public void MixedLiquid()
    {
        if (!isClosed) return;
        shakerLiquids.Sort((a, b) => string.Compare(a.liquidName, b.liquidName, StringComparison.Ordinal));
        foreach(var wt in shakerLiquids)
        {
            mixedLiquid.liquidName += mixedLiquid.liquidName=="" ? wt.liquidName : " + " + wt.liquidName;
            mixedLiquid.liquidAmount += wt.liquidAmount;
            mixedLiquid.liquidColor += wt.liquidColor * wt.liquidAmount;
            mixedLiquid.murkColor += wt.murkColor * wt.liquidAmount;
            mixedLiquid.murkiness += wt.murkiness * wt.liquidAmount;
        }
        mixedLiquid.liquidColor /= mixedLiquid.liquidAmount;
        mixedLiquid.murkColor /= mixedLiquid.liquidAmount;
        mixedLiquid.murkiness /= mixedLiquid.liquidAmount;
        _block.Clear();
        _block.SetColor("_BaseColor", mixedLiquid.liquidColor);
        _block.SetColor("_EmissionColor", mixedLiquid.liquidColor);
        psr.SetPropertyBlock(_block);
        shakerLiquids.Clear();
    }
    public void ClearLiquid()
    {
        shakerLiquids.Clear();
        mixedLiquid.liquidName = "";
        mixedLiquid.liquidAmount = 0f;
        mixedLiquid.liquidColor = Color.clear;
        mixedLiquid.murkColor = Color.clear;
        mixedLiquid.murkiness = 0f;
        isInIce = false;
        ice.SetActive(false);
    }
    private void Update()
    {
        float dot = Vector3.Dot(transform.up, Vector3.up);
        if (dot < 0 && shakeDetector.isShaken && capOpenScript.isCapOpened && mixedLiquid.liquidAmount > 0) // 일정 수준 이상 아래로 향하면
        {
            if (mixedLiquid.liquidAmount > 0)
            {
                if (spillParticle == null)
                {
                    print("물파티클 안넣음");
                }
                ShootParabolicRay();
                mixedLiquid.liquidAmount -= spillRate * Time.deltaTime;
                if (!spillParticle.isPlaying)
                    spillParticle.Play();
            }
            else
            {
                mixedLiquid.liquidAmount = 0;
                if (spillParticle.isPlaying)
                    spillParticle.Stop();
            }
        }
        else
        {
            if (spillParticle.isPlaying)
                spillParticle.Stop();
        }
    }
    public void UpdateUI()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var layer in shakerLiquids)
        {
            sb.AppendLine($"\n{layer.liquidName} : {Mathf.Round((layer.liquidAmount * 300))}mL");
        }
        if (mixedLiquid.liquidName != "")
        {
            sb.AppendLine($"\n<color=yellow>Mixed Liquid :{mixedLiquid.liquidName} : {Mathf.Round((mixedLiquid.liquidAmount * 300))}mL</color>");
        }
        liquidText.text = sb.ToString();
    }
    float SpillAmount(GlassType spillGlassType)
    {
        float multicount;
        switch (spillGlassType)
        {
            case GlassType.OnTheRockGlass:
                multicount = 1.5f;
                break;
            case GlassType.WineGlass:
                multicount = 1f;
                break;
            case GlassType.CocktailGlass:
                multicount = 2.5f;
                break;
            case GlassType.WhiskeyGlass:
                multicount = 5f;
                break;
            case GlassType.LongWineGlass:
                multicount = 1.5f;
                break;
            case GlassType.BeerGlass:
                multicount = 1f;
                break;
            default:
                multicount = 999f;
                break;
        }
        return multicount;
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
        print(other.name);
        LiquidVolume hitlv = other.GetComponent<LiquidVolume>();
        SpillController spillCtrl = other.GetComponent<SpillController>();
        if (hitlv != null)
        {
            if (hitlv.level >= 0.9f) return;
            int hitIndex = -1;
            for (int i = 0; i < hitlv.liquidLayers.Length; i++)
            {
                if (hitlv.liquidLayers[i].layerName == mixedLiquid.liquidName)
                {
                    if (hitlv.liquidLayers[i].color == mixedLiquid.liquidColor)
                    {
                        hitIndex = i;
                        break;
                    }
                }
            }
            if (hitIndex == -1)
            {
                for (int i = 0; i < hitlv.liquidLayers.Length; i++)
                {
                    if (hitlv.liquidLayers[i].amount == 0)
                    {
                        hitlv.liquidLayers[i].layerName = mixedLiquid.liquidName;
                        hitlv.liquidLayers[i].color = mixedLiquid.liquidColor;
                        hitlv.liquidLayers[i].murkColor = mixedLiquid.murkColor;
                        hitlv.liquidLayers[i].murkiness = mixedLiquid.murkiness;
                        hitlv.UpdateLayers();
                        hitlv.UpdateMaterialProperties();
                        spillCtrl.UpdateUI();
                        break;
                    }
                }
            }
            else
            {
                hitlv.liquidLayers[hitIndex].amount += spillRate * SpillAmount(spillCtrl.glassType) * Time.deltaTime;
                if (hitlv.liquidLayers[hitIndex].amount > 0.9f) hitlv.liquidLayers[hitIndex].amount = 0.9f;
                hitlv.UpdateLayers();
                hitlv.UpdateMaterialProperties();
                spillCtrl.UpdateUI();
            }
        }
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
}
