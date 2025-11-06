/// <authors> Zach Mattson <authors/>
namespace Matrix;

/// <summary>
///  Wrapper class for matrices.
/// </summary>
public class Matrix<T>
{
    /// <summary>
    ///  Backing 2D array for the Matrix
    /// </summary>
    private readonly T[][] _backing2DArray;

    public int Rows => _backing2DArray.Length;
    public int Cols => _backing2DArray[0].Length;
    

    /// <summary>
    ///  Constructs a Matrix of size row x col.
    /// </summary>
    /// <param name="row"> Number of rows. </param>
    /// <param name="col"> Number of columns. </param>
    public Matrix (int row, int col)
    {
        // Create Matrix of size row x col
        _backing2DArray = new T[row][];
        for (int i = 0; i < row; i++)
            _backing2DArray[i] = new T[col];
    }

    /// <summary>
    ///  Constructs a Matrix from existing 2D array
    /// </summary>
    public Matrix(T[][] matrix){
        _backing2DArray = matrix;
    }
}