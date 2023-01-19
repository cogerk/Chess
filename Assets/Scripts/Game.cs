using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Data;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Instatiated when game begins, controls turn order, board control, piece movement, and game status updates
/// </summary>
public class Game : MonoBehaviour
{
    /// <summary>
    /// A generic chesspiece object that can be generated at will
    /// </summary>
    public GameObject chesspiece;
    /// <summary>
    /// A scrollview object where previous moves are stored with algebraic notation
    /// </summary>
    public GameObject movehistory;
    /// <summary>
    /// A row in a MoveTable object that describes a move that may or may not put opposing player in check
    /// </summary>
    private DataRow theoreticalMove;
    /// <summary>
    /// A piece captured by a theoreticalMove
    /// </summary>
    private GameObject theoreticalCapture;
    /// <summary>
    /// The piece moved in a theoretical move
    /// </summary>
    private GameObject theoreticalMovedPiece;
    /// <summary>
    ///  Boolean describing whether following status update should be temporary and flashing (i.e. "INVALID MOVE") or semi-permanent and solid (i.e. "white turn")
    /// </summary>
    public bool flash;
    /// <summary>
    ///
    /// </summary>
    //public bool hasJustSwitched;
    /// <summary>
    /// When true, autoplay mode will be halted
    /// </summary>
    public bool stopAutoPlay;
    /// <summary>
    /// Table of possible moves active player can make
    /// </summary>
    private MoveTable nextPossibleMoves = new MoveTable();
    /// <summary>
    /// When invalid move is made, this string is retrieved by UpdateStatus() to let the player know what invalid move was attempted
    /// </summary>
    private string invalidMove;
    /// <summary>
    /// Array of positions on the board, mostly used as a check to confirm a given position is on the board.
    /// </summary>
    private GameObject[,] positions = new GameObject[8, 8];
    /// <summary>
    /// List of strings describing previous moves in algebraic notations, used to copy move history to clipboard.
    /// </summary>
    private List<string> moveHistoryList = new List<string>();
    /// <summary>
    /// Array of white pieces, loop through each and get possible moves to generate all possible moves. 
    /// </summary>
    private GameObject[] playerWhite;
    /// <summary>
    /// Array of black pieces, loop through each and get possible moves to generate all possible moves.
    /// </summary>
    private GameObject[] playerBlack;
    /// <summary>
    /// Who's turn is it?
    /// </summary>
    private string activePlayer;
    /// <summary>
    /// Boolean denoting whether active player is in check
    /// </summary>
    private bool check;
    /// <summary>
    /// Boolean denoting whether active player is in checkmate.
    /// </summary>
    private bool checkmate;
    /// <summary>
    /// What turn number is this? (Note one player goes per turn number so instead of 1. e4 e5, this value goes 1. e4 2. e5.  This value is converted to standard turn count notaion when updating move history.)
    /// </summary>
    private int turnCount = 0;
    /// <summary>
    /// Runs at game start or when Reset button is hit. Spawn chesspieces in a starting positions
    /// </summary>
    private void Start()
     
    {
        
        playerWhite = new GameObject[]
        {
            Create("white_rook",'a',1), Create("white_knight",'b',1), Create("white_bishop",'c',1),
            Create("white_queen",'d',1), Create("white_king",'e',1),
            Create("white_rook",'h',1), Create("white_knight",'g',1), Create("white_bishop",'f',1),
            Create("white_pawn",'a',2), Create("white_pawn",'b',2), Create("white_pawn",'c',2), Create("white_pawn",'d',2),
            Create("white_pawn",'e',2), Create("white_pawn",'f',2), Create("white_pawn",'g',2), Create("white_pawn",'h',2)
        };

        playerBlack = new GameObject[]
        {
            Create("black_rook",'a',8), Create("black_knight",'b',8), Create("black_bishop",'c',8),
            Create("black_queen",'d',8), Create("black_king",'e',8),
            Create("black_rook",'h',8), Create("black_knight",'g',8), Create("black_bishop",'f',8),
            Create("black_pawn",'a',7), Create("black_pawn",'b',7), Create("black_pawn",'c',7), Create("black_pawn",'d',7),
            Create("black_pawn",'e',7), Create("black_pawn",'f',7), Create("black_pawn",'g',7), Create("black_pawn",'h',7)
        };

        check = false;
        checkmate = false;

        StartCoroutine(StartTurn());
    }

