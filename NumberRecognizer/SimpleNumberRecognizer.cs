// <authors> Zach Mattson </authors>
// <date> 12/2/25 </date>

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.Json.Serialization;
using Matrix;

namespace NumberRecognizer
{
    public class SimpleNumberRecognizer
    {
        private readonly Matrix<double> _pixels = new(784, 1);

        [JsonInclude] [JsonPropertyName("Weights1")]
        private Matrix<double> _weights1 = new Matrix<double>(64, 784);

        [JsonInclude] [JsonPropertyName("Biases1")]
        private Matrix<double> _biases1 = new Matrix<double>(64, 1);
        
        [JsonInclude] [JsonPropertyName("Weights2")]
        private Matrix<double> _weights2 = new Matrix<double>(10, 64);
        
        [JsonInclude] [JsonPropertyName("Biases2")]
        private Matrix<double> _biases2 = new Matrix<double>(10, 1);

        private Matrix<double> _preActivationHiddenLayerZ1 = new Matrix<double>(64, 1);
        private readonly Matrix<double> _hiddenLayerA1 = new Matrix<double>(64, 1);
        private Matrix<double> _preActivationOutputLayerZ2 = new Matrix<double>(10, 1);
        private readonly Matrix<double> _outputLayerA2 = new Matrix<double>(10, 1);

        private List<Matrix<double>> _allMnistImages; // List of 784x1 matrices
        private List<Matrix<double>> _allMnistLabels; // List of 10x1 matrices

        private double _loss;
        private int _numOfMnistImages = 1;
        private const double LearningLevel = .01;

        private readonly string _filename =
            @"..\..\..\Data\Numbers\CurrentNumber.png";

        /// <summary>
        ///  Constructs a SimpleNumberRecognizer and initialize random weights and biases. 
        /// </summary>
        public SimpleNumberRecognizer()
        {
            // Initialize random weights and biases (W1: 784 x 64, b1: 64x1, W2: 64x10, b2: 10x1)
            InitializeWeightsAndBiases();
        }

        /// <summary>
        ///  Predicts the number from the current drawn image.
        /// </summary>
        /// <returns> The final prediction. </returns>
        public int PredictNumber()
        {
            // Get Normalized Pixel Vector
            var drawnImageMatrix = GetPixelMatrix();

            // Forward Propagate (Make guess)
            var output = ForwardPropagation(drawnImageMatrix);

            return FinalGuess(output);
        }

        /// <summary>
        ///  Trains the model given the correct answer
        /// </summary>
        /// <param name="label"> The correct answer. </param>
        /// <returns> The new guess after the model has been trained once. </returns>
        public int TrainModel(int label)
        {
            var oneHot = new Matrix<double>(10, 1);
            oneHot[label, 0] = 1;

            // Backward propagation (Train weights)
            TrainWeights(_pixels, oneHot);

            // Forward Prop again to recalculate outputs
            var output = ForwardPropagation(_pixels);

            return FinalGuess(output);
        }

        /// <summary>
        ///  Trains the model on iteration MNIST images from the MNIST dataset.
        /// </summary>
        /// <param name="iterations"> The number of images to train the model on </param>
        public void TrainModelWithMnist(int iterations)
        {
            _numOfMnistImages = iterations;

            // Read and store MNIST dataset
            ReadMnist(_numOfMnistImages);

            int numCorrect = 0;
            for (int i = 0; i < iterations; i++)
            {
                // Feed forward (Compute initial predictions)
                var output = ForwardPropagation(_allMnistImages[i]);

                // Backward propagation (Train weights)
                TrainWeights(_allMnistImages[i], _allMnistLabels[i]);

                // Print the guess every 100 times 
                // PrintMNISTGuess(i, 100, output, ref numCorrect);
            }
        }

