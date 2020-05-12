using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveCharts.Wpf;
using LiveCharts;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageProcessing
{
    /// <summary>
    /// Interaction logic for HistogramWindow.xaml
    /// </summary>
    public partial class HistogramWindow : Window
    {

        Bitmap Image;
        Bitmap ModifiedImage;
        String SelectedColorName = "Red";
        int SelecterColorNumber = 0;

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
        byte[] rgbValues;
        byte[] r;
        byte[] g;
        byte[] b;
        int[] colorHistogramArray = new int[256];
        MainWindow mainWindow;
        public HistogramWindow(MainWindow main)
        {
            InitializeComponent();
            mainWindow = main;
            this.Image = mainWindow.bitmapImage;    // Get reference to original Bitmap image
            this.ModifiedImage = this.Image;
            this.GetColorArrays();
            this.GetSelectedColorArray();

            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = SelectedColorName+" Amount",
                    Values = new ChartValues<int>(colorHistogramArray.ToArray())
                }
            };

            //also adding values updates and animates the chart automatically
            Formatter = value => value.ToString("N");
            var stringList = new List<String>();
            for (int i = 0; i < 256; i++)
            {
                stringList.Add(i.ToString());
            }
            Labels = stringList.ToArray();

            DataContext = this;
        }

        public void DrawHistogram()
        {
            switch (SelecterColorNumber)
            {
                case 0:
                    SelectedColorName = "Red";
                    break;
                case 1:
                    SelectedColorName = "Green";
                    break;
                case 2:
                    SelectedColorName = "Blue";
                    break;
            }

            SeriesCollection.Clear();
            SeriesCollection.Add(new ColumnSeries()
            {
                    Title = SelectedColorName+" Amount",
                    Values = new ChartValues<int>(colorHistogramArray.ToArray())
            });

            //also adding values updates and animates the chart automatically
            Formatter = value => value.ToString("N");
            var stringList = new List<String>();
            for (int i = 0; i < 256; i++)
            {
                stringList.Add(i.ToString());
            }
            Labels = stringList.ToArray();

            DataContext = this;
        }

        public void GetColorArrays()
        {

            // Lock the bitmap's bits.  
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, ModifiedImage.Width, ModifiedImage.Height);
            BitmapData bmpData = ModifiedImage.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = (bmpData.Stride - (bmpData.Stride % 3)) * ModifiedImage.Height;
            rgbValues = new byte[bytes];
            r = new byte[bytes / 3];
            g = new byte[bytes / 3];
            b = new byte[bytes / 3];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            int count = 0;

            for (int i = 0; i < bytes; i+=3)
            {
                    r[count] = (byte)(rgbValues[i]);
                    g[count] = (byte)(rgbValues[i+1]);
                    b[count] = (byte)(rgbValues[i+2]);
                    count++;
            }

            ModifiedImage.UnlockBits(bmpData);
        }

        public void SetColorArrays()
        {

            // Lock the bitmap's bits.  
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, ModifiedImage.Width, ModifiedImage.Height);
            BitmapData bmpData = ModifiedImage.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = (bmpData.Stride - (bmpData.Stride % 3)) * ModifiedImage.Height;

            int count = 0;

            for (int i = 0; i < bytes; i += 3)
            {
                rgbValues[i] = r[count];
                rgbValues[i+1] = g[count];
                rgbValues[i+2] = b[count];
                count++;
            }

            // Copy the RGB values into the array.
            Marshal.Copy(rgbValues, 0, ptr, bytes);

            ModifiedImage.UnlockBits(bmpData);
        }

        public void GetSelectedColorArray()
        {
            for (int i = 0; i < colorHistogramArray.Length; i++)
            {
                colorHistogramArray[i] = 0;
            }
            switch (SelecterColorNumber)
            {
                case 0:
                    foreach(var e in r)
                    {
                        colorHistogramArray[e] += 1;
                    }
                    break;
                case 1:
                    foreach (var e in g)
                    {
                        colorHistogramArray[e] += 1;
                    }
                    break;
                case 2:
                    foreach (var e in b)
                    {
                        colorHistogramArray[e] += 1;
                    }
                    break;
                case 3:
                    for(var i = 0; i < r.Length; i++)
                    {
                        int rgb = (r[i] + g[i] + b[i]) / 3;
                        colorHistogramArray[rgb] += 1;
                    }
                    break;
                case 4:
                    for (var i = 0; i < r.Length; i++)
                    {
                        int brightness = (int)((0.2126 * r[i]) + (0.7152 * g[i]) + (0.0722 * b[i]));
                        colorHistogramArray[brightness] += 1;
                    }
                    break;
            }
        }

        public int[] CalculateLUT(int color)
        {
            int size = ModifiedImage.Height + ModifiedImage.Width;
            int[] selectedArray;
            int[] LUTArray = new int[256];
            int minValue = 0;
            int sumElement = 0;

            for (int i = 0; i < colorHistogramArray.Length; i++)
            {
                colorHistogramArray[i] = 0;
            }

            switch (color)
            {
                case 0:
                    foreach (var e in r)
                    {
                        colorHistogramArray[e] += 1;
                    }
                    selectedArray = colorHistogramArray;
                    break;
                case 1:
                    foreach (var e in g)
                    {
                        colorHistogramArray[e] += 1;
                    }
                    selectedArray = colorHistogramArray;
                    break;
                case 2:
                    foreach (var e in b)
                    {
                        colorHistogramArray[e] += 1;
                    }
                    selectedArray = colorHistogramArray;
                    break;
                default:
                    foreach (var e in r)
                    {
                        colorHistogramArray[e] += 1;
                    }
                    selectedArray = colorHistogramArray;
                    break;
            }

            for(int i=0; i<256; i++)
            {
                if (selectedArray[i] != 0)
                {
                    minValue = selectedArray[i];
                    break;
                }
            }

            // Calculating cumulative distribution
            for (int j=0; j<256; j++)
            {
                sumElement += selectedArray[j];
                LUTArray[j] = (int)((sumElement- minValue)* 255) / (r.Length- minValue);
            }

            return LUTArray;
        }

        public int[] CalculateStretchLUT(int color, int min, int max)
        {
            int A = min;
            int B = max;

            int[] resultLUT = new int[256];
            double result = 0.0;

            for (int i = 0; i < 256; i++)
            {
                //result = ((255.0 / (B - A)) * (i - A));
                result = ((i*(B - A)) / 255.0) + A;

                if(result > 255)
                {
                    resultLUT[i] = 255;
                }
                else if(result < 0)
                {
                    resultLUT[i] = 0;
                }
                else
                {
                    resultLUT[i] = (int)result;
                }
            }

            return resultLUT;


        }

        private void Red_Button_Click(object sender, RoutedEventArgs e)
        {
            this.SelecterColorNumber = 0;
            this.GetSelectedColorArray();
            this.DrawHistogram();
        }

        private void Green_Button_Click(object sender, RoutedEventArgs e)
        {
            this.SelecterColorNumber = 1;
            this.GetSelectedColorArray();
            this.DrawHistogram();
        }

        private void Blue_Button_Click(object sender, RoutedEventArgs e)
        {
            this.SelecterColorNumber = 2;
            this.GetSelectedColorArray();
            this.DrawHistogram();
        }

        private void Average_Button_Click(object sender, RoutedEventArgs e)
        {
            this.SelecterColorNumber = 3;
            this.GetSelectedColorArray();
            this.DrawHistogram();
        }

        private void Luminance_Click(object sender, RoutedEventArgs e)
        {
            this.SelecterColorNumber = 4;
            this.GetSelectedColorArray();
            this.DrawHistogram();
        }

        private void Enlighten_Button_Click(object sender, RoutedEventArgs e)
        {
            int rValue = 0;
            int gValue = 0;
            int bValue = 0;
            for (var x = 0; x < r.Length; x++)
            {
                rValue = (int)(Math.Pow(r[x], 1.2));
                gValue = (int)(Math.Pow(g[x], 1.2));
                bValue = (int)(Math.Pow(b[x], 1.2));

                if (rValue > 255)
                {
                    r[x] = 255;
                }
                else if (rValue < 0)
                {
                    r[x] = 0;
                }
                else
                {
                    r[x] = (byte)rValue;
                }

                if (gValue > 255)
                {
                    g[x] = 255;
                }
                else if (gValue < 0)
                {
                    g[x] = 0;
                }
                else
                {
                    g[x] = (byte)gValue;
                }

                if (bValue > 255)
                {
                    b[x] = 255;
                }
                else if (bValue < 0)
                {
                    b[x] = 0;
                }
                else
                {
                    b[x] = (byte)bValue;
                }
            }
            this.GetSelectedColorArray();
            this.DrawHistogram();
            this.SetColorArrays();
            mainWindow.CreateEditableImage(ModifiedImage);
        }

        private void Dim_Button_Click(object sender, RoutedEventArgs e)
        {
            int rValue = 0;
            int gValue = 0;
            int bValue = 0;
            for (var x = 0; x < r.Length; x++)
            {
                rValue = (int)(Math.Pow(r[x], 0.8));
                gValue = (int)(Math.Pow(g[x], 0.8));
                bValue = (int)(Math.Pow(b[x], 0.8));

                if (rValue > 255)
                {
                    r[x] = 255;
                }
                else if (rValue < 0)
                {
                    r[x] = 0;
                }
                else
                {
                    r[x] = (byte)rValue;
                }

                if (gValue > 255)
                {
                    g[x] = 255;
                }
                else if (gValue < 0)
                {
                    g[x] = 0;
                }
                else
                {
                    g[x] = (byte)gValue;
                }

                if (bValue > 255)
                {
                    b[x] = 255;
                }
                else if (bValue < 0)
                {
                    b[x] = 0;
                }
                else
                {
                    b[x] = (byte)bValue;
                }
            }
            this.GetSelectedColorArray();
            this.DrawHistogram();
            this.SetColorArrays();
            mainWindow.CreateEditableImage(ModifiedImage);
        }

        private void Equalization_Button_Click(object sender, RoutedEventArgs e)
        {
            int[] RedLUT = CalculateLUT(0);
            int[] GreenLUT = CalculateLUT(1);
            int[] BlueLUT = CalculateLUT(2);

            this.GetSelectedColorArray();
            this.DrawHistogram();

            // Lock the bitmap's bits.  
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, ModifiedImage.Width, ModifiedImage.Height);
            BitmapData bmpData = ModifiedImage.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = (bmpData.Stride - (bmpData.Stride % 3)) * ModifiedImage.Height;

            int count = 0;

            for (int i = 0; i < bytes; i += 3)
            {
                int rValue = (int)r[count];
                int gValue = (int)g[count];
                int bValue = (int)b[count];

                var byteRValue = (byte)RedLUT[rValue];
                var byteGValue = (byte)GreenLUT[gValue];
                var byteBValue = (byte)BlueLUT[bValue];

                r[count] = byteRValue;
                g[count] = byteGValue;
                b[count] = byteBValue;
                count++;
            }

            // Copy the RGB values into the array.
            Marshal.Copy(rgbValues, 0, ptr, bytes);

            ModifiedImage.UnlockBits(bmpData);

            this.GetSelectedColorArray();
            this.SetColorArrays();
            this.DrawHistogram();
            mainWindow.CreateEditableImage(ModifiedImage);

        }

        private void Stretch_Button_Click(object sender, RoutedEventArgs e)
        {
            string minmum_text = Minimum_Strech.Text;
            string maximum_text = Maximum_Strech.Text;
            int minimum = 0;
            int maximum = 255;

            try
            {
                minimum = Int32.Parse(minmum_text);
                maximum = Int32.Parse(maximum_text);

                if (minimum > maximum || maximum<0 || maximum>256 || minimum<0 || minimum>256)
                {
                    Minimum_Strech.Text = "Enter: 0-255";
                    Maximum_Strech.Text = "Enter: 0-255";
                    return;
                }


                int[] RedStretchLUT = CalculateStretchLUT(0, minimum, maximum);
                int[] GreenStretchUT = CalculateStretchLUT(1, minimum, maximum);
                int[] BluetretchLUT = CalculateStretchLUT(2, minimum, maximum);

                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, ModifiedImage.Width, ModifiedImage.Height);
                BitmapData bmpData = ModifiedImage.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = (bmpData.Stride - (bmpData.Stride % 3)) * ModifiedImage.Height;

                int stride = bmpData.Stride;
                int count = 0;

                for (int i = 0; i < bytes; i += 3)
                {
                    int rValue = (int)r[count];
                    int gValue = (int)g[count];
                    int bValue = (int)b[count];

                    var byteRValue = (byte)RedStretchLUT[rValue];
                    var byteGValue = (byte)GreenStretchUT[gValue];
                    var byteBValue = (byte)BluetretchLUT[bValue];

                    r[count] = byteRValue;
                    g[count] = byteGValue;
                    b[count] = byteBValue;
                    count++;
                }

                // Copy the RGB values into the array.
                Marshal.Copy(rgbValues, 0, ptr, bytes);

                ModifiedImage.UnlockBits(bmpData);

                this.GetSelectedColorArray();
                this.SetColorArrays();
                this.DrawHistogram();
                mainWindow.CreateEditableImage(ModifiedImage);

            }
            catch (Exception){
                Minimum_Strech.Text = "Enter: 0-255";
                Maximum_Strech.Text = "Enter: 0-255";
            }

        }
    }
}
