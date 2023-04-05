using UnityEngine;

public class StarRaysController : MonoBehaviour
{
    public float maxLength = 5.0f;
    public float fattnessFactor = 3.0f;
    private StarSystemController starSystemController;
    private MeshRenderer[] rayRenderers;
    public Material transparentMaterial;

    private void Start()
    {
        starSystemController = GetComponentInParent<StarSystemController>();
        transform.position = starSystemController.transform.position; // Set "Rays" GameObject position to the star's position
        CreateRays();
    }

    private void CreateRays()
    {
        int numberOfRays = starSystemController.NumberOfPlanets;
        rayRenderers = new MeshRenderer[numberOfRays];
        float starSize = starSystemController.transform.localScale.x; // Get the star size

        for (int i = 0; i < numberOfRays; i++)
        {
            GameObject ray = new GameObject($"Ray {i}");
            ray.transform.SetParent(transform);

            MeshFilter meshFilter = ray.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = ray.AddComponent<MeshRenderer>();
            rayRenderers[i] = meshRenderer;

            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(0, maxLength, 0),
                new Vector3(-1f * fattnessFactor * starSize, 0, 0),
                new Vector3(fattnessFactor * starSize, 0, 0)
            };
            mesh.triangles = new int[] { 0, 1, 2 };

            meshFilter.mesh = mesh;

            // Set material with transparent shader
            meshRenderer.material = transparentMaterial;

            // Set the ray color to match the star color
            meshRenderer.material.color = starSystemController.spriteRenderer.color;

            // Set the ray position to match the star position
            ray.transform.position = starSystemController.transform.position;

            // Set the ray color to match the star color, but with lower alpha for transparency
            Color starColor = starSystemController.spriteRenderer.color;
            meshRenderer.material.SetColor("_Color", new Color(starColor.r, starColor.g, starColor.b, starColor.a * 0.5f));

            // Set the ray's local scale with Z set to 1
            ray.transform.localScale = new Vector3(1, 1, -1);

            // Rotate the ray around the star
            float angle = 360f / numberOfRays * i;
            ray.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}