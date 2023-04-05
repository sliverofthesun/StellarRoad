using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // Add any variables you want the player to inherit between scenes
    public int exampleVariable;

    // Make this script a singleton
    public static PlayerStats Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
