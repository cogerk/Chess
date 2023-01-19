using UnityEngine;
using System.Data;

/// <summary>
/// A chesspiece that has been spawned on the board
/// </summary>
public class Chesspiece : Boardpiece
{
    /// <summary>
    /// The Game's empty controller object
    /// </summary>
    public GameObject controller;
    /// <summary>
    /// The moveplate object to instatiate that will indicate possible moves for player
    /// </summary>
    public GameObject moveplate;

    //References for all chesspiece sprites
    /// <summary>
    /// The sprites used to represent various pieces
    /// </summary>
    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    /// <summary>
    /// The sprites used to represent various pieces
    /// </summary>
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    /// <summary>
    /// The player who owns the piece
    /// </summary>
    private string player;

    /// <summary>
    /// Has this piece moved before? No at instantiation
    /// </summary>
    private bool has_moved = false;

    /// <summary>
    /// Has this piece been captured?
    /// </summary>
    private bool captured = false;

    /// <summary>
    /// What kind of piece is this? (i.e. "rook" or "pawn")
    /// </summary>
    private string piece;



    /// <summary>
    /// This should be executed after piece is spawned to place on board and populate key variables like name and player
    /// </summary>
    public void Activate()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");

        player = this.name.Split("_")[0];
        piece = this.name.Split("_")[1];

        //take the instatiated location and adjust the transform
        SetCoords();

