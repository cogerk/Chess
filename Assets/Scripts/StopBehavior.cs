using UnityEngine;

/// <summary>
/// How autoplay stops
/// </summary>
public class StopBehavior : MonoBehaviour
{
    public string moves;
    public void OnButtonPress()
    {
        GameObject controller = GameObject.FindGameObjectWithTag("GameController");
        controller.GetComponent<Game>().stopAutoPlay = true;
        

        }
    }
