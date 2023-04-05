using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] public float panSpeed;
    [SerializeField] private WorldGenerator worldGenerator;
    [SerializeField] private Camera cam;
    [SerializeField] private float baseZoomSpeed = 1.0f;
    [SerializeField] private float maxZoomOut = 20.0f;
    [SerializeField] private GameObject player;
    [SerializeField] public float cameraAccelerationTime = 0.33f;
    [SerializeField] private float maxSpeed = 10f;
    public bool isMovingToPlayer = false;

    private Vector3 lastMousePosition;
    private HashSet<Vector2Int> loadedChunks = new HashSet<Vector2Int>();
    private float fKeyHoldTime = 0f;

    public bool cameraFollowsPlayer = false;
    private bool cameraFollowToggleEnabled = true;
    private bool cameraFlyToPlayerEnabled = true;

    private Vector3 targetPosition;
    private float toggleTimeLockingCamera = 0.3f;

    public int renderDistance = 1; // Change this value to control the number of chunks generated around the current chunk

    private void Start()
    {
        GenerateNewStarsIfNecessary();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 direction = lastMousePosition - cam.ScreenToViewportPoint(Input.mousePosition);
            cam.transform.position += new Vector3(direction.x * panSpeed, direction.y * panSpeed, 0);
            lastMousePosition = cam.ScreenToViewportPoint(Input.mousePosition);

            GenerateNewStarsIfNecessary();
            cameraFollowsPlayer = false;
        }

        // Handle zooming in and out
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            Vector3 mousePositionBeforeZoom = cam.ScreenToWorldPoint(Input.mousePosition);
            float newSize = cam.orthographicSize - scrollInput * baseZoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, 1, maxZoomOut);
            Vector3 mousePositionAfterZoom = cam.ScreenToWorldPoint(Input.mousePosition);

            cam.transform.position += mousePositionBeforeZoom - mousePositionAfterZoom;
            GenerateNewStarsIfNecessary();
        }

        // Handle F key input and camera movement
        if (Input.GetKeyUp(KeyCode.F) && cameraFlyToPlayerEnabled)
        {
            targetPosition = player.transform.position;
            isMovingToPlayer = true;
            fKeyHoldTime = 0f;
            cameraFlyToPlayerEnabled = false;
        }

        if (isMovingToPlayer)
        {
            float speed = Mathf.MoveTowards(0, maxSpeed, Time.deltaTime * cameraAccelerationTime);
            cameraAccelerationTime = cameraAccelerationTime * 2;
            Vector3 direction = (targetPosition - cam.transform.position).normalized;
            cam.transform.position += new Vector3(direction.x * speed * Time.deltaTime, direction.y * speed * Time.deltaTime, 0);

            if (Vector2.Distance(new Vector2(cam.transform.position.x, cam.transform.position.y), new Vector2(targetPosition.x, targetPosition.y)) < 0.02f)
            {
                isMovingToPlayer = false;
                GenerateNewStarsIfNecessary();
                cameraFlyToPlayerEnabled = true;
                cameraAccelerationTime = 100f;
            }
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            cameraFlyToPlayerEnabled = true;
        }

        if (Input.GetKey(KeyCode.F))
        {
            fKeyHoldTime += Time.deltaTime;

            if (fKeyHoldTime >= toggleTimeLockingCamera && cameraFollowToggleEnabled && !isMovingToPlayer)
            {
                cameraFollowsPlayer = !cameraFollowsPlayer;
                fKeyHoldTime = 0f;
                cameraFollowToggleEnabled = false;
            }
        }
        else
        {
            fKeyHoldTime = 0f;
            cameraFollowToggleEnabled = true;
        }

        if (cameraFollowsPlayer)
        {
            SetCameraAbovePlayer(player.transform.position);
            GenerateNewStarsIfNecessary();
        }
    }

    private void UnloadStarsOutsideVisibleRange(HashSet<Vector2Int> newLoadedChunks)
    {
        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        foreach (Vector2Int chunkCoords in loadedChunks)
        {
            if (!newLoadedChunks.Contains(chunkCoords))
            {
                chunksToUnload.Add(chunkCoords);
            }
        }

        foreach (Vector2Int chunkCoords in chunksToUnload)
        {
            worldGenerator.UnloadStarsInChunk(chunkCoords);
            loadedChunks.Remove(chunkCoords);
        }

        loadedChunks.UnionWith(newLoadedChunks);
    }

    public void GenerateNewStarsIfNecessary()
    {
        int renderBuffer = 1; // You can adjust this value to control the buffer size

        Vector2 camBottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0));
        Vector2 camTopRight = cam.ViewportToWorldPoint(new Vector3(1, 1));
        
        Vector2Int bottomLeftChunkCoords = new Vector2Int(Mathf.FloorToInt(camBottomLeft.x / worldGenerator.ChunkSize) - renderBuffer, Mathf.FloorToInt(camBottomLeft.y / worldGenerator.ChunkSize) - renderBuffer);
        Vector2Int topRightChunkCoords = new Vector2Int(Mathf.FloorToInt(camTopRight.x / worldGenerator.ChunkSize) + renderBuffer, Mathf.FloorToInt(camTopRight.y / worldGenerator.ChunkSize) + renderBuffer);
        
        HashSet<Vector2Int> newLoadedChunks = new HashSet<Vector2Int>();

        for (int x = bottomLeftChunkCoords.x; x <= topRightChunkCoords.x; x++)
        {
            for (int y = bottomLeftChunkCoords.y; y <= topRightChunkCoords.y; y++)
            {
                Vector2Int chunkCoords = new Vector2Int(x, y);
                worldGenerator.GenerateStarsInChunk(chunkCoords);
                newLoadedChunks.Add(chunkCoords);
            }
        }

        UnloadStarsOutsideVisibleRange(newLoadedChunks);
    }

    public void SetCameraAbovePlayer(Vector3 playerPosition)
    {
        cam.transform.position = new Vector3(playerPosition.x, playerPosition.y, cam.transform.position.z);
    }

}