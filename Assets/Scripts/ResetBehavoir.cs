using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Behavior of reset button
/// </summary>
public class ResetBehavoir : MonoBehaviour
{
    /// <summary>
    /// When reset button is hit, load new Game scene.
    /// </summary>
    public void OnButtonPress()
    {
        SceneManager.LoadScene("Game");

    }


}