        /// <summary>
        ///  Reads and stores iteration number of images and labels 
        /// </summary>
        /// <exception cref="Exception"> Thrown if the number of images and number of labels from the file aren't equal</exception>
        private void ReadMnist(int iterations)
        {
            using var imageReader =
                new BinaryReader(File.OpenRead(
                    @"..\..\..\Data\MNIST Dataset\train-images.idx3-ubyte"));

            using var labelReader =
                new BinaryReader(File.OpenRead(
                    @"..\..\..\Data\MNIST Dataset\train-labels.idx1-ubyte"));

            // read magic number, number of images and num of rows and columns for image and label
            ReadBigInt32(imageReader); // magic number
            var numImages = ReadBigInt32(imageReader);
            var numRows = ReadBigInt32(imageReader);
            var numCols = ReadBigInt32(imageReader);
            ReadBigInt32(labelReader); // magic number
            var numLabels = ReadBigInt32(labelReader);

            // Throw Exception if num of labels and images don't match
            if (numImages != numLabels)
                throw new Exception("Number of images and labels do not match.");

            // change to only do a limited number
            numImages = iterations;
            numLabels = iterations;

            // Read and store MNIST Images
            _allMnistImages = new List<Matrix<double>>(numImages);
            for (var i = 0; i < iterations; i++)
            {
                var pixels = imageReader.ReadBytes(numRows * numCols);
                _allMnistImages.Add(new Matrix<double>(numRows * numCols, 1)); // 784x1 matrix
                for (var j = 0; j < _allMnistImages[i].Rows; j++)
                    _allMnistImages[i][j, 0] = pixels[j] / 255.0; // normalize and save image
            }


            // Read and store MNIST Labels as one-hot vector
            // [0, 0, 0, 0, 1, 0, 0, 0, 0, 0] if the label is 5
            _allMnistLabels = new List<Matrix<double>>(numLabels);
            for (var i = 0; i < iterations; i++)
            {
                _allMnistLabels.Add(new Matrix<double>(10, 1)); // number of digits is 10
                _allMnistLabels[i][labelReader.ReadByte(), 0] = 1;
            }
        }

        /// <summary>
        ///  Get and returns the pixel matrix from the drawn image.
        /// </summary>
        /// <returns> The pixel matrix from the drawn image. </returns>
        private Matrix<double> GetPixelMatrix()
        {
            const int width = 28;
            const int height = 28;
            const double rScale = 0.299;
            const double gScale = 0.587;
            const double bScale = 0.114;

            using var image = Image.Load<Rgba32>(_filename);

            // Convert to grayscale
            var gray = new Matrix<double>(image.Height, image.Width);
            for (int row = 0; row < image.Height; row++)
            {
                for (int col = 0; col < image.Width; col++)
                {
                    var px = image[col, row]; // col = x, row = y
                    gray[row, col] = (px.R * rScale + px.G * gScale + px.B * bScale) / 255.0;
                }
            }

            // Find bounding box of the digit (non-zero pixels)
            var minX = image.Width - 1;
            var minY = image.Height - 1;
            var maxX = 0;
            var maxY = 0;

            for (int row = 0; row < gray.Rows; row++)
            {
                for (int col = 0; col < gray.Cols; col++)
                {
                    if (gray[row, col] > 0.01) // threshold for non-background
                    {
                        if (col < minX) minX = col;
                        if (row < minY) minY = row;
                        if (col > maxX) maxX = col;
                        if (row > maxY) maxY = row;
                    }
                }
            }

            var boxWidth = maxX - minX + 1;
            var boxHeight = maxY - minY + 1;

            // Resize and center digit into 28x28 frame
            var scale = Math.Min((float)width / boxWidth, (float)height / boxHeight);
            var newWidth = (int)(boxWidth * scale);
            var newHeight = (int)(boxHeight * scale);

            var canvas = new Matrix<double>(height, width);
            var offsetX = (width - newWidth) / 2;
            var offsetY = (height - newHeight) / 2;

            // Resample pixels (nearest neighbor)
            for (var y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int srcX = minX + (int)(x / scale);
                    int srcY = minY + (int)(y / scale);
                    canvas[y + offsetY, x + offsetX] = gray[srcY, srcX]; // row = y, col = x
                }
            }

            // Flatten into 784x1 matrix
            for (var row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    _pixels[row * width + col, 0] = canvas[row, col];
                }
            }

