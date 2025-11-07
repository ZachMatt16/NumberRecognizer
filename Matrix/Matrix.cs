/// <authors> Zach Mattson <authors/>

namespace Matrix;

/// <summary>
///  Wrapper class for matrices.
/// </summary>
public class Matrix<T> where T : IComparable<T>
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
        for (var i = 0; i < row; i++)
            _backing2DArray[i] = new T[col];
    }

    /// <summary>
    ///  Constructs a matrix from an existing 2D array.
    /// </summary>
    public Matrix(T[][] matrix)
    {
        _backing2DArray = matrix;
    }

    /// <summary>
    ///  Constructs a matrix from an existing matrix.
    /// </summary>
    public Matrix(Matrix<T> matrix)
    {
        _backing2DArray = new T[matrix.Rows][];
        for (var i = 0; i < matrix.Rows; i++)
        {
            _backing2DArray[i] = new T[matrix.Cols];
            for (var j = 0; j < matrix.Cols; j++)
                _backing2DArray[i][j] = matrix[i, j];
        }
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
        var transpose = new Matrix<T>(matrix.Cols, matrix.Rows);

        // Transpose array
        for (var i = 0; i < matrix.Rows; i++)
        for (var j = 0; j < matrix.Cols; j++)
            transpose[j, i] = matrix[i, j];

        return transpose;
    }

    /// <summary>
    ///  Calculates the max element in the matrix.
    /// </summary>
    /// <returns> The max element in the matrix. </returns>
    public T Max()
    {
        T max = _backing2DArray[0][0];
        foreach (var array in _backing2DArray)
        {
            if (array.Max().CompareTo(max) > 0)
                max = array.Max();
        }

        return max;
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

    public static Matrix<double> ElementWiseMultiplication(Matrix<double> m1, Matrix<double> m2)
    {
        // Check if dimensions match
        if (m1.Rows != m2.Rows && m1.Cols != m2.Cols)
            throw new MatrixDimensionMismatchException("Matrix size mismatch");

        var result = new Matrix<double>(m1.Rows, m1.Cols);
        for (int i = 0; i < m1.Rows; i++)
        for (int j = 0; j < m1.Cols; j++)
            result[i, j] = m1[i, j] * m2[i, j];

        return result;
    }

    public static Matrix<double> MatrixAddition(Matrix<double> m1, Matrix<double> m2)
    {
        // Check if dimensions match
        if (m1.Rows != m2.Rows && m1.Cols != m2.Cols)
            throw new MatrixDimensionMismatchException("Matrix size mismatch");

        // Matrix addition
        var result = new Matrix<double>(m1.Rows, m1.Cols);
        for (var i = 0; i < m1.Rows; i++)
        for (var j = 0; j < m1.Cols; j++)
            result[i, j] = m1[i, j] + m2[i, j];

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