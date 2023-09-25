using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LightWeightAppLauncher
{
    /// <summary>
    /// Interaction logic for AddApplicationWindow.xaml
    /// </summary>
    public partial class AddApplicationWindow : Window
    {
        public string? UserInput_ImageSource = null;
        public string? UserInput_AppSource = null;

        public event EventHandler Save;

        private readonly string _UserInputOK = "✔";
        private readonly Brush _UserInputOkColor = (Brush)new BrushConverter().ConvertFromString("#2ecc71");
        private readonly string _UserInputMissing = "❗";
        private readonly Brush _UserInputMissingColor = (Brush)new BrushConverter().ConvertFromString("#e74c3c");

        public AddApplicationWindow()
        {
            InitializeComponent();
        }

        private void ExitClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.Close();
            }
        }

        private void ConfirmClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            if (UserInput_AppSource == null || UserInput_ImageSource == null)
            {
                return;
            }
            Save?.Invoke(this, e);
            this.Close();
        }

        private void ChooseApplicationPath(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select an Application";
            op.Filter = "Exe Files (.exe)|*.exe";
            if (op.ShowDialog() == true)
            {
                if (op.FileName.ToLower().EndsWith(".exe"))
                {
                    UserInputValidationAppPath.Text = _UserInputOK;
                    UserInputValidationAppPath.Foreground = _UserInputOkColor;
                    UserInput_AppSource = op.FileName;
                }
                else
                {
                    UserInputValidationAppPath.Text = _UserInputMissing;
                    UserInputValidationAppPath.Foreground = _UserInputMissingColor;
                    UserInput_AppSource = null;
                }
            }
        }

        private void ChooseImagePath(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == false)
            { return; }
            try
            {
                Converter.ConvertStringToImageSource(op.FileName);

                string str = op.FileName.ToLower();
                if (str.EndsWith(".jpg") || str.EndsWith(".jpeg") || str.EndsWith(".png"))
                {
                    UserInputValidationImagePath.Text = _UserInputOK;
                    UserInputValidationImagePath.Foreground = _UserInputOkColor;
                    UserInput_ImageSource = op.FileName;
                }
                else
                {
                    throw new Exception("Wrong File Ending");
                }
            }
            catch (Exception)
            {
                UserInputValidationImagePath.Text = _UserInputMissing;
                UserInputValidationImagePath.Foreground = _UserInputMissingColor;
                UserInput_ImageSource = null;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}