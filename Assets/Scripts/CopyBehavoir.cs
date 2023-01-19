using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/// <summary>
/// The copy move history button's behavior
/// </summary>
public class CopyBehavoir : MonoBehaviour
{
    /// <summary>
    /// When the button is hit, take the entire move history, convert it to a string, and put it in your clipboard
    /// </summary>
    public void OnButtonPress()
    {
        Game sc = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        string clipboard = sc.GetMoveHistoryAsString();
        GUIUtility.systemCopyBuffer = clipboard;
        sc.flash = true;
        sc.UpdateStatus("Move History Copied to Clipboard");
    }



}