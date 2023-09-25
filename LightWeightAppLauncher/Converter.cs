using System;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LightWeightAppLauncher
{
    internal class Converter
    {
        public static ImageSource? ConvertStringToImageSource(string imageSource)
        {
            try
            {
                return new BitmapImage(new Uri(imageSource, UriKind.RelativeOrAbsolute));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return null;
        }
    }
}
