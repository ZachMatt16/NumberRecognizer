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
        public SimpleNumberRecognizer()
        {
        }

        public int PredictNumber()
        {
            return 0;
        }

        public void GetPixelVector(string filename)
        {
            using var image = Image.Load<Rgba32>(filename);

            // capture image dimensions
            var width = image.Width;
            var height = image.Height;

            // create a pixel span to hold the pixel data
            Span<Rgba32> pixelSpan = new Rgba32[width * height];
            image.CopyPixelDataTo(pixelSpan);

            // print the length of the pixel span
            Console.WriteLine("Length: " + pixelSpan.Length);

            const double rScale = .299;
            const double gScale = 0.587;
            const double bScale = .114;
            var normalizedGrayScalePixels = new double[pixelSpan.Length];

            // normalize and grayscale the pixels
            for (var i = 0; i < pixelSpan.Length; i++)
                normalizedGrayScalePixels[i] =
                    (pixelSpan[i].R * rScale + pixelSpan[i].G * gScale + pixelSpan[i].B * bScale) / 255;

            Console.WriteLine();

            // print the values
            for (int i = 0; i < normalizedGrayScalePixels.Length; i++)
            {
                for (int j = i; j < i + 28; j++)
                {
                    Console.Write(normalizedGrayScalePixels[j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}