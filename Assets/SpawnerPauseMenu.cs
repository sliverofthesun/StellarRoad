using UnityEngine;

public class SpawnerPauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPrefab;

    private void Start()
    {
        if (FindObjectOfType<PauseMenuController>() == null)
        {
            Instantiate(pauseMenuPrefab);
        }
    }
}