using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlanetDataNamespace;

public class PlanetManager : MonoBehaviour
{
    public Planet planetData;

    public float starLuminosity;
    public float starMass;
    public float starSize;

    public Camera cam;

    public float massMarkerLineWidth;
    public float massMarkerLogBase;
    public float distanceOfMarkerFromPlanet;
    public float baseLogMilestoneLen;
    public float scalingOfMassMarkersLen;
    public Material massMarkerMaterial;
    
    public Color coldColor;
    public Color goldilocksLowerColor;
    public Color goldilocksUpperColor;
    public Color hotColor;
    public Color scorchingColor;

    public Color metalColor;
    public Color sillicaColor;
    public Color gasColor;
    public Color liquidColor;

    public float lastRecordedTime = 0;

    void Awake()
    {
        cam = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        Random.InitState(planetData.PlanetSeed);
        GenerateComposition();
        UpdatePlanetDensity();
        CalculateGravity();
        CalculatePeriod();
        //GenerateAtmosphere();
        UpdateOrbitColor();
        DisplayPlanetComposition();

        // Create mass and radius markers
        CreateMarkers(planetData.mass, planetData.radius);
    }

    void Update()
    {
        ScalePlanetCollider();
        UpdateTime();
    }

    private void UpdateTime()
    {
        if (GameData.Instance.DaysPassed - lastRecordedTime > 0f)
        {
            // Get the orbital period in days
            float orbitalPeriodInDays = planetData.orbitalPeriod * 365.25f;

            // Calculate the rotation angle based on GameData.DaysPassed and the orbital period
            float rotationAngle = ((GameData.Instance.DaysPassed - lastRecordedTime) / orbitalPeriodInDays) * 360f;

            lastRecordedTime = GameData.Instance.DaysPassed;

            // Store the planet's position before rotation
            Vector3 previousPosition = transform.position;

            // Rotate the planet around the orbit
            transform.RotateAround(transform.parent.position, Vector3.forward, rotationAngle);

            // Calculate the difference between the previous and current position
            Vector3 positionDifference = transform.position - previousPosition;

            // Update marker positions
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                LineRenderer lineRenderer = child.GetComponent<LineRenderer>();

                if (lineRenderer != null && child.tag != "OrbitRenderer")
                {
                    Vector3 startPosition = lineRenderer.GetPosition(0);
                    Vector3 endPosition = lineRenderer.GetPosition(1);

                    // Add the position difference to the marker positions
                    startPosition += positionDifference;
                    endPosition += positionDifference;

                    lineRenderer.SetPosition(0, startPosition);
                    lineRenderer.SetPosition(1, endPosition);
                }
            }
        }
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 direction = point - pivot;
        direction = Quaternion.Euler(angles) * direction;
        point = direction + pivot;
        return point;
    }

    private void ScalePlanetCollider()
    {
        float cameraOrthoSize = cam.orthographicSize;
        float colliderScaleFactor = cameraOrthoSize; // Adjust this value as needed

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null && colliderScaleFactor >= 1f)
        {
            collider.radius = colliderScaleFactor;
        }
    }

    private void GenerateAtmosphere()
    {

    }
    private void CalculateGravity()
    {
        float G = 6.674e-11f;
        float massOfPlanet = planetData.mass * 5.97e24f;
        float radiusOfPlanet = planetData.radius*6371000f;
        planetData.surfaceGravity = G * massOfPlanet / Mathf.Pow(radiusOfPlanet, 2f);
        planetData.escapeVelocity = Mathf.Pow((2f * G * massOfPlanet)/radiusOfPlanet, 1/2f);
    }

    private void CalculatePeriod()
    {
        float G = 6.674e-11f;
        float massOfStar = starMass * 1.989e30f;
        float orbitalDistanceInMeters = planetData.SMA_AU * 1.496e11f;
        planetData.orbitalPeriod = Mathf.Pow((4f * Mathf.Pow(Mathf.PI, 2f) * Mathf.Pow(orbitalDistanceInMeters, 3f))/(massOfStar * G), 1f/2f) / 31557600f;
    }

    private void DisplayPlanetComposition()
    {
        float percentage = 0f;

        if(planetData.composition.percentageOfMetals > 0)
        {
            percentage = percentage + planetData.composition.percentageOfMetals;
            float metalScale = Mathf.Pow(percentage * 4.18879f/(4/3*Mathf.PI) , 1f/3f);
            CreateCompositionCircle(metalColor, metalScale, 4);
        }

        if(planetData.composition.percentageOfSilicates > 0)
        {
            percentage = percentage + planetData.composition.percentageOfSilicates;
            float silicaScale = Mathf.Pow(percentage * 4.18879f/(4/3*Mathf.PI) , 1f/3f);
            CreateCompositionCircle(sillicaColor, silicaScale, 3);
        }

        if(planetData.composition.percentageOfLiquids > 0)
        {
            percentage = percentage + planetData.composition.percentageOfLiquids;
            float liquidScale = Mathf.Pow(percentage * 4.18879f/(4/3*Mathf.PI) , 1f/3f);
            CreateCompositionCircle(liquidColor, liquidScale, 2);
        }

        if(planetData.composition.percentageOfGases > 0)
        {
            percentage = percentage + planetData.composition.percentageOfGases;
            float gasScale = Mathf.Pow(percentage * 4.18879f/(4/3*Mathf.PI) , 1f/3f);
            CreateCompositionCircle(gasColor, gasScale, 1);
        }
    }

    private void CreateCompositionCircle(Color color, float scaleFactor, int order)
    {
        GameObject compositionCircle = new GameObject("CompositionCircle");
        compositionCircle.transform.SetParent(transform);
        compositionCircle.transform.localPosition = Vector3.zero;
        compositionCircle.transform.localScale = Vector3.one * scaleFactor;

        SpriteRenderer spriteRenderer = compositionCircle.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/filledCircle");
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = order; //Change order here
    }

    private void CreateMarkers(float mass, float radius)
    {
        // Create mass markers
        CreateMassOrRadiusMarkers(mass, new Vector3(1, 0, 0), false); // left
        CreateMassOrRadiusMarkers(mass, new Vector3(-1, 0, 0), false); // right

        // Create radius markers
        CreateMassOrRadiusMarkers(radius, new Vector3(0, 1, 0), true); // up
        CreateMassOrRadiusMarkers(radius, new Vector3(0, -1, 0), true); // down
    }

    private void CreateMassOrRadiusMarkers(float value, Vector3 direction, bool isRadius)
    {
        GameObject marker = new GameObject(isRadius ? "RadiusMarker" : "MassMarker");
        marker.transform.SetParent(transform, true);

        LineRenderer lineRenderer = marker.AddComponent<LineRenderer>();
        lineRenderer.material = massMarkerMaterial;
        lineRenderer.startWidth = massMarkerLineWidth;
        lineRenderer.endWidth = massMarkerLineWidth;

        float lineLength = Mathf.Log(value * 10f, massMarkerLogBase) * scalingOfMassMarkersLen;
        lineLength = Mathf.Abs(lineLength);

        Vector3 startPosition = transform.position + direction * distanceOfMarkerFromPlanet;
        Vector3 endPosition = startPosition + direction * lineLength;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);

        float temp = 0.1f;
        float nOfMarker = 0;
        do
        {
            CreateMilestoneMarker(transform.position + direction * (distanceOfMarkerFromPlanet + (nOfMarker * scalingOfMassMarkersLen)), temp, baseLogMilestoneLen, isRadius);
            temp = temp * massMarkerLogBase;
            nOfMarker = nOfMarker + 1f;
        } while (temp < value);
    }

    private void CreateMilestoneMarker(Vector3 position, float milestone, float lineLength, bool isRadius)
    {
        GameObject milestoneMarker = new GameObject($"MilestoneMarker_{milestone}");
        milestoneMarker.transform.SetParent(transform, true);

        LineRenderer milestoneLineRenderer = milestoneMarker.AddComponent<LineRenderer>();
        milestoneLineRenderer.material = massMarkerMaterial;
        milestoneLineRenderer.startWidth = massMarkerLineWidth;
        milestoneLineRenderer.endWidth = massMarkerLineWidth;

        Vector3 startPosition;
        Vector3 endPosition;

        if (isRadius)
        {
            startPosition = position + new Vector3(lineLength * 0.5f, 0, 0);
            endPosition = startPosition + new Vector3(lineLength * -1f, 0, 0);
        }
        else
        {
            startPosition = position + new Vector3(0, lineLength * 0.5f, 0);
            endPosition = startPosition + new Vector3(0, lineLength * -1f, 0);
        }

        milestoneLineRenderer.SetPosition(0, startPosition);
        milestoneLineRenderer.SetPosition(1, endPosition);
    }

    private void UpdateOrbitColor()
    {
        LineRenderer orbitLineRenderer = GetOrbitLineRenderer();

        if (orbitLineRenderer != null)
        {
            // Define temperature ranges
            float coldTemperature = 200f;
            float goldilocksLowerTemperature = 270f;
            float goldilocksUpperTemperature = 350f;
            float hotTemperature = 450f;
            float scorchingTemperature = 600f;

            // Calculate the orbit color based on the planet's temperature
            Color orbitColor = GetOrbitColor(coldTemperature, goldilocksLowerTemperature, goldilocksUpperTemperature,
                hotTemperature, scorchingTemperature);

            // Set the orbit color
            SetOrbitColor(orbitLineRenderer, orbitColor);
        }
    }

    private LineRenderer GetOrbitLineRenderer()
    {
        // Find the orbit child
        Transform orbitChild = transform.Find("orbitPrefab(Clone)");
        if (orbitChild != null)
        {
            // Access the LineRenderer component
            return orbitChild.GetComponent<LineRenderer>();
        }

        return null;
    }

    private Color GetOrbitColor(float coldTemperature, float goldilocksLowerTemperature, float goldilocksUpperTemperature,
        float hotTemperature, float scorchingTemperature)
    {
        // Define the color ranges
        ColorRange[] colorRanges = new ColorRange[]
        {
            new ColorRange(0, coldTemperature, Color.white, coldColor),
            new ColorRange(coldTemperature, goldilocksLowerTemperature, coldColor, goldilocksLowerColor),
            new ColorRange(goldilocksLowerTemperature, goldilocksUpperTemperature, goldilocksLowerColor, goldilocksUpperColor),
            new ColorRange(goldilocksUpperTemperature, hotTemperature, goldilocksUpperColor, hotColor),
            new ColorRange(hotTemperature, scorchingTemperature, hotColor, scorchingColor)
        };

        // Calculate the lerp factor and interpolate between the start and end colors
        foreach (ColorRange colorRange in colorRanges)
        {
            if (planetData.temperature_K < colorRange.upperTemperature)
            {
                float lerpFactor = Mathf.InverseLerp(colorRange.lowerTemperature, colorRange.upperTemperature, planetData.temperature_K);
                return Color.Lerp(colorRange.startColor, colorRange.endColor, lerpFactor);
            }
        }

        // Default to scorching color if the temperature is above the highest range
        return scorchingColor;
    }

    private void SetOrbitColor(LineRenderer orbitLineRenderer, Color orbitColor)
    {
        orbitLineRenderer.startColor = orbitColor;
        orbitLineRenderer.endColor = orbitColor;

        // Access the material for the specific orbit
        Material orbitMaterial = orbitLineRenderer.material;

        // Set the material's color to match the orbit color
        orbitMaterial.SetColor("_Color", orbitColor);
    }

    // Helper class to store color range data
    private class ColorRange
    {
        public float lowerTemperature;
        public float upperTemperature;
        public Color startColor;
        public Color endColor;

        public ColorRange(float lowerTemperature, float upperTemperature, Color startColor, Color endColor)
        {
            this.lowerTemperature = lowerTemperature;
            this.upperTemperature = upperTemperature;
            this.startColor = startColor;
            this.endColor = endColor;
        }
    }


    public void SetPlanetData(Planet data)
    {
        planetData = data;
    }

    public void SetStarData(float luminosity, float massOfStar, float sizeOfStar)
    {
        starLuminosity = luminosity;
        starMass = massOfStar;
        starSize = sizeOfStar;
    }

    public void UpdatePlanetDensity()
    {
        // Define the density constants (in kg/m^3)
        float gasDensity = 1.2f;
        float waterDensity = 1000f;
        float silicateDensity = 2320f; //from bing AI
        float metalDensity = 5500f; //from bing AI i eyeballed the averages

        // Calculate the average density of the planet based on its composition
        float averageDensity = planetData.composition.percentageOfGases * gasDensity +
                            planetData.composition.percentageOfLiquids * waterDensity +
                            planetData.composition.percentageOfSilicates * silicateDensity +
                            planetData.composition.percentageOfMetals * metalDensity;

        // Calculate the volume of the planet based on its mass and average density (mass = density * volume)
        float planetVolume = planetData.mass * 5.972e24f / averageDensity; // mass in kg (5.972e24f converts Earth masses to kg)

        // Calculate the radius of the planet using the volume (volume = 4/3 * pi * radius^3)
        float planetRadius = Mathf.Pow(planetVolume / (4f/3f * Mathf.PI), 1f / 3f); // radius in meters

        // Convert the radius to Earth radii (1 Earth radius = 6,371,000 meters)
        planetData.radius = planetRadius / 6371000f;
        planetData.density = averageDensity;
    }

    public float AUToKelvin(float distanceInAU)
    {
        float sigma = 5.670374419e-8f; // Stefan-Boltzmann constant
        float auToMeters = 149597870700f; // 1 AU in meters
        float distanceInMeters = distanceInAU * auToMeters;

        // Calculate incident stellar flux
        float Ix = (starLuminosity * 3.826e26f) / (4 * Mathf.PI * distanceInMeters * distanceInMeters);
        // Calculate the temperature in Kelvin
        float temperature = Mathf.Pow(Ix * (0.67f) / (4 * sigma), 0.25f);
        return temperature;
    }

    public float randomValueBasedONGauss(float mean, float standardDeviation, float minimumValue, float maximumValue)
    {
        System.Random random = new System.Random(planetData.PlanetSeed);

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

    public void GenerateComposition()
    {
        float distanceInAU = planetData.SMA_AU;
        float planetMass = planetData.mass;
        float temperature = AUToKelvin(distanceInAU);
        PlanetComposition composition = new PlanetComposition();

        // Calculate odds of each composition type
        float oddsOfGasGiant = CalculateGasGiantOdds(temperature, planetMass);
        float oddsOfWater = CalculateWaterOdds(temperature);
        float oddsOfSilicaAndMetal = CalculateSilicaAndMetalOdds(temperature);
        float totalOdds = oddsOfGasGiant + oddsOfWater + oddsOfSilicaAndMetal;

        // Normalize odds
        oddsOfGasGiant /= totalOdds;
        oddsOfWater /= totalOdds;

        // Determine composition type based on random value and calculated odds
        float compositionOutline = Random.value;
        if (compositionOutline < oddsOfGasGiant)
        {
            SetGasGiantComposition(ref composition, temperature);
        }
        else if (compositionOutline < oddsOfWater + oddsOfGasGiant)
        {
            SetWaterComposition(ref composition);
        }
        else
        {
            SetSilicaAndMetalComposition(ref composition);
        }

        planetData.composition = composition;
    }

    private float CalculateGasGiantOdds(float temperature, float planetMass)
    {
        float baseOdds = temperature > 340f ? 0.05f : 0.1f * ((temperature - 340f) / 50f + 1f);
        return baseOdds * (planetMass / 3f);
    }

    private float CalculateWaterOdds(float temperature)
    {
        return temperature > 340f ? 0f : 0.1f * ((temperature - 340f) / 30f + 1f);
    }

    private float CalculateSilicaAndMetalOdds(float temperature)
    {
        return temperature < 340f ? 0.9f / ((Mathf.Abs(temperature - 340f) / 30f) + 1f) : 0.9f;
    }

    private void SetGasGiantComposition(ref PlanetComposition composition, float temperature)
    {
        composition.percentageOfGases = temperature > 303f ? 1f : randomValueBasedONGauss(100f, 3f, 25f, 100f) / 100f;
        composition.percentageOfLiquids = 1f - composition.percentageOfGases;
        composition.percentageOfSilicates = 0f;
        composition.percentageOfMetals = 0f;
        planetData.compositionDescriptor = "Gaseous";
    }

    private void SetWaterComposition(ref PlanetComposition composition)
    {
        composition.percentageOfGases = 0f;
        composition.percentageOfLiquids = Random.value;
        composition.percentageOfSilicates = (1f - composition.percentageOfLiquids) * Random.value;
        composition.percentageOfMetals = 1f - composition.percentageOfLiquids - composition.percentageOfSilicates;
        planetData.compositionDescriptor = "Aqueous";
    }

    private void SetSilicaAndMetalComposition(ref PlanetComposition composition)
    {
        composition.percentageOfGases = 0f;
        composition.percentageOfLiquids = 0f;
        composition.percentageOfSilicates = randomValueBasedONGauss(50f, 50f, 0f, 100f) / 100f;
        composition.percentageOfMetals = 1f - composition.percentageOfSilicates;
        planetData.compositionDescriptor = composition.percentageOfSilicates > composition.percentageOfMetals ? "Silicic" : "Metallic";
    }

}