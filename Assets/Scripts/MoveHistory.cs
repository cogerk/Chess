using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using TMPro;


/// <summary>
/// The scrollview containing the move history of the game so far
/// </summary>
public class MoveHistory : MonoBehaviour
{
    /// <summary>
    /// A game object describing a move histroy label, a new label is generated per turn and it is populated with the move notation text from that turn
    /// </summary>
    public GameObject movehistorylabel;

    /// <summary>
    /// The current movehistoryLabel is stored here.
    /// </summary>
    private GameObject obj=null;

    /// <summary>
    /// In most of the code, the turn # is incremented between each player, but in move notation, both players move before turn number is incremented
    /// So this is where I store the calculation to bring the rest of the code's turn count to the turn count used in move notation.
    /// </summary>
    int trueMoveNo;

    /// <summary>
    /// Using a row from a MoveHistory table, add the move notation to the current turn's move history label object .
    /// </summary>
    /// <param name="move">Row in a move history table</param>
    public void UpdateMoveHistory(DataRow move)
    {
        
        trueMoveNo = Mathf.RoundToInt(((float)((int)move["turn"]) + 0.1f) / 2);
        if ((int)move["turn"] % 2 == 1)
        {
            
            obj = Instantiate(movehistorylabel, new Vector3(0, 0, -1), Quaternion.identity);
            obj.transform.SetParent(GameObject.FindGameObjectWithTag("MoveHistory").transform, false);

            obj.GetComponent<TMP_Text>().text = trueMoveNo + "." + (string)move["algebraicNotation"];

        } else
        {
            obj.GetComponent<TMP_Text>().text = obj.GetComponent<TMP_Text>().text +" "+ (string)move["algebraicNotation"];
        }
            
        
    }

    /// <summary>
    /// Add a '+' to the current move history label to denote check
    /// </summary>
    public void Check()
    {
        obj.GetComponent<TMP_Text>().text = obj.GetComponent<TMP_Text>().text + "+";
    }

    /// <summary>
    /// Add a '#' to the current move notation to denote checkmate
    /// </summary>
    public void CheckMate()
    {
        obj.GetComponent<TMP_Text>().text = obj.GetComponent<TMP_Text>().text + "#";
    }
}
