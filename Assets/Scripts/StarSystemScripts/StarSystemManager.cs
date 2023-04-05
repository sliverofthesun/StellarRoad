using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PlanetDataNamespace;

public class StarSystemManager : MonoBehaviour
{
    [Header("Star Prefab")]
    [SerializeField] private GameObject starPrefab;

    [Header("Orbit Prefab")]
    [SerializeField] private GameObject orbitPrefab;
    public float firstOrbitRadiusInUnits = 3f;

    public int starSystemSeed;
    public int nOfPlanets;

    public float starSize;
    public float starLuminosity;
    public float starMass;
    public Color starColor;

    public float distanceSeed;
    public float distanceSeedStandardDiv;
    public float firstOrbit;

    public float ratioOfPlanetaryMassToStar; //used to generate how massive planets are

    public List<Planet> planets = new List<Planet>();

    public List<float> massLine = new List<float>();

    public GameObject planetPrefab;
    
    // Define a uniform scale for all planets in the solar system view
    public float uniformPlanetScale = 0.02f;

    [Header("Orbit Line Settings")]
    public float maxLineWidth = 0.5f;
    public float minLineWidth = 0.05f;
    public float lineWidthDistanceFactor = 10f;

    public float minDist;
    public float maxDist;
    public float probOfAtmo;

    private void Start()
    {
        //Setting all the values into variables within this script
        StarSystemController starSystemController = GameData.Instance.CurrentStarSystemController;
        starSize = GameData.Instance.StarSize;
        starLuminosity = GameData.Instance.StarLuminosity;
        starMass = GameData.Instance.StarMass;
        starSystemSeed = GameData.Instance.SystemSeed;
        nOfPlanets = GameData.Instance.NumberOfPlanets;
        starColor = GameData.Instance.StarColor;

        Debug.Log("Now in star system " + starSystemSeed + " with " + nOfPlanets + " planets.");

        // Set the random seed to ensure the same planets are generated for the same star
        Random.InitState(starSystemSeed);
        // Instantiate the Star prefab and remove the StarSystemController component
        GameObject star = Instantiate(starPrefab);
        Destroy(star.GetComponent<StarSystemController>());

        foreach (Transform child in star.transform) {
            GameObject.Destroy(child.gameObject);
        }

        // Set the star's position, size, color, and luminosity
        star.transform.position = Vector3.zero;

        star.GetComponent<SpriteRenderer>().color = starColor;

        GenerateBlaggFormulation();

        // Add the scaling factor calculation
        float scalingFactor = firstOrbitRadiusInUnits / (firstOrbit * 5f);
        Debug.Log(scalingFactor);
        if (!float.IsInfinity(scalingFactor) && !float.IsNaN(scalingFactor) && scalingFactor != 0)
        {
            star.transform.localScale = Vector3.one * starSize * scalingFactor;
        }
        else
        {
            Debug.LogWarning("Invalid scaling factor. Star size not updated.");
        }

        // Draw the smooth outline around the star
        float outlineRadius = star.transform.localScale.x * 0.495f;
        Color outlineColor = GameData.Instance.StarColor;
        float outlineWidth = 0.025f * starSize / 2.5f; // Adjust this value for the desired outline thickness
        DrawStarOutline(star, outlineRadius, outlineColor, outlineWidth);

        GenerateMassRatio();
        GenerateMassLine();
        GeneratePlanets();
        //Add a method to draw orbits of the planets
    }

    private void Update()
    {
        UpdateOrbitLineWidth();
    }

    public float AUToKelvin(float distanceInAU)
    {
        float sigma = 5.670374419e-8f; // Stefan-Boltzmann constant
        float auToMeters = 149597870700f; // 1 AU in meters
        float distanceInMeters = distanceInAU * auToMeters;

        // Calcualte incident stellar flux
        float Ix = (starLuminosity*3.826e26f) / (4 * Mathf.PI * distanceInMeters * distanceInMeters);
        // Calculate the temperature in Kelvin
        float temperature = Mathf.Pow(Ix * (0.67f) / (4 * sigma), 0.25f);
        return temperature;
    }