    /// <summary>
    /// Create a new chesspiece using the public 'chesspiece' object defined above
    /// </summary>
    /// <param name="name">Name of chess piece in format "player_piece" i.e. "black_pawn" </param>
    /// <param name="file">Piece spawn location in x dimenstion described as A-H, aka the "file"</param>
    /// <param name="rank">Piece spawn location in y dimension described as 1-8, aka the "rank"</param>
    /// <returns>Chesspiece's GameObject</returns>
    public GameObject Create(string name, char file, int rank)
    {
        // Make chess pieces
        GameObject obj = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        Chesspiece cm = obj.GetComponent<Chesspiece>();
        cm.name = name;
        cm.SetRank(rank);
        cm.SetFile(file);
        cm.Activate();
        return obj;
    }

    /// <summary>
    /// Begin a new turn by incrementing turnCount, using turnCount to determine active player, GetPossibleMoves(), RemoveIllegalMoves(), CheckForCastles(), and then UpdateTurnStatus()
    /// </summary>
    /// <returns>null</returns>
    IEnumerator StartTurn()
    {

        invalidMove = "";
        turnCount += 1;
        yield return null;
        if (turnCount % 2 == 1)
        {
            activePlayer = "white";
            nextPossibleMoves = GetPossibleMoves();

        }
        else
        {
            activePlayer = "black";
            nextPossibleMoves = GetPossibleMoves();
        }
        yield return null;
        RemoveIllegalMoves();
        yield return null;
        if (nextPossibleMoves.Rows.Count == 0)
        {
            checkmate = true;
            stopAutoPlay = true;
        }
        yield return null;
        if (!check) { CheckForCastles(); }
        yield return null;

        foreach (DataRow r in nextPossibleMoves.Select("player<>'white' and player<>'black'"))
        {
            Debug.Log(r["piece"] + " has no valid player for move '" + r["algebraicNotation"] + "'");
        }
        yield return null;
        UpdateTurnStatus();
        yield return null;
    }

    /// <summary>
    /// Get all possible moves for player determined by "activePlayer" and optionally get moves omitting piece given in "omitPiece"
    /// </summary>
    /// <param name="inactivePlayer">if true, get moves for inactive player, if false get moves for active player</param>
    /// <param name="omitPiece">Chesspiece (as GameObject) to NOT get moves for</param>
    /// <returns></returns>
    public MoveTable GetPossibleMoves(bool inactivePlayer = false, GameObject omitPiece = null)
    {
        // Loop through either black or white pieces depending on active player
        GameObject[] pieces = null;
        if (!inactivePlayer)
        {

            switch (activePlayer)
            {
                case "white": { pieces = playerWhite; break; }
                case "black": { pieces = playerBlack; break; }
            }
        }
        else
        {
            switch (activePlayer)
            {
                case "white": { pieces = playerBlack; break; }
                case "black": { pieces = playerWhite; break; }
            }
        }

        MoveTable possibleMoves = new MoveTable();
        foreach (GameObject obj in pieces)
        {
            if (obj != null)
            {
                possibleMoves.Merge(obj.GetComponent<Chesspiece>().PossibleMoves(omitPiece));

            }
        }

        possibleMoves.Disambiguate();

        return possibleMoves;
    }

