using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planetPointerController : MonoBehaviour
{
    public Camera cam;

    [SerializeField] private GameObject planetPointerPrefab;
    [SerializeField] private string planetTag = "Planet";

    private GameObject closerPointer;
    private GameObject fartherPointer;

    void Start()
    {
        closerPointer = Instantiate(planetPointerPrefab);
        fartherPointer = Instantiate(planetPointerPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        (GameObject closestPlanet, GameObject farthestPlanet) = FindClosestAndFarthestPlanets();
        PointTowardPlanet(closerPointer, closestPlanet);
        PointTowardPlanet(fartherPointer, farthestPlanet);
    }

    private void PointTowardPlanet(GameObject pointer, GameObject planet)
    {
        if (planet != null)
        {
            Vector3 direction = (planet.transform.position - cam.transform.position).normalized;
            pointer.transform.position = cam.transform.position + direction * 2f; // Adjust the 2f value to set the desired distance of the pointer from the camera
            pointer.transform.LookAt(planet.transform);
        }
    }

    private (GameObject, GameObject) FindClosestAndFarthestPlanets()
    {
        GameObject[] planets = GameObject.FindGameObjectsWithTag(planetTag);
        GameObject closestPlanet = null;
        GameObject farthestPlanet = null;
        float closestDistance = float.MaxValue;
        float farthestDistance = float.MinValue;
        float camDistance = Vector3.Distance(cam.transform.position, Vector3.zero);

        foreach (GameObject planet in planets)
        {
            float distance = Vector3.Distance(planet.transform.position, Vector3.zero);
            if (distance < camDistance && distance < closestDistance)
            {
                closestDistance = distance;
                closestPlanet = planet;
            }
            if (distance > camDistance && distance > farthestDistance)
            {
                farthestDistance = distance;
                farthestPlanet = planet;
            }
        }

        return (closestPlanet, farthestPlanet);
    }
}