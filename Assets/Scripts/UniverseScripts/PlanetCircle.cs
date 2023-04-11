using UnityEngine;

public class PlanetCircle : MonoBehaviour
{
    public int circleSegmentsPerPlanet = 2;
    public Material circleMaterial;
    public float planetCircleRadius = 0.1f;

    private StarSystemController starSystemController;
    private LineRenderer lineRenderer;

    private void Start()
    {
        starSystemController = GetComponent<StarSystemController>();
        //CreateCircle();
    }

    private void CreateCircle()
    {
        int numberOfCircles = starSystemController.NumberOfPlanets;
        int totalCircleSegments = numberOfCircles * circleSegmentsPerPlanet;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = circleMaterial;
        lineRenderer.widthMultiplier = 0.0025f;
        lineRenderer.positionCount = totalCircleSegments + 1;

        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[totalCircleSegments + 1];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[totalCircleSegments + 1];

        for (int i = 0; i < numberOfCircles; i++)
        {
            float angle = 2 * Mathf.PI * i / numberOfCircles;
            float x = (starSystemController.starSize + planetCircleRadius) * Mathf.Cos(angle);
            float y = (starSystemController.starSize + planetCircleRadius) * Mathf.Sin(angle);
            Vector3 position = transform.position + new Vector3(x, y, 0);

            lineRenderer.SetPosition(i * 2, position);
            lineRenderer.SetPosition(i * 2 + 1, position);
        }

        for (int i = 0; i < totalCircleSegments; i++)
        {
            float t = (float)i / (totalCircleSegments - 1);
            colorKeys[i].color = i % 2 == 0 ? starSystemController.starColor : new Color(0, 0, 0, 0);
            colorKeys[i].time = t;
            alphaKeys[i].alpha = i % 2 == 0 ? 1.0f : 0.0f;
            alphaKeys[i].time = t;
        }

        colorKeys[totalCircleSegments].color = starSystemController.starColor;
        colorKeys[totalCircleSegments].time = 1.0f;
        alphaKeys[totalCircleSegments].alpha = 1.0f;
        alphaKeys[totalCircleSegments].time = 1.0f;

        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }
}