    /// <summary>
    /// Go through current nextPossibleMoves, "theoretically" make each move, remove if that move causes check, undo each theoretical move
    /// </summary>
    public void RemoveIllegalMoves()
    {
        // Illegal Moves put you or keep you in check
        MoveTable theoreticallyAvailableMoves;

        // For every move you could make...
        foreach (DataRow move in nextPossibleMoves.Select())
        {
            //"theoretically" make each possible move 
            //if ((bool)move["kingSideCastle"] || (bool)move["queenSideCastle"]) { continue; }
            MakeMove(move, true);
            if (theoreticalCapture != null) { theoreticalCapture.SetActive(false); Debug.Log(theoreticalCapture); }
            // Look at all your opponent's possible moves if you made that move
            theoreticallyAvailableMoves = GetPossibleMoves(true, theoreticalCapture);
            if (theoreticalCapture != null) { theoreticalCapture.SetActive(true); }
            if (theoreticallyAvailableMoves.CheckForCheck(move).Count > 0)
            {

                UndoTheoreticalMove();
                move.Delete();
            }
            else
            {
                UndoTheoreticalMove();
            }

        }

    }

    /// <summary>
    /// Check if player can castle. If King in home spot and hasn't moved and if queen's rook jasn't moved, then take nextPossibleMoves and add queen side castle. If king's rook hasn't move add king side castle.
    /// </summary>
    public void CheckForCastles()
    {
        int rank = 0;
        if (activePlayer == "white")
        {
            rank = 1;
        }
        else if (activePlayer == "black")
        {
            rank = 8;
        }

        // If king has moved can't castle
        GameObject king = GetLocationStatus(rank, 'e');
        if (king != null && king.GetComponent<Chesspiece>().GetHasMoved()) { return; }

        GameObject queensRook = GetLocationStatus(rank, 'a');
        GameObject kingsRook = GetLocationStatus(rank, 'h');



        if (queensRook != null && !queensRook.GetComponent<Chesspiece>().GetHasMoved() && GetLocationStatus(rank, 'b') == null && GetLocationStatus(rank, 'c') == null && GetLocationStatus(rank, 'd') == null)
        {
            nextPossibleMoves.AddQueenSideCastle(this.GetComponent<Game>(), activePlayer);
        }


        if (kingsRook != null && !kingsRook.GetComponent<Chesspiece>().GetHasMoved() && GetLocationStatus(rank, 'f') == null && GetLocationStatus(rank, 'g') == null)
        {
            nextPossibleMoves.AddKingSideCastle(this.GetComponent<Game>(), activePlayer);
        }


    }

    /// <summary>
    /// If there are no nextPossibleMoves, put active player in checkmate, otherwise, update bottom text to state who's turn it is
    /// </summary>
    public void UpdateTurnStatus()
    {
        if (nextPossibleMoves.Rows.Count == 0)
        {
            Checkmate(activePlayer);
            GameObject.FindGameObjectWithTag("MoveHistory").GetComponent<MoveHistory>().CheckMate();
        }
        else if (check)
        {
            flash = false;
            UpdateStatus(activePlayer + " in check");


            //Filter down moves to only those that will make you safe
        }
        else
        {

            flash = false;
            UpdateStatus(activePlayer + " turn");
        }
    }

    /// <summary>
    /// Put active player in checkmate by setting checkmate to true and updating status to inform 
    /// </summary>
    /// <param name="activePlayer"></param>
    public void Checkmate(string activePlayer)
    {
        checkmate = true;
        Debug.Log("Checkmate");
        flash = false;
        UpdateStatus("checkmate " + activePlayer);
        GameObject.FindGameObjectWithTag("TopText").GetComponent<TMP_Text>().text = "Click to Restart";
        GameObject.FindGameObjectWithTag("TopText").GetComponent<TMP_Text>().enabled = true;

    }

