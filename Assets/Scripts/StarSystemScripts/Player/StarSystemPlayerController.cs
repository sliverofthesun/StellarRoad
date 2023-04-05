using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StarSystemPlayerController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ReturnToUniverseView();
        }
    }

    private void ReturnToUniverseView()
    {
        Debug.Log("Called return to unvierse view.");
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (GameData.Instance != null && playerController != null)
        {
            GameData.Instance.UniversePlayerPosition = playerController.transform.position;
        }
        // Call the method to load the UniverseViewScene here
        SceneManager.LoadScene("GameScene");
        StartCoroutine(SetPlayerPositionAfterSceneLoad());
    }

    private IEnumerator SetPlayerPositionAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.1f); // Give the scene enough time to load before setting the player's position
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (GameData.Instance != null && GameData.Instance.UniversePlayerPosition != Vector3.zero && playerController != null)
        {
            playerController.SetPlayerPosition(GameData.Instance.UniversePlayerPosition);
        }
    }
}
