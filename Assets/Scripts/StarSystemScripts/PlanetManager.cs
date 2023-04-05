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
    public Color warmColor;
    public Color hotColor;
    public Color veryHotColor;

    public Color metalColor;
    public Color sillicaColor;
    public Color gasColor;
    public Color liquidColor;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        Random.InitState(planetData.PlanetSeed);
        GenerateComposition();
        UpdatePlanetDensity();
        CalculateGravity();
        //GenerateAtmosphere();
        UpdateOrbitColor();
        DisplayPlanetComposition();
        CreateMassMarkers(1);
        CreateMassMarkers(-1);
        CreateRadiusMarkers(1);
        CreateRadiusMarkers(-1);

    }

    void Update()
    {
        //UpdatePlanetScale();
    }

    private void GenerateAtmosphere()
    {

    }
    private void CalculateGravity()
    {
        float G = 6.674e-11f;
        planetData.surfaceGravity = G * (planetData.mass * 5.97e24f) / Mathf.Pow(planetData.radius*6371000f, 2f);
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


    private void CreateRadiusMarkers(int direction)
    {
        GameObject radiusMarker = new GameObject("RadiusMarker");
        radiusMarker.transform.SetParent(transform, false);

        LineRenderer lineRenderer = radiusMarker.AddComponent<LineRenderer>();
        lineRenderer.material = massMarkerMaterial;
        lineRenderer.startWidth = massMarkerLineWidth;
        lineRenderer.endWidth = massMarkerLineWidth;

        float lineLength = Mathf.Log(planetData.radius*10f, massMarkerLogBase) * scalingOfMassMarkersLen;

        Vector3 startPosition = transform.position + new Vector3(0, distanceOfMarkerFromPlanet*direction, 0);
        Vector3 endPosition = startPosition + new Vector3(0, lineLength*direction, 0);

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);

        float temp = massMarkerLogBase / massMarkerLogBase / massMarkerLogBase;
        float nOfMarker = 0;
        do 
        {
            CreateMarker(direction * (distanceOfMarkerFromPlanet + (nOfMarker*scalingOfMassMarkersLen)), temp, baseLogMilestoneLen, true);
            temp = temp * massMarkerLogBase;
            nOfMarker = nOfMarker + 1f;
        }while (temp<planetData.radius);
    }

    private void CreateMassMarkers(int direction)
    {
        GameObject massMarker = new GameObject("MassMarker");
        massMarker.transform.SetParent(transform, false);

        LineRenderer lineRenderer = massMarker.AddComponent<LineRenderer>();
        lineRenderer.material = massMarkerMaterial;
        lineRenderer.startWidth = massMarkerLineWidth;
        lineRenderer.endWidth = massMarkerLineWidth;

        float lineLength = Mathf.Log(planetData.mass*10f, massMarkerLogBase) * scalingOfMassMarkersLen;
        // Ensure lineLength is always positive
        lineLength = Mathf.Abs(lineLength);

        Vector3 startPosition = transform.position + new Vector3(distanceOfMarkerFromPlanet*direction, 0, 0);
        Vector3 endPosition = startPosition + new Vector3(lineLength*direction, 0, 0);

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);

        float temp = 0.1f;//massMarkerLogBase / massMarkerLogBase / massMarkerLogBase;
        float nOfMarker = 0;
        do 
        {
            CreateMarker(direction * (distanceOfMarkerFromPlanet + (nOfMarker*scalingOfMassMarkersLen)), temp, baseLogMilestoneLen, false);
            temp = temp * massMarkerLogBase;
            nOfMarker = nOfMarker + 1f;
        }while (temp<planetData.mass);
    }

    private void CreateMarker(float position, float milestone, float lineLength, bool isUp)
    {
        GameObject milestoneMarker = new GameObject($"MilestoneMarker_{milestone}");
        milestoneMarker.transform.SetParent(transform, false);

        LineRenderer milestoneLineRenderer = milestoneMarker.AddComponent<LineRenderer>();
        milestoneLineRenderer.material = massMarkerMaterial;
        milestoneLineRenderer.startWidth = massMarkerLineWidth;
        milestoneLineRenderer.endWidth = massMarkerLineWidth;

        if (isUp)
        {
            Vector3 startPosition = transform.position + new Vector3(lineLength * 0.5f, position, 0);
            Vector3 endPosition = startPosition + new Vector3(lineLength * -1f, 0, 0); // Adjust the -0.02f value to change the length of the vertical line
               milestoneLineRenderer.SetPosition(0, startPosition);
        milestoneLineRenderer.SetPosition(1, endPosition);
        }
        else{
            Vector3 startPosition = transform.position + new Vector3(position, lineLength * 0.5f, 0);
            Vector3 endPosition = startPosition + new Vector3(0, lineLength * -1f, 0); // Adjust the -0.02f value to change the length of the vertical line
               milestoneLineRenderer.SetPosition(0, startPosition);
        milestoneLineRenderer.SetPosition(1, endPosition);
        }
        
 
    }

    private void UpdateOrbitColor()
    {
        // Find the orbit child
        Transform orbitChild = transform.Find("orbitPrefab(Clone)");
        if (orbitChild != null)
        {
            // Access the LineRenderer component
            LineRenderer orbitLineRenderer = orbitChild.GetComponent<LineRenderer>();

            if (orbitLineRenderer != null)
            {
                // Define temperature ranges and corresponding colors
                float coldTemperature = 200f;
                float warmTemperature = 300f;
                float hotTemperature = 450f;

                // Change the orbit color based on the planet's temperature
                if (planetData.temperature_K < coldTemperature)
                {
                    orbitLineRenderer.startColor = coldColor;
                    orbitLineRenderer.endColor = coldColor;
                }
                else if (planetData.temperature_K < warmTemperature)
                {
                    orbitLineRenderer.startColor = warmColor;
                    orbitLineRenderer.endColor = warmColor;
                }
                else if (planetData.temperature_K < hotTemperature)
                {
                    orbitLineRenderer.startColor = hotColor;
                    orbitLineRenderer.endColor = hotColor;
                }
                else
                {
                    orbitLineRenderer.startColor = veryHotColor;
                    orbitLineRenderer.endColor = veryHotColor;
                }

                // Access the material for the specific orbit
                Material orbitMaterial = orbitLineRenderer.material;

                // Set the material's color to match the start color
                orbitMaterial.SetColor("_Color", orbitLineRenderer.startColor);
            }
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

        float compositionOutline = Random.value;
        float oddsOfGasGiant = temperature > 340f ? 0.05f : 0.1f*((temperature-340f)/50f+1f); //5% chance for gas world if too hot
        oddsOfGasGiant = oddsOfGasGiant * (planetMass/3f);
        float oddsOfWater = temperature > 340f ? 0f : 0.1f*((temperature-340f)/30f+1f); //0 chance for water world if too hot
        float oddsOfSilicaAndMetal = temperature < 340f ? 0.9f/((Mathf.Abs(temperature-340f)/30f)+1f) : 0.9f; //cloase t0 chance for no water when cold

        float t = oddsOfGasGiant + oddsOfWater + oddsOfSilicaAndMetal;
        oddsOfGasGiant = oddsOfGasGiant / t;
        oddsOfWater = oddsOfWater / t;

        if (compositionOutline < oddsOfGasGiant)
        {
            // Set gas giant composition
            if (temperature > 303f) //to account for no water where too hot
            {
                composition.percentageOfGases = 1f;
            }
            else
            {
                composition.percentageOfGases = randomValueBasedONGauss(100f, 3f, 25f, 100f)/100f;
            }

            composition.percentageOfLiquids = 1f - composition.percentageOfGases;
            composition.percentageOfSilicates = 0f;
            composition.percentageOfMetals = 0f;

            planetData.compositionDescriptor = "Gaseous";
        }
        else if(compositionOutline < oddsOfWater + oddsOfGasGiant)
        {
            composition.percentageOfGases = 0f;
            composition.percentageOfLiquids = Random.value;
            composition.percentageOfSilicates = (1f-composition.percentageOfLiquids)*Random.value;
            composition.percentageOfMetals = 1f - composition.percentageOfLiquids - composition.percentageOfSilicates;
            planetData.compositionDescriptor = "Aqueous";
        }
        else
        {
            composition.percentageOfGases = 0f;
            composition.percentageOfLiquids = 0f;
            composition.percentageOfSilicates = randomValueBasedONGauss(50f, 50f, 0f, 100f)/100f;
            composition.percentageOfMetals = 1f - composition.percentageOfSilicates;
            if(composition.percentageOfSilicates>composition.percentageOfMetals)
            {
                planetData.compositionDescriptor = "Silicic";
            }
            else{
                planetData.compositionDescriptor = "Metallic";
            }
        }

        planetData.composition = composition;
    }
}