    /// <summary>
    /// Make a move described in the movetable
    /// </summary>
    /// <param name="move">A row from the MoveTable object</param>
    /// <param name="theoretical">Is this move "theoretical" (used to determine if player is in check?) </param>
    public void MakeMove(DataRow move, bool theoretical = false)
    {
        // Execute a move given a row in MoveTable object. This needs to be a theoretical move when we are checking for checkmate.

        GameObject move_obj;
        Chesspiece move_cp = null;
        theoreticalCapture = null;

        theoreticalMove = move;
        // Castle
        if ((bool)move["kingSideCastle"] || (bool)move["queenSideCastle"])
        {
            
            int rank = 0;

            // Get player performing move
            var myObject = move["player"] as string;
            if (myObject == null) { Debug.Log("This is about to throw an error while assigned player==" + move["player"]); }
            
            // All castling will happen at one of these ranks depending on the plauer
            if ((string)move["player"] == "white"){rank = 1;}
            else if ((string)move["player"] == "black"){rank = 8;}

            // Get king 
            GameObject king_obj = GetLocationStatus(rank, (char)'e');
            Chesspiece king = king_obj.GetComponent<Chesspiece>();
            theoreticalMovedPiece = king_obj;
            move_cp = king;

            // King side castle
            if ((bool)move["kingSideCastle"])
            {
                //Get king's rook
                GameObject rook_obj = GetLocationStatus(rank, (char)'h');
                Chesspiece rook = rook_obj.GetComponent<Chesspiece>();

                // Perform the castle, toggle "have moved" status on (assuming move isn't theoretical)
                king.MoveMe(rank, 'g');
                if (!theoretical) { king.SetHasMoved(true); } 

                rook.MoveMe(rank, 'f');
                if (!theoretical) { rook.SetHasMoved(true); }

            }

            //Queen side castle
            if ((bool)move["queenSideCastle"])
            {
                // Get queen's rook
                GameObject rook_obj = GetLocationStatus(rank, (char)'a');
                Chesspiece rook = rook_obj.GetComponent<Chesspiece>();

                // Perform castle, toggle "have moved" status on (assuming move isn't theoretical)
                king.MoveMe(rank, 'b');
                if (!theoretical) { king.SetHasMoved(true); }

                rook.MoveMe(rank, 'c');
                if (!theoretical) { rook.SetHasMoved(true); }
            }

        }
        else

        // Regular move
        {
            // Get piece we are moving
            move_obj = GetLocationStatus((int)move["startRank"], (char)move["startFile"]);
            move_cp = move_obj.GetComponent<Chesspiece>();
            
            if (!theoretical) { move_cp.SetHasMoved(true); } // If not theoretical toggle "have moved" status on (assuming move isn't theoretical)

            theoreticalMovedPiece = move_obj;

            // Delete Captured piece (if not an en passant)
            if (move["capture"] != null)
            {
                GameObject capture_obj = GetLocationStatus((int)move["endRank"], (char)move["endFile"]);
                
                // If an en passant, get the en passant captured piece and delete that
                if ((bool)move["enPassant"])
                {
                    int direction = 0;
                    if ((string)move["player"] == "white") { direction = -1; }
                    else if ((string)move["player"] == "black") { direction = 1; }
                    capture_obj = GetLocationStatus((int)move["endRank"] + direction, (char)move["endFile"]);

                }

                // If it's theoretical, don't delete the piece
                if (capture_obj != null)
                {
                    if (theoretical)
                    {
                        theoreticalCapture = capture_obj;
                    }
                    else
                    {
                        Destroy(capture_obj);
                    }
                        
                }
            }
            // Then actually move the doggone piece 
            move_cp.MoveMe((int)move["endRank"], (char)move["endFile"]);


        }
        // Then reset vars, destroy any move plates you need to and add move to move history 
        if (!theoretical)
        {
            theoreticalMove = null;
            // Change status to "hasMoved" if applicable

            move_cp.DestroyMovePlates();
            moveHistoryList.Add((string)move["algebraicNotation"]);
            GameObject.FindGameObjectWithTag("MoveHistory").GetComponent<MoveHistory>().UpdateMoveHistory(move);
            EndTurn();
        }
    }

