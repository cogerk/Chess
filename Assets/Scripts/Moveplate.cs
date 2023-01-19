using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;

/// <summary>
/// A plate indicating where a player could move
/// </summary>
public class Moveplate : Boardpiece
{

    /// <summary>
    /// Game Controller object
    /// </summary>
    public GameObject controller;

    /// <summary>
    /// Each move is generated using the column values of a specific MoveTable row
    /// </summary>
    private DataRow move;

    /// <summary>
    /// Is this a capture, castle, or regular ol' move?
    /// </summary>
    private string status;

    /// <summary>
    /// Will this move cause check?
    /// </summary>
    private bool check;

    /// <summary>
    /// Was this moveplate selected?
    /// </summary>
    private bool active = true;
    
    /// <summary>
    /// Once moveplate has been spawned activate it by assigning it's properties, using status to determin it's color
    /// If this is a castling move, highlight the piece the player DIDN'T select to castle with
    /// Otherwise use rank and file to set plate's coordinates
    /// </summary>
    /// <param name="set_move"></param>
    /// <param name="cp"></param>
    public void Activate(DataRow set_move, Chesspiece cp)
    {
        // Store move's metadata
        move = set_move;

        // Move plate color will be determined by the type of move being taken
        if ((bool)move["check"])
        {
            check =true;
        }


        if (!System.DBNull.Value.Equals(move["capture"]))
        {
            status = "capture";
        }
        else if ((bool)move["kingSideCastle"] || (bool)move["queenSideCastle"])
        {
            status = "castle";
        }
        else
        {
            status = "move";
        }

        controller = GameObject.FindGameObjectWithTag("GameController");
        switch (status)
        {
            case "move":
                gameObject.GetComponent<SpriteRenderer>().color = new Color(0.62f, 0.55f, 0.77f, 1.0f);
                break;
            case "capture":
                gameObject.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.4f, 0.2f, 1.0f);
                break;
            case "castle":
                gameObject.GetComponent<SpriteRenderer>().color = new Color(0.93f, 0.84f, 0.62f, 1.0f);
                break;
        }
        if ((bool)move["kingSideCastle"])
        {
            
            if (cp.GetPiece() == "king")
            {
                SetRank(cp.GetRank());
                SetFile('h');

            } else if (cp.GetPiece() == "rook")
            {
                SetRank(cp.GetRank());
                SetFile('e');
            }
        }
        else if((bool)move["queenSideCastle"])
        {

            if (cp.GetPiece() == "king")
            {
                SetRank(cp.GetRank());
                SetFile('a');

            }
            else if (cp.GetPiece() == "rook")
            {
                SetRank(cp.GetRank());
                SetFile('e');
            }
        } else
        {
            SetRank((int)move["endRank"]);
            SetFile((char)move["endFile"]);
            
        }
        SetCoords();
    }


    /// <summary>
    /// When moveplate is clicked, make that move
    /// </summary>
    public void OnMouseUp()
    {
        if (active) { 
            controller.GetComponent<Game>().MakeMove(move);
            // Destory a captured piece if applicable
        }
    }

    public string GetStatus()
    {
        return status;
    }
    public bool GetCheck()
    {
        return check;
    }
}
