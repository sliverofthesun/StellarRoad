using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

//This is a script that is added to each star that is spawned in the universe view.
public class StarSystemController : MonoBehaviour
{
    [SerializeField]
    public StarClass StarClass; //star class hold the information regarding the possible star classes: O, B, A, F, G, K, M.

    [SerializeField]
    public float StarMass; //mass of the star - the most important metric

    [SerializeField]
    public float StarLuminosity; //derived from the mass

    public float starVariability = 4; //used to render glow effects around the star in universeview. So far is a placeholder

    public GameObject glowEffectPrefab; //prefab used to generate the glow effect

    public float starSize; //derived from mass
    public Color starColor; //derived from mass

    public int StarSystemSeed; //seed for each star system
    public int NumberOfPlanets; //number of planets in the star system
    private CircleCollider2D circleCollider2D; //used to create a collider around the star that makes it easier to click on a star

    public SpriteRenderer spriteRenderer; //used to render various sprites

    //these variables are used for the generation of the glow effect
    public float oscillationPercentage;
    public float oscillationSpeed;
    public float variationPercentage;

    public float uniqueOscillationPercentage;
    public float uniqueOscillationSpeed;

    public float minGlowSize = 1.0f;
    public float maxGlowSize = 3.0f;

    public float minGlowOpacity;
    public float maxLuminosity = 5.0f; //gives a maximum to the alpha channel of a star's glow

    private List<GameObject> glowEffectInstances = new List<GameObject>(); //instances of the glow effect for each star

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider2D = GetComponent<CircleCollider2D>() ?? gameObject.AddComponent<CircleCollider2D>();
        circleCollider2D.transform.localScale = Vector3.one;
    }

    private void Update()
    {
        UpdateGlowEffects();
    }

    public void SetStarClassAndMassAndSeed(StarClass starClass, float starMass, int starSystemSeed)
    {
        StarClass = starClass;
        StarMass = starMass;
        StarSystemSeed = starSystemSeed;

        //System.Random starSystemRandom = new System.Random(starSystemSeed);
        //numberOfPlanets = starSystemRandom.Next(starClass.minPlanets, starClass.maxPlanets + 1);

        UpdateStarSize();
        UpdateStarColor();
        UpdateStarLuminosity();
        GenerateNumberOfPlanets();
        InitializeUniqueOscillationValues();
    }

    private void UpdateStarSize()
    {
        // Get the largest and smallest scale from all star classes
        float minScale = float.MaxValue;
        float maxScale = float.MinValue;
        foreach (StarClass sc in WorldGenerator.Instance.StarClasses)
        {
            minScale = Mathf.Min(minScale, sc.minScale);
            maxScale = Mathf.Max(maxScale, sc.maxScale);
        }

        // Calculate star size based on StarClass and StarMass
        starSize = Mathf.Lerp(StarClass.minScale, StarClass.maxScale, Mathf.InverseLerp(StarClass.minStarMass, StarClass.maxStarMass, StarMass));

        // Map star size to minStarSizeOnMap and maxStarSizeOnMap
        float mappedStarSize = Mathf.Lerp(WorldGenerator.Instance.minStarSizeOnMap, WorldGenerator.Instance.maxStarSizeOnMap, Mathf.InverseLerp(minScale, maxScale, starSize));

        transform.localScale = Vector3.one * mappedStarSize;

        float colliderRadius = 0.25f;
        circleCollider2D.radius = colliderRadius / mappedStarSize;
    }

    private void UpdateStarColor()
    {
        // Calculate star color based on StarClass and StarMass
        float t = Mathf.InverseLerp(StarClass.minStarMass, StarClass.maxStarMass, StarMass);
        starColor = Color.Lerp(StarClass.minColor, StarClass.maxColor, t);

        starColor.a = 1f;
        spriteRenderer.color = starColor;
    }

    private void InitializeUniqueOscillationValues()
    {
        System.Random random = new System.Random(StarSystemSeed);
        
        float randomPercentage = (float)random.NextDouble() * 2 * variationPercentage - variationPercentage;
        uniqueOscillationPercentage = oscillationPercentage * (1 + randomPercentage);

        randomPercentage = (float)random.NextDouble() * 2 * variationPercentage - variationPercentage;
        uniqueOscillationSpeed = oscillationSpeed * (1 + randomPercentage);
    }

    private void UpdateGlowEffects()
    {
        int numGlowEffects = Mathf.RoundToInt(starVariability);
        while (glowEffectInstances.Count < numGlowEffects)
        {
            GameObject glowEffectInstance = Instantiate(glowEffectPrefab, transform.position, Quaternion.identity, transform);
            glowEffectInstances.Add(glowEffectInstance);
        }

        for (int i = 0; i < glowEffectInstances.Count; i++)
        {
            GameObject glowEffectInstance = glowEffectInstances[i];

            // Set the glow effect's sprite color
            Color starColor = spriteRenderer.color;
            float alpha = Mathf.Lerp(minGlowOpacity, 1.0f, StarLuminosity / maxLuminosity) / numGlowEffects;
            Color glowColor = new Color(starColor.r, starColor.g, starColor.b, alpha);

            SpriteRenderer glowEffectSpriteRenderer = glowEffectInstance.GetComponent<SpriteRenderer>();
            glowEffectSpriteRenderer.color = glowColor;

            // Oscillate the glow effect based on the star's scale and time
            float currentOscillationSpeed = uniqueOscillationSpeed * (1 + (i + 1) * 0.25f);
            float glowOscillation = 1.0f + Mathf.PingPong(Time.time * currentOscillationSpeed, uniqueOscillationPercentage * 2) - uniqueOscillationPercentage;
            float glowScale = Mathf.Lerp(minGlowSize, maxGlowSize, StarLuminosity / maxLuminosity) * glowOscillation;
            glowEffectInstance.transform.localScale = Vector3.one * glowScale;
        }
    }

    private void UpdateStarLuminosity()
    {
        float t = Mathf.InverseLerp(StarClass.minStarMass, StarClass.maxStarMass, StarMass);
        StarLuminosity = Mathf.Lerp(StarClass.minL, StarClass.maxL, t);
    }

    private void GenerateNumberOfPlanets()
    {
        if(Mathf.Abs(transform.position.y) < 1f && Mathf.Abs(transform.position.x) < 1f)
        {
            Debug.Log("Star at:" + transform.position.x + " , " + transform.position.y + " generating planets");
        }

        System.Random random = new System.Random(StarSystemSeed);

        NumberOfPlanets = (int)randomValueBasedONGauss(7f, 5f, 3f, StarMass > 1 ? 10f * (1+Mathf.Log(StarMass, 10f)) : 10f);

        if(Mathf.Abs(transform.position.y) < 1f && Mathf.Abs(transform.position.x) < 1f)
        {
            Debug.Log("Star at:" + transform.position.x + " , " + transform.position.y + " generated " + NumberOfPlanets + " planets");
        }
    }

    public float randomValueBasedONGauss(float mean, float standardDeviation, float minimumValue, float maximumValue)
    {
        System.Random random = new System.Random(StarSystemSeed);

        float u1;
        float u2;
        float z0 ;

        float temp;
        do
        {
            u1 = (float)random.NextDouble();
            u2 = (float)random.NextDouble();

            z0 = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);
            temp = mean + z0 * standardDeviation;
        } while (temp < minimumValue || temp > maximumValue);

        return temp;
    }
}