using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPointerController : MonoBehaviour
{
    public Camera cam;
    public string planetTag = "Planet";
    public Image closerPointer;
    public Image fartherPointer;

    void Update()
    {
        (GameObject closestPlanet, GameObject farthestPlanet) = FindClosestAndFarthestPlanets();

        PositionPointer(closerPointer, closestPlanet);
        PositionPointer(fartherPointer, farthestPlanet);
    }

    private void PositionPointer(Image pointer, GameObject planet)
    {
        if (planet != null)
        {
            Vector3 direction = (planet.transform.position - cam.transform.position).normalized;
            Vector3 screenPosition = cam.WorldToViewportPoint(planet.transform.position);

            // Check if the planet is in view
            bool planetInView = screenPosition.x > 0 && screenPosition.x < 1 && screenPosition.y > 0 && screenPosition.y < 1;

            if (!planetInView)
            {
                pointer.enabled = true;

                // Clamp screen position
                screenPosition.x = Mathf.Clamp(screenPosition.x, 0.1f, 0.9f);
                screenPosition.y = Mathf.Clamp(screenPosition.y, 0.1f, 0.9f);

                pointer.transform.position = new Vector3(screenPosition.x * Screen.width, screenPosition.y * Screen.height, 0);

                // Calculate the direction between the pointer and the planet in screen space
                Vector2 pointerScreenPos = pointer.transform.position;
                Vector2 planetScreenPos = cam.WorldToScreenPoint(planet.transform.position);
                Vector2 screenDirection = (planetScreenPos - pointerScreenPos).normalized;

                // Set the pointer rotation based on the screen direction
                float angle = Mathf.Atan2(screenDirection.y, screenDirection.x) * Mathf.Rad2Deg - 90;
                pointer.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                pointer.enabled = false;
            }
        }
        else
        {
            pointer.enabled = false;
        }
    }

    private (GameObject, GameObject) FindClosestAndFarthestPlanets()
    {
        GameObject[] planets = GameObject.FindGameObjectsWithTag(planetTag);
        GameObject closestPlanet = null;
        GameObject farthestPlanet = null;
        float closestDistance = float.MinValue;
        float farthestDistance = float.MaxValue;
        float camDistance = Vector2.Distance(cam.transform.position, Vector2.zero);
        foreach (GameObject planet in planets)
        {
            float distance = Vector2.Distance(planet.transform.position, Vector2.zero);
            if (distance < camDistance && distance > closestDistance)
            {
                closestDistance = distance;
                closestPlanet = planet;
            }
            if (distance > camDistance && distance < farthestDistance)
            {
                farthestDistance = distance;
                farthestPlanet = planet;
            }
        }

        return (closestPlanet, farthestPlanet);
    }
}