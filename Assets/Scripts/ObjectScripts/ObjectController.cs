using System.Collections;
using System.Collections.Generic;
using PlanetDataNamespace;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public GameObject planetPrefab;

    // Start is called before the first frame update
    void Start()
    {
        // Instantiate the planet at the center of the world
        GameObject planet = Instantiate(planetPrefab, Vector3.zero, Quaternion.identity);

        // Set the planet's scale based on its radius
        float planetRadius = GameData.Instance.CurrentPlanet.radius;
        planet.transform.localScale = new Vector3(planetRadius, planetRadius, planetRadius);
    }

    // Update is called once per frame
    void Update()
    {

    }
}