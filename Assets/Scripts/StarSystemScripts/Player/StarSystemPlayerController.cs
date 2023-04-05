using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StarSystemPlayerController : MonoBehaviour
{
    public GameObject selectedPlanet;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        StartCoroutine(MovePlayerToFarthestPlanetAfterDelay());
    }

    private IEnumerator MovePlayerToFarthestPlanetAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // Wait for 0.5 seconds before moving the player
        MovePlayerToFarthestPlanet();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ReturnToUniverseView();
        }

        if (Input.GetMouseButtonDown(0))
        {
            CheckForPlanetClick();
        }
    }

    private void MovePlayerToFarthestPlanet()
    {
        PlanetManager[] planets = FindObjectsOfType<PlanetManager>();
        PlanetManager farthestPlanet = null;
        int highestOrder = -1;

        foreach (PlanetManager planet in planets)
        {
            if (planet.planetData.orderInSystem > highestOrder)
            {
                highestOrder = planet.planetData.orderInSystem;
                farthestPlanet = planet;
            }
        }

        if (farthestPlanet != null)
        {
            transform.position = farthestPlanet.transform.position;
        }
    }

    private void CheckForPlanetClick()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

        if (hit.collider != null && hit.collider.CompareTag("Planet"))
        {
            selectedPlanet = hit.collider.gameObject;
            Debug.Log("Selected planet: " + selectedPlanet.name);
        }
    }

    private void ReturnToUniverseView()
    {
        Debug.Log("Called return to unvierse view.");
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (GameData.Instance != null && playerController != null)
        {
            GameData.Instance.UniversePlayerPosition = playerController.transform.position;
        }
        // Call the method to load the UniverseViewScene here
        SceneManager.LoadScene("GameScene");
        StartCoroutine(SetPlayerPositionAfterSceneLoad());
    }

    private IEnumerator SetPlayerPositionAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.1f); // Give the scene enough time to load before setting the player's position
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (GameData.Instance != null && GameData.Instance.UniversePlayerPosition != Vector3.zero && playerController != null)
        {
            playerController.SetPlayerPosition(GameData.Instance.UniversePlayerPosition);
        }
    }
}
