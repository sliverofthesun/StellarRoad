using System.Collections;
using UnityEngine;
using PlanetDataNamespace;
using UnityEngine.SceneManagement;

public class StarSystemPlayerController : MonoBehaviour
{
    // Player selection and movement
    private Camera cam; // Camera component to detect clicks on planets
    public GameObject selectedPlanet; // The currently selected planet GameObject
    private bool isMovingToTarget; // Flag to determine if the player is currently moving to the target position
    private Vector3 targetPosition; // The target position for the player to move to

    // Distance and time calculations
    public float speedAUPerHour = 1f; // Speed of the player in Astronomical Units (AU) per hour
    public float unityUnitsPerAU = 10f; // Conversion factor for Unity units to Astronomical Units (AU)
    private float timeTakenInHours; // Time taken to travel between planets in hours
    public float optimalHours; // Optimal hours needed to travel to the selected planet
    public float timeScaleFactor = 3600f; // Time scale factor to convert in-game hours to real-time seconds

    // Tracking distance to target
    private float originalDistanceToTarget; // Original distance to the target position
    private float previousDistanceToTarget; // Previous distance to the target position for percentage calculation

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        StartCoroutine(MovePlayerToFarthestPlanetAfterDelay());
        GameData.Instance.PlayerScenePosition = 2;
    }

    private IEnumerator MovePlayerToFarthestPlanetAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        MovePlayerToFarthestPlanet();
        InitializeUnityUnitsPerAU();
    }

    private void InitializeUnityUnitsPerAU()
    {
        GameObject planetObject = GameObject.FindWithTag("Planet");

        if (planetObject != null)
        {
            PlanetManager planetManager = planetObject.GetComponent<PlanetManager>();

            if (planetManager != null && planetManager.planetData != null)
            {
                float distanceFromOrigin = Vector3.Distance(planetObject.transform.position, Vector3.zero);
                unityUnitsPerAU = distanceFromOrigin / planetManager.planetData.SMA_AU;
            }
        }
    }

    private void Update()
    {
        ProcessInput();

        if (isMovingToTarget)
        {
            MovePlayerToTargetPosition();
        }
    }

    private void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ReturnToUniverseView();
        }

        if (Input.GetMouseButtonDown(0))
        {
            CheckForPlanetClick();
        }

        if (Input.GetKeyDown(KeyCode.Space) && selectedPlanet != null)
        {
            CalculateOptimalHours();
            targetPosition = GetFuturePlanetPosition(optimalHours);
            StartMovingToTargetPosition();
        }

        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            timeScaleFactor = timeScaleFactor * 2f;
        }

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            timeScaleFactor = timeScaleFactor * 0.5f;
        }

        //Add this block inside the ProcessInput() method
        if (Input.GetKeyDown(KeyCode.E))
        {
            EnterPlanetSystem();
        }
    }

    private void CalculateOptimalHours()
    {
        float minHours = 0;
        float maxHours = 1000;
        float tolerance = 0.1f;

        while (maxHours - minHours > tolerance)
        {
            float midHours = (minHours + maxHours) / 2;

            float distancePlayerCanCrossInHours = midHours * speedAUPerHour;
            float distanceToPlanetInMidHours = getDistanceToSelectedPlanet(midHours);
            float difference = distanceToPlanetInMidHours - distancePlayerCanCrossInHours;

            if (difference > 0)
            {
                minHours = midHours;
            }
            else
            {
                maxHours = midHours;
            }
        }

        optimalHours = (minHours + maxHours) / 2;
    }

    public float getDistanceToSelectedPlanet(float atTime) //atTime must be in hours
    {
        float planetSMAInAU = selectedPlanet.GetComponent<PlanetManager>().planetData.SMA_AU;
        float playerDistanceFromStarInAU = Vector3.Distance(transform.position, selectedPlanet.transform.position) / unityUnitsPerAU;
        float startingAngleOfPlanet = CalculateAngleBetweenPlayerAndPlanet();
        float periodOfPlanet = selectedPlanet.GetComponent<PlanetManager>().planetData.orbitalPeriod * 365.25f * 24f;
        periodOfPlanet = 1 / periodOfPlanet;

        float sinPlanetDistance = planetSMAInAU * (Mathf.Sin(periodOfPlanet * (atTime + startingAngleOfPlanet / periodOfPlanet))) + playerDistanceFromStarInAU;
        return sinPlanetDistance;
    }

    private float CalculateAngleBetweenPlayerAndPlanet()
    {
        Vector3 playerPosition = transform.position;
        Vector3 planetPosition = selectedPlanet.transform.position;

        Vector3 playerNormalized = playerPosition.normalized;
        Vector3 planetNormalized = planetPosition.normalized;

        float dotProduct = Vector3.Dot(playerNormalized, planetNormalized);
        float angle = Mathf.Acos(dotProduct);

        return angle;
    }

    private void StartMovingToTargetPosition()
    {
        originalDistanceToTarget = Vector3.Distance(transform.position, targetPosition);
        previousDistanceToTarget = originalDistanceToTarget;
        isMovingToTarget = true;
    }

    private void MovePlayerToTargetPosition()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        float step = (speedAUPerHour * unityUnitsPerAU) / 3600f * Time.deltaTime * timeScaleFactor;
        float percentageChange;

        if (distanceToTarget > step)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            percentageChange = (previousDistanceToTarget - distanceToTarget) / originalDistanceToTarget;
            previousDistanceToTarget = distanceToTarget;

            GameData.Instance.DaysPassed += percentageChange * (optimalHours / 24f);
        }
        else
        {
            transform.position = targetPosition;
            GameData.Instance.CurrentPlanet = selectedPlanet.GetComponent<PlanetManager>().planetData;
            isMovingToTarget = false;
        }
    }

    private Vector3 GetFuturePlanetPosition(float hoursIntoFuture)
    {
        float periodOfPlanet = selectedPlanet.GetComponent<PlanetManager>().planetData.orbitalPeriod * 365.25f * 24f;
        float rotationAngle = (hoursIntoFuture / periodOfPlanet) * 360f;

        Vector3 futurePlanetPosition = RotatePointAroundPivot(
            selectedPlanet.transform.position,
            selectedPlanet.transform.parent.position,
            new Vector3(0, 0, rotationAngle)
        );

        return futurePlanetPosition;
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 direction = point - pivot;
        direction = Quaternion.Euler(angles) * direction;
        point = direction + pivot;
        return point;
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

        GameData.Instance.CurrentPlanet = farthestPlanet.planetData;
    }

    private void CheckForPlanetClick()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

        if (hit.collider != null && hit.collider.CompareTag("Planet"))
        {
            selectedPlanet = hit.collider.gameObject;
        }
    }

    private void ReturnToUniverseView()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (GameData.Instance != null && playerController != null)
        {
            GameData.Instance.PlayerPosition = playerController.transform.position;
        }

        SceneManager.LoadScene("GameScene");
        StartCoroutine(SetPlayerPositionAfterSceneLoad());
    }

    private void EnterPlanetSystem()
    {
        SceneManager.LoadScene(3);
    }

    private IEnumerator SetPlayerPositionAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.1f);
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (GameData.Instance != null && GameData.Instance.PlayerPosition != Vector3.zero && playerController != null)
        {
        playerController.SetPlayerPosition(GameData.Instance.PlayerPosition);
        }
    }
}