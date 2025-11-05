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

        private readonly double[] _preActivationHiddenLayer = new double[64];
        private readonly double[] _hiddenLayer = new double[64];
        private readonly double[] _preActivationOutputLayer = new double[10];
        private readonly double[] _outputLayer = new double[10];


        private readonly string _filename;

        public SimpleNumberRecognizer(string filename)
        {
            _filename = filename;
        }

        public int PredictNumber()
        {
            // Get Normalized Pixel Vector
            GetPixelVector();

            // Initialize random weights and biases (W1: 784 x 64, b1: 64x1, W2: 64x10, b2: 10x1)
            InitializeWeightsAndBiases();

            // Feed forward / Matrix multiplication (Compute initial predictions)
            double[] output = ForwardPropagation();

            int maxIndex = 0;
            double maxValue = double.MinValue;
            for (int i = 0; i < output.Length; i++)
            {
                if (output[i] > maxValue)
                {
                    maxValue = output[i];
                    maxIndex = i;
                }
            }
            
            return maxIndex;
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
                    Math.Round((pixelSpan[i].R * rScale + pixelSpan[i].G * gScale + pixelSpan[i].B * bScale) / 255, 3);
            
            PrintPixels(normalizedGrayScalePixels);

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

            PrintWeights();
        }

        //TODO: xml
        private double[] ForwardPropagation()
        {
            // Matrix multiplication between hidden layer weights and initial pixel values and biases
            for (var i = 0; i < _weights1.Length; i++)
            for (var j = 0; j < _weights1[i].Length; j++)
                _preActivationHiddenLayer[i] = _weights1[i][j] * _pixels[i] + _biases1[i];

            // ReLU activation 
            for (var i = 0; i < _preActivationHiddenLayer.Length; i++)
                _hiddenLayer[i] = Math.Max(0, _preActivationHiddenLayer[i]);

            // Matrix multiplication between output layer weights and hidden layer output values and biases
            for (var i = 0; i < _weights2.Length; i++)
            for (var j = 0; j < _weights2[i].Length; j++)
                _preActivationOutputLayer[i] = _weights2[i][j] * _hiddenLayer[i] + _biases2[i];

            // Softmax activation 
            double exponentiatedOutputSum = 0;
            foreach (var value in _preActivationOutputLayer)
                exponentiatedOutputSum += Math.Exp(value);

            for (var i = 0; i < _preActivationOutputLayer.Length; i++)
                _outputLayer[i] = Math.Exp(_preActivationOutputLayer[i]) / exponentiatedOutputSum;

            PrintOutputLayer();
            return _outputLayer;
        }

        //TODO: xml
        private void PrintOutputLayer()
        {
            Console.WriteLine("Output layer: ");
            foreach (var value in _outputLayer)
                Console.Write(value + " ");
        }
        
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
        private void PrintPixels(double[] pixels)
        {
            // print the length of the pixel vector
            Console.WriteLine("Length: " + pixels.Length);
            Console.WriteLine();

            for (int i = 0; i < pixels.Length; i += 28)
            {
                for (int j = i; j < i + 28; j++)
                {
                    var pixel = pixels[j];
                    Console.ForegroundColor = pixel < 1.0 ? ConsoleColor.Green : ConsoleColor.Blue;
                    Console.Write($"{pixel:F2} ");
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}