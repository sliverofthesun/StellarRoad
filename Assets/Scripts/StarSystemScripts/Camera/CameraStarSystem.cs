using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStarSystem : MonoBehaviour
{
    [SerializeField] public float basePanSpeed = 1.0f;
    [SerializeField] private Camera cam;
    [SerializeField] private float baseZoomSpeed = 1.0f;
    [SerializeField] private float maxZoomOut = 500.0f;

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
    }
}