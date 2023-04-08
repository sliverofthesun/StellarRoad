using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int playerScenePosition;
    public Vector3 playerPosition;
    public float daysPassed;
    public Vector3 cameraPosition;

    // Universe save data
    public int universeSeed;

    // System save data
    public int systemSeed;
    public Color starColor;
    public float starMass;
    public float starSize;
    public float starLuminosity;
    public int numberOfPlanets;
}