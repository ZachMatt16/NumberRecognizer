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
using NumberRecognizer;

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
        private bool _isDrawing = false;

        /// <summary>
        /// Field to track if erasing is in progress
        /// </summary>
        private bool _isErasing = false;

        /// <summary>
        /// Field to store the previous point for drawing lines
        /// </summary>
        private Point _previousPoint;

        /// <summary>
        ///  File path of the image
        /// </summary>
        private string _localFileName = "C:\\Users\\zachs\\RiderProjects\\NumberRecognizer\\Numbers\\CurrentNumber.png";
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
                _isDrawing = true;


            if (e.RightButton == MouseButtonState.Pressed)
                _isErasing = true;

            _previousPoint = e.GetPosition(Canvas);
        }

        /// <summary>
        /// Checks if the mouse button is released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            _isErasing = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            // create point and store radius value
            var currentPoint = e.GetPosition(Canvas);
            double.TryParse(Radius.Text, out var radius);

            // create circle
            var circle = new Ellipse()
            {
                Width = radius,
                Height = radius
            };

            // Center the circle where the mouse is
            Canvas.SetLeft(circle, currentPoint.X - radius / 2);
            Canvas.SetTop(circle, currentPoint.Y - radius / 2);

            // make circle black if drawing
            if (_isDrawing)
                circle.Fill = new SolidColorBrush(Colors.Black);

            // make circle black if erasing
            if (_isErasing)
            {
                circle.Fill = new SolidColorBrush(Colors.White);
                circle.Width = radius * 2;
                circle.Height = radius * 2;
            }

            // draw it on the canvas
            Canvas.Children.Add(circle);
        }

        private void Handle_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void SaveFile()
        {
            SaveWindowAsImage(_localFileName);
        }

        private void SaveWindowAsImage(string filename)
        {
            // Get the size of the canvas
            double width = Canvas.ActualWidth;
            double height = Canvas.ActualHeight;

            // Create a RenderTargetBitmap to render the canvas
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)width, (int)height, 96d, 96d, PixelFormats.Default);
            rtb.Render(Canvas);

            // Transform BitMap to 28x28 pixels
            var rtb28 = new TransformedBitmap();
            rtb28.BeginInit();
            rtb28.Source = rtb;
            rtb28.Transform = new ScaleTransform(28.0 / width, 28.0 / height);
            rtb28.EndInit();

            // Encode the RenderTargetBitmap to a PNG file
            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb28));
            using (var fs = System.IO.File.OpenWrite(filename))
            {
                pngEncoder.Save(fs);
            }
        }

        private void NumberRecognizer_Click(object sender, RoutedEventArgs e)
        {
            var num = 5;
            var nr = new SimpleNumberRecognizer();
            nr.GetPixelVector(_localFileName);
            FinalNumber.Text = "This number is a " + num;
            // implement NumberRecognizer call here
        }
    }
}