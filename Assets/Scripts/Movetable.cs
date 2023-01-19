using UnityEngine;
using System.Data;
using System;

/// <summary>
/// Table of potential moves, closest thing to a threat map 
/// </summary>
public class MoveTable : DataTable
{
    /// <summary>
    /// When initialized, add move characteristics to movetable as columns
    /// </summary>
    public MoveTable()
    {

        Columns.Add("turn", typeof(int));
        Columns.Add("player", typeof(string));
        Columns.Add("piece", typeof(string));
        Columns.Add("hasMoved", typeof(bool));
        Columns.Add("startRank", typeof(int));
        Columns.Add("endRank", typeof(int));
        Columns.Add("startFile", typeof(char));
        Columns.Add("endFile", typeof(char));
        Columns.Add("capture", typeof(string));
        Columns.Add("check", typeof(bool));
        Columns.Add("kingSideCastle", typeof(bool));
        Columns.Add("queenSideCastle", typeof(bool));
        Columns.Add("enPassant", typeof(bool));
        Columns.Add("algebraicNotation", typeof(string));
    }

    /// <summary>
    /// Add a king side castle move to the move table
    /// </summary>
    /// <param name="sc">Object containing the game</param>
    /// <param name="player">Name of player to add the move for</param>
    public void AddKingSideCastle(Game sc, string player)
    {
        this.Rows.Add(sc.GetTurn(), 
                      player,
                      null,
                      false,
                      null, null,
                      null, null,
                      null,
                      false,
                      true,
                      false,
                      false,
                      "0-0");
    }

    /// <summary>
    /// Add a queen side castle move to the move table
    /// </summary>
    /// <param name="sc">Object containing the game</param>
    /// <param name="player">Name of player to add the move for</param>
    public void AddQueenSideCastle(Game sc, string player)
    {
        this.Rows.Add(sc.GetTurn(),
                      player,
                      null,
                      false,
                      null, null,
                      null, null,
                      null,
                      false,
                      false,
                      true,
                      false,
                      "0-0-0");
    }


    /// <summary>
    /// Add new move option to the table, takes some parameters, infers others such as player, piece name, whether it's moved, start rank/file, and algebraic notation
    /// </summary>
    /// <param name="sc">Game we are adding the move to</param>
    /// <param name="cp">Chesspiece we are moving</param>
    /// <param name="endRank">Rank piece would move to</param>
    /// <param name="endFile">File piece would end to</param>
    /// <param name="capture"></param>
    /// <param name="check"></param>
    /// <param name="enPassant"></param>
    public void AddMoveOption(Game sc, Chesspiece cp,
    int endRank, char endFile,
    string capture = null, bool check = false,
    bool enPassant = false)
    {
        this.Rows.Add(sc.GetTurn(),
            cp.GetPlayer(),
            cp.GetPiece(),
            cp.GetHasMoved(),
            cp.GetRank(), endRank,
            cp.GetFile(), endFile,
            capture,
            check,
            false,
            false,
            enPassant,
            CreateAlgebraicNotation(cp, endRank, endFile,
                (capture != null),
                check,
                enPassant));


    }

    /// <summary>
    /// Return all moves in table that put other player in check
    /// </summary>
    /// <param name="opponents_last_move">Optionally specify the previous move since it may change board position</param>
    /// <returns></returns>
    public DataRowCollection CheckForCheck(DataRow opponents_last_move= null)
    {
        // Determine what moves in the move table will put or keep opponent in check
        DataRowCollection checkMoves = new MoveTable().Rows;
        // Start with all moves that allow capture of king

        foreach (DataRow r in this.Select("capture='king'"))
        {
            if (opponents_last_move != null)
            {


                // If opponent's last move captured a checking piece, that piece can't cause check anymore. (But this may happen all in the same frame so game might not know piece was captured)
                if ((int)r["startRank"] == (int)opponents_last_move["endRank"] && (char)r["startFile"]== (char)opponents_last_move["endFile"])
                {
                    
                    continue;
                }
                else
                {
                    checkMoves.Add(r.ItemArray);

                    r["algebraicNotation"] = r["algebraicNotation"] + "+";
                }
            }
            else {
                checkMoves.Add(r.ItemArray);
                r["algebraicNotation"] = r["algebraicNotation"] + "+";
            }
        }
        // Return collection of all moves that put opponent in check
        return checkMoves;
    }

