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
    public Matrix(int row, int col)
    {
        // Create Matrix of size row x col
        _backing2DArray = new T[row][];
        for (int i = 0; i < row; i++)
            _backing2DArray[i] = new T[col];
    }

    /// <summary>
    ///  Constructs a Matrix from existing 2D array
    /// </summary>
    public Matrix(T[][] matrix)
    {
        _backing2DArray = matrix;
    }

    public T GetValue(int rowIndex, int colIndex)
    {
        return _backing2DArray[rowIndex][colIndex];
    }

    public static Matrix<T> Transpose(Matrix<T> matrix)
    {
        // Create new 2D array
        var transpose = new T[matrix.Cols][];
        
        // Transpose array
        for (var i = 0; i < matrix.Rows; i++)
        {
            transpose[i] = new T[matrix.Rows];
            for (var j = 0; j < matrix.Cols; j++)
                transpose[j][i] = matrix.GetValue( i, j);
        }
        return new Matrix<T>(transpose);
    }

    public static Matrix<double> MatrixMultiplication(Matrix<double> m1, Matrix<double> m2)
    {
        // Throw error if dimensions don't match
        if (m1.Cols != m2.Rows)
            throw new MatrixDimensionMismatchException("Matrix size mismatch");
        
        // Matrix multiplication
        var result = new double[m1.Rows][];
        for (int i = 0; i < m1.Rows; i++) // iterate over m1 rows
        {
            result[i] = new double[m2.Cols];
            for (int j = 0; j < m2.Cols; j++) // iterate over m2 columns
            {
                double sum = 0;
                for (int k = 0; k < m1.Cols; k++) // iterate over shared dimension
                    sum += m1.GetValue(i, k) * m2.GetValue(k, j);
                result[i][j] = sum;
            }
        }

        return new Matrix<double>(result);
    }
}

/// <summary>
///  Custom exception for matrix dimension mismatches
/// </summary>
public class MatrixDimensionMismatchException : Exception
{
    /// <summary>
    ///  Constructs a new exception
    /// </summary>
    public MatrixDimensionMismatchException()
    {
    }

    /// <summary>
    ///  Constructs a new exception with message
    /// </summary>
    /// <param name="message"> The message of the error</param>
    public MatrixDimensionMismatchException(string message)
        : base(message)
    {
    }
}