using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumberRecognizer
{
    public class SimpleNumberRecognizer
    {
        private double[] _pixels = new double[784];

        private readonly double[][] _weights1 = new double[64][];
        private readonly double[] _biases1 = new double[64];
        private readonly double[][] _weights2 = new double[10][];
        private readonly double[] _biases2 = new double[10];

        private readonly double[] _preActivationHiddenLayerZ1 = new double[64];
        private readonly double[] _hiddenLayerA1 = new double[64];
        private readonly double[] _preActivationOutputLayerZ2 = new double[10];
        private readonly double[] _outputLayerA1 = new double[10];

        private double[][] _allMNISTImages;
        private int[][] _allMNISTLabels;

        private const int NumOfMNISTImages = 2;

        private readonly string _filename;

        public SimpleNumberRecognizer(string filename)
        {
            _filename = filename;
        }

        public int PredictNumber()
        {
            // Read and store MNIST dataset
            ReadMNIST();
            SaveMNISTImageAsPNG();

            // Get Normalized Pixel Vector
            GetPixelVector();
            //PrintPixels();

            // Initialize random weights and biases (W1: 784 x 64, b1: 64x1, W2: 64x10, b2: 10x1)
            InitializeWeightsAndBiases();
            //PrintWeights();

            for (int i = 0; i < NumOfMNISTImages; i++)
            {
                // Feed forward / Matrix multiplication (Compute initial predictions)
                ForwardPropagation(i);
                //PrintWeights();

                // Backward propagation (Train weights)
                TrainWeights(i);
            }

            // int maxIndex = 0;
            // double maxValue = double.MinValue;
            // for (int i = 0; i < _outputLayerA1.Length; i++)
            // {
            //     if (_outputLayerA1[i] > maxValue)
            //     {
            //         maxValue = _outputLayerA1[i];
            //         maxIndex = i;
            //     }
            // }
            //
            // return maxIndex;
            return 0;
        }

        private void ReadMNIST()
        {
            using var imageReader =
                new BinaryReader(File.OpenRead(
                    "C:\\Users\\zachs\\RiderProjects\\NumberRecognizer\\MNIST Dataset\\t10k-images.idx3-ubyte"));

            using var labelReader =
                new BinaryReader(File.OpenRead(
                    "C:\\Users\\zachs\\RiderProjects\\NumberRecognizer\\MNIST Dataset\\t10k-labels.idx1-ubyte"));

            // read magic number, number of images and num of rows and columns for image and label
            int magicImages = ReadBigInt32(imageReader);
            int numImages = ReadBigInt32(imageReader);
            int numRows = ReadBigInt32(imageReader);
            int numCols = ReadBigInt32(imageReader);

            int magicLabels = ReadBigInt32(labelReader);
            int numLabels = ReadBigInt32(labelReader);

            if (numImages != numLabels)
                throw new Exception("Number of images and labels do not match.");

            // change to only do a limited number
            numImages = NumOfMNISTImages;
            numLabels = NumOfMNISTImages;

            // Read and store MNIST Images
            _allMNISTImages = new double[numImages][];
            for (int i = 0; i < numImages; i++)
            {
                byte[] pixels = imageReader.ReadBytes(numCols * numRows);
                double[] normalizedPixels = new double[numCols * numRows];
                for (int j = 0; j < normalizedPixels.Length; j++)
                    normalizedPixels[j] = pixels[j] / 255.0; // convert + normalize

                _allMNISTImages[i] = normalizedPixels; // save each image
            }

            // Read and store MNIST Labels as one-hot vector
            // [0, 0, 0, 0, 1, 0, 0, 0, 0, 0] if the label is 5
            _allMNISTLabels = new int[numLabels][];
            for (int i = 0; i < numLabels; i++)
            {
                _allMNISTLabels[i] = new int[10]; // number of digits is 10
                byte index = labelReader.ReadByte();
                for (int j = 0; j < index; j++)
                    _allMNISTLabels[i][j] = 0;
                _allMNISTLabels[i][index] = 1;
                for (int j = index + 1; j < _allMNISTLabels[i].Length; j++)
                    _allMNISTLabels[i][j] = 0;
            }
        }

        // TODO: XML
        private double[] GetPixelVector()
        {
            // image dimensions and grayscale factors
            const int width = 28;
            const int height = 28;
            const double rScale = .299;
            const double gScale = 0.587;
            const double bScale = .114;

            // create a pixel span to hold the pixel data
            using var image = Image.Load<Rgba32>(_filename);
            Span<Rgba32> pixelSpan = new Rgba32[width * height];
            image.CopyPixelDataTo(pixelSpan);

            var normalizedGrayScalePixels = new double[pixelSpan.Length];

            // normalize and grayscale the pixels
            for (var i = 0; i < pixelSpan.Length; i++)
                normalizedGrayScalePixels[i] =
                    (pixelSpan[i].R * rScale + pixelSpan[i].G * gScale + pixelSpan[i].B * bScale) / 255;

            _pixels = normalizedGrayScalePixels;

            return normalizedGrayScalePixels;
        }

        //TODO: xml
        private void InitializeWeightsAndBiases()
        {
            var random = new Random();

            // initialize the weights for the hidden layer
            for (var i = 0; i < _weights1.Length; i++)
            {
                _weights1[i] = new double[784];
                for (var j = 0; j < _weights1[i].Length; j++)
                    _weights1[i][j] = -0.01 + 0.02 * random.NextDouble(); // range [-0.01, +0.01)
            }

            // initialize the biases for the hidden layer
            for (var i = 0; i < _biases1.Length; i++)
                _biases1[i] = 0; // starts at 0, later updated by gradient

            // initialize the weights for the output layer
            for (var i = 0; i < _weights2.Length; i++)
            {
                _weights2[i] = new double[64];
                for (var j = 0; j < _weights2[i].Length; j++)
                    _weights2[i][j] = -0.01 + 0.02 * random.NextDouble(); // range [-0.01, +0.01)
            }

            // initialize the biases for the output layer
            for (var i = 0; i < _biases2.Length; i++)
                _biases2[i] = 0; // starts at 0, later updated by gradient
        }

        //TODO: xml
        private double[] ForwardPropagation(int mnistIndex)
        {
            // Matrix multiplication between hidden layer weights and initial pixel values and biases
            for (var i = 0; i < _weights1.Length; i++)
            for (var j = 0; j < _weights1[i].Length; j++)
                _preActivationHiddenLayerZ1[i] = _weights1[i][j] * _allMNISTImages[mnistIndex][i] + _biases1[i];

            // ReLU activation 
            for (var i = 0; i < _preActivationHiddenLayerZ1.Length; i++)
                _hiddenLayerA1[i] = Math.Max(0, _preActivationHiddenLayerZ1[i]);

            // Matrix multiplication between output layer weights and hidden layer output values and biases
            for (var i = 0; i < _weights2.Length; i++)
            for (var j = 0; j < _weights2[i].Length; j++)
                _preActivationOutputLayerZ2[i] = _weights2[i][j] * _hiddenLayerA1[i] + _biases2[i];

            // Softmax activation 
            double exponentiatedOutputSum = 0;
            foreach (var value in _preActivationOutputLayerZ2)
                exponentiatedOutputSum += Math.Exp(value);
            for (var i = 0; i < _preActivationOutputLayerZ2.Length; i++)
                _outputLayerA1[i] = Math.Exp(_preActivationOutputLayerZ2[i]) / exponentiatedOutputSum;

            return _outputLayerA1;
        }


        private void TrainWeights(int mnistIndex)
        {
            // Loss Function
            var loss = -Math.Log2(_outputLayerA1.Max());

            // Output layer loss
            var d2 = CalculateOutputLayerError(mnistIndex);


            // Output layer weight gradient

            // Output layer biases gradient

            // Hidden layer loss

            // Hidden layer weight gradient

            // Hidden layer biases gradient
        }

        private double[] CalculateOutputLayerError(int mnistIndex)
        {
            var d2 = new double[_outputLayerA1.Length];
            for (var i = 0; i < _outputLayerA1.Length; i++)
            {
                d2[i] = _outputLayerA1[i] - _allMNISTLabels[mnistIndex][i];
                Console.WriteLine($"{_outputLayerA1[i]} - {_allMNISTLabels[mnistIndex][i]}");
            }

            Console.WriteLine();
            return d2;
        }

        //TODO: xml
        private int ReadBigInt32(BinaryReader br)
        {
            byte[] bytes = br.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes); // convert big-endian -> little-endian
            return BitConverter.ToInt32(bytes, 0);
        }

        //TODO: xml
        private void SaveMNISTImageAsPNG()
        {
            int width = 28;
            int height = 28;

            using var image = new Image<L8>(width, height); // L8 = 8-bit grayscale

            for (int i = 0; i < NumOfMNISTImages; i++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double value = _allMNISTImages[i][y * width + x];
                        byte gray = (byte)(value * 255); // convert 0–1 to 0–255
                        image[x, y] = new L8(gray);
                    }
                }

                int label = 0;
                for (int j = 0; j < _allMNISTLabels[i].Length; j++)
                    if (_allMNISTLabels[i][j] == 1)
                        label = j;


                // Save as PNG
                image.Save(
                    $"C:\\Users\\zachs\\RiderProjects\\NumberRecognizer\\Numbers\\mnist_image{i}_label_{label}.png");
            }
        }

        //TODO: xml
        private void PrintOutputLayer()
        {
            Console.WriteLine("Output layer: ");
            foreach (var value in _outputLayerA1)
                Console.Write(value + " ");
        }

        //TODO: xml
        private void PrintWeights()
        {
            Console.WriteLine("Weights for hidden layer: ");
            foreach (var row in _weights1)
            {
                foreach (var weight in row)
                    Console.Write($"{weight:F3} ");
                Console.WriteLine();
            }

            Console.WriteLine();

            Console.WriteLine("Biases for hidden layer: ");
            foreach (var bias in _biases1)
                Console.Write($"{bias:F3} ");

            Console.WriteLine();

            Console.WriteLine("Weights for output layer: ");
            foreach (var row in _weights2)
            {
                foreach (var weight in row)
                    Console.Write($"{weight:F3} ");
                Console.WriteLine();
            }

            Console.WriteLine();

            Console.WriteLine("Biases for output layer: ");
            foreach (var bias in _biases2)
                Console.Write($"{bias:F3} ");

            Console.WriteLine();
        }

        // TODO: XML
        private void PrintPixels()
        {
            // print the length of the pixel vector
            Console.WriteLine("Length: " + _pixels.Length);
            Console.WriteLine();

            for (int i = 0; i < _pixels.Length; i += 28)
            {
                for (int j = i; j < i + 28; j++)
                {
                    var pixel = _pixels[j];
                    Console.BackgroundColor = pixel < 1.0 ? ConsoleColor.Gray : ConsoleColor.Black;
                    Console.Write($"{pixel:F2} ");
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}