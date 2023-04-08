using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class WorldGenerator : MonoBehaviour
{
    // Universe-related variables
    [SerializeField] private int chunkSize;
    public int ChunkSize => chunkSize;
    private Dictionary<Vector2Int, List<GameObject>> starChunks = new Dictionary<Vector2Int, List<GameObject>>();
    private System.Random random;
    private int seed;
    public int Seed => seed;
    public static WorldGenerator Instance;
    public event Action OnWorldGenerated;

    // Star-related variables
    [SerializeField] private int starCount;
    [SerializeField] private float minStarMass;
    [SerializeField] private float maxStarMass;
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private List<StarClass> starClasses;
    public List<StarClass> StarClasses => starClasses;
    public float minStarSizeOnMap;
    public float maxStarSizeOnMap;

    // Player-related variables
    public PlayerController playerController;

    // Camera-related variables
    public CameraController cameraController;

    private void Start()
    {
        Instance = this;
        GameData.Instance.PlayerScenePosition = 1;

        seed = GameData.Instance != null ? GameData.Instance.UniverseSeed : 0; // Using ternary operator to simplify the assignment
        GenerateWorld(seed);
    }

    public void GenerateWorld(int newSeed)
    {
        SetSeed(newSeed);
        GenerateStarsInChunk(Vector2Int.zero);
        OnWorldGenerated?.Invoke();

        Vector3 playerPos = GameData.Instance.PlayerPosition != null ? GameData.Instance.PlayerPosition : Vector3.zero; // Using ternary operator to simplify the assignment
        playerController.SetPlayerPosition(playerPos);

        cameraController.SetCameraAbovePlayer(playerController.transform.position);
        GenerateChunksAroundPlayer();
        playerController.PlacePlayerAtClosestStar();
    }

    public void SetSeed(int newSeed)
    {
        seed = newSeed;
        random = new System.Random(newSeed);
    }

    public void UnloadStarsInChunk(Vector2Int chunkCoords)
    {
        if (starChunks.TryGetValue(chunkCoords, out List<GameObject> starsInChunk))
        {
            foreach (GameObject star in starsInChunk)
            {
                Destroy(star);
            }

            starChunks.Remove(chunkCoords);
        }
    }

    public void GenerateChunksAroundPlayer()
    {
        Vector3 playerPos = playerController.transform.position;
        Vector2Int playerChunk = new Vector2Int(Mathf.FloorToInt(playerPos.x / chunkSize), Mathf.FloorToInt(playerPos.y / chunkSize));

        int renderDistance = 1; // You can change this value to control the number of chunks generated around the player

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                Vector2Int chunkPos = playerChunk + new Vector2Int(x, y);
                if (!starChunks.ContainsKey(chunkPos))
                {
                    GenerateStarsInChunk(chunkPos);
                }
            }
        }
    }

    public void UnloadAllChunks()
    {
        // Create a temporary list to store the keys of the chunks to unload
        List<Vector2Int> chunksToUnload = new List<Vector2Int>(starChunks.Keys);

        // Iterate over the temporary list and unload the chunks
        foreach (var chunkKey in chunksToUnload)
        {
            UnloadStarsInChunk(chunkKey);
        }
    }

    private void Update() {
        GenerateChunksAroundPlayer();
    }

    public void GenerateStarsInChunk(Vector2Int chunkCoords)
    {
        if (starChunks.ContainsKey(chunkCoords))
        {
            return; // Stars in this chunk have already been generated
        }

        System.Random chunkRandom = new System.Random(seed ^ chunkCoords.GetHashCode());
        List<GameObject> starsInChunk = new List<GameObject>();

        for (int i = 0; i < starCount; i++)
        {
            Vector2 position = new Vector2(chunkCoords.x * chunkSize + (float)chunkRandom.NextDouble() * chunkSize, chunkCoords.y * chunkSize + (float)chunkRandom.NextDouble() * chunkSize);

            GameObject newStar = Instantiate(starPrefab, new Vector3(position.x, position.y, 0f), Quaternion.identity, transform);

            // Assign a random star class based on likelihood
            StarClass starClass = GetRandomStarClass(chunkRandom);

            // Generate a unique seed for the star system
            int starSystemSeed = HashSeed(seed, chunkCoords, i);
            if(Mathf.Abs(position.y) < 1f && Mathf.Abs(position.x) < 1f)
            {
                Debug.Log("Star at:" + position.x + " , " + position.y + " has seed: " + starSystemSeed);
            }

            // Generate a random star mass based on the star class
            float starMass = (float)chunkRandom.NextDouble() * (starClass.maxStarMass - starClass.minStarMass) + starClass.minStarMass;

            // Assign the star class and star mass to the Star component
            StarSystemController starComponent = newStar.GetComponent<StarSystemController>();
            starComponent.SetStarClassAndMassAndSeed(starClass, starMass, starSystemSeed);

            // Create a new StarSystem and set it to the StarSystemController
            // StarSystem newStarSystem = new StarSystem
            // {
            //     Seed = starComponent.StarSystemSeed,
            //     NumberOfPlanets = starComponent.NumberOfPlanets
            // };
            starComponent.StarSystemSeed = starSystemSeed;

            starsInChunk.Add(newStar);
        }
        starChunks.Add(chunkCoords, starsInChunk);
    }

    private int HashSeed(int worldSeed, Vector2Int chunkCoords, int starIndex)
    {
        int hash = 23;
        hash = hash * 31 + worldSeed;
        hash = hash * 31 + chunkCoords.x;
        hash = hash * 31 + chunkCoords.y;
        hash = hash * 31 + starIndex;
        return hash;
    }

    private StarClass GetRandomStarClass(System.Random random)
    {
        float totalLikelihood = 0f;

        foreach (StarClass starClass in starClasses)
        {
            totalLikelihood += starClass.likelihood;
        }

        float randomValue = (float)random.NextDouble() * totalLikelihood;
        float currentLikelihood = 0f;

        foreach (StarClass starClass in starClasses)
        {
            currentLikelihood += starClass.likelihood;

            if (randomValue < currentLikelihood)
            {
                return starClass;
            }
        }

        return starClasses[starClasses.Count - 1];
    }
}