    private void UpdateOrbitLineWidth()
    {
        Camera mainCamera = Camera.main;
        float cameraSize = mainCamera.orthographicSize;
        float lineWidth;

        GameObject[] orbitObjects = GameObject.FindGameObjectsWithTag("OrbitRenderer");

        foreach (GameObject orbit in orbitObjects)
        {
            LineRenderer lineRenderer = orbit.GetComponent<LineRenderer>();
            lineWidth = Mathf.Lerp(minLineWidth, maxLineWidth, 1 - Mathf.Exp(-cameraSize / lineWidthDistanceFactor));
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
        }
    }

    private PlanetComposition GenerateComposition(float distanceInAU, float planetMass)
    {
        float temperature = AUToKelvin(distanceInAU);
        PlanetComposition composition = new PlanetComposition();

        // Determine the odds of gas based on planet mass and temperature
        float gasOdds = Mathf.Clamp01(planetMass * 0.1f + Mathf.InverseLerp(300f, 2000f, temperature));

        // Determine the odds of liquids based on temperature
        float liquidOdds = temperature < 300f ? Mathf.Clamp01(Mathf.InverseLerp(200f, 300f, temperature)) : 0f;

        composition.percentageOfGases = gasOdds;
        composition.percentageOfLiquids = liquidOdds;
        composition.percentageOfSilicates = 1f - (composition.percentageOfGases + composition.percentageOfLiquids);

        // For simplicity, metals are considered part of the silicates for this example
        composition.percentageOfMetals = 0f;

        return composition;
    }

    public void GenerateMassLine()
    {
        float temp = 0f;
        float rand = 0f;
        float massLeftToBuildPlanets = ratioOfPlanetaryMassToStar * starMass / 3.00273e-6f;
        for (int i = 0; i < nOfPlanets; i++)
        {
            do
            {
                rand = Random.value;
                if (rand < 0.233f)//terrestrial
                {
                    temp = randomValueBasedONGauss(0.494434f, 0.48337f, 0.00046f, 100000f);
                }
                else if(rand < 0.5f)//large-terrestrial
                {
                    temp = randomValueBasedONGauss(5f, 5f, 1f, 100000f);

                }
                else if(rand < 0.75f)//small gas-giant
                {
                    temp = randomValueBasedONGauss(30f, 10f, 10f, 100000f);
                }
                else//large gas-giant
                {
                    temp = randomValueBasedONGauss(206.5f, 157.68f, 40f, 100000f);
                }
            }while(temp > massLeftToBuildPlanets);

            massLeftToBuildPlanets = massLeftToBuildPlanets - temp;

            massLine.Add(temp);
        }
    }

    private void DrawStarOutline(GameObject star, float radius, Color color, float lineWidth)
    {
        GameObject outline = new GameObject("Outline");
        outline.transform.SetParent(star.transform);
        outline.transform.localPosition = Vector3.zero;

        LineRenderer lineRenderer = outline.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        
        Shader shader = Shader.Find("Unlit/Color");
        if (shader == null)
        {
            Debug.LogError("Shader not found!");
        }
        else
        {
            Debug.Log("Shader found!");
            lineRenderer.material = new Material(shader);
        }

        lineRenderer.material.color = color;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        int numSegments = 360;
        float angle = 0f;
        float angleStep = 360f / numSegments;

        lineRenderer.positionCount = numSegments + 1;

        for (int i = 0; i <= numSegments; i++)
        {
            float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector3 pos = new Vector3(x, y, 0);
            lineRenderer.SetPosition(i, pos);
            angle += angleStep;
        }
    }

    public float randomValueBasedONGauss(float mean, float standardDeviation, float minimumValue, float maximumValue)
    {
        float u1;
        float u2;
        float z0 ;

        float temp;
        do
        {
            u1 = Random.value;
            u2 = Random.value;

            z0 = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);
            temp = mean + z0 * standardDeviation;
        } while (temp < minimumValue || temp > maximumValue);