    /// <summary>
    /// If two of the same piece can move to the same spot, disambiguate the move notation by including start file and/or rank.
    /// </summary>
    public void Disambiguate()
    {
        foreach (DataRow r in this.Rows)
        {
            DataRow[] unique_moves = this.Select("algebraicNotation = '" + (string)r["algebraicNotation"] + "'");
            if (unique_moves.Length > 1)
            {

                if ( unique_moves.Length > 2)
                {
                    Debug.Log("Warning: More than 2 identical moves named " + r["algebraicNotation"]);
                }

                DataRow move1 = unique_moves[0];
                DataRow move2 = unique_moves[1];


                if (move1["startFile"] != move2["startFile"])
                {
                    move1["algebraicNotation"] = RecreateAlgebraicNotation(move1, ((char)move1["startFile"]).ToString());
                    move2["algebraicNotation"] = RecreateAlgebraicNotation(move2, ((char)move2["startFile"]).ToString());
                } else
                if (move1["startRank"] != move2["startRank"])
                {
                    move1["algebraicNotation"] = RecreateAlgebraicNotation(move1, ((int)move1["startRank"]).ToString());
                    move2["algebraicNotation"] = RecreateAlgebraicNotation(move2, ((int)move2["startRank"]).ToString());
                } 
                else
                {
                    Debug.Log("Could Not Disambiguate move");
                }

            }
        }
    }


    /// <summary>
    /// Use the columns of a row in a move table to build algebraic move notation, this time with a 
    /// </summary>
    /// <param name="move">the row to make notation more</param>
    /// <param name="disambiguator">Extra character(s) required to make this move unique</param>
    /// <returns></returns>
    public string RecreateAlgebraicNotation(DataRow move, string disambiguator)
    {
        
        // piece ident
        string piece_ident = "";
        switch (move["piece"])
        {
            case "rook":
                piece_ident = "R";
                break;
            case "knight":
                piece_ident = "N";
                break;
            case "bishop":
                piece_ident = "B";
                break;
            case "king":
                piece_ident = "K";
                break;
            case "queen":
                piece_ident = "Q";
                break;

        }
        // if capturing
        string capture_ident = "";
        if (move["capture"] != null & move["capture"].ToString() != "")
        {
            if ((string)move["piece"] != "pawn")
                {
                    capture_ident = "x";
                }
                else
                {
                    capture_ident = move["startfile"] + "x";
                }

        }

        // other special cases
        string special_indicators = "";
        if ((bool)move["enPassant"])
        {
            special_indicators += " e.p.";
        }
        if ((bool)move["check"])
        {
            special_indicators += "+";
        }


        //To Do: pawn promotion


        return piece_ident + disambiguator + capture_ident + move["endFile"] + move["endRank"] + special_indicators;
    }

    /// <summary>
    /// Build algebraic notation from a move's characteristics
    /// </summary>
    /// <param name="cp">Chesspiece being moved as object</param>
    /// <param name="endRank">Rank of position piece is moving to</param>
    /// <param name="endFile">File of position piece is moving to</param>
    /// <param name="capture">Is this a capturing move?</param>
    /// <param name="check">Is this a checking move?</param>
    /// <param name="enPassant">Is this an en .passant?</param>
    /// <returns></returns>
    public string CreateAlgebraicNotation(Chesspiece cp, int endRank, char endFile, bool capture = false,
            bool check = false,
            bool enPassant = false)
    {

        // piece ident
        string piece_ident = "";
        switch (cp.GetPiece())
        {
            case "rook":
                piece_ident = "R";
                break;
            case "knight":
                piece_ident = "N";
                break;
            case "bishop":
                piece_ident = "B";
                break;
            case "king":
                piece_ident = "K";
                break;
            case "queen":
                piece_ident = "Q";
                break;

        }
        // if capturing
        string capture_ident = "";
        if (capture)
        {
            if (cp.GetPiece() != "pawn")
            {
                capture_ident = "x";
            }
            else
            {
                capture_ident = cp.GetFile() + "x";
            }
        }

        // other special cases
        string special_indicators = "";
        if (enPassant)
        {
            special_indicators += " e.p.";
        }
        if (check)
        {
            special_indicators += "+";
        }


        //To Do: Disambiguating moves, checkmate, pawn promotion


        return piece_ident + capture_ident +  endFile + endRank + special_indicators;
    }
}
