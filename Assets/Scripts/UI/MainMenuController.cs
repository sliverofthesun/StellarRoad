using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO; // Add this to use Path.Combine

public class MainMenuController : MonoBehaviour
{
    private int newSeed;
    [SerializeField] private TMP_InputField seedInputField; 
    [SerializeField] private GameObject seedAndGenerateWorldParent;
    private string saveFilePath; // Add this to store the save file path

    [SerializeField] private GameObject savePanel;
    [SerializeField] private Transform savesListParent;
    [SerializeField] private GameObject saveItemPrefab;
    [SerializeField] private Button loadGameButton;

    private void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "player_position.json");
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Set up the save panel
        savePanel.SetActive(false);
        loadGameButton.onClick.AddListener(OpenLoadPanel);
    }

    public void SetSeed(int seed)
    {
        if (GameData.Instance != null)
        {
            GameData.Instance.UniverseSeed = seed;
        }
    }

    public void NewGame()
    {
        seedAndGenerateWorldParent.SetActive(true);
    }

    public void HideSeedAndGenerateWorldParent()
    {
        seedAndGenerateWorldParent.SetActive(false);
    }

    private void OpenLoadPanel()
    {
        savePanel.SetActive(true);
        RefreshSavesList();
    }

    private void RefreshSavesList()
    {
        // Clear the existing list items
        foreach (Transform child in savesListParent)
        {
            Destroy(child.gameObject);
        }

        // Get the list of save files
        string savesFolderPath = Application.persistentDataPath + "/saves";
        if (!Directory.Exists(savesFolderPath))
        {
            Directory.CreateDirectory(savesFolderPath);
        }
        string[] saveFiles = Directory.GetFiles(savesFolderPath, "*.json");

        // Create list items for each save file
        foreach (string saveFile in saveFiles)
        {
            GameObject saveItem = Instantiate(saveItemPrefab, savesListParent);
            saveItem.GetComponentInChildren<TextMeshProUGUI>(true).text = Path.GetFileNameWithoutExtension(saveFile);
            Button saveItemButton = saveItem.GetComponentInChildren<Button>();

            // Left-click to load the save file
            saveItemButton.onClick.AddListener(() => LoadGame(saveFile));
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            WorldGenerator worldGenerator = FindObjectOfType<WorldGenerator>();
            worldGenerator.GenerateWorld(newSeed);

            PlayerController playerController = FindObjectOfType<PlayerController>();
            playerController.PlacePlayerAtClosestStar();
        }
    }
    
    public void OnGenerateWorldButtonClicked()
    {
        int seed;
        if (int.TryParse(seedInputField.text, out seed))
        {
            SetSeed(seed);
        }
        else
        {
            SetSeed(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        }

        HideSeedAndGenerateWorldParent();
        SceneManager.LoadScene("GameScene");
    }

    public void LoadGame(string saveFilePath)
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (GameData.Instance == null)
            {
                Debug.LogError("GameData.Instance is null.");
                return;
            }

            // Set the seed in the GameData instance
            GameData.Instance.UniverseSeed = saveData.universeSeed;

            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
            {
                if (scene.name == "GameScene")
                {
                    // Load the world with the saved seed
                    WorldGenerator worldGenerator = FindObjectOfType<WorldGenerator>();

                    if (worldGenerator == null)
                    {
                        Debug.LogError("WorldGenerator is null.");
                        return;
                    }

                    // Update the seed in the WorldGenerator
                    worldGenerator.SetSeed(saveData.universeSeed);

                    // Unload existing chunks
                    worldGenerator.UnloadAllChunks();

                    // Generate the new world
                    worldGenerator.GenerateWorld(saveData.universeSeed);

                    // Move the camera to the saved position
                    if (Camera.main == null)
                    {
                        Debug.LogError("Camera.main is null.");
                        return;
                    }

                    Camera.main.transform.position = saveData.cameraPosition;

                    // Generate new stars immediately after loading the game
                    CameraController cameraController = Camera.main.GetComponent<CameraController>();
                    if (cameraController == null)
                    {
                        Debug.LogError("CameraController is null.");
                        return;
                    }

                    cameraController.GenerateNewStarsIfNecessary();

                    // Set the player's position
                    PlayerController playerController = FindObjectOfType<PlayerController>();
                    if (playerController == null)
                    {
                        Debug.LogError("PlayerController is null.");
                        return;
                    }

                    playerController.transform.position = saveData.playerUniversePosition;

                    // Generate new chunks around the player
                    worldGenerator.GenerateChunksAroundPlayer();

                    Debug.Log("Game loaded from: " + saveFilePath);
                }
            };

            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.Log("No save file found.");
        }
    }

    public void Options()
    {
        // Implement options functionality here
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}