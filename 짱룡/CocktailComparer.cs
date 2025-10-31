using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LiquidVolumeFX.LiquidVolume;
using static SpillController;

public class CocktailComparer : MonoBehaviour
{
    [SerializeField] private List<Recipe> recipes;
    private void Start()
    {
        recipes = GameManager.Resource.Load<Recipes>("Data/CocktailRecipes").recipes;
    }
    public (string matchedRecipe, int completenessScore,GameObject obj) CompareRecipe(LiquidLayer[] waterLayers, GlassType glassType, bool hasIce, bool hasFire, IceType iceType)
    {
        // 1. 잔 타입, 얼음 유무, 불 유무 필터링
        var filteredRecipes = recipes.Where(r => r.glassType == glassType).ToList();
        if (filteredRecipes.Count == 0)
            return (null, 0,null);
        var filteredIceRecipes = filteredRecipes.Where(r => r.hasIce == hasIce).ToList();
        if (filteredIceRecipes.Count == 0)
            return (null, 0, null);
        var filterdFireRecipes = filteredIceRecipes.Where(r => r.isFire == hasFire).ToList();
        if (filterdFireRecipes.Count == 0)
            return (null, 0, null);

        // 2. 활성화된 레이어만 추출
        List<LiquidLayer> activeLayers = waterLayers.Where(l => l.amount > 0).ToList();
        int liquidCount = activeLayers.Count;
        if (liquidCount == 0)
            return (null, 0, null);

        // 3. 개수 일치하는 레시피만 추림
        var sameCountRecipes = filterdFireRecipes.Where(r => r.recipeLayers.Count == liquidCount).ToList();
        if (sameCountRecipes.Count == 0)
            return (null, 0, null);

        // 4. 이름 일치 (순서 무시)
        foreach (var recipe in sameCountRecipes)
        {
            int compareNum = 0;
            foreach (var layer in activeLayers)
            {
                if (recipe.recipeLayers.Any(r => r.layerName == layer.layerName))
                    compareNum++;
            }

            // 모든 레이어 이름이 일치하면 완성도 비교 시작
            if (compareNum == liquidCount)
            {
                int totalScore = hasIce ? (liquidCount + 1) * 5 : liquidCount * 5;
                totalScore += hasFire ? 5 : 0;
                if (hasIce)
                {
                    if (recipe.iceType != iceType)
                        totalScore -= hasFire ? liquidCount + 2 : liquidCount+1;
                }
                // miscible == true → 단일 음료
                if (liquidCount == 1 && recipe.recipeLayers[0].miscible)
                {
                    var recipeLayer = recipe.recipeLayers[0];
                    var waterLayer = activeLayers[0];

                    float amountDiff = Mathf.Abs(recipeLayer.amount - waterLayer.amount);
                    float colorDiff = ColorDifference(recipeLayer.color, waterLayer.color);

                    float diffScore = Mathf.Clamp01((amountDiff * 2f + colorDiff * 0.5f));

                    if (diffScore < 0.1f) totalScore -= 0;
                    else if (diffScore < 0.2f) totalScore -= 1;
                    else if (diffScore < 0.35f) totalScore -= 2;
                    else if (diffScore < 0.5f) totalScore -= 3;
                    else totalScore -= 4;
                    if (hasIce && hasFire)
                    {
                        totalScore = Mathf.RoundToInt(totalScore * 0.3333333f);
                    }
                    else if (hasIce || hasFire)
                    {
                        totalScore = Mathf.RoundToInt(totalScore >> 1);
                    }
                }
                else
                {
                    // miscible == false → 여러 레이어의 amount만 비교
                    foreach (var recipeLayer in recipe.recipeLayers)
                    {
                        var waterLayer = activeLayers.FirstOrDefault(l => l.layerName == recipeLayer.layerName);
                        if (waterLayer.layerName == null) continue;

                        float amountDiff = Mathf.Abs(recipeLayer.amount - waterLayer.amount);

                        if (amountDiff < 0.05f) totalScore -= 0;
                        else if (amountDiff < 0.1f) totalScore -= 1;
                        else if (amountDiff < 0.2f) totalScore -= 2;
                        else if (amountDiff < 0.35f) totalScore -= 3;
                        else totalScore -= 4;
                    }
                    if (hasIce && hasFire)
                    {
                        totalScore = Mathf.RoundToInt(totalScore / liquidCount+2);
                    }
                    else if(hasIce || hasFire)
                    {
                        totalScore = Mathf.RoundToInt(totalScore / liquidCount+1);
                    }
                    else
                    {
                        totalScore = Mathf.RoundToInt(totalScore / liquidCount);
                    }
                }
                return (recipe.recipeName, totalScore, recipe.cocktailObject);
            }
        }
        return (null, 0, null);
    }

    private float ColorDifference(Color a, Color b)
    {
        float dr = a.r - b.r;
        float dg = a.g - b.g;
        float db = a.b - b.b;
        return Mathf.Sqrt(dr * dr + dg * dg + db * db);
    }
}
