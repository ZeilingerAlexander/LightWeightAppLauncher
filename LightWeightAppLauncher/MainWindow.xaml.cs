using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace LightWeightAppLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<int, string> AllSourcePaths = new Dictionary<int, string>();
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
            AllSourcePaths.Clear();
            int CurrentIndex = 0;
            foreach (string str in File.ReadLines(_configPath))
            {
                AllSourcePaths.Add(CurrentIndex, str);
                CurrentIndex++;
            }
        }
        void WriteConfig()
        {
            File.WriteAllLines(_configPath, AllSourcePaths.Values);
        }
        void LoadAllApps()
        {
            ApplicationPanel.Children.Clear();
            foreach (KeyValuePair<int, string> ID_PathAndImage in AllSourcePaths)
            {
                string[] SplittedPathAndImage = ID_PathAndImage.Value.Split('|');
                string App_Path = SplittedPathAndImage[0];
                string App_ImagePath = SplittedPathAndImage[1];
                ApplicationItemView applicationView = new ApplicationItemView(App_Path, App_ImagePath);
                applicationView.Tag = ID_PathAndImage.Key;
                applicationView.app_OpenApp += OpenApplicationClick;
                applicationView.app_OpenContextMenu += OpenApplicationContextMenuClick;
                MenuItem dltMenuItem = new MenuItem();
                dltMenuItem.Header = "Delete";
                dltMenuItem.Tag = ID_PathAndImage.Key;
                dltMenuItem.Click += DeleteAppClick;
                applicationView.ContextMenu = new ContextMenu();
                applicationView.ContextMenu.Items.Add(dltMenuItem);
                ApplicationPanel.Children.Add(applicationView);
            }
        }

        private void DeleteAppClick(object sender, RoutedEventArgs e)
        {
            string ID = (sender as FrameworkElement).Tag.ToString();
            AllSourcePaths.Remove(int.Parse(ID));
            WriteConfig();
            LoadAllApps();
        }

        private void OpenApplicationContextMenuClick(object? sender, System.EventArgs e)
        {
            ApplicationItemView appview = sender as ApplicationItemView;
            if (appview == null)
            {
                throw new InvalidDataException(nameof(appview));
            }
            appview.ContextMenu.Visibility = Visibility.Visible;
        }

        private void OpenApplicationClick(object? sender, System.EventArgs e)
        {
            string ProcessPath = AllSourcePaths.ElementAt(
                    int.Parse((sender as FrameworkElement).Tag.ToString())
                        ).Value.Split('|')[0];

            Process.Start(ProcessPath);
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
            AllSourcePaths.Add(AllSourcePaths.ToArray().Length, wd.UserInput_AppSource + "|" + wd.UserInput_ImageSource);
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
