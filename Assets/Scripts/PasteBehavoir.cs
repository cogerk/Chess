using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/// <summary>
/// The behavior of the paste button, intended to fill the 'enter pgn file path...'
/// </summary>
public class PasteBehavoir : MonoBehaviour
{
    private string clipboard;

    /// <summary>
    /// On button press take the clipboard contents and place it in the text box
    /// </summary>
    public void OnButtonPress()
    {
        GameObject paste_field = GameObject.FindGameObjectWithTag("Path");

        clipboard = GUIUtility.systemCopyBuffer;

        paste_field.GetComponent<TMP_InputField>().text = clipboard.Replace("\"", ""); ;

    }


}