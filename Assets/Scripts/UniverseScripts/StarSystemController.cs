using UnityEngine;
using System.Collections.Generic;

// This script is added to each star that is spawned in the universe view.
public class StarSystemController : MonoBehaviour
{
    // Star properties
    public StarClass StarClass;
    public float StarMass;
    public float StarLuminosity;
    public float starVariability = 4;
    public float starSize;
    public Color starColor;

    // Star system properties
    public int StarSystemSeed;
    public int NumberOfPlanets;

    // Components and prefabs
    public GameObject glowEffectPrefab;
    private CircleCollider2D circleCollider2D;
    private SpriteRenderer spriteRenderer;
    private List<GameObject> glowEffectInstances = new List<GameObject>();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider2D = GetComponent<CircleCollider2D>() ?? gameObject.AddComponent<CircleCollider2D>();
        circleCollider2D.transform.localScale = Vector3.one;
    }

    public void SetStarClassAndMassAndSeed(StarClass starClass, float starMass, int starSystemSeed)
    {
        StarClass = starClass;
        StarMass = starMass;
        StarSystemSeed = starSystemSeed;

        UpdateStarSize();
        UpdateStarColor();
        UpdateStarLuminosity();
        GenerateNumberOfPlanets();
        DrawRegularConvexPolygon();
    }

    private void UpdateStarSize()
    {
        float minScale = float.MaxValue;
        float maxScale = float.MinValue;
        foreach (StarClass sc in WorldGenerator.Instance.StarClasses)
        {
            minScale = Mathf.Min(minScale, sc.minScale);
            maxScale = Mathf.Max(maxScale, sc.maxScale);
        }

        starSize = Mathf.Lerp(StarClass.minScale, StarClass.maxScale, Mathf.InverseLerp(StarClass.minStarMass, StarClass.maxStarMass, StarMass));
        float mappedStarSize = Mathf.Lerp(WorldGenerator.Instance.minStarSizeOnMap, WorldGenerator.Instance.maxStarSizeOnMap, Mathf.InverseLerp(minScale, maxScale, starSize));
        transform.localScale = Vector3.one * mappedStarSize;

        float colliderRadius = 0.25f;
        circleCollider2D.radius = colliderRadius / mappedStarSize;
    }

    private void UpdateStarColor()
    {
        float t = Mathf.InverseLerp(StarClass.minStarMass, StarClass.maxStarMass, StarMass);
        starColor = Color.Lerp(StarClass.minColor, StarClass.maxColor, t);
        starColor.a = 1f;
        spriteRenderer.color = starColor;
    }

    private void UpdateStarLuminosity()
    {
        float t = Mathf.InverseLerp(StarClass.minStarMass, StarClass.maxStarMass, StarMass);
        StarLuminosity = Mathf.Lerp(StarClass.minL, StarClass.maxL, t);
    }

    private void GenerateNumberOfPlanets()
    {
        System.Random random = new System.Random(StarSystemSeed);
        NumberOfPlanets = (int)randomValueBasedOnGauss(7f, 5f, 3f, StarMass > 1 ? 10f * (1 + Mathf.Log(StarMass, 10f)) : 10f);
    }

    private float randomValueBasedOnGauss(float mean, float standardDeviation, float minimumValue, float maximumValue)
    {
        System.Random random = new System.Random(StarSystemSeed);

        float u1;
        float u2;
        float z0;
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

    private void DrawRegularConvexPolygon()
    {
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = NumberOfPlanets + 1;
        lineRenderer.loop = true;

        // Set the line renderer's material and color to match the star's
        lineRenderer.material = spriteRenderer.material;
        lineRenderer.startColor = starColor;
        lineRenderer.endColor = starColor;

        float polygonRadius = 0.3f; // Set this to the desired radius of the polygon
        float lineWidth = 0.03f; // Set this to the desired line width
        lineRenderer.widthMultiplier = lineWidth;

        for (int i = 0; i < NumberOfPlanets; i++)
        {
            float angle = 2 * Mathf.PI * i / NumberOfPlanets;
            float x = polygonRadius * Mathf.Cos(angle);
            float y = polygonRadius * Mathf.Sin(angle);

            // Add the star's position to the calculated position to center the polygon on the star
            Vector3 position = new Vector3(x, y, 0) + transform.position;

            lineRenderer.SetPosition(i, position);
        }

        // Close the polygon
        lineRenderer.SetPosition(NumberOfPlanets, lineRenderer.GetPosition(0));
    }

}