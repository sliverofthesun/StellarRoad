using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StarSystemPlayerController : MonoBehaviour
{
    public GameObject selectedPlanet;
    public float speedAUPerHour = 1f; // Set this to the desired value
    public float unityUnitsPerAU = 10f; // Set this value to define how many Unity units are in 1 AU
    private Camera cam;
    private float timeTakenInHours;
    private bool isMovingToTarget;
    private Vector3 targetPosition;
    public float optimalHours;

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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ReturnToUniverseView();
        }

        if (Input.GetMouseButtonDown(0))
        {
            CheckForPlanetClick();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (selectedPlanet != null)
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
                Debug.Log("It will take: " + ((int)(optimalHours*100))/100f + " hours");
                Debug.Log("Will arrive on: " + (optimalHours/24f+GameData.Instance.DaysPassed));

                targetPosition = GetFuturePlanetPosition(optimalHours);
                StartMovingToTargetPosition();
            }
        }

        if (isMovingToTarget)
        {
            MovePlayerToTargetPosition();
        }
    }

    // private void FindTargetPosition()
    // {
    //     float distanceToPlanet = Vector3.Distance(transform.position, selectedPlanet.transform.position) / unityUnitsPerAU;
    //     float tolerance = 5f;
    //     float daysPassed = 0;

    //     bool targetFound = false;
    //     while (!targetFound)
    //     {
    //         float timeToPlanet = distanceToPlanet / speedAUPerHour;
    //         float orbitalPeriodInDays = selectedPlanet.GetComponent<PlanetManager>().planetData.orbitalPeriod * 365.25f;
    //         float rotationAngle = (timeToPlanet / 24f + daysPassed) / orbitalPeriodInDays * 360f;

    //         Vector3 predictedPosition = RotatePointAroundPivot(selectedPlanet.transform.position, selectedPlanet.transform.parent.position, new Vector3(0, 0, rotationAngle));
    //         float predictedDistance = Vector3.Distance(transform.position, predictedPosition);

    //         if (Mathf.Abs(predictedDistance - distanceToPlanet) < tolerance)
    //         {
    //             targetFound = true;
    //             timeTakenInHours = timeToPlanet;
    //             targetPosition = predictedPosition;
    //             isMovingToTarget = true;
    //         }
    //         else
    //         {
    //             daysPassed += 1;
    //         }
    //     }
    // }

    public float getDistanceToSelectedPlanet(float atTime) //atTime must be in hours
    {    
        float planetSMAInAU = selectedPlanet.GetComponent<PlanetManager>().planetData.SMA_AU; //b
        float playerDistanceFromStarInAU = Vector3.Distance(transform.position, selectedPlanet.transform.position) / unityUnitsPerAU; //c
        float startingAngleOfPlanet = CalculateAngleBetweenPlayerAndPlanet(); //a
        float periodOfPlanet = selectedPlanet.GetComponent<PlanetManager>().planetData.orbitalPeriod; //period in years
        periodOfPlanet = periodOfPlanet * 365.25f * 24f;
        periodOfPlanet = 1 / periodOfPlanet;

        float sinPlanetDistance = planetSMAInAU * (Mathf.Sin(periodOfPlanet * (atTime + startingAngleOfPlanet / periodOfPlanet) )) + playerDistanceFromStarInAU;

        return sinPlanetDistance;
    }

    private float CalculateAngleBetweenPlayerAndPlanet()
    {
        // Get the position vectors for the player and the planet
        Vector3 playerPosition = transform.position;
        Vector3 planetPosition = selectedPlanet.transform.position;

        // Normalize the position vectors
        Vector3 playerNormalized = playerPosition.normalized;
        Vector3 planetNormalized = planetPosition.normalized;

        // Calculate the dot product between the normalized vectors
        float dotProduct = Vector3.Dot(playerNormalized, planetNormalized);

        // Calculate the angle between the vectors using the dot product (in radians)
        float angle = Mathf.Acos(dotProduct);

        return angle;
    }

    public float timeScaleFactor = 3600f; // 1 hour = 1 second

    private float originalDistanceToTarget;

    private void StartMovingToTargetPosition()
    {
        originalDistanceToTarget = Vector3.Distance(transform.position, targetPosition);
        previousDistanceToTarget = originalDistanceToTarget;
        isMovingToTarget = true;
    }

    private float previousDistanceToTarget;

    private void MovePlayerToTargetPosition()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        float step = (speedAUPerHour * unityUnitsPerAU) / 3600f * Time.deltaTime * timeScaleFactor;
        float percentageChange;

        if (distanceToTarget > step)
        {
            // Move the player towards the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            percentageChange = (previousDistanceToTarget - distanceToTarget) / originalDistanceToTarget;
            previousDistanceToTarget = distanceToTarget;
            // Calculate the remaining distance percentage
            //float remainingDistancePercentage = distanceToTarget / originalDistanceToTarget;

            // Update GameData.Instance.DaysPassed gradually based on the remaining distance
            GameData.Instance.DaysPassed += percentageChange * (optimalHours / 24f);
        }
        else
        {
            // Stop moving the player when it reaches the target position
            transform.position = targetPosition;
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