            return _pixels;
        }

        /// <summary>
        ///  Initialize random weights from [-0.01, +0.01] and all biases to 0. 
        /// </summary>
        private void InitializeWeightsAndBiases()
        {
            var random = new Random();

            // initialize the weights for the hidden layer
            for (var i = 0; i < _weights1.Rows; i++)
            for (var j = 0; j < _weights1.Cols; j++)
                _weights1[i, j] = -0.01 + 0.02 * random.NextDouble(); // range [-0.01, +0.01]

            // initialize the biases for the hidden layer
            for (var i = 0; i < _biases1.Rows; i++)
                _biases1[i, 0] = 0; // starts at 0, later updated by gradient

            // initialize the weights for the output layer
            for (var i = 0; i < _weights2.Rows; i++)
            for (var j = 0; j < _weights2.Cols; j++)
                _weights2[i, j] = -0.01 + 0.02 * random.NextDouble(); // range [-0.01, +0.01)

            // initialize the biases for the output layer
            for (var i = 0; i < _biases2.Rows; i++)
                _biases2[i, 0] = 0; // starts at 0, later updated by gradient
        }

        /// <summary>
        /// Forward propagate the given matrix through the weights using
        /// ReLU and softmax as hidden and output layer activation functions.
        /// </summary>
        /// <param name="imageMatrix"> The given matrix. </param>
        /// <returns> The final output layer. </returns>
        private Matrix<double> ForwardPropagation(Matrix<double> imageMatrix)
        {
            // Multiply image pixels by hidden layer weights then add the hidden layer biases
            _preActivationHiddenLayerZ1 = Matrix<double>.MatrixAddition(
                Matrix<double>.MatrixMultiplication(_weights1, imageMatrix), _biases1); // 64x1 matrix

            // ReLU activation 
            for (var i = 0; i < _hiddenLayerA1.Rows; i++)
                _hiddenLayerA1[i, 0] = Math.Max(0, _preActivationHiddenLayerZ1[i, 0]); // 64x1 matrix

            // Multiply image pixels by output layer weights then add the output layer biases
            _preActivationOutputLayerZ2 = Matrix<double>.MatrixAddition(
                Matrix<double>.MatrixMultiplication(_weights2, _hiddenLayerA1), _biases2); // 10x1 matrix

            // Softmax activation 
            double maxZ = _preActivationOutputLayerZ2.Max();
            double exponentiatedSum = 0;
            for (int i = 0; i < _preActivationOutputLayerZ2.Rows; i++)
                exponentiatedSum += Math.Exp(_preActivationOutputLayerZ2[i, 0] - maxZ);
            for (var i = 0; i < _preActivationOutputLayerZ2.Rows; i++)
                _outputLayerA2[i, 0] =
                    Math.Exp(_preActivationOutputLayerZ2[i, 0] - maxZ) / exponentiatedSum; // 10x1 matrix

            return _outputLayerA2;
        }

        /// <summary>
        ///  Trains the weights given an image and correct label.
        /// </summary>
        /// <param name="imageMatrix"> The image matrix. </param>
        /// <param name="labelMatrix"> The label matrix. </param>
        private void TrainWeights(Matrix<double> imageMatrix, Matrix<double> labelMatrix)
        {
            // Loss Function
            _loss = -Math.Log2(_outputLayerA2.Max());

            // Output layer error
            var d2 = CalculateOutputLayerError(labelMatrix);

            // Output layer weight and biases gradient
            (Matrix<double> owg, Matrix<double> obg) = CalculateOutputLayerGradient(d2);

            // Hidden layer error
            var d1 = CalculateHiddenLayerError(d2);

            // Hidden layer weight and biases gradient
            (Matrix<double> hwg, Matrix<double> hbg) = CalculateHiddenLayerGradient(d1, imageMatrix);

            // Update weights based on gradients
            UpdateWeights(hwg, hbg, owg, obg);
        }

        /// <summary>
        ///  Calculates and returns the error from the output layer.
        /// </summary>
        /// <param name="labelMatrix"> The one-hot label matrix. </param>
        /// <returns> A matrix representing the error of the output layer. </returns>
        private Matrix<double> CalculateOutputLayerError(Matrix<double> labelMatrix)
        {
            var d2 = new Matrix<double>(_outputLayerA2.Rows, 1); // 10x1 matrix
            for (var i = 0; i < d2.Rows; i++)
                d2[i, 0] = _outputLayerA2[i, 0] - labelMatrix[i, 0];
            return d2;
        }

        /// <summary>
        /// Calculates and returns the gradients for the output layer weights and biases.
        /// </summary>
        /// <param name="d2"> The output layer error. </param>
        /// <returns> The gradients for the output layer weights and biases. </returns>
        private (Matrix<double>, Matrix<double>) CalculateOutputLayerGradient(Matrix<double> d2)
        {
            // calculate the gradient for the weights of the output layer
            var outputWeightGradient =
                Matrix<double>.MatrixMultiplication(d2, Matrix<double>.Transpose(_hiddenLayerA1)); // 10x64 matrix

            // calculate the gradient for the biases of the output layer
            var outputBiasesGradient = d2; // 10x1 matrix

            return (outputWeightGradient, outputBiasesGradient);
        }

        /// <summary>
        ///  Calculates and returns the error from the hidden layer.
        /// </summary>
        /// <param name="d2"> The error of the output layer. </param>
        /// <returns> A matrix representing the error of the hidden layer. </returns>
        private Matrix<double> CalculateHiddenLayerError(Matrix<double> d2)
        {
            // Matrix multiplication between the transpose of the output layer weights and the output layer error 
            var d1 = Matrix<double>.MatrixMultiplication(Matrix<double>.Transpose(_weights2), d2); // 64x1 matrix 

            // ElementWiseMultiplication between d1 and the ReLU derivation of z1
            d1 = Matrix<double>.ElementWiseMultiplication(d1,
                ReLuDerivative(_preActivationHiddenLayerZ1)); // 64x1 matrix 

            return d1;
        }

        /// <summary>
        /// Calculates and returns the gradients for the hidden layer weights and biases.
        /// </summary>
        /// <param name="d1"> The hidden layer error. </param>
        /// <param name="imageMatrix"> The image matrix. </param>
        /// <returns> The gradients for the hidden layer weights and biases. </returns>
        private (Matrix<double>, Matrix<double>) CalculateHiddenLayerGradient(Matrix<double> d1,
            Matrix<double> imageMatrix)
        {
            // calculate the gradient for the weights of the hidden layer 
            var hiddenWeightGradient =
                Matrix<double>.MatrixMultiplication(d1,
                    Matrix<double>.Transpose(imageMatrix)); // 64x784 matrix

            // calculate the gradient for the biases of the hidden layer
            var hiddenBiasesGradient = d1; // 64x1 matrix 
            return (hiddenWeightGradient, hiddenBiasesGradient);
        }

        /// <summary>
        ///  Updates the hidden and output layer weights and biases based on their gradients.
        /// </summary>
        /// <param name="hWG"> Hidden layer weight gradient (∇W1) </param>
        /// <param name="hBG"> Hidden layer biases gradient (∇B1) </param>
        /// <param name="oWG"> Output layer weight gradient (∇W2) </param>
        /// <param name="oBG"> Output layer biases gradient (∇B2) </param>
        private void UpdateWeights(Matrix<double> hWG, Matrix<double> hBG, Matrix<double> oWG, Matrix<double> oBG)
        {
            // Update hidden layer weights 
            _weights1 = GradientDifference(_weights1, hWG);

            // Update hidden layer biases 
            _biases1 = GradientDifference(_biases1, hBG);

            // Update output layer weights 
            _weights2 = GradientDifference(_weights2, oWG);

            // Update output layer biases 
            _biases2 = GradientDifference(_biases2, oBG);
        }

        /// <summary>
        ///  Make the final guess
        /// </summary>
        /// <param name="matrix"> </param>
        /// <returns></returns>
        private int FinalGuess(Matrix<double> matrix)
        {
            int maxIndex = 0;
            double maxValue = double.MinValue;
            for (var i = 0; i < matrix.Rows; i++)
            {
                if (matrix[i, 0] > maxValue)
                {
                    maxValue = matrix[i, 0];
                    maxIndex = i;
                }
            }

            return maxIndex;
        }

        /// <summary>
        ///  Private helper method that takes the difference between a matrix and its gradient multiplied by a learning level.
        /// </summary>
        /// <param name="minuend"> The matrix to be subtracted from. </param>
        /// <param name="subtrahend"> The matrix to subtract from. </param>
        /// <returns> The difference between a matrix and its gradient multiplied by a learning level. </returns>
        private Matrix<double> GradientDifference(Matrix<double> minuend, Matrix<double> subtrahend)
        {
            var result = new Matrix<double>(minuend.Rows, minuend.Cols);
            for (int i = 0; i < result.Rows; i++)
            for (int j = 0; j < result.Cols; j++)
                result[i, j] = minuend[i, j] - LearningLevel * subtrahend[i, j];
            return result;
        }

        /// <summary>
        ///  Calculates and returns the derivative of the ReLU function
        /// </summary>
        /// <param name="z1"> Give matrix to be derived from. </param>
        /// <returns> Derived matrix of given matrix. </returns>
        private Matrix<double> ReLuDerivative(Matrix<double> z1)
        {
            var result = new Matrix<double>(z1.Rows, z1.Cols);
            for (int i = 0; i < z1.Rows; i++)
            for (int j = 0; j < z1.Cols; j++)
                result[i, j] = z1[i, j] > 0 ? 1.0 : 0.0;

            return result;
        }

        /// <summary>
        ///  Catches and converts big-endian -> little-endian
        /// </summary>
        /// <param name="br"> Binary reader. </param>
        /// <returns> The read int. </returns>
        private int ReadBigInt32(BinaryReader br)
        {
            byte[] bytes = br.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes); // convert big-endian -> little-endian
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        ///  Saves all the stored MNIST images as PNGs with their labels.
        /// </summary>
        private void SaveMNISTImageAsPNG()
        {
            int width = 28;
            int height = 28;

            using var image = new Image<L8>(width, height); // L8 = 8-bit grayscale

            for (var i = 0; i < _numOfMnistImages; i++)
            {
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        double value = _allMnistImages[i][y * width + x, 0];
                        byte gray = (byte)(value * 255); // convert 0–1 to 0–255
                        image[x, y] = new L8(gray);
                    }
                }

                var label = 0;
                for (var j = 0; j < _allMnistLabels[i].Rows; j++)
                    if (_allMnistLabels[i][j, 0] >= 1)
                        label = j;

                // Save as PNG
                image.Save(
                    $"Numbers\\mnist_image{i}_label_{label}.png");
            }
        }

        /// <summary>
        ///  Prints the output layer matrix to the console.
        /// </summary>
        private void PrintOutputLayer()
        {
            Console.WriteLine("Output layer: ");
            for (int i = 0; i < _outputLayerA2.Rows; i++)
            {
                for (int j = 0; j < _outputLayerA2.Cols; j++)
                    Console.Write($"{i} -- {_outputLayerA2[i, j]:F6} ");
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        /// <summary>
        ///  Prints all the weight and bias matrices to the console.
        /// </summary>
        private void PrintWeights()
        {
            Console.WriteLine("Weights for hidden layer: ");
            PrintMatrix(_weights1);

            Console.WriteLine("Biases for hidden layer: ");
            PrintMatrix(_biases1);

            Console.WriteLine("Weights for output layer: ");
            PrintMatrix(_weights2);

            Console.WriteLine("Biases for output layer: ");
            PrintMatrix(_biases2);
        }

        /// <summary>
        ///  Prints the drawn image's pixels.
        /// </summary>
        private void PrintPixels(Matrix<double> matrix)
        {
            // print the length of the pixel vector
            Console.WriteLine("Length: " + matrix.Rows);
            Console.WriteLine();

            for (int i = 0; i < matrix.Rows; i += 28)
            {
                for (int j = i; j < i + 28; j++)
                {
                    var pixel = matrix[j, 0];
                    Console.BackgroundColor = pixel > 0 ? ConsoleColor.Gray : ConsoleColor.Black;
                    Console.Write($"{pixel:F2} ");
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }

        private void PrintMatrix(Matrix<double> matrix)
        {
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Cols; j++)
                    Console.Write($"{matrix[i, j]:F3} ");
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        private void PrintMnistGuess(int i, int iterations, Matrix<double> output, ref int numCorrect)
        {
            var label = FinalGuess(_allMnistLabels[i]);
            var guess = FinalGuess(output);
            if (guess == label && i % 100 != 0)
                numCorrect++;

            // only print every 100 guesses
            if (i % iterations != 0) return;

            Console.Write($"Image: {i} Correct answer: {label} Guess: {guess} Loss: {_loss:F4} ");
            if (guess == label)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" CORRECT!");
                numCorrect++;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" INCORRECT!");
            }

            Console.ResetColor();
            Console.WriteLine();
        }
    }
}