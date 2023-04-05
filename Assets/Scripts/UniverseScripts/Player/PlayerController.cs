using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public Camera mainCamera;
    public LineRenderer lineRenderer;
    public float moveSpeed = 5f;
    public GameObject pulsatingCirclePrefab;
    private GameObject pulsatingCircleInstance;
    private float travelProgress = 0f;
    public WorldGenerator worldGenerator;
    private const float SafeDistanceFromStar = 5.0f;
    private GameObject nextTargetStar;

    public float maxTravelDistance = 50f;
    public LineRenderer travelRadiusLineRenderer;

    private GameObject targetStar;
    private bool moving;

    private string saveFilePath;
    private bool positionSetFromSave = false;

    [SerializeField]
    private GameObject _currentStarInEditor;
    public GameObject CurrentStar { get; private set; }

    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "player_position.json");

        // Add these lines to initialize GameData.Instance.CurrentStarSystem
        if (GameData.Instance.CurrentStarSystem == null)
        {
            GameData.Instance.CurrentStarSystem = new StarSystem();
        }
    }

    private void Start()
    {
        worldGenerator.OnWorldGenerated += () =>
        {
            DrawTravelRadiusCircle();

            if (GameData.Instance != null && GameData.Instance.UniversePlayerPosition != Vector3.zero)
            {
                Debug.Log("Loaded player location as: " + GameData.Instance.UniversePlayerPosition);
                Debug.Log("Player is currently at: " + transform.position);
                SetPlayerPosition(GameData.Instance.UniversePlayerPosition);
            }
            else
            {
                if (!positionSetFromSave)
                {
                    SetPlayerPosition(new Vector3(0, 0, 0));
                    PlacePlayerAtClosestStar();
                }
            }
        };
    }

    public void PlacePlayerAtCoord(Vector2 position)
    {
        transform.position = position;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Star"))
            {
                float distance = Vector3.Distance(transform.position, hit.collider.transform.position);
                if (distance <= maxTravelDistance)
                {
                    if (!moving)
                    {
                        targetStar = hit.collider.gameObject;
                    }
                    else
                    {
                        nextTargetStar = hit.collider.gameObject;
                    }
                    DrawLineToTarget();
                }
                else if (nextTargetStar != null && !moving)
                {
                    // Remove the line renderer connection to the previously selected star
                    lineRenderer.SetPosition(0, Vector3.zero);
                    lineRenderer.SetPosition(1, Vector3.zero);
                    nextTargetStar = null;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && !moving)
        {
            StarSystemController starSystemController = GetNearestStarSystem(); // Implement this function to get the nearest StarSystemController
            GameData.Instance.UniversePlayerPosition = transform.position;
            EnterStarSystem();
        }

        if (Input.GetKeyDown(KeyCode.Space) && targetStar != null && !moving)
        {
            moving = true;
        }
        
        if (moving)
        {
            CurrentStar = null; // Player is in transit, so there is no current star
            // Update line renderer to shorten the line based on player's travel
            lineRenderer.SetPosition(0, transform.position);
            MovePlayerToTargetStar();

            // Hide pulsating circle while moving
            if (pulsatingCircleInstance != null)
            {
                pulsatingCircleInstance.SetActive(false);
            }
        }
        else
        {
            GameObject closestStar = FindClosestStar();
            if (closestStar != null && Vector3.Distance(transform.position, closestStar.transform.position) < 0.1f)
            {
                CreatePulsatingCircle(closestStar.transform.position);
                pulsatingCircleInstance.SetActive(true);
            }
            else if (pulsatingCircleInstance != null)
            {
                pulsatingCircleInstance.SetActive(false);
            }
        }

        // Update the travel radius circle position
        travelRadiusLineRenderer.transform.position = transform.position;
    }

    private StarSystemController GetNearestStarSystem()
    {
        GameObject nearestStar = FindClosestStar();
        if (nearestStar != null)
        {
            StarSystemController starSystemController = nearestStar.GetComponent<StarSystemController>();
            if (starSystemController != null)
            {
                return starSystemController;
            }
        }
        return null;
    }

    private void DrawTravelRadiusCircle()
    {
        int numSegments = 360;
        float radius = maxTravelDistance;
        travelRadiusLineRenderer.positionCount = numSegments + 1;
        travelRadiusLineRenderer.useWorldSpace = false;

        for (int i = 0; i < numSegments + 1; i++)
        {
            float angle = (float)i / (float)numSegments * 360f * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);
            travelRadiusLineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    public void PlacePlayerAtClosestStar()
    {
        WorldGenerator worldGenerator = FindObjectOfType<WorldGenerator>();
        float closestDistance = float.MaxValue;
        GameObject closestStar = null;
            
        foreach (Transform star in worldGenerator.transform)
        {
            float distance = Vector2.Distance(transform.position, star.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestStar = star.gameObject;
            }
        }

        if (closestStar != null)
        {
            SetCurrentStarSystem(closestStar);
            transform.position = closestStar.transform.position;
        }
    }

    public void SetPlayerPositionAfterWorldGeneration(Vector3 position)
    {
        WorldGenerator worldGenerator = FindObjectOfType<WorldGenerator>();

        worldGenerator.OnWorldGenerated += () => { transform.position = position; };
    }

    private void OnApplicationQuit()
    {
        SavePosition();
    }

    private void DrawLineToTarget()
    {
        Vector3 playerCenter = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Vector3 targetStarCenter = new Vector3(targetStar.transform.position.x, targetStar.transform.position.y, targetStar.transform.position.z);

        lineRenderer.SetPosition(0, playerCenter);
        lineRenderer.SetPosition(1, targetStarCenter);
    }

    private void MovePlayerToTargetStar()
    {
        Time.timeScale = 1;
        float distance = Vector3.Distance(transform.position, targetStar.transform.position);
        if (distance < 0.01f)
        {
            moving = false;

            // Set the current star system in the GameData
            StarSystemController starSystemController = targetStar.GetComponent<StarSystemController>();

            if (starSystemController != null)
            {
                Debug.Log("Reached star");
                GameData.Instance.CurrentStarSystemController = starSystemController;

            }
            else
            {
                Debug.Log("StarSystemController or StarSystem not found in the GameObject");
            }

            SetCurrentStarSystem(targetStar);

            // Check if the next target star is still within the travel radius
            if (nextTargetStar != null)
            {
                float nextTargetDistance = Vector3.Distance(transform.position, nextTargetStar.transform.position);
                if (nextTargetDistance <= maxTravelDistance)
                {
                    targetStar = nextTargetStar;
                    DrawLineToTarget();
                }
                else
                {
                    targetStar = null;
                    lineRenderer.SetPosition(0, Vector3.zero);
                    lineRenderer.SetPosition(1, Vector3.zero);
                }
            }
            else
            {
                targetStar = null;
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, Vector3.zero);
            }

            nextTargetStar = null;
            travelProgress = 0f;

            // Position the pulsating circle around the current star
            if (pulsatingCircleInstance != null)
            {
                pulsatingCircleInstance.transform.position = transform.position;
            }
        }
        else
        {
            travelProgress += Time.deltaTime * moveSpeed / distance;
            travelProgress = Mathf.Clamp01(travelProgress);
            float smoothStepValue = Mathf.SmoothStep(0, 1, travelProgress);
            transform.position = Vector3.Lerp(transform.position, targetStar.transform.position, smoothStepValue);
        }
    }

    private void CreatePulsatingCircle(Vector3 position)
    {
        if (pulsatingCirclePrefab != null)
        {
            if (pulsatingCircleInstance == null)
            {
                pulsatingCircleInstance = Instantiate(pulsatingCirclePrefab, position, Quaternion.identity);
            }
            else
            {
                pulsatingCircleInstance.transform.position = position;
            }
        }
    }

    private void OnDestroy()
    {
        if (pulsatingCircleInstance != null)
        {
            Destroy(pulsatingCircleInstance);
        }

        worldGenerator.OnWorldGenerated -= PlacePlayerAtClosestStar;
    }

    public void SavePosition()
    {
        Vector3 position = transform.position;
        string json = JsonUtility.ToJson(position);
        File.WriteAllText(saveFilePath, json);
    }

    private GameObject FindClosestStar()
    {
        GameObject[] stars = GameObject.FindGameObjectsWithTag("Star");
        GameObject closestStar = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject star in stars)
        {
            float distance = Vector3.Distance(transform.position, star.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestStar = star;
            }
        }

        return closestStar;
    }

    private void EnterStarSystem()
    {
        SceneManager.LoadScene("StarSystemScene");
    }

    public void SetPlayerPosition(Vector3 position)
    {
        positionSetFromSave = true;
        transform.position = position;
        mainCamera.transform.position = new Vector3(position.x, position.y, -10f);
        WorldGenerator.Instance.GenerateChunksAroundPlayer(); // Use the instance to call the method
        SetCurrentStarSystem(FindClosestStar()); // Set the current star system based on the new position
    }

    private void SetCurrentStarSystem(GameObject star)
    {
        if (star == null)
        {
            Debug.LogError("The provided star GameObject is null.");
            return;
        }

        StarSystemController starSystemController = star.GetComponent<StarSystemController>();
        if (starSystemController != null)
        {
            if (GameData.Instance != null)
            {
                CurrentStar = star; // Set the CurrentStar property here
                _currentStarInEditor = star; // Set the value for the Inspector
                GameData.Instance.UniversePlayerPosition = transform.position;
                GameData.Instance.SystemSeed = starSystemController.StarSystemSeed;
                GameData.Instance.StarMass = starSystemController.StarMass;
                GameData.Instance.StarSize = starSystemController.starSize;
                GameData.Instance.StarLuminosity = starSystemController.StarLuminosity;
                GameData.Instance.NumberOfPlanets = starSystemController.NumberOfPlanets;
                GameData.Instance.StarColor = starSystemController.starColor;
            }
            else
            {
                Debug.LogError("GameData.Instance or GameData.Instance.CurrentStarSystem is null");
            }
        }
        else
        {
            Debug.LogError("StarSystemController or StarSystem not found in the GameObject");
        }
    }
}
