using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System.IO;

/// <summary>
/// Object controlling how autoplaying a given pgn file works
/// </summary>
public class AutoPlayBehavior : MonoBehaviour
{
    /// <summary>
    /// The path of the pgn file
    /// </summary>
    string path;

    /// <summary>
    /// Time between each move
    /// </summary>
    public float waitTime;

    /// <summary>
    /// Variable to store the coroutine with
    /// </summary>
    private Coroutine autoMoveCoroutine;

    /// <summary>
    /// Take the path given in the textbook, get the game's moves from it, and start the autoplay coroutine.
    /// </summary>
    public void LoadInPGN()
    {
        
        GameObject paste_field = GameObject.FindGameObjectWithTag("Path");
        path = paste_field.GetComponent<TMP_InputField>().text;
        bool isPath = path.IndexOfAny(Path.GetInvalidPathChars()) == -1;
        string fileContents;

        /// Error handing incase invalid file pandling is given
        try
        {
            fileContents = File.ReadAllText(@path);
        }
        catch (FileNotFoundException e)
        {
            Debug.Log("Invalid Path.. handling todo " + e);
        }
        
        // Read and parse all text
        fileContents = File.ReadAllText(@path);
        string moves = "1." + fileContents.Split("\n1.")[1];
        moves = moves.Split("#")[0]; //Two lines between meta data and moves & game ends at checkmate
        moves = moves.Replace("\r\n", " "); //Get rid of all new lines
        //moves = regexCheckmate.Replace(moves, "#");
        Regex regexBrackets = new Regex(@"{(.*)}");
        
        // remove all annotations
        MatchCollection annotations = regexBrackets.Matches(moves);//Find all {} annotations

        foreach (Match a in annotations)// Loop through and delete em
        {
            moves = moves.Replace(a.Value, "");
        }

        // Start the AutoMove coroutine
        autoMoveCoroutine = StartCoroutine(AutoMove(moves, waitTime));
    }
    public void StopAutoPlay()
    {
        string invalidMove ="";
        Game sc = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        sc.flash = true;
        if (sc.GetInvalidMove() != "")
        {
            invalidMove = " " + sc.GetInvalidMove();
        }
        sc.UpdateStatus("Stopping Auto-Play" + invalidMove);
        StopCoroutine(autoMoveCoroutine);
    }


    /// <summary>
    /// Coroutine that goes that makes each move in a long string of moves
    /// </summary>
    /// <param name="moves">The long string of moves</param>
    /// <param name="waittime">How long to wait between moves (you gotta pass it to coroutines even though its a global variable, idk why</param>
    /// <returns></returns>
    IEnumerator AutoMove(string moves, float waittime)
        {
        Game sc = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        Regex regexMoveNo = new Regex(@"[1-9]\d{0,2}\.");// Break into array of strings
        moves = Regex.Replace(moves, @" \{[^\}]*\}", "");
        foreach (string s in regexMoveNo.Split(moves))
            {
            
                
                if (sc.stopAutoPlay) { yield return null; }
                string whiteMove;
                string blackMove = null;
                if (s == "") { continue; }
                string turnMoves = s.Trim().Replace("\n", " ");


            if (!turnMoves.Contains(" "))
                {

                if (turnMoves.Contains("+"))
                {
                    whiteMove = turnMoves;
                }
                else
                {
                    

                    Regex guessWhiteTurnEnd = new Regex(@"[1-9]");
                    Match splitGuess = guessWhiteTurnEnd.Match(s);

                    Debug.Log(splitGuess.Index);
                    whiteMove = turnMoves.Substring(0, splitGuess.Index + 1);
                    blackMove = turnMoves.Substring(splitGuess.Index + 1, turnMoves.Length - (splitGuess.Index + 1));

                }

                }
                else
                {
                    whiteMove = turnMoves.Split(" ")[0];
                    blackMove = turnMoves.Split(" ")[1];
                }

                //Debug.Log(whiteMove);

                if (sc.stopAutoPlay) { Debug.Log("Trying to stop...");  yield return null; }
                if (sc.GetCheckmate()) { yield return null; }
                sc.MoveUsingNotation(whiteMove);
                if (sc.stopAutoPlay) { yield return null; }
                yield return new WaitForSeconds(waittime);
                if (blackMove != null & blackMove != " " & blackMove != "" & !sc.GetCheckmate())
                {
                        sc.MoveUsingNotation(blackMove);
                        
                        yield return new WaitForSeconds(waittime);
                    }
                }
            }


   
    }

