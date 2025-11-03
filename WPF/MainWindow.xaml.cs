using System;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Field to track if drawing is in progress
        /// </summary>
        private bool isDrawing = false;

        /// <summary>
        /// Field to track if erasing is in progress
        /// </summary>
        private bool isErasing = false;

        /// <summary>
        /// Field to store the previous point for drawing lines
        /// </summary>
        private Point previousPoint;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Checks if the mouse button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDrawing = true;
                previousPoint = e.GetPosition(Canvas);
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                isErasing = true;
                previousPoint = e.GetPosition(Canvas);
            }
        }

        /// <summary>
        /// Checks if the mouse button is released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            isErasing = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                Point currentPoint = e.GetPosition(Canvas);

                if (!Double.TryParse(Radius.Text, out double radius))
                    radius = 20;

                Ellipse circle = new Ellipse
                {
                    Fill = Brushes.Black,
                    Width = radius,
                    Height = radius
                };

                // Center the circle where the mouse is
                Canvas.SetLeft(circle, currentPoint.X - radius / 2);
                Canvas.SetTop(circle, currentPoint.Y - radius / 2);

                Canvas.Children.Add(circle);
            }

            if (isErasing)
            {
                Point currentPoint = e.GetPosition(Canvas);

                double radius = 100;
                Ellipse circle = new Ellipse
                {
                    Fill = Brushes.White,
                    Width = radius,
                    Height = radius
                };

                // Center the circle where the mouse is
                Canvas.SetLeft(circle, currentPoint.X - radius / 2);
                Canvas.SetTop(circle, currentPoint.Y - radius / 2);

                Canvas.Children.Add(circle);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string filename = "default.png";
            try
            {
                filename = "C:\\Users\\zachs\\source\\repos\\NumberRecognizer\\WPF\\Numbers\\" + Save_File.Text + ".png";
            }

            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            SaveWindowAsImage(filename);
        }

        private void SaveWindowAsImage(string filename)
        {
            // Get the size of the canvas
            double width = Canvas.ActualWidth;
            double height = Canvas.ActualHeight;

            // Create a RenderTargetBitmap to render the canvas
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)width, (int)height, 96d, 96d, PixelFormats.Default);
            rtb.Render(Canvas);

            // Encode the RenderTargetBitmap to a PNG file
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
            using (var fs = System.IO.File.OpenWrite(filename))
            {
                pngEncoder.Save(fs);
            }
        }
    }
}