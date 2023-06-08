using UnityEngine;
using PlanetDataNamespace;
using UnityEngine.SceneManagement;

// In GameData.cs
[System.Serializable]
public class GameData : MonoBehaviour
{
    [SerializeField]
    public static GameData Instance;

    [SerializeField]
    private bool _gameLoaded;
    public bool GameLoaded { get => _gameLoaded; set => _gameLoaded = value; }

    [SerializeField]
    private StarSystem _currentStarSystem;
    public StarSystem CurrentStarSystem { get => _currentStarSystem; set => _currentStarSystem = value; }

    [SerializeField]
    private Planet _currentPlanet;
    public Planet CurrentPlanet { get => _currentPlanet; set => _currentPlanet = value; }

    [SerializeField]
    private StarSystemController _currentStarSystemController;
    public StarSystemController CurrentStarSystemController { get => _currentStarSystemController; set => _currentStarSystemController = value; }

    [SerializeField]
    private Vector3 _playerPositionInUniverse;
    public Vector3 PlayerPositionInUniverse { get => _playerPositionInUniverse; set => _playerPositionInUniverse = value; }
    
    [SerializeField]
    private Vector3 _playerPositionInStarSystem;
    public Vector3 PlayerPositionInStarSystem { get => _playerPositionInStarSystem; set => _playerPositionInStarSystem = value; }

    [SerializeField]
    private Vector3 _cameraPosition;
    public Vector3 CameraPosition { get => _cameraPosition; set => _cameraPosition = value; }

    [SerializeField]
    private int _universeSeed;
    public int UniverseSeed { get => _universeSeed; set => _universeSeed = value; }

    [SerializeField]
    private int _systemSeed;
    public int SystemSeed { get => _systemSeed; set => _systemSeed = value; }

    [SerializeField]
    private Planet _planet;
    public Planet Planet { get => _planet; set => _planet = value; }

    [SerializeField]
    private Color _starColor;
    public Color StarColor { get => _starColor; set => _starColor = value; }

    [SerializeField]
    private int _playerScenePosition;
    public int PlayerScenePosition { get => _playerScenePosition; set => _playerScenePosition = value; }

    [SerializeField]
    private float _starMass;
    public float StarMass { get => _starMass; set => _starMass = value; }

    [SerializeField]
    private float _starSize;
    public float StarSize { get => _starSize; set => _starSize = value; }

    [SerializeField]
    private float _starLuminosity;
    public float StarLuminosity { get => _starLuminosity; set => _starLuminosity = value; }

    [SerializeField]
    private int _numberOfPlanets;
    public int NumberOfPlanets { get => _numberOfPlanets; set => _numberOfPlanets = value; }

    [SerializeField]
    private float _daysPassed;
    public float DaysPassed { get => _daysPassed; set => _daysPassed = value; }

    [SerializeField]
    private float _lightYearsPerUnit;
    public float LightYearsPerUnit { get => _lightYearsPerUnit; set => _lightYearsPerUnit = value; }

    [SerializeField]
    private float _speedInLightYearsPerDay;
    public float SpeedInLightYearsPerDay { get => _speedInLightYearsPerDay; set => _speedInLightYearsPerDay = value; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Add a new method to create a SaveData instance from the current GameData instance
    public SaveData ToSaveData()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        SaveData saveData = new SaveData
        {
            playerScenePosition = currentSceneIndex,
            playerPositionInUniverse = PlayerPositionInUniverse,
            playerPositionInStarSystem = PlayerPositionInStarSystem,
            //planet = Planet,
            currentPlanet = CurrentPlanet,
            daysPassed = DaysPassed,
            cameraPosition = CameraPosition,
            universeSeed = UniverseSeed,
            systemSeed = SystemSeed,
            numberOfPlanets = NumberOfPlanets,
            starMass = StarMass,
            starLuminosity = StarLuminosity,
            starSize = StarSize,
            starColor = StarColor,
        };

        return saveData;
    }

    // Update FromSaveData() method
    public void FromSaveData(SaveData saveData)
    {
        PlayerScenePosition = saveData.playerScenePosition;
        PlayerPositionInUniverse = saveData.playerPositionInUniverse;
        PlayerPositionInStarSystem = saveData.playerPositionInStarSystem;
        //Planet = saveData.planet;
        CurrentPlanet = saveData.currentPlanet;
        DaysPassed = saveData.daysPassed;
        CameraPosition = saveData.cameraPosition;
        UniverseSeed = saveData.universeSeed;
        SystemSeed = saveData.systemSeed;
        StarColor = saveData.starColor;
        DaysPassed = saveData.daysPassed;
        NumberOfPlanets = saveData.numberOfPlanets;
        StarMass = saveData.starMass;
        StarLuminosity = saveData.starLuminosity;
        StarSize = saveData.starSize;
        StarColor = saveData.starColor;
    }
}