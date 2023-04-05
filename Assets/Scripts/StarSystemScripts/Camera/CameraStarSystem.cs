using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStarSystem : MonoBehaviour
{
    [SerializeField] public float basePanSpeed = 1.0f;
    [SerializeField] private Camera cam;
    [SerializeField] private float baseZoomSpeed = 1.0f;
    [SerializeField] private float maxZoomOut = 500.0f;

    public string inputBuffer = "";

    private Vector3 lastMousePosition;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            float panSpeed = basePanSpeed * cam.orthographicSize;
            Vector3 direction = lastMousePosition - cam.ScreenToViewportPoint(Input.mousePosition);
            cam.transform.position += new Vector3(direction.x * panSpeed, direction.y * panSpeed, 0);
            lastMousePosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        // Handle zooming in and out
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            Vector3 mousePositionBeforeZoom = cam.ScreenToWorldPoint(Input.mousePosition);
            float newSize = cam.orthographicSize - scrollInput * baseZoomSpeed * cam.orthographicSize;
            cam.orthographicSize = Mathf.Clamp(newSize, 0.01f, maxZoomOut);
            Vector3 mousePositionAfterZoom = cam.ScreenToWorldPoint(Input.mousePosition);

            cam.transform.position += mousePositionBeforeZoom - mousePositionAfterZoom;
        }

        // Handle input for numbers 0-9 and backspace
        foreach (char c in Input.inputString)
        {
            if (char.IsDigit(c) || c == '\b') // Check if the character is a digit or a backspace
            {
                inputBuffer += c;
            }
        }

        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            inputBuffer = "";
        }

        // Check if the 'Enter' key is pressed
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (int.TryParse(inputBuffer, out int orderInSystem))
            {
                GameObject targetPlanet = FindPlanetByOrder(orderInSystem);
                if (targetPlanet != null)
                {
                    float distance = Vector3.Distance(cam.transform.position, targetPlanet.transform.position);
                    float duration = Mathf.Clamp(distance * 0.01f, 0.5f, 3.0f); // Adjust the speed based on distance
                    StartCoroutine(MoveCameraToPlanet(targetPlanet, duration));
                }
            }
            inputBuffer = ""; // Clear the input buffer
        }
    }

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