    /// <summary>
    /// If a theoretical move was just made, undo it once we've checked for check
    /// </summary>
    public void UndoTheoreticalMove()
    {
        if ((bool)theoreticalMove["queenSideCastle"] || (bool)theoreticalMove["kingSideCastle"])
        {

            int rank = 0;
            if ((string)theoreticalMove["player"] == "white")
            {
                rank = 1;
            }
            else if ((string)theoreticalMove["player"] == "black")
            {
                rank = 8;
            }

            if ((bool)theoreticalMove["kingSideCastle"])
            {
                GetLocationStatus(rank, 'g').GetComponent<Chesspiece>().MoveMe(rank, 'e');
                GetLocationStatus(rank, 'f').GetComponent<Chesspiece>().MoveMe(rank, 'h');
            }
            else
            {
                GetLocationStatus(rank, (char)'c').GetComponent<Chesspiece>().MoveMe(rank, 'a');
                GetLocationStatus(rank, (char)'b').GetComponent<Chesspiece>().MoveMe(rank, 'e');
            }
        } else
        {
            theoreticalMovedPiece.GetComponent<Chesspiece>().MoveMe((int)theoreticalMove["startRank"], (char)theoreticalMove["startFile"]);
            theoreticalMovedPiece.GetComponent<Chesspiece>().SetHasMoved((bool)theoreticalMove["hasMoved"]);
        }

    }

    /// <summary>
    /// Once move has been made, end players turn, place player in check if necessary
    /// </summary>
    public void EndTurn()
    {
        // Find Moves That are Putting Non-Active Player in Check
        DataRowCollection checkMoves = GetPossibleMoves().CheckForCheck(null);
        if (checkMoves.Count>0) {
            PutInCheck();
            GameObject.FindGameObjectWithTag("MoveHistory").GetComponent<MoveHistory>().Check();


        } else { Safe(); }
        StartCoroutine(StartTurn());

    }

    /// <summary>
    /// Get if active player is in checkmate
    /// </summary>
    /// <returns></returns>
    public bool GetCheckmate()
    {
        return checkmate;
    }
    



    /// <summary>
    /// Get turn count
    /// </summary>
    /// <returns></returns>
    public int GetTurn()
    {
        return turnCount;
    }

    /// <summary>
    /// Get move history as a list
    /// </summary>
    /// <returns></returns>
    public List<string> GetMoveHistory()
    {
        return moveHistoryList;
    }

    /// <summary>
    /// Get next possible moves
    /// </summary>
    /// <returns></returns>
    public DataTable GetMoveOptions()
    {
        return nextPossibleMoves;
    }
    /// <summary>
    /// return any occupying pieces in board location (or null if unoccupied)
    /// </summary>
    /// <param name="rank">rank to search</param>
    /// <param name="file">file to search</param>
    /// <returns></returns>
    public GameObject GetLocationStatus(int rank, char file)
    {

        GameObject occupying_piece = null;
        foreach (GameObject obj in playerWhite)
        {
            if (obj != null)
            {
                Chesspiece piece = obj.GetComponent<Chesspiece>();
                if (piece.GetRank() == rank && piece.GetFile() == file && !piece.GetCaptured())
                {
                    occupying_piece = obj;
                    break;
                }
            }
        }

        foreach(GameObject obj in playerBlack)
        {

            if (obj != null)
            {
                Chesspiece piece = obj.GetComponent<Chesspiece>();
                if (piece.GetRank() == rank && piece.GetFile() == file)
                {
                    occupying_piece = obj;
                    break;
                }
            }
        }

        return occupying_piece;
    }

    /// <summary>
    /// Determine if rank/file is on the chess board (used extensively when generating straight lines of moves
    /// </summary>
    /// <param name="rank">rank to search</param>
    /// <param name="file">file to search</param>
    /// <returns></returns>
    public bool PositionOnBoard(int rank, char file)
    {   
        int y = rank;
        int x = char.ToUpper(file) - 64; // Convert file to integer we can place on board
        if (y < 1 || x < 1 || x > positions.GetLength(0) | y > positions.GetLength(1) ) return false; return true;

    }


    /// <summary>
    /// Set check=true
    /// </summary>
    public void PutInCheck()
    {
        check = true;
        //Check for Checkmate
    }

    /// <summary>
    /// Set check=false
    /// </summary>
    public void Safe()
    {
        check = false;
    }


    /// <summary>
    /// Update bottom text to give game status
    /// </summary>
    /// <param name="status">game status</param>
    public void UpdateStatus(string status)
    {
        //Debug.Log(status);
        // Debug.Log(GameObject.FindGameObjectWithTag("BottomText").GetComponent<TMP_Text>());
        if (flash)
        {
            
            StartCoroutine(FlashingText(status));
        } else
        {
            GameObject.FindGameObjectWithTag("BottomText").GetComponent<TMP_Text>().text = status;
        }
        

    }

