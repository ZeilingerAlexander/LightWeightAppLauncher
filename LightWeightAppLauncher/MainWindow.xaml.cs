using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LightWeightAppLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Contains all keybinds with their app paths, dont directly modify this, only on loading
        /// </summary>
        Dictionary<string, string> AllKeybindsWithAppPaths = new Dictionary<string, string>();
        /// <summary>
        /// Contains all ids and their source paths
        /// </summary>
        Dictionary<int, string> AllSourcePaths = new Dictionary<int, string>();
        static readonly string _DirectoryConfigPath = Directory.GetCurrentDirectory().ToString() + "\\HV_AppLauncher";
        public static readonly string _defaultImagePath = "pack://application:,,,/Images/default.png";
        public static readonly string _configPath = _DirectoryConfigPath + "\\config.hv";
        public static readonly string _DefaultConfigEmptyString = "NONE";

        bool useKeybindsToLaunch = true;
        public MainWindow()
        {
            InitializeComponent();
            ValidateConfigFile();
            LoadConfig();
            LoadAllApps();


            // Thread for launching with keybinds
            Thread t = new Thread(StartKeyboardListener);
            ProcessManager.ActiveThreads.Add(t);
            t.Start();
        }

        /// <summary>
        /// starts the background keyboard listener for launching apps via keybinds
        /// </summary>
        void StartKeyboardListener()
        {
            while (true)
            {
                InputManager.UpdateTimestamps();
                if (!useKeybindsToLaunch) { continue; }

                string? PressedKey = InputManager.GetPressedKeyOrNull();
                if (PressedKey == null) { continue; }
                if (PressedKey == "CLOSEPROGRAM")
                {
                    ProcessManager.DispenseAllThreads();
                    this.Close();
                }

                // try to start the process with associated key
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProcessManager.StartProccess(AllKeybindsWithAppPaths.GetValueOrDefault(PressedKey));
                });
            }
        }



        /// <summary>
        /// Whenever user wants to toggle using keybinds to launch apps
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleKeybindMode(object sender, MouseButtonEventArgs e)
        {
            useKeybindsToLaunch = !useKeybindsToLaunch;
            (sender as TextBlock).Foreground = useKeybindsToLaunch ? FindResource("Color_TextImportant") as Brush : FindResource("Color_TextInvalid") as Brush;
        }

        void LoadConfig()
        {
            AllSourcePaths.Clear();
            AllKeybindsWithAppPaths.Clear();
            int CurrentIndex = 0;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(_configPath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    lines.Add(line);
                }
            }

            foreach (string str in lines)
            {
                AllSourcePaths.Add(CurrentIndex, str);
                AllKeybindsWithAppPaths.Add(str.Split('|')[2], str.Split('|')[0]);
                CurrentIndex++;
            }
        }
        void WriteConfig()
        {
            using (StreamWriter writer = new StreamWriter(_configPath))
            {
                foreach (string strToWrite in AllSourcePaths.Values)
                {
                    writer.WriteLine(strToWrite);
                }
            }
        }
        /// <summary>
        /// Updates config and view by first wrting config then loading all apps
        /// </summary>
        void UpdateConfigAndView()
        {
            WriteConfig();
            LoadConfig();
            LoadAllApps();
        }
        void LoadAllApps()
        {
            ApplicationPanel.Children.Clear();
            foreach (KeyValuePair<int, string> ID_PathAndImage in AllSourcePaths)
            {
                // Get Variables
                string[] SplittedPathAndImage = ID_PathAndImage.Value.Split('|');
                string App_Path = SplittedPathAndImage[0];
                string App_ImagePath = SplittedPathAndImage[1];
                string Keybind = SplittedPathAndImage[2];

                // Create App View
                ApplicationItemView applicationView = new ApplicationItemView(App_Path, App_ImagePath, Keybind);
                applicationView.Tag = ID_PathAndImage.Key;
                applicationView.app_OpenApp += OpenApplicationClick;
                applicationView.app_OpenContextMenu += OpenApplicationContextMenuClick;

                // Add Context Menu for deletion
                MenuItem dltMenuItem = new MenuItem();
                dltMenuItem.Header = "Delete";
                dltMenuItem.Tag = ID_PathAndImage.Key;
                dltMenuItem.Click += DeleteAppClick;
                applicationView.ContextMenu = new ContextMenu();
                applicationView.ContextMenu.Items.Add(dltMenuItem);


                ApplicationPanel.Children.Add(applicationView);
            }
        }

        /// <summary>
        /// Whenever delete click in context menu delete that app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteAppClick(object sender, RoutedEventArgs e)
        {
            string ID = (sender as FrameworkElement).Tag.ToString();
            AllSourcePaths.Remove(int.Parse(ID));
            UpdateConfigAndView();
        }

        /// <summary>
        /// Opens Context Menu for given Application on right click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="InvalidDataException"></exception>
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

            ProcessManager.StartProccess(ProcessPath);
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

        /// <summary>
        /// Adds a new app from the options user chose in sender
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewApp_Save(object? sender, System.EventArgs e)
        {
            AddApplicationWindow? wd = sender as AddApplicationWindow;
            if (wd == null)
            {
                return;
            }
            AllSourcePaths.Add(AllSourcePaths.ToArray().Length, wd.UserInput_AppSource + "|" + wd.UserInput_ImageSource + "|" + wd.UserInput_Keybind);
            UpdateConfigAndView();
        }

        private void Window_DragMoveOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ProcessManager.DispenseAllThreads();
        }
    }
}