        return temp;
    }

    public void GenerateMassRatio()
    {
        ratioOfPlanetaryMassToStar = randomValueBasedONGauss(0.01f, 0.01f, 0.003f, 100000f);
    }


    public void GenerateBlaggFormulation()
    {
        distanceSeed = randomValueBasedONGauss(1.8275f, 0.125f, 1.2f, 100000f);
        Debug.Log("Distance factor is: " + distanceSeed);

        distanceSeedStandardDiv = Mathf.Abs(randomValueBasedONGauss(0f,10f,-1000f, 100000f)/100f);
        Debug.Log("Distance variance standard div is: " + distanceSeedStandardDiv);

        //Calculating the innermost orbit using trappist one and solar system as raff guideline
        firstOrbit = randomValueBasedONGauss(45f, 25f, 5f, 100000f);
        firstOrbit = firstOrbit * starSize / 86.0129f;
        Debug.Log("First orbit is: " + firstOrbit);
    }

    public void GeneratePlanets()
    {
        // Add the scaling factor calculation outside the loop
        float scalingFactor = firstOrbitRadiusInUnits / (firstOrbit * 5f);

        for (int i = 0; i < nOfPlanets; i++)
        {

        float distance = 0f;
        // If it's the first planet, set the distance to firstOrbit, otherwise calculate it using GetNextPlanetaryDistance
        if (i == 0)
        {
            distance = firstOrbit;
        }
        else
        {
            distance = GetNextPlanetaryDistance(distanceSeed, i, firstOrbit);
        }

            //PlanetComposition planetComposition = GenerateComposition(distance, massLine[i]);

            Planet planet = new Planet
            {
                orderInSystem = i+1,
                SMA_AU = distance,
                temperature_K = AUToKelvin(distance),
                hasAtmosphere = true,
                mass = massLine[i],
                trueAnomaly = Random.value,
                PlanetSeed = starSystemSeed + i
                //,
                //composition = planetComposition
            };

            planets.Add(planet);

            // Convert distance from AU to Unity units using the scaling factor
            float orbitRadius = planet.SMA_AU * 5 * scalingFactor;
            GameObject orbit = DrawOrbit(orbitRadius);

            // Instantiate the planet prefab and set its position and rotation
            GameObject newPlanet = Instantiate(planetPrefab, transform);
            newPlanet.transform.localPosition = new Vector3(orbitRadius * Mathf.Cos(planet.trueAnomaly * 2 * Mathf.PI), orbitRadius * Mathf.Sin(planet.trueAnomaly * 2 * Mathf.PI), 0f);
            newPlanet.transform.RotateAround(transform.position, Vector3.forward, planet.trueAnomaly * 360f);

            // Set the planet to the same size in the solar system view
            newPlanet.transform.localScale = Vector3.one * uniformPlanetScale;

            // Make the orbit a child of the planet
            orbit.transform.SetParent(newPlanet.transform);

            // Set the planet data in the PlanetManager script
            PlanetManager planetManager = newPlanet.GetComponent<PlanetManager>();
            planetManager.SetPlanetData(planet);
            planetManager.SetStarData(starLuminosity, starMass, starSize);
        }
    }

    public float GetNextPlanetaryDistance(float distanceSeed, int nthPlanet, float firstOrbit)
    {
        float mean = firstOrbit*Mathf.Pow(distanceSeed, nthPlanet);
        return randomValueBasedONGauss(mean,distanceSeedStandardDiv,mean*0.1f, 100000f);
    }

    private GameObject DrawOrbit(float radius)
    {
        GameObject orbit = Instantiate(orbitPrefab);
        orbit.transform.position = Vector3.zero;
        LineRenderer lineRenderer = orbit.GetComponent<LineRenderer>();

        int numSegments = 360;
        float angle = 0f;
        float angleStep = 360f / numSegments;

        lineRenderer.positionCount = numSegments + 1;

        for (int i = 0; i <= numSegments; i++)
        {
            float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector3 pos = new Vector3(x, y, 0);
            lineRenderer.SetPosition(i, pos);
            angle += angleStep;
        }

        return orbit;
    }
}