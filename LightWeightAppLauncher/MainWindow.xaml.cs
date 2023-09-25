using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
namespace LightWeightAppLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> AllSourcePaths = new List<string>();
        static readonly string _DirectoryConfigPath = Directory.GetCurrentDirectory().ToString() + "\\HV_AppLauncher";
        static readonly string _configPath = _DirectoryConfigPath + "\\config.hv";
        public MainWindow()
        {
            InitializeComponent();
            ValidateConfigFile();
            LoadConfig();
            LoadAllApps();
        }
        void LoadConfig()
        {
            foreach (string str in File.ReadLines(_configPath))
            {
                AllSourcePaths.Add(str);
            }
        }
        void WriteConfig()
        {
            File.WriteAllLines(_configPath, AllSourcePaths);
        }
        void LoadAllApps()
        {
            ApplicationPanel.Children.Clear();
            int CurrentIndex = 0;
            foreach (string PathAndImage in AllSourcePaths)
            {
                string[] SplittedPathAndImage = PathAndImage.Split('|');
                string App_Path = SplittedPathAndImage[0];
                string App_ImagePath = SplittedPathAndImage[1];
                ApplicationItemView applicationView = new ApplicationItemView(App_Path, App_ImagePath);
                applicationView.Tag = CurrentIndex;
                ApplicationPanel.Children.Add(applicationView);
                CurrentIndex++;
            }
            AddPlusButtonToAppPanel();
        }
        /// <summary>
        /// Validates the config file, if it doesnt exist it creates it
        /// </summary>
        void ValidateConfigFile()
        {
            if (!Directory.Exists(_DirectoryConfigPath))
            {
                Directory.CreateDirectory(_DirectoryConfigPath);
            }
            if (!File.Exists(_configPath))
            {
                File.Create(_configPath);
            }
        }
        /// <summary>
        /// Adds plus button to app panel
        /// </summary>
        void AddPlusButtonToAppPanel()
        {
            PlusButton g = new PlusButton();
            g.PreviewMouseDown += AddApplication;
            ApplicationPanel.Children.Add(g);
        }
        /// <summary>
        /// button press for add application, opens dialoug for adding app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddApplication(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            AddApplicationWindow addApplicationWindow = new AddApplicationWindow();
            addApplicationWindow.Save += AddNewApp_Save;
            addApplicationWindow.Show();
        }

        private void AddNewApp_Save(object? sender, System.EventArgs e)
        {
            AddApplicationWindow? wd = sender as AddApplicationWindow;
            if (wd == null)
            {
                return;
            }
            AllSourcePaths.Add(wd.UserInput_AppSource + "|" + wd.UserInput_ImageSource);
            WriteConfig();
            LoadAllApps();
        }

        private void Window_DragMoveOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
