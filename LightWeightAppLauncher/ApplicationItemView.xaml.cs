﻿using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace LightWeightAppLauncher
{
    /// <summary>
    /// Interaction logic for ApplicationItemView.xaml
    /// </summary>
    public partial class ApplicationItemView : Grid
    {
        public ApplicationItemView(string Source, string ImagePath)
        {
            InitializeComponent();
            AppName.Text = Source.Split("\\").Last();
            AppImage.Source = Converter.ConvertStringToImageSource(ImagePath);
        }
        public event EventHandler app_OpenApp;
        public event EventHandler app_OpenContextMenu;
        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                app_OpenApp?.Invoke(this, e);
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                app_OpenContextMenu?.Invoke(this, e);
            }
        }
    }
}
