using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// General class of any object on the board, could be chesspiece or moveplate
/// </summary>
public class Boardpiece : MonoBehaviour
{
    // Initally piece is off the board, 
    /// <summary>
    /// Rank of board piece, initially place off board
    /// rank = bot -> top 
    /// </summary>
    private int rank = -1;
    /// <summary>
    /// File of piece, initially place off board (not between A-H)
    /// file = left -> right (char)
    /// </summary>
    private char file = '*'; // '*' = -1, off board
    
    /// <summary>
    /// Slope to convert board position (rank and file) to euclidean position on canvas
    /// </summary>
    private float shift_mult = 1.1f;
    /// <summary>
    /// Intercept to convert board position (rank and file) to euclidean position on canvas
    /// </summary>
    private float shift_add = -3.85f;

    /// <summary>
    /// Transform piece's file and rank to euclidean position in game
    /// </summary>
    public void SetCoords()
    {
        float y = rank - 1;
        float x = GetFileAsInt() - 1;

        x *= shift_mult;
        y *= shift_mult;

        x += shift_add;
        y += shift_add;
        this.transform.position = new Vector3(x, y, -2.0f);

    }
    public void SetFile(char f)
    {
        file = f;
    }
    public void SetRank(int r)
    {
        rank = r;
    }
    public int GetRank()
    {
        return rank;
    }



    public char IntToFile(int x)
    {
        return char.ToLower((char)(x + 64));
    }
    public char GetFile()
    {
        return file;
    }

    public int GetFileAsInt()
    {

        return char.ToUpper(file) - 64;
    }

}