    /// <summary>
    /// For urgent status messages, flash briefly and then return to previous status
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    IEnumerator FlashingText(string status = "")
    {
        int i = 0;
        string loop_status = status;
        bool replace_flashing_text=false;
        while (flash)
        {
            if (i>3) { flash = false; replace_flashing_text = true; }
            i += 1;
            GameObject.FindGameObjectWithTag("BottomText").GetComponent<TMP_Text>().text = loop_status;
            yield return new WaitForSeconds(0.3f);
            if (!flash) { break; }
            GameObject.FindGameObjectWithTag("BottomText").GetComponent<TMP_Text>().text = "";
            yield return new WaitForSeconds(0.3f);
            if (!flash) { break; }

        }
        if (replace_flashing_text)
        {
            UpdateTurnStatus();
        }


    }

    /// <summary>
    /// Get Invalid Move string
    /// </summary>
    /// <returns></returns>
    public string GetInvalidMove()
    {
        return invalidMove;


    }

    /// <summary>
    /// Instead of clicking, make a move using move notation 
    /// </summary>
    /// <param name="algebraicNotation">Move to attempt</param>
    /// <param name="autoplay"></param>
    public void MoveUsingNotation(string algebraicNotation, bool autoplay=false)
    {
        /*        Debug.Log(algebraicNotation);
                Debug.Log(algebraicNotation.Length);
                Debug.Log(nextPossibleMoves.Select("algebraicNotation='" + algebraicNotation + "'").Length>0);*/
        // Game figures out if a move is check or checkmate at time of moving
        
        
        // Check and checkmate will confuse it
        algebraicNotation=algebraicNotation.Replace("+", "");
        algebraicNotation=algebraicNotation.Replace("#", "");



        if (algebraicNotation.Contains("O-O"))
        {
            
            algebraicNotation = "0-0";
        } else if (algebraicNotation.Contains("O-O-O"))
        {
            algebraicNotation = "0-0-0";
        }
        //Debug.Log(algebraicNotation);

        if (nextPossibleMoves.Select("algebraicNotation='" + algebraicNotation + "'").Length > 0)
        {
            DataRow move = nextPossibleMoves.Select("algebraicNotation='" + algebraicNotation + "'")[0];
            MakeMove(move);

        }
        else
        {
            flash = true;
            invalidMove = algebraicNotation + " Is Invalid Move";
            UpdateStatus(invalidMove);
            Debug.Log(invalidMove);
            if (autoplay)
            {
                checkmate = true;
                Debug.Log("Autoplay failed");
                GameObject.FindGameObjectWithTag("TopText").GetComponent<TMP_Text>().text = "Click to Restart";
                GameObject.FindGameObjectWithTag("TopText").GetComponent<TMP_Text>().enabled = true;
                stopAutoPlay = true;
                

}
            


        }
    }

    /// <summary>
    /// If player hits return and submit move text box not empty, try to move using that notation
    /// </summary>
    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            string requestedMoveNotation = GameObject.FindGameObjectWithTag("SubmitMove").GetComponent<TextMeshProUGUI>().text.Replace(((char)8203).ToString(), "");
            GameObject.FindGameObjectWithTag("SubmitMove").GetComponent<TextMeshProUGUI>().SetText("");

            if (requestedMoveNotation.Length> 0)
            {
                MoveUsingNotation(requestedMoveNotation);
                
            }
        }
    }

    /// <summary>
    /// Take current move history list and turn it into a string that includes turn count
    /// </summary>
    /// <returns></returns>
    public string GetMoveHistoryAsString()
    {
        string GetMoveHistoryAsString = "";

        for (int i = 0; i < moveHistoryList.Count; i++)
        {
            if (i > 0)
            {
                GetMoveHistoryAsString += " ";
            }
            GetMoveHistoryAsString += i + 1 + "." + moveHistoryList[i];
        }

        return GetMoveHistoryAsString;
    }
}