        switch (this.name)
        {
            case "black_queen": this.GetComponent<SpriteRenderer>().sprite = black_queen; break;
            case "black_king": this.GetComponent<SpriteRenderer>().sprite = black_king; break;
            case "black_bishop": this.GetComponent<SpriteRenderer>().sprite = black_bishop; break;
            case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; break;
            case "black_rook": this.GetComponent<SpriteRenderer>().sprite = black_rook; break;
            case "black_pawn": this.GetComponent<SpriteRenderer>().sprite = black_pawn; break;
            case "white_queen": this.GetComponent<SpriteRenderer>().sprite = white_queen; break;
            case "white_king": this.GetComponent<SpriteRenderer>().sprite = white_king; break;
            case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; break;
            case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; break;
            case "white_rook": this.GetComponent<SpriteRenderer>().sprite = white_rook; break;
            case "white_pawn": this.GetComponent<SpriteRenderer>().sprite = white_pawn; break;

        }
    }

    /// <summary>
    /// When piece is clicked, generate move plates showing potential moves
    /// </summary>
    public void OnMouseUp()
    {
        Game sc = controller.GetComponent<Game>();

        DestroyMovePlates();

        // Special Castle Moveplates
        if (this.piece =="king" || this.piece == "rook")
        {
            if (sc.GetMoveOptions().Select("kingSideCastle=true").Length>0)
            {
                SpawnMovePlate(sc.GetMoveOptions().Select("kingSideCastle=true")[0]);
            }

            if (sc.GetMoveOptions().Select("queenSideCastle=true").Length > 0)
            {
                SpawnMovePlate(sc.GetMoveOptions().Select("queenSideCastle=true")[0]);
            }
        }




        DataRow[] pieceMoveOptions = sc.GetMoveOptions().Select("startRank='"+ GetRank() + "' and startFile='"+ GetFile() + "' and player='"+this.player+"'");
        foreach (DataRow m in pieceMoveOptions)
        {
            SpawnMovePlate(m);
            //string str = string.Format("Piece: {0}\t endFile: {1} \t endRank: {2} \t capture: {3} \t notation: {4}", m["piece"], m["endFile"], m["endRank"], m["capture"], m["algebraicNotation"]);
            //Debug.Log(str);
        }
    }

    /// <summary>
    /// Instantiate & activate a new moveplate 
    /// </summary>
    /// <param name="move"></param>
    public void SpawnMovePlate(DataRow move)
    {
        // create the move plate object (a moveplate is a place you can move a piece)
        GameObject obj = Instantiate(moveplate, new Vector3(0, 0, -1), Quaternion.identity);
        Moveplate mp = obj.GetComponent<Moveplate>();

        //mp.SetCapture(move["capture"]);
        mp.Activate(move, this); 

    }

    /// <summary>
    /// Find and destroy all move plates on board
    /// </summary>
    public void DestroyMovePlates()
    {
        GameObject[] mps = GameObject.FindGameObjectsWithTag("MovePlate");

        foreach (GameObject mp in mps)
        {
            Destroy(mp);
        }
    }

    /// <summary>
    /// Move piece to given rank and file
    /// </summary>
    /// <param name="moveRank"></param>
    /// <param name="moveFile"></param>
    public void MoveMe(int moveRank, char moveFile)
    {
        SetRank(moveRank);
        SetFile(moveFile);
        SetCoords();
    }

    public string GetPlayer()
    {
        return player;
    }

    public string GetPiece()
    {
        return piece;
    }
    public bool GetHasMoved()
    {
        return has_moved;
    }
    public void SetHasMoved(bool set_has_moved)
    {
        has_moved = set_has_moved;
    }

    public bool GetCaptured()
    {
        return captured;
    }
    public void SetCaptured(bool set_captured)
    {
        captured = set_captured;
    }

    /// <summary>
    /// Use piece type and board status to generate possible moves for this piece, optionally omitting a piece.
    /// </summary>
    /// <param name="omitPiece">ignore this piece when generating moves, useful when checking for check</param>
    /// <returns></returns>
    public MoveTable PossibleMoves(GameObject omitPiece=null)
    {

        Game sc = controller.GetComponent<Game>();
        if (this == omitPiece)
        {
            return new MoveTable();
        }
        switch (this.piece)
        {
            case "rook":
                {
                    MoveTable moves = new MoveTable();
                    moves.Merge(MoveStraight(1, 0));
                    moves.Merge(MoveStraight(-1, 0));
                    moves.Merge(MoveStraight(0, 1));
                    moves.Merge(MoveStraight(0, -1));
                    return moves;
                }
            case "bishop":
                {
                    MoveTable moves = MoveStraight(1, 1);
                    moves.Merge(MoveStraight(1, -1));
                    moves.Merge(MoveStraight(-1, 1));
                    moves.Merge(MoveStraight(-1, -1));
                    return moves;
                }
            case "queen":
                {
                    MoveTable moves = MoveStraight(1, 0);
                    moves.Merge(MoveStraight(-1, 0));
                    moves.Merge(MoveStraight(0, 1));
                    moves.Merge(MoveStraight(0, -1));
                    moves.Merge(MoveStraight(1, 1));
                    moves.Merge(MoveStraight(1, -1));
                    moves.Merge(MoveStraight(-1, 1));
                    moves.Merge(MoveStraight(-1, -1));
                    return moves;
                }
            case "king":
                {
                    MoveTable moves = MovePoint(1, 1);
                    moves.Merge(MovePoint(1, 0));
                    moves.Merge(MovePoint(1, -1));
                    moves.Merge(MovePoint(0, 1));
                    moves.Merge(MovePoint(0, -1));
                    moves.Merge(MovePoint(-1, 1));
                    moves.Merge(MovePoint(-1, 0));
                    moves.Merge(MovePoint(-1, -1));
                    return moves;
                }
            case "knight":
                {
                    MoveTable moves = MovePoint(1, 2);
                    moves.Merge(MovePoint(1, -2));
                    moves.Merge(MovePoint(-1, 2));
                    moves.Merge(MovePoint(-1, -2));
                    moves.Merge(MovePoint(2, 1));
                    moves.Merge(MovePoint(2, -1));
                    moves.Merge(MovePoint(-2, 1));
                    moves.Merge(MovePoint(-2, -1));
                    return moves;
                }
            case "pawn":
                {
                    MoveTable moves = MovePawn();
                    return moves;
                }


        }

        return new MoveTable();
    }

    /// <summary>
    /// Generate moves in a straight line until you hit another piece, then determine if that piece is capturable
    /// </summary>
    /// <param name="rankIncrement">Amount to increase rank by each move</param>
    /// <param name="fileIncrement">Amount to increase file by each move</param>
    /// <returns></returns>
    public MoveTable MoveStraight(int rankIncrement, int fileIncrement)
    {
        MoveTable straightMoves = new MoveTable();
        Game sc = controller.GetComponent<Game>();

        int moveRank = GetRank() + rankIncrement;
        int moveFileInt = GetFileAsInt() +fileIncrement;
        char moveFile = IntToFile(moveFileInt);

        
        while (sc.PositionOnBoard(moveRank, moveFile) && sc.GetLocationStatus(moveRank, moveFile) == null)
        {
            straightMoves.AddMoveOption(sc, this, moveRank, moveFile);
            moveRank += rankIncrement;
            moveFileInt += fileIncrement;
            moveFile = IntToFile(moveFileInt);
        }
        if (sc.PositionOnBoard(moveRank, moveFile) && (sc.GetLocationStatus(moveRank, moveFile).GetComponent<Chesspiece>().player != player | !sc.GetLocationStatus(moveRank, moveFile).activeSelf))
        {

            straightMoves.AddMoveOption(sc, this, moveRank, moveFile, sc.GetLocationStatus(moveRank, moveFile).GetComponent<Chesspiece>().GetPiece()); ;
        }
        return straightMoves;
    }

    /// <summary>
    /// Look at on board at point relative to this piece and see if it's a valid move
    /// </summary>
    /// <param name="rankIncrement"># of ranks away from this piece</param>
    /// <param name="fileIncrement"># of files away from this piece</param>
    /// <returns></returns>
    public MoveTable MovePoint(int rankIncrement, int fileIncrement)
    {
        // Create a possible move for a piece to go to a specific point relative to itself
        Game sc = controller.GetComponent<Game>(); // Game Object
        MoveTable pointMove = new MoveTable(); // Store move option in a move table
        

        // Get absolute rank and file of location to move to
        int moveRank = GetRank() + rankIncrement; 
        int moveFileInt = GetFileAsInt() + fileIncrement;
        char moveFile = IntToFile(moveFileInt);

        // If point to move to is on board 
        if (sc.PositionOnBoard(moveRank, moveFile))
        {
            if (sc.GetLocationStatus(moveRank, moveFile) == null)
            {
                pointMove.AddMoveOption(sc, this, moveRank, moveFile);
            } else if (sc.GetLocationStatus(moveRank, moveFile).GetComponent<Chesspiece>().player != player | !sc.GetLocationStatus(moveRank, moveFile).activeSelf)
            {
                pointMove.AddMoveOption(sc, this, moveRank, moveFile, sc.GetLocationStatus(moveRank, moveFile).GetComponent<Chesspiece>().GetPiece());
            }
        }
 
        return pointMove;
    }

    /// <summary>
    /// Check what pawn moves are valid
    /// </summary>
    /// <returns></returns>
    public MoveTable MovePawn()
    {
        
        Game sc = controller.GetComponent<Game>();
        MoveTable pawnMove = new MoveTable();
        //Pawns can only move one direction depending on their color
        int direction;
        if (player=="white") { direction = 1; }else { direction = -1; }

        //Move forward
        int moveRank = GetRank() + direction;
        char moveFile = GetFile();
        if (sc.PositionOnBoard(moveRank, moveFile) && (sc.GetLocationStatus(moveRank, moveFile) == null))
            pawnMove.AddMoveOption(sc, this, moveRank, moveFile);
        // If pawn hasn't moved it can move 2 spaces
            if (!this.has_moved) {
                int firstMoveRank = moveRank + direction;
               
                if (sc.GetLocationStatus(firstMoveRank, moveFile) == null) { pawnMove.AddMoveOption(sc, this, firstMoveRank, moveFile); ; }
        }

        // Can capture diagonally
        char captureFileRight = IntToFile(GetFileAsInt() +1);
        char captureFileLeft = IntToFile(GetFileAsInt() - 1);
        GameObject captureRight = sc.GetLocationStatus(moveRank, captureFileRight);
        GameObject captureLeft = sc.GetLocationStatus(moveRank, captureFileLeft);

        if (captureRight != null && captureRight.GetComponent<Chesspiece>().player != player)
        {

            pawnMove.AddMoveOption(sc, this, moveRank, captureFileRight, captureRight.GetComponent<Chesspiece>().GetPiece());
        }
        if (captureLeft != null && captureLeft.GetComponent<Chesspiece>().player != player)
        {


            pawnMove.AddMoveOption(sc, this, moveRank, captureFileLeft, captureLeft.GetComponent<Chesspiece>().GetPiece());
        }
        
        //En Passant
        //Only happens when pawn has advanced exactly three ranks
        if (this.player=="white" && GetRank()==5 || this.player == "black" && GetRank() == 4)
        {
            GameObject epCaptureRight = sc.GetLocationStatus(GetRank(), captureFileRight);
            GameObject epCaptureLeft = sc.GetLocationStatus(GetRank(), captureFileLeft);

            // Only if pawn located right next to capturing pawn
            if (epCaptureRight != null && epCaptureRight.GetComponent<Chesspiece>().GetPiece()=="pawn")
            {

                string lastMove = sc.GetMoveHistory()[sc.GetMoveHistory().Count- 1];
                // Pawn has to have moved right next to pawn last move AND has to have moved two moves
                if (lastMove.Substring(1) == GetRank().ToString() && lastMove.Substring(0,1) == captureFileRight.ToString() // Pawn just moved to this spot
                    && !sc.GetMoveHistory().Contains((GetRank()-direction).ToString()  //pawn moved two spaces
                    ))
                {
                    pawnMove.AddMoveOption(sc, this, moveRank, captureFileRight, epCaptureRight.GetComponent<Chesspiece>().GetPiece(), false, true) ;
                }
            }

            // Only if pawn located right next to capturing pawn
            if (epCaptureLeft != null && epCaptureLeft.GetComponent<Chesspiece>().GetPiece() == "pawn")
            {


                string lastMove = sc.GetMoveHistory()[sc.GetMoveHistory().Count - 1];

                // Pawn has to have moved right next to pawn last move AND has to have moved two moves
                if (lastMove.Substring(1) == GetRank().ToString() && lastMove.Substring(0,1) == captureFileLeft.ToString() && !sc.GetMoveHistory().Contains((GetRank() - direction).ToString()))
                {
                    pawnMove.AddMoveOption(sc, this, moveRank, captureFileLeft, epCaptureLeft.GetComponent<Chesspiece>().GetPiece(), false, true);
                }
            }
        }
       
        return pawnMove;
    }


}
