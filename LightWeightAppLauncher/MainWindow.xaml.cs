using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            new Thread(CheckKeyBinds).Start();
        }

        void CheckKeyBinds()
        {
            /// <summary>
            /// Checks if any key is pressed and puts that into a list
            /// </summary>
            /// <returns></returns>
            List<Key> IsAnyKeyPressed()
            {
                var allPossibleKeys = Enum.GetValues(typeof(Key));
                List<Key> results = new List<Key>();
                foreach (var currentKey in allPossibleKeys)
                {
                    Key key = (Key)currentKey;
                    if (key != Key.None)
                    {
                        // Use Dispatcher to invoke on the UI thread
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (Keyboard.IsKeyDown((Key)currentKey))
                            {
                                results.Add((Key)currentKey);
                            }
                        });
                    }
                }
                return results;
            }

            // Contains keys and their expiration date
            Dictionary<string, DateTime> KeysAndTimestamps = new Dictionary<string, DateTime>();

            // updates the timestamps
            void UpdateTimestamps()
            {
                List<string> KeysToRemove = new List<string>();
                foreach (KeyValuePair<string, DateTime> keyandTime in KeysAndTimestamps)
                {
                    if (DateTime.Now > keyandTime.Value)
                    {
                        KeysToRemove.Add(keyandTime.Key);
                    }
                }
                foreach (string keytoremove in KeysToRemove)
                {
                    KeysAndTimestamps.Remove(keytoremove);
                }
            }

            while (true)
            {
                UpdateTimestamps();
                if (!useKeybindsToLaunch) { continue; }
                List<Key> keys = IsAnyKeyPressed();

                if (keys.Count != 0)
                {
                    // If the key is on cooldown
                    if (KeysAndTimestamps.Keys.Contains(keys[0].ToString())) { continue; }

                    // If esc close program
                    if (keys[0] == Key.Escape) { this.Close(); }

                    foreach (Key key in keys)
                    {
                        KeysAndTimestamps.Add(key.ToString(), DateTime.Now.Add(TimeSpan.FromSeconds(1)));
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StartProccess(AllKeybindsWithAppPaths.GetValueOrDefault(keys[0].ToString()));
                    });
                }
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
            foreach (string str in File.ReadLines(_configPath))
            {
                AllSourcePaths.Add(CurrentIndex, str);
                AllKeybindsWithAppPaths.Add(str.Split('|')[2], str.Split('|')[0]);
                CurrentIndex++;
            }
        }
        void WriteConfig()
        {
            File.WriteAllLines(_configPath, AllSourcePaths.Values);
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

        /// <summary>
        /// Starts a given process
        /// </summary>
        void StartProccess(string path)
        {
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source + "\n" + ex.Data);
            }
        }

        private void OpenApplicationClick(object? sender, System.EventArgs e)
        {
            string ProcessPath = AllSourcePaths.ElementAt(
                    int.Parse((sender as FrameworkElement).Tag.ToString())
                        ).Value.Split('|')[0];

            StartProccess(ProcessPath);
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
    }
}
