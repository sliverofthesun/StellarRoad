using UnityEngine;

// The SaveData class is a serializable class that represents the data needed to save the game state.
[System.Serializable]
public class SaveData
{
    // int used to decide which scene to load when loading the game (1 for universe, 2 for star system)
    public int playerScenePosition;

    public Vector3 cameraPosition;

    // Universe save data
    public int universeSeed;
    public Vector3 playerUniversePosition;

    // System save data
    public int systemSeed;
    public Color starColor;
    public float starMass;
    public float starSize;
    public float starLuminosity;
    public int numberOfPlanets;

    public float timeInDays;

    // Constructor to create a new SaveData instance with the given seed, camera position, and player position
    public SaveData(int playerScenePosition, Vector3 playerUniversePosition, int systemSeed, Vector3 cameraPosition)
    {
        this.playerScenePosition = playerScenePosition;

        this.playerUniversePosition = playerUniversePosition;
        this.universeSeed = GameData.Instance.UniverseSeed;

        this.systemSeed = systemSeed;
        this.starColor = GameData.Instance.StarColor;
        this.starMass = GameData.Instance.StarMass;
        this.starSize = GameData.Instance.StarSize;
        this.starLuminosity = GameData.Instance.StarLuminosity;
        this.numberOfPlanets = GameData.Instance.NumberOfPlanets;

        this.timeInDays = GameData.Instance.DaysPassed;

        this.cameraPosition = cameraPosition;
    }
}