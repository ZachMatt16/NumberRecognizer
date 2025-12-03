// <authors> Zach Mattson </authors>
// <date> 12/2/25 </date>

using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
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
        ///  Initializes a new SimpleNumberRecognizer
        /// </summary>
        private SimpleNumberRecognizer _nr = new();

        /// <summary>
        ///  Initializes the component.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Checks if the mouse button is pressed.
        /// </summary>
        /// <param name="sender">The Canvas that raised the event. </param>
        /// <param name="e"> Mouse event data, including which button was pressed and cursor position. </param>
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                _isDrawing = true;

            if (e.RightButton == MouseButtonState.Pressed)
                _isErasing = true;
            
            _previousPoint = e.GetPosition(Canvas);
        }

        /// <summary>
        ///  Checks if the mouse button is released
        /// </summary>
        /// <param name="sender">The Canvas that raised the event. </param>
        /// <param name="e"> Mouse event data, including which button was pressed and cursor position. </param>
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            _isErasing = false;
        }

        /// <summary>
        ///  Handles mouse movement. 
        /// </summary>
        /// <param name="sender">The Canvas that raised the event. </param>
        /// <param name="e"> Mouse event data, including which button was pressed and cursor position. </param>
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

            // make circle black if drawing
            if (_isDrawing)
                circle.Fill = new SolidColorBrush(Colors.White);

            // make circle black if erasing
            if (_isErasing)
            {
                circle.Fill = new SolidColorBrush(Colors.Black);
                circle.Width = radius * 5;
                circle.Height = radius * 5;
            }

            // Center the circle where the mouse is
            Canvas.SetLeft(circle, currentPoint.X - circle.Width / 2);
            Canvas.SetTop(circle, currentPoint.Y - circle.Height / 2);

            // draw it on the canvas
            Canvas.Children.Add(circle);
        }

        /// <summary>
        ///  Saves the model to the bin/Debug/net9.0-windows/Models folder with the given name.
        /// </summary>
        /// <param name="sender"> The "Save File" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Handle_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText($"Models/{Save_File_Name.Text}.txt", GetJsonStringRepresentation());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        ///  Returns the json string representing this spreadsheet.
        /// </summary>
        /// <returns> the json string representing this spreadsheet. </returns>
        private string GetJsonStringRepresentation()
        {
            return JsonSerializer.Serialize(_nr, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        ///  Loads a json file into the number recognizer. 
        /// </summary>
        /// <param name="sender"> The "Load File" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Handle_Load_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select a file",
                Filter = "Text Files (*.txt)|*.txt",
                Multiselect = false
            };
            var path = string.Empty;
            if (openFileDialog.ShowDialog() == true)
                path = openFileDialog.FileName;

            using (StreamReader reader = new StreamReader(path))
            {
                var content = reader.ReadToEnd();
                var nr = JsonSerializer.Deserialize<SimpleNumberRecognizer>(content);
                _nr = nr;
            }
        }

        /// <summary>
        ///  Clears the canvas. 
        /// </summary>
        /// <param name="sender"> The "Clear" button</param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Handle_Clear_Click(object sender, RoutedEventArgs e)
        {
            Canvas.Children.Clear();
        }

        /// <summary>
        ///  Saves the current drawn images to the bin/Debug/net9.0-windows/Numbers/CurrentNumber.png file. 
        /// </summary>
        private void SaveWindowAsImage()
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
            using (var fs = System.IO.File.OpenWrite("Numbers/CurrentNumber.png"))
            {
                pngEncoder.Save(fs);
            }
        }

        /// <summary>
        ///  Displays the final predicted number. 
        /// </summary>
        /// <param name="sender"> The "What number is this?" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void NumberRecognizer_Click(object sender, RoutedEventArgs e)
        {
            SaveWindowAsImage();
            FinalNumber.Text = "" + _nr.PredictNumber();
        }

        /// <summary>
        ///  Trains the current model with the given number of iterations or images. 
        /// </summary>
        /// <param name="sender"> The "Train Model" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Train_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(Train_Iterations.Text, out int iterations);
            if (iterations > 60_000)
                iterations = 60_000;
            _nr.TrainModelWithMnist(iterations);
        }

        /// <summary>
        ///  Trains the model once with a label of 0. 
        /// </summary>
        /// <param name="sender"> The "0" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Click_0(object sender, RoutedEventArgs e)
        {
            FinalNumber.Text = "" + _nr.TrainModel(0);
        }

        /// <summary>
        ///  Trains the model once with a label of 1. 
        /// </summary>
        /// <param name="sender"> The "0" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Click_1(object sender, RoutedEventArgs e)
        {
            FinalNumber.Text = "" + _nr.TrainModel(1);
        }

        /// <summary>
        ///  Trains the model once with a label of 2. 
        /// </summary>
        /// <param name="sender"> The "0" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Click_2(object sender, RoutedEventArgs e)
        {
            FinalNumber.Text = "" + _nr.TrainModel(2);
        }

        /// <summary>
        ///  Trains the model once with a label of 3. 
        /// </summary>
        /// <param name="sender"> The "0" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Click_3(object sender, RoutedEventArgs e)
        {
            FinalNumber.Text = "" + _nr.TrainModel(3);
        }

        /// <summary>
        ///  Trains the model once with a label of 4. 
        /// </summary>
        /// <param name="sender"> The "0" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Click_4(object sender, RoutedEventArgs e)
        {
            FinalNumber.Text = "" + _nr.TrainModel(4);
        }

        /// <summary>
        ///  Trains the model once with a label of 5. 
        /// </summary>
        /// <param name="sender"> The "0" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Click_5(object sender, RoutedEventArgs e)
        {
            FinalNumber.Text = "" + _nr.TrainModel(5);
        }

        /// <summary>
        ///  Trains the model once with a label of 6. 
        /// </summary>
        /// <param name="sender"> The "0" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Click_6(object sender, RoutedEventArgs e)
        {
            FinalNumber.Text = "" + _nr.TrainModel(6);
        }

        /// <summary>
        ///  Trains the model once with a label of 7. 
        /// </summary>
        /// <param name="sender"> The "0" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Click_7(object sender, RoutedEventArgs e)
        {
            FinalNumber.Text = "" + _nr.TrainModel(7);
        }

        /// <summary>
        ///  Trains the model once with a label of 8. 
        /// </summary>
        /// <param name="sender"> The "0" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Click_8(object sender, RoutedEventArgs e)
        {
            FinalNumber.Text = "" + _nr.TrainModel(8);
        }

        /// <summary>
        ///  Trains the model once with a label of 9. 
        /// </summary>
        /// <param name="sender"> The "0" button. </param>
        /// <param name="e"> Event data associated with the click. </param>
        private void Click_9(object sender, RoutedEventArgs e)
        {
            FinalNumber.Text = "" + _nr.TrainModel(9);
        }
    }
}