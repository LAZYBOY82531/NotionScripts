using UnityEngine;
using System.Collections;
using LiquidVolumeFX;
using Unity.VisualScripting;
using UnityEngine.Scripting;
using TMPro;
using System.Text;
using static LiquidVolumeFX.LiquidVolume;

/// <summary>
/// Shows the spill point on the glass when it's rotated.
/// </summary>
public class SpillController : MonoBehaviour
{
    public enum GlassType
    {
        OnTheRockGlass = 0,
        WineGlass,
        CocktailGlass,
        WhiskeyGlass,
        LongWineGlass,
        BeerGlass
    }
    public GlassType glassType;
    GameObject spill;
    LiquidVolume lv;
    int currentIndex;
    Ray ray;
    RaycastHit hit;
    public float rayDistance = 50f;
    public GameObject olive;
    public GameObject straw;
    public GameObject umbrella;
    public GameObject swordtember;
    public GameObject sphereIce;
    public GameObject cubeIce;
    public GameObject fire;
    public bool isInOlive = false;
    public bool isInStraw = false;
    public bool isInUmbrella = false;
    public bool isInSwordtember = false;
    public bool isInIce = false;
    public bool isFire = false;
    public LayerMask layerMask;
    public IceType iceType = IceType.None;
    static MaterialPropertyBlock _block;
    ParticleSystem spillParticle;
    ParticleSystemRenderer particleSystemRenderer;
    public TextMeshProUGUI textUI;
    WaterType newWaterType;
    GameObject cocktailObj;
    public GameObject glass;
    void Start()
    {
        lv = GetComponent<LiquidVolume>();
        if (lv.detail.isMultiple())
        {
            currentIndex = lv.liquidLayers.Length - 1;
        }
    }
    private void OnDisable()
    {
        if (lv.detail == DETAIL.MultipleNoFlask)
        {
            for (int i = 0; i < lv.liquidLayers.Length; i++)
            {
                lv.liquidLayers[i].amount = 0f;
            }
        }
        else if (lv.detail == DETAIL.DefaultNoFlask)
        {
            lv.level = 0.8f;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        var deco = other.GetComponent<GlassDeco>();
        var ice = other.GetComponent<Ice>();
        if (deco != null)
        {
            switch (deco.decoType)
            {
                case GlassDeco.DecoType.Olive:
                    olive.SetActive(true);
                    isInOlive = true;
                    break;
                case GlassDeco.DecoType.Straw:
                    straw.SetActive(true);
                    isInStraw = true;
                    break;
                case GlassDeco.DecoType.Umbrella:
                    umbrella.SetActive(true);
                    isInUmbrella = true;
                    break;
                case GlassDeco.DecoType.Swordtember:
                    swordtember.SetActive(true);
                    isInSwordtember = true;
                    break;
            }
            GameManager.Resource.Destroy(other.gameObject);
        }
        if (ice != null)
        {
            if (ice.iceType == IceType.None) return;
            if (ice.iceType == IceType.Sphere && sphereIce == null) return;
            isInIce = true;
            iceType = ice.iceType;
            switch (ice.iceType)
            {
                case IceType.Sphere:
                    if (sphereIce != null)
                        sphereIce.SetActive(true);
                    break;
                case IceType.Cube:
                    cubeIce.SetActive(true);
                    break;
            }
            GameManager.Resource.Destroy(other.gameObject);
        }
        if (other.CompareTag("Fire") && lv.level != 0)
        {
            isFire = true;
            fire.SetActive(true);
        }
    }
    void FixedUpdate()
    {
        if (_block == null)
            _block = new MaterialPropertyBlock();
        if (lv.level == 0f && isFire)
        {
            isFire = false;
            fire.SetActive(false);
        }
        Vector3 spillPos;
        float spillAmount;
        if (lv.GetSpillPoint(out spillPos, out spillAmount))
        {
            if (spillParticle == null)
            {
                spill = GameManager.Resource.Instantiate<GameObject>("Particle/LiquidParticle", gameObject.transform, true);
                spillParticle = spill.GetComponent<ParticleSystem>();
                particleSystemRenderer = spill.GetComponent<ParticleSystemRenderer>();
            }
            spillParticle.transform.position = spillPos;
            DropObject(spillPos);
            ray = new Ray(spillPos, Vector3.down);
            if (!spillParticle.isPlaying) spillParticle.Play();
            if (lv.detail.isMultiple())
            {
                if (currentIndex < 0)
                {
                    currentIndex = lv.liquidLayers.Length - 1;
                    spillParticle.Stop();
                    if (spill.activeSelf)
                    {
                        GameManager.Resource.Destroy(spill);
                        spill = null;
                        spillParticle = null;
                        particleSystemRenderer = null;
                    }
                    return; // 다 줄였으면 종료
                }
                if (lv.liquidLayers[currentIndex].amount <= 0f)
                {
                    lv.liquidLayers[currentIndex].amount = 0f;
                    lv.liquidLayers[currentIndex].layerName = "NoLiquid";
                    currentIndex--;
                    return;
                }
                _block.Clear();
                _block.SetColor("_BaseColor", lv.liquidLayers[currentIndex].color);
                _block.SetColor("_EmissionColor", lv.liquidLayers[currentIndex].color);
                particleSystemRenderer.SetPropertyBlock(_block);
                lv.liquidLayers[currentIndex].amount -= spillAmount * 0.1f + 0.001f;
                lv.UpdateLayers();
                lv.UpdateMaterialProperties();
                UpdateUI();
                if (Physics.Raycast(ray, out hit, rayDistance, layerMask))
                {
                    print(hit.collider.name);
                    ShakerLiquid shakerLiquid = hit.collider.GetComponent<ShakerLiquid>();
                    LiquidVolume hitlv = hit.collider.GetComponent<LiquidVolume>();
                    SpillController hitsp = hit.collider.GetComponent<SpillController>();
                    if (hitlv != null)
                    {
                        if (hitlv.level > 0.9f) return;
                        int hitIndex = -1;
                        for (int i = 0; i < hitlv.liquidLayers.Length; i++)
                        {
                            if (hitlv.liquidLayers[i].layerName == lv.liquidLayers[currentIndex].layerName)
                            {
                                hitIndex = i;
                                break;
                            }
                        }
                        if (hitIndex == -1)
                        {
                            for (int i = 0; i < hitlv.liquidLayers.Length; i++)
                            {
                                if (hitlv.liquidLayers[i].amount == 0)
                                {
                                    hitIndex = i;
                                    hitlv.liquidLayers[i].layerName = lv.liquidLayers[currentIndex].layerName;
                                    hitlv.liquidLayers[i].color = lv.liquidLayers[currentIndex].color;
                                    hitlv.liquidLayers[i].murkColor = lv.liquidLayers[currentIndex].murkColor;
                                    hitlv.liquidLayers[i].murkiness = lv.liquidLayers[currentIndex].murkiness;
                                    hitlv.UpdateLayers();
                                    hitlv.UpdateMaterialProperties();
                                    hitsp.UpdateUI();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            hitlv.liquidLayers[hitIndex].amount += spillAmount * SpillAmountCompare(hitsp.glassType) * 0.1f + 0.001f;
                            hitlv.UpdateLayers();
                            hitlv.UpdateMaterialProperties();
                            hitsp.UpdateUI();
                        }
                    }
                    else if (shakerLiquid != null)
                    {
                        newWaterType = new WaterType();
                        newWaterType.liquidName = lv.liquidLayers[currentIndex].layerName;
                        newWaterType.liquidColor = lv.liquidLayers[currentIndex].color;
                        newWaterType.murkColor = lv.liquidLayers[currentIndex].murkColor;
                        newWaterType.murkiness = lv.liquidLayers[currentIndex].murkiness;
                        shakerLiquid.AddLiquid(newWaterType, spillAmount * SpillAmountCompare(GlassType.BeerGlass) * 0.1f + 0.001f);
                        shakerLiquid.UpdateUI();
                    }
                }
            }
            else
            {
                print("잘못만들어짐!");
            }
        }
        else
        {
            if (spill != null)
                if (spillParticle.isPlaying)
                    spillParticle.Stop();
        }
    }
    public void UpdateUI()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var layer in lv.liquidLayers)
        {
            if (layer.amount > 0)
                sb.AppendLine($"\n{layer.layerName} : {Mathf.Round((layer.amount * VolumeCompare()))}mL");
        }
        textUI.text = sb.ToString();
    }
    float VolumeCompare()
    {
        float volume = 0;
        switch (glassType)
        {
            case GlassType.OnTheRockGlass:
                volume = 200f;
                break;
            case GlassType.WineGlass:
                volume = 300f;
                break;
            case GlassType.CocktailGlass:
                volume = 130f;
                break;
            case GlassType.WhiskeyGlass:
                volume = 60f;
                break;
            case GlassType.LongWineGlass:
                volume = 200f;
                break;
            case GlassType.BeerGlass:
                volume = 300f;
                break;
        }
        return volume;
    }
    float SpillAmountCompare(GlassType spillGlassType)
    {
        float multicount = 0f;
        switch (glassType)
        {
            case GlassType.OnTheRockGlass:
                switch (spillGlassType)
                {
                    case GlassType.OnTheRockGlass:
                        multicount = 1f;
                        break;
                    case GlassType.WineGlass:
                        multicount = 0.666666f;
                        break;
                    case GlassType.CocktailGlass:
                        multicount = 1.666666f;
                        break;
                    case GlassType.WhiskeyGlass:
                        multicount = 1.666666f;
                        break;
                    case GlassType.LongWineGlass:
                        multicount = 1f;
                        break;
                    case GlassType.BeerGlass:
                        multicount = 0.666666f;
                        break;
                    default:
                        multicount = 999f;
                        break;
                }
                break;
            case GlassType.WineGlass:
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
                break;
            case GlassType.CocktailGlass:
                switch (spillGlassType)
                {
                    case GlassType.OnTheRockGlass:
                        multicount = 0.6f;
                        break;
                    case GlassType.WineGlass:
                        multicount = 0.4f;
                        break;
                    case GlassType.CocktailGlass:
                        multicount = 1f;
                        break;
                    case GlassType.WhiskeyGlass:
                        multicount = 2f;
                        break;
                    case GlassType.LongWineGlass:
                        multicount = 0.6f;
                        break;
                    case GlassType.BeerGlass:
                        multicount = 0.4f;
                        break;
                    default:
                        multicount = 999f;
                        break;
                }
                break;
            case GlassType.WhiskeyGlass:
                switch (spillGlassType)
                {
                    case GlassType.OnTheRockGlass:
                        multicount = 0.3f;
                        break;
                    case GlassType.WineGlass:
                        multicount = 0.2f;
                        break;
                    case GlassType.CocktailGlass:
                        multicount = 0.5f;
                        break;
                    case GlassType.WhiskeyGlass:
                        multicount = 1f;
                        break;
                    case GlassType.LongWineGlass:
                        multicount = 0.3f;
                        break;
                    case GlassType.BeerGlass:
                        multicount = 0.2f;
                        break;
                    default:
                        multicount = 999f;
                        break;
                }
                break;
            case GlassType.LongWineGlass:
                switch (spillGlassType)
                {
                    case GlassType.OnTheRockGlass:
                        multicount = 1f;
                        break;
                    case GlassType.WineGlass:
                        multicount = 0.666666f;
                        break;
                    case GlassType.CocktailGlass:
                        multicount = 1.666666f;
                        break;
                    case GlassType.WhiskeyGlass:
                        multicount = 1.666666f;
                        break;
                    case GlassType.LongWineGlass:
                        multicount = 1f;
                        break;
                    case GlassType.BeerGlass:
                        multicount = 0.666666f;
                        break;
                    default:
                        multicount = 999f;
                        break;
                }
                break;
            case GlassType.BeerGlass:
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
                break;
        }
        return multicount;
    }
    public void ComparerLiquid()
    {
        var (cocktailName, num, cocktailObj) = GameManager.Comparer.CompareRecipe(lv.liquidLayers, glassType, isInIce, isFire, iceType);
        if (cocktailName != null)
        {
            Debug.Log($"{cocktailName} 칵테일 완성! 완성도: {num}");
            GameManager.Resource.Instantiate<GameObject>(cocktailObj, this.transform.position, Quaternion.Euler(0, 0, 0), true);
            glass.SetActive(false);
        }
        else
        {
            Debug.Log("일치하는 칵테일 레시피가 없습니다.");
        }
    }
    private void DropObject(Vector3 spillPos)
    {
        if (isInOlive)
        {
            GameObject oliveClone = GameManager.Resource.Instantiate<GameObject>("DropObj/Olive", spillPos, Quaternion.Euler(0, 0, 0), true);
            olive.SetActive(false);
            isInOlive = false;
            GameManager.Resource.Destroy(oliveClone, 5f);
        }
        if (isInStraw)
        {
            GameObject strawClone = GameManager.Resource.Instantiate<GameObject>("DropObj/Straw", spillPos, Quaternion.Euler(0, 0, 0), true);
            straw.SetActive(false);
            isInStraw = false;
            GameManager.Resource.Destroy(strawClone, 5f);
        }
        if (isInSwordtember)
        {
            GameObject swordtemberClone = GameManager.Resource.Instantiate<GameObject>("DropObj/Swordtember", spillPos, Quaternion.Euler(0, 0, 0), true);
            swordtember.SetActive(false);
            isInSwordtember = false;
            GameManager.Resource.Destroy(swordtemberClone, 5f);
        }
        if (isInUmbrella)
        {
            GameObject umbrellaClone = GameManager.Resource.Instantiate<GameObject>("DropObj/Umbrella", spillPos, Quaternion.Euler(0, 0, 0), true);
            umbrella.SetActive(false);
            isInUmbrella = false;
            GameManager.Resource.Destroy(umbrellaClone, 5f);
        }
        if (isInIce)
        {
            if (iceType == IceType.Sphere)
            {
                GameObject sphereIceClone = GameManager.Resource.Instantiate<GameObject>("DropObj/SphereIce", spillPos, Quaternion.Euler(0, 0, 0), true);
                sphereIce.SetActive(false);
                isInIce = false;
                GameManager.Resource.Destroy(sphereIceClone, 5f);
            }
            else if (iceType == IceType.Cube)
            {
                GameObject cubeIceClone = GameManager.Resource.Instantiate<GameObject>("DropObj/CubeIce", spillPos, Quaternion.Euler(0, 0, 0), true);
                cubeIce.SetActive(false);
                isInIce = false;
                GameManager.Resource.Destroy(cubeIceClone, 5f);
            }
        }
    }
}
