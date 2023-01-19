using UnityEngine.SceneManagement;
using UnityEngine;


/// <summary>
/// Behavior of the actual board. Specified outside the game only for post-checkmate reasons
/// </summary>
public class Board : MonoBehaviour
{
    // The game controller object
    public GameObject controller;
    
    /// <summary>
    /// If currently in checkmate, clicking on the board will allow you to restart the game.
    /// </summary>
    public void OnMouseUp()
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.GetCheckmate())
        {
            SceneManager.LoadScene("Game"); //Restarts the game by loading the scene over again
        }


    }
    }
