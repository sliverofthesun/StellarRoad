using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameData.Instance.PlayerScenePosition = 3;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    private void ReturnToStarSystemView()
    {
        StarSystemPlayerController playerController = FindObjectOfType<StarSystemPlayerController>();

        Debug.Log("Called return to star system.");
        // if (GameData.Instance != null && playerController != null)
        // {
        //     GameData.Instance.PlayerPosition = playerController.transform.position;
        // }

        SceneManager.LoadScene("StarSystemScene");
        //Debug.Log("Reached start corutine call");
        //StartCoroutine(SetPlayerPositionAfterSceneLoad());
    }

    private IEnumerator SetPlayerPositionAfterSceneLoad()
    {
        Debug.Log("In corutine, before wait for seconds.");

        yield return new WaitForSeconds(0.1f);
        Debug.Log("Passed wait for seconds.");

        StarSystemPlayerController playerController = FindObjectOfType<StarSystemPlayerController>();
        Debug.Log("Reached the if statement. Player controller: " + playerController);

        if (GameData.Instance != null)
        {
            Debug.Log("Trying to call move player to gamedata planet.");
            playerController.MovePlayerToGameDataPlanet();
        }
    }

    private void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ReturnToStarSystemView();
        }

        // if (Input.GetMouseButtonDown(0))
        // {
        //     CheckForObjectClick();
        // }

        // if (Input.GetKeyDown(KeyCode.Space) && selectedPlanet != null)
        // {
        //     CalculateOptimalHours();
        //     targetPosition = GetFuturePlanetPosition(optimalHours);
        //     StartMovingToTargetPosition();
        // }

        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            //timeScaleFactor = timeScaleFactor * 2f;
        }

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            //timeScaleFactor = timeScaleFactor * 0.5f;
        }

        //Add this block inside the ProcessInput() method
        if (Input.GetKeyDown(KeyCode.E))
        {
            //EnterPlanet();
        }
    }
}
