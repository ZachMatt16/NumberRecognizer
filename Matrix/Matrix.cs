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
    ///  Constructs a Matrix from existing 2D array.
    /// </summary>
    public Matrix(T[][] matrix)
    {
        _backing2DArray = matrix;
    }

    /// <summary>
    ///  Public property for the number of rows of the matrix.
    /// </summary>
    public int Rows => _backing2DArray.Length;

    /// <summary>
    ///  Public property for the number of columns of the matrix.
    /// </summary>
    public int Cols => _backing2DArray[0].Length;

    /// <summary>
    ///  Indexer for the matrix.
    /// </summary>
    /// <param name="row"> Row index. </param>
    /// <param name="col"> Column index. </param>
    public T this[int row, int col]
    {
        get => _backing2DArray[row][col];
        set => _backing2DArray[row][col] = value;
    }

    /// <summary>
    ///  Takes the transpose of the given matrix and returns it.
    /// </summary>
    /// <param name="matrix"> The given matrix. </param>
    /// <returns> The transpose of the given matrix. </returns>
    public static Matrix<T> Transpose(Matrix<T> matrix)
    {
        // Create matrix
        var transpose = new Matrix<T>(matrix.Rows, matrix.Cols);

        // Transpose array
        for (var i = 0; i < matrix.Rows; i++)
        for (var j = 0; j < matrix.Cols; j++)
            transpose[j, i] = matrix[i, j];

        return transpose;
    }

    /// <summary>
    ///  Preforms matrix multiplication on the two matrices in the order given, ie: m1 x m2.
    ///  If the number of columns of m1 don't match the number of rows of m2 then a 
    ///  MatrixDimensionMismatchException is thrown. 
    /// </summary>
    /// <param name="m1"> The first matrix. </param>
    /// <param name="m2"> The second matrix. </param>
    /// <returns> The product of the two matrices. </returns>
    /// <exception cref="MatrixDimensionMismatchException"> Thrown if the number of columns of m1 don't match the number of rows of m2 </exception>
    public static Matrix<double> MatrixMultiplication(Matrix<double> m1, Matrix<double> m2)
    {
        // Check if dimensions match
        if (m1.Cols != m2.Rows)
            throw new MatrixDimensionMismatchException("Matrix size mismatch");

        // Matrix multiplication
        var result = new Matrix<double>(m1.Rows, m2.Cols);
        for (var i = 0; i < m1.Rows; i++) // iterate over m1 rows
        for (var j = 0; j < m2.Cols; j++) // iterate over m2 columns
        for (var k = 0; k < m1.Cols; k++) // iterate over shared dimension
            result[i, j] += m1[i, k] * m2[k, j];

        return result;
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