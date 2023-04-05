using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using System.Collections;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button saveGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitGameButton;
    [SerializeField] private GameObject savePanel;
    [SerializeField] private Transform savesListParent;
    [SerializeField] private Button createNewSaveButton;
    [SerializeField] private GameObject saveItemPrefab;


    private bool isPaused = false;

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
        savePanel.SetActive(false);

        saveGameButton.onClick.AddListener(() => { OpenSavePanel();
        });

        createNewSaveButton.onClick.AddListener(() => SaveGame(""));


        loadGameButton.onClick.AddListener(OpenLoadPanel);


        exitGameButton.onClick.AddListener(ExitGame);
        optionsButton.onClick.AddListener(Options);
    }

    private enum PanelMode { Save, Load }

    private void OpenLoadPanel()
    {
        OpenSaveLoadPanel(PanelMode.Load);
    }

    private void OpenSaveLoadPanel(PanelMode mode)
    {
        savePanel.SetActive(true);
        RefreshSavesList(mode);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void TogglePauseMenu()
    {
        if (savePanel.activeInHierarchy)
        {
            savePanel.SetActive(false);
        }
        else
        {
            isPaused = !isPaused;
            pauseMenuPanel.SetActive(isPaused);

            if (isPaused)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }

    private void OpenSavePanel()
    {
        OpenSaveLoadPanel(PanelMode.Save);
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

            // Set the data in the GameData instance
            GameData.Instance.PlayerScenePosition = saveData.playerScenePosition;
            GameData.Instance.UniverseSeed = saveData.universeSeed;
            GameData.Instance.SystemSeed = saveData.systemSeed;
            GameData.Instance.UniversePlayerPosition = saveData.playerUniversePosition;
            GameData.Instance.CameraPosition = saveData.cameraPosition;
            GameData.Instance.StarMass = saveData.starMass;
            GameData.Instance.StarSize = saveData.starSize;
            GameData.Instance.StarLuminosity = saveData.starLuminosity;
            GameData.Instance.NumberOfPlanets = saveData.numberOfPlanets;
            GameData.Instance.StarColor = saveData.starColor;

            Debug.Log("Game loaded from: " + saveFilePath);

            // Load the proper scene based on the playerScenePosition and start the WaitForSceneLoad coroutine
            StartCoroutine(WaitForSceneLoad(saveData));
        }
    }

    private IEnumerator WaitForSceneLoad(SaveData saveData)
    {
        Debug.Log("Starting scene loading");
        // Load the scene asynchronously
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(saveData.playerScenePosition);

            Debug.Log("ZERO");

        // Wait for the scene to load
        while (!asyncOperation.isDone)
        {
            Debug.Log("Scene loading progress: " + asyncOperation.progress);

            yield return null;
        }

        Debug.Log("Scene loaded");

        if (saveData.playerScenePosition == 1) // Universe view
        {
            Debug.Log("TWO");
            // Load the world with the saved universe seed
            WorldGenerator worldGenerator = FindObjectOfType<WorldGenerator>();

            if (worldGenerator == null)
            {
                Debug.LogError("WorldGenerator is null.");
                yield break;
            }

            // Update the seed in the WorldGenerator
            worldGenerator.SetSeed(saveData.universeSeed);

            // Unload existing chunks
            Debug.Log("THREE");
            worldGenerator.UnloadAllChunks();

            // Generate the new world
            worldGenerator.GenerateWorld(saveData.universeSeed);

            // Set the camera and player positions
            //Camera.main.transform.position = saveData.cameraPosition;
            //PlayerController playerController = FindObjectOfType<PlayerController>();
            //playerController.transform.position = saveData.playerUniversePosition;
            Debug.Log("Moving player!");

            // Generate new chunks around the player
            worldGenerator.GenerateChunksAroundPlayer();
        }
        else if (saveData.playerScenePosition == 2) // Star system view
        {
            // Load the star system with the saved system seed
            //StarSystemManager starSystemManager = FindObjectOfType<StarSystemManager>();

            // if (starSystemManager == null)
            // {
            //     Debug.LogError("StarSystemManager is null.");
            //     yield break;
            // }

            // Generate the star system
            //starSystemManager.starSystemSeed = saveData.systemSeed;

            // Set the camera position
            Debug.Log("FOUR");
            //Camera.main.transform.position = saveData.cameraPosition;
        }
            Debug.Log("FIVE");


    }



    private void Options()
    {
        // Implement option functionality here
    }

    public void SaveGame(string saveFilePath = "")
    {
        // Determine the current scene index (0 for universe, 1 for star system)
        int playerScenePosition = SceneManager.GetActiveScene().buildIndex;

        // Save data based on the current scene
        if (playerScenePosition == 1) // Universe view
        {
            WorldGenerator worldGenerator = FindObjectOfType<WorldGenerator>();
            int seed = worldGenerator.Seed;
            if (string.IsNullOrEmpty(saveFilePath))
            {
                string savesFolderPath = Application.persistentDataPath + "/saves";
                if (!Directory.Exists(savesFolderPath))
                {
                    Directory.CreateDirectory(savesFolderPath);
                }
                int saveNumber = Directory.GetFiles(savesFolderPath, "*.json").Length + 1;

                string dateTimeString = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                saveFilePath = $"{savesFolderPath}/save_{saveNumber:000}_seed_{seed}_{dateTimeString}.json";
            }
            
            Vector3 cameraPosition = Camera.main.transform.position;
            PlayerController playerController = FindObjectOfType<PlayerController>();
            Vector3 playerPosition = playerController.transform.position;

            SaveData saveData = new SaveData(playerScenePosition, playerPosition, 0, cameraPosition);
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(saveFilePath, json);
        }
        else if (playerScenePosition == 2) // Star system view
        {
            int seed = GameData.Instance.UniverseSeed;
            if (string.IsNullOrEmpty(saveFilePath))
            {
                string savesFolderPath = Application.persistentDataPath + "/saves";
                if (!Directory.Exists(savesFolderPath))
                {
                    Directory.CreateDirectory(savesFolderPath);
                }
                int saveNumber = Directory.GetFiles(savesFolderPath, "*.json").Length + 1;

                string dateTimeString = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                saveFilePath = $"{savesFolderPath}/save_{saveNumber:000}_seed_{seed}_{dateTimeString}.json";
            }
            
            Vector3 cameraPosition = Camera.main.transform.position;
            //PlayerController playerController = FindObjectOfType<PlayerController>();
            //Vector3 playerPosition = playerController.transform.position;

            StarSystemManager systemManager = FindObjectOfType<StarSystemManager>();
            int starSystemSeed = systemManager.starSystemSeed;

            SaveData saveData = new SaveData(playerScenePosition, GameData.Instance.UniversePlayerPosition, starSystemSeed, cameraPosition);
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(saveFilePath, json);
        }

        Debug.Log("Game saved to: " + saveFilePath);
        RefreshSavesList(PanelMode.Save);
    }

    private void RefreshSavesList(PanelMode mode)
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

            // Left-click to overwrite the save file
            if (mode == PanelMode.Save)
            {
                saveItemButton.onClick.AddListener(() => SaveGame(saveFile));
            }
            else if (mode == PanelMode.Load)
            {
                saveItemButton.onClick.AddListener(() => LoadGame(saveFile));
            }

            // Right-click to delete the save file
            EventTrigger eventTrigger = saveItemButton.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) => {
                PointerEventData pointerEventData = (PointerEventData)eventData;
                if (pointerEventData.button == PointerEventData.InputButton.Right)
                {
                    DeleteSave(saveFile);
                }
            });
            eventTrigger.triggers.Add(entry);
        }
    }

    private void DeleteSave(string saveFilePath)
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Deleted save file: " + saveFilePath);
            RefreshSavesList(PanelMode.Save);
        }
    }

    private void ExitGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu"); // Replace "MainMenu" with the name of your main menu scene
    }
}
