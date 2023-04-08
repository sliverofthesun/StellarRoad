using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStarSystem : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject player;

    [SerializeField] private float basePanSpeed = 1.0f;
    [SerializeField] private float baseZoomSpeed = 1.0f;
    [SerializeField] private float maxZoomOut = 500.0f;
    [SerializeField] private float cameraAccelerationTime = 0.33f;
    [SerializeField] private float maxSpeed = 10f;

    private Vector3 lastMousePosition;
    private Vector3 targetPosition;

    private bool movingCamToPlayer = false;
    private float fKeyHoldTime = 0f;
    private bool cameraFollowsPlayer = false;

    public string inputBuffer = "";

    void Update()
    {
        HandleCameraPanning();
        HandleCameraZooming();
        HandlePlanetSelectionInput();
        HandleCameraFlyToPlayer();
    }

    // Handles camera panning when middle mouse button is held down
    private void HandleCameraPanning()
    {
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            movingCamToPlayer = false;
            cameraFollowsPlayer = false;
            float panSpeed = basePanSpeed * cam.orthographicSize;
            Vector3 direction = lastMousePosition - cam.ScreenToViewportPoint(Input.mousePosition);
            cam.transform.position += new Vector3(direction.x * panSpeed, direction.y * panSpeed, 0);
            lastMousePosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }
    }

    // Handles camera zooming based on mouse scroll wheel input
    private void HandleCameraZooming()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            Vector3 mousePositionBeforeZoom = cam.ScreenToWorldPoint(Input.mousePosition);
            float newSize = cam.orthographicSize - scrollInput * baseZoomSpeed * cam.orthographicSize;
            cam.orthographicSize = Mathf.Clamp(newSize, 0.01f, maxZoomOut);
            Vector3 mousePositionAfterZoom = cam.ScreenToWorldPoint(Input.mousePosition);

            cam.transform.position += mousePositionBeforeZoom - mousePositionAfterZoom;
        }
    }

    // Handles the input of numbers and backspace for planet selection
    private void HandlePlanetSelectionInput()
    {
        foreach (char c in Input.inputString)
        {
            if (char.IsDigit(c) || c == '\b')
            {
                inputBuffer += c;
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            inputBuffer = "";
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (int.TryParse(inputBuffer, out int orderInSystem))
            {
                GameObject targetPlanet = FindPlanetByOrder(orderInSystem);
                if (targetPlanet != null)
                {
                    movingCamToPlayer = false;
                    cameraFollowsPlayer = false;
                    float distance = Vector3.Distance(cam.transform.position, targetPlanet.transform.position);
                    float duration = Mathf.Clamp(distance * 0.01f, 0.5f, 3.0f);
                    StartCoroutine(MoveCameraToPlanet(targetPlanet, duration));
                }
            }
            inputBuffer = "";
        }
    }

    private void HandleCameraFlyToPlayer()
    {
        if (Input.GetKeyDown(KeyCode.F) && !cameraFollowsPlayer)
        {
            movingCamToPlayer = true;
            targetPosition = new Vector3(player.transform.position.x, player.transform.position.y, cam.transform.position.z);
        }

        if (Input.GetKey(KeyCode.F))
        {
            fKeyHoldTime += Time.deltaTime;
            if (fKeyHoldTime >= 1.0f)
            {
                cameraFollowsPlayer = !cameraFollowsPlayer;
                movingCamToPlayer = false;
                fKeyHoldTime = 0f;
            }
        }
        else
        {
            fKeyHoldTime = 0f;
        }

        if (movingCamToPlayer)
        {
            targetPosition = new Vector3(player.transform.position.x, player.transform.position.y, cam.transform.position.z);
            float distanceToTarget = Vector3.Distance(cam.transform.position, targetPosition);
            float speed = Mathf.Min(maxSpeed, distanceToTarget / cameraAccelerationTime);
            Vector3 direction = (targetPosition - cam.transform.position).normalized;
            cam.transform.position += new Vector3(direction.x * speed * Time.deltaTime, direction.y * speed * Time.deltaTime, 0);

            if (Vector2.Distance(new Vector2(cam.transform.position.x, cam.transform.position.y), new Vector2(targetPosition.x, targetPosition.y)) < 0.02f)
            {
                movingCamToPlayer = false;
            }
        }

        if (cameraFollowsPlayer && !movingCamToPlayer)
        {
            cam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, cam.transform.position.z);
        }
    }


    // Finds a planet GameObject by its order in the system
    private GameObject FindPlanetByOrder(int orderInSystem)
    {
        GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
        foreach (GameObject planet in planets)
        {
            PlanetManager planetManager = planet.GetComponent<PlanetManager>();
            if (planetManager != null && planetManager.planetData.orderInSystem == orderInSystem)
            {
                return planet;
            }
        }
        return null;
    }

    // Coroutine for moving the camera to a target planet smoothly
    IEnumerator MoveCameraToPlanet(GameObject targetPlanet, float duration)
    {
        Vector3 startPosition = cam.transform.position;
        Vector3 targetPosition = new Vector3(targetPlanet.transform.position.x, targetPlanet.transform.position.y, cam.transform.position.z);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            cam.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cam.transform.position = targetPosition;
    }
}