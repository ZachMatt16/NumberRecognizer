using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace NumberRecognizer
{
    internal class NumberRecognizer
    {
        public NumberRecognizer()
        {
        }

        public void GetPixelMatrix(string filename)
        {
            try
            {
                using Image<Rgba32> image = Image.Load<Rgba32>(filename);
                // capture image dimensions
                int width = image.Width;
                int height = image.Height;

                // create a pixel span to hold the pixel data
                Span<Rgba32> pixelSpan = new Rgba32[width * height];
                image.CopyPixelDataTo(pixelSpan);

                // print the length of the pixel span
                Console.WriteLine(pixelSpan.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
