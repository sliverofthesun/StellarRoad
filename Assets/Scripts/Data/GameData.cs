using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    [SerializeField]
    private StarSystem _currentStarSystem;
    public StarSystem CurrentStarSystem { get => _currentStarSystem; set => _currentStarSystem = value; }

    [SerializeField]
    private StarSystemController _currentStarSystemController;
    public StarSystemController CurrentStarSystemController { get => _currentStarSystemController; set => _currentStarSystemController = value; }

    [SerializeField]
    private Vector3 _universePlayerPosition;
    public Vector3 UniversePlayerPosition { get => _universePlayerPosition; set => _universePlayerPosition = value; }

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
}