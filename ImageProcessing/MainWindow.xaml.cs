using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Image = System.Drawing.Image;

namespace ImageProcessing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public Bitmap bitmapImage { get; set; }
        public int R_Value { get; set; }
        public int G_Value { get; set; }
        public int B_Value { get; set; }

        byte[] r_array;
        byte[] g_array;
        byte[] b_array;

        byte[,] r_array_index;
        byte[,] g_array_index;
        byte[,] b_array_index;

        int[] histogramArray = new int[256];

        public MainWindow()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            R_Value = 0;
            B_Value = 0;
            G_Value = 0;
        }

        public void GetColorArrays()
        {
            try
            {
                int count = 0;
                r_array_index = new byte[bitmapImage.Height, bitmapImage.Width];
                g_array_index = new byte[bitmapImage.Height, bitmapImage.Width];
                b_array_index = new byte[bitmapImage.Height, bitmapImage.Width];
                r_array = new byte[bitmapImage.Height * bitmapImage.Width];
                g_array = new byte[bitmapImage.Height * bitmapImage.Width];
                b_array = new byte[bitmapImage.Height * bitmapImage.Width];

                for (int i = 0; i < bitmapImage.Height; i++)
                {
                    for (int j = 0; j < bitmapImage.Width; j++)
                    {
                        System.Drawing.Color pixelColor = bitmapImage.GetPixel(j, i);
                        r_array[count] = (byte)pixelColor.R;
                        r_array_index[i,j] = (byte)pixelColor.R;

                        g_array[count] = (byte)pixelColor.G;
                        g_array_index[i,j] = (byte)pixelColor.G;

                        b_array[count] = (byte)pixelColor.B;
                        b_array_index[i,j] = (byte)pixelColor.B;
                        count++;
                    }
                }
            }
            catch (NullReferenceException) { }
        }

        public void SetColorArrays()
        {
            int count = 0;

            for (int i = 0; i < bitmapImage.Height; i++)
            {
                for (int j = 0; j < bitmapImage.Width; j++)
                {
                    System.Drawing.Color pixelColor = new System.Drawing.Color();
                    pixelColor = System.Drawing.Color.FromArgb(r_array[count], g_array[count], b_array[count]);
                    bitmapImage.SetPixel(j, i, pixelColor);
                    count += 1;
                }
            }
        }

        public int[] CalculateHistogram()
        {
            int[] histogram = new int[256];

            foreach (var e in r_array)
            {
                histogram[e] += 1;
            }

            return histogram;
        }

        private void MenuItemFileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|PNG Image|*.png|TIFF Image|*.tiff|TIFF Image|*.tif";
            if (openFileDialog.ShowDialog() == true)
            {
                Uri fileUri = new Uri(openFileDialog.FileName);
                loadedImage.Source = new BitmapImage(fileUri);
                Image img = Image.FromFile(openFileDialog.FileName);
                bitmapImage = new Bitmap(openFileDialog.FileName);
                MemoryStream MS = new MemoryStream();
                bitmapImage.Save(MS, System.Drawing.Imaging.ImageFormat.Jpeg);
                bitmapImage = new Bitmap(MS);


                using (MemoryStream memory = new MemoryStream())
                {
                    bitmapImage.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmap= new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = memory;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    modifiedImage.Source = bitmap;
                }
            }
        }

        private void MenuItemFileSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "Document";
            dlg.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|PNG Image|*.png|TIFF Image|*.tiff|TIFF Image|*.tif";
            dlg.Title = "Save and Image File";
            var encoder = new PngBitmapEncoder();
            if (dlg.ShowDialog() == true && modifiedImage != null)
            {
                try
                {
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)modifiedImage.Source));
                    using (var stream = dlg.OpenFile())
                    {
                        encoder.Save(stream);
                    }
                }
                catch (System.ArgumentNullException)
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        private void MenuItemFileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void CreateEditableImage(Bitmap image)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                image.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                modifiedImage.Source = bitmapimage;
            }
        }

        private void ModifiedImage_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(modifiedImage);

            double originalModifiedImageWidth = bitmapImage.Width;
            double originalModifiedImageHeight = bitmapImage.Height;
            double actualModifiedImageWidth = modifiedImage.ActualWidth;
            double actualModifiedImageHeight = modifiedImage.ActualHeight;

            double pixelIndexX = p.X * originalModifiedImageWidth / actualModifiedImageWidth;
            double pixelIndexY = p.Y * originalModifiedImageHeight / actualModifiedImageHeight;

            System.Drawing.Color CurrentColor = bitmapImage.GetPixel(Convert.ToInt32(Math.Floor(pixelIndexX)), Convert.ToInt32(Math.Floor(pixelIndexY)));

            current_R_Value.Text = CurrentColor.R.ToString();
            current_G_Value.Text = CurrentColor.G.ToString();
            current_B_Value.Text = CurrentColor.B.ToString();
        }

        private void ModifiedImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(modifiedImage);

            double originalModifiedImageWidth = bitmapImage.Width;
            double originalModifiedImageHeight = bitmapImage.Height;
            double actualModifiedImageWidth = modifiedImage.ActualWidth;
            double actualModifiedImageHeight = modifiedImage.ActualHeight;

            double pixelIndexX = originalModifiedImageWidth * p.X / actualModifiedImageWidth;
            double pixelIndexY = originalModifiedImageHeight * p.Y / actualModifiedImageHeight;

            int x = Convert.ToInt32(Math.Floor(pixelIndexX));
            int y = Convert.ToInt32(Math.Floor(pixelIndexY));

            System.Drawing.Color CurrentColor = new System.Drawing.Color();
            CurrentColor = System.Drawing.Color.FromArgb(255, R_Value, G_Value, B_Value);

            bitmapImage.SetPixel(x,y, CurrentColor);
            this.CreateEditableImage(bitmapImage);


        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double newUserZoomValue = e.NewValue;
            ScaleTransform scaleTransform = new ScaleTransform(newUserZoomValue, newUserZoomValue);
            modifiedImage.LayoutTransform = scaleTransform;
        }

        private void RSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            R_Value = Convert.ToInt32(e.NewValue);
        }

        private void GSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            G_Value = Convert.ToInt32(e.NewValue);
        }

        private void BSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            B_Value = Convert.ToInt32(e.NewValue);
        }

        private void GoToHistogram_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HistogramWindow histogramWindow = new HistogramWindow(this);
                histogramWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                histogramWindow.Show();
            }
            catch (NullReferenceException){}

        }

        private void Convert_To_Greyscale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GetColorArrays();

                for (int i = 0; i < r_array.Length; i++)
                {
                    byte luminance = (byte)((0.2126 * r_array[i]) + (0.7152 * g_array[i]) + (0.0722 * b_array[i]));
                    r_array[i] = (byte)luminance;
                    g_array[i] = (byte)luminance;
                    b_array[i] = (byte)luminance;
                }

                this.SetColorArrays();
                this.CreateEditableImage(bitmapImage);
            }
            catch (NullReferenceException) { }
        }

        private void Global_Binarization_Click(object sender, RoutedEventArgs e)
        {
            try {
                this.GetColorArrays();
                string threshold = Global_Binarization_Border.Text;
                int thresholdValue = 0;

                thresholdValue = Int32.Parse(threshold);
                if(thresholdValue < 0 || thresholdValue > 255)
                {
                    Global_Binarization_Border.Text = "Enter: 0-255";
                }
                else
                {
                    for(int i = 0; i < r_array.Length; i++)
                    {
                        if(r_array[i] >= thresholdValue)
                        {
                            r_array[i] = 255;
                            g_array[i] = 255;
                            b_array[i] = 255;
                        }
                        else
                        {
                            r_array[i] = 0;
                            g_array[i] = 0;
                            b_array[i] = 0;
                        }
                    }
                    this.SetColorArrays();
                    this.CreateEditableImage(bitmapImage);
                }
            }
            catch (Exception) { }
        }

        private void Otsu_Binarization_Click(object sender, RoutedEventArgs e)
        {
            this.GetColorArrays();
            histogramArray = this.CalculateHistogram();
            int N = bitmapImage.Width * bitmapImage.Height;
            double Wb = 0.0;
            double Wf = 0.0;
            double VarianceB = 0.0;
            double VarianceF = 0.0;
            double GammaB = 0.0;
            double GammaF = 0.0;
            double[] minimumValues = new double[256];

            for (int T = 0; T < 256; T++)
            {
                Wb = 0.0;
                Wf = 0.0;
                VarianceB = 0.0;
                VarianceF = 0.0;
                GammaB = 0.0;
                GammaF = 0.0;
                for (int i = 0; i <= (T-1); i++)
                {
                    Wb += histogramArray[i] / (double)N;
                }

                for (int i = 0; i <= (T-1); i++)
                {
                    GammaB += i * (histogramArray[i] / (double)N) / Wb;
                }

                for (int i = T; i <= 255; i++)
                {
                    Wf += histogramArray[i] / (double)N;
                }

                for (int i = T; i <= 255; i++)
                {
                    GammaF += i * (histogramArray[i] / (double)N) / Wf;
                }

                for (int i = 0; i <= (T-1); i++)
                {
                    VarianceB += (histogramArray[i] * (double)N) * (Math.Pow((i - GammaB), 2) / Wb);
                }

                for (int i = T; i <= 255; i++)
                {
                    VarianceF += (histogramArray[i] * (double)N) * (Math.Pow((i - GammaF), 2) / Wf);
                }

                double currentValue = (Wf * Math.Pow(VarianceF, 2)) + (Wb * Math.Pow(VarianceB, 2));

                minimumValues[T] = currentValue;
            }

            double minimumValue = minimumValues[0];
            int threshold = 0;

            for(int i = 1; i < 256; i++)
            {
                if(minimumValues[i] < minimumValue)
                {
                    minimumValue = minimumValues[i];
                    threshold = i;
                }
            }

            for (int i = 0; i < r_array.Length; i++)
            {
                if (r_array[i] >= threshold)
                {
                    r_array[i] = 255;
                    g_array[i] = 255;
                    b_array[i] = 255;
                }
                else
                {
                    r_array[i] = 0;
                    g_array[i] = 0;
                    b_array[i] = 0;
                }
            }
            this.SetColorArrays();
            this.CreateEditableImage(bitmapImage);
        }

        private void Niblack_Binarization_Click(object sender, RoutedEventArgs e)
        {
                this.GetColorArrays();

                string k_value = Nibalck_k.Text;
                string width_value = Niblack_window_width.Text;
                string height_value = Niblack_window_height.Text;

                double k = 0.0;
                int width, height = 0;

                k = Double.Parse(k_value);
                width = Int32.Parse(width_value);
                height = Int32.Parse(height_value);
                if (k >= 1 || width > bitmapImage.Width || width <= 0 || height > bitmapImage.Height || height <= 0 || width % 2 == 0 || height % 2 == 0)
                {
                    Nibalck_k.Text = "k < 1";
                    Niblack_window_width.Text = "Odd number";
                    Niblack_window_height.Text = "Odd number";
                }
                else
                {
                    byte[] modifiedNiblackImage = r_array;
                    int counter = 0;

                    for (int i = 0; i < bitmapImage.Height; i++)
                    {
                        for (int j = 0; j < bitmapImage.Width; j++)
                        {
                            int currentX = j;
                            int currentY = i;
                            modifiedNiblackImage[counter] = this.CalculateNiblackPixel(width, height, k, currentX, currentY);
                            counter++;
                        }
                    }

                    for (int l = 0; l < r_array.Length; l++)
                    {
                        r_array[l] = modifiedNiblackImage[l];
                        g_array[l] = modifiedNiblackImage[l];
                        b_array[l] = modifiedNiblackImage[l];
                    }
                    this.SetColorArrays();
                    this.CreateEditableImage(bitmapImage);
                }
        }

        public byte CalculateNiblackPixel(int w, int h, double k, int x, int y)
        {
            double variance = 0.0;
                double standard_deviation = 0.0;
                double average = 0.0;
                int xRange = (w - 1) / 2;
                int yRange = (h - 1) / 2;
                int currentX = 0;
                int currentY = 0;
                int[] windowHistogram = new int[256];
                System.Drawing.Color c = new System.Drawing.Color();

                for (int i = x - xRange; i <= x + xRange; i++)
                {
                    for (int j = y - yRange; j <= y + yRange; j++)
                    {

                        if (i < 0)
                        {
                            currentX = 0;
                        }
                        else if (i >= bitmapImage.Width)
                        {
                            currentX = bitmapImage.Width-1;
                        }
                        else
                        {
                            currentX = i;
                        }

                        if (j < 0)
                        {
                            currentY = 0;
                        }
                        else if (j >= bitmapImage.Height){
                            currentY = bitmapImage.Height-1;
                        }
                        else
                        {
                            currentY = j;
                        }

                    c = bitmapImage.GetPixel(currentX, currentY);
                    average += c.R;
                    }
                }

                int divider = 0;
                divider = w * h;

                average = average / (double)divider;

                for (int i = x - xRange; i <= x + xRange; i++)
                {
                    for (int j = y - yRange; j <= y + yRange; j++)
                    {
                    if (i < 0)
                    {
                        currentX = 0;
                    }
                    else if (i >= bitmapImage.Width)
                    {
                        currentX = bitmapImage.Width - 1;
                    }
                    else
                    {
                        currentX = i;
                    }

                    if (j < 0)
                    {
                        currentY = 0;
                    }
                    else if (j >= bitmapImage.Height)
                    {
                        currentY = bitmapImage.Height - 1;
                    }
                    else
                    {
                        currentY = j;
                    }

                    c = bitmapImage.GetPixel(currentX, currentY);
                    variance += Math.Pow(c.R - average, 2);
                    }
                }

                variance = variance / (double)divider;

                standard_deviation = Math.Sqrt(variance);

                int T = 0;

                T = (int)(average + k * standard_deviation);

                c = bitmapImage.GetPixel(x, y);
                int current_value = c.R;
                
                if (current_value < T)
                {
                    return 0;
                }
                else
                {
                    return 255;
                }

        }

        private void Linear_3x3_Filter_Click(object sender, RoutedEventArgs e)
        {
            LinearFilterPopup.IsOpen = true;
        }

        private void Kuwahar_Filter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GetColorArrays();
                string widthMaskValueString;
                string heightMaskValueString;

                widthMaskValueString = Kuwahar_Filter_Width.Text;
                heightMaskValueString = Kuwahar_Filter_Height.Text;

                int maskWidth = 0;
                int maskHeight = 0;

                maskWidth = Int32.Parse(widthMaskValueString);
                maskHeight = Int32.Parse(heightMaskValueString);

                if(maskWidth <= 0 || maskHeight <= 0 || maskWidth > bitmapImage.Width || maskHeight > bitmapImage.Height || maskWidth != maskHeight)
                {
                    return;
                }
                else
                {
                    this.FilterImage(new int[1], 1, maskWidth, maskHeight);
                }
            }
            catch (Exception) { }
        }

        private void Median_3x3_Filter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GetColorArrays();
                this.FilterImage(new int[1], 2, 3, 3);
            }
            catch (Exception) { }
        }

        private void Median_5x5_Filter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GetColorArrays();
                this.FilterImage(new int[1], 2, 5, 5);
            }
            catch (Exception) { }
        }

        private void Popup_Submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GetColorArrays();
                string[] maskValueString = new string[9];

                maskValueString[0] = Linear_Value_0.Text;
                maskValueString[1] = Linear_Value_1.Text;
                maskValueString[2] = Linear_Value_2.Text;
                maskValueString[3] = Linear_Value_3.Text;
                maskValueString[4] = Linear_Value_4.Text;
                maskValueString[5] = Linear_Value_5.Text;
                maskValueString[6] = Linear_Value_6.Text;
                maskValueString[7] = Linear_Value_7.Text;
                maskValueString[8] = Linear_Value_8.Text;

                int[] maskValue = new int[9];

                for (int i = 0; i < maskValue.Length; i++)
                {
                    maskValue[i] = Int32.Parse(maskValueString[i]);
                }

                this.FilterImage(maskValue, 0, 3, 3);

                LinearFilterPopup.IsOpen = false;
            }
            catch (Exception) { }
        }

        private void Popup_Cancel_Click(object sender, RoutedEventArgs e)
        {
            LinearFilterPopup.IsOpen = false;
        }

        public void FilterImage(int[] mask, int mode, int maskWidth, int maskHeight)
        {
            switch (mode)
            {
                case 0:
                    this.CalculateLinearFilter(mask, maskWidth, maskHeight);
                    break;
                case 1:
                    this.CalculateKuwaharFilter(maskWidth, maskHeight);
                    break;
                case 2:
                    this.CalculateMedianFilter(maskWidth, maskHeight);
                    break;
            }
        }

        public void CalculateLinearFilter(int[] mask, int maskWidth, int maskHeight)
        {
            byte[,] modifiedRedValue = new byte[bitmapImage.Height, bitmapImage.Width];
            byte[,] modifiedGreenValue = new byte[bitmapImage.Height, bitmapImage.Width];
            byte[,] modifiedBlueValue = new byte[bitmapImage.Height, bitmapImage.Width];
            byte[] returnedPixelRGB = new byte[3];

            for (int i = 0; i < bitmapImage.Height; i++)
            {
                for (int j = 0; j < bitmapImage.Width; j++)
                {
                    int currentX = j;
                    int currentY = i;
                    returnedPixelRGB = this.CalculateLinearFilterPixel(mask, maskWidth, maskHeight, currentX, currentY);
                    modifiedRedValue[i, j] = returnedPixelRGB[0];
                    modifiedGreenValue[i, j] = returnedPixelRGB[1];
                    modifiedBlueValue[i, j] = returnedPixelRGB[2];
                }
            }

            int counter = 0;

            for (int i = 0; i < bitmapImage.Height; i++)
            {
                for (int j = 0; j < bitmapImage.Width; j++)
                {
                    r_array[counter] = modifiedRedValue[i, j];
                    g_array[counter] = modifiedGreenValue[i, j];
                    b_array[counter] = modifiedBlueValue[i, j];
                    counter++;
;                }
            }

            this.SetColorArrays();
            this.CreateEditableImage(bitmapImage);
        }

        public byte[] CalculateLinearFilterPixel(int[] mask, int maskWidth, int maskHeight, int x, int y)
        {
            int xRange = (maskWidth - 1) / 2;
            int yRange = (maskHeight - 1) / 2;
            int currentX = 0;
            int currentY = 0;
            int sumRed = 0;
            int sumGreen = 0;
            int sumBlue = 0;
            byte newRed = 0;
            byte newGreen = 0;
            byte newBlue = 0;
            int maskIteratorCounter = 0;
            int maskSum = 0;

            for (int i = x - xRange; i <= x + xRange; i++)
            {
                for (int j = y - yRange; j <= y + yRange; j++)
                {

                    if (i < 0)
                    {
                        currentX = 0;
                    }
                    else if (i >= bitmapImage.Width)
                    {
                        currentX = bitmapImage.Width - 1;
                    }
                    else
                    {
                        currentX = i;
                    }

                    if (j < 0)
                    {
                        currentY = 0;
                    }
                    else if (j >= bitmapImage.Height)
                    {
                        currentY = bitmapImage.Height - 1;
                    }
                    else
                    {
                        currentY = j;
                    }

                    sumRed += r_array_index[currentY, currentX] * mask[maskIteratorCounter];
                    sumGreen += g_array_index[currentY, currentX] * mask[maskIteratorCounter];
                    sumBlue += b_array_index[currentY, currentX] * mask[maskIteratorCounter];
                    maskSum += mask[maskIteratorCounter];
                    maskIteratorCounter++;
                }
            }

            if(maskSum == 0)
            {
                maskSum = 1;
            }

            newRed = (byte)(Math.Abs(sumRed) / maskSum);
            newGreen = (byte)(Math.Abs(sumGreen) / maskSum);
            newBlue = (byte)(Math.Abs(sumBlue) / maskSum);

            byte[] newPixelRGB = new byte[3];
            newPixelRGB[0] = (byte)newRed;
            newPixelRGB[1] = (byte)newGreen;
            newPixelRGB[2] = (byte)newBlue;

            return newPixelRGB;
        }

        public void CalculateMedianFilter(int maskWidth, int maskHeight)
        {
            byte[,] modifiedRedValue = new byte[bitmapImage.Height, bitmapImage.Width];
            byte[,] modifiedGreenValue = new byte[bitmapImage.Height, bitmapImage.Width];
            byte[,] modifiedBlueValue = new byte[bitmapImage.Height, bitmapImage.Width];
            byte[] returnedPixelRGB = new byte[3];

            for (int i = 0; i < bitmapImage.Height; i++)
            {
                for (int j = 0; j < bitmapImage.Width; j++)
                {
                    int currentX = j;
                    int currentY = i;
                    returnedPixelRGB = this.CalculateMedianFilterPixel(maskWidth, maskHeight, currentX, currentY);
                    modifiedRedValue[i, j] = returnedPixelRGB[0];
                    modifiedGreenValue[i, j] = returnedPixelRGB[1];
                    modifiedBlueValue[i, j] = returnedPixelRGB[2];
                }
            }

            int counter = 0;

            for (int i = 0; i < bitmapImage.Height; i++)
            {
                for (int j = 0; j < bitmapImage.Width; j++)
                {
                    r_array[counter] = modifiedRedValue[i, j];
                    g_array[counter] = modifiedGreenValue[i, j];
                    b_array[counter] = modifiedBlueValue[i, j];
                    counter++;
                }
            }

            this.SetColorArrays();
            this.CreateEditableImage(bitmapImage);
        }

        public byte[] CalculateMedianFilterPixel(int maskWidth, int maskHeight, int x, int y)
        {
            int xRange = (maskWidth - 1) / 2;
            int yRange = (maskHeight - 1) / 2;
            int currentX = 0;
            int currentY = 0;
            byte[] newRedArray = new byte[maskHeight * maskWidth];
            byte[] newGreenArray = new byte[maskHeight * maskWidth];
            byte[] newBlueArray = new byte[maskHeight * maskWidth];
            byte newRed = 0;
            byte newGreen = 0;
            byte newBlue = 0;
            int maskIteratorCounter = 0;

            for (int i = x - xRange; i <= x + xRange; i++)
            {
                for (int j = y - yRange; j <= y + yRange; j++)
                {

                    if (i < 0)
                    {
                        currentX = 0;
                    }
                    else if (i >= bitmapImage.Width)
                    {
                        currentX = bitmapImage.Width - 1;
                    }
                    else
                    {
                        currentX = i;
                    }

                    if (j < 0)
                    {
                        currentY = 0;
                    }
                    else if (j >= bitmapImage.Height)
                    {
                        currentY = bitmapImage.Height - 1;
                    }
                    else
                    {
                        currentY = j;
                    }

                    newRedArray[maskIteratorCounter] = r_array_index[currentY, currentX];
                    newGreenArray[maskIteratorCounter] = g_array_index[currentY, currentX];
                    newBlueArray[maskIteratorCounter] = b_array_index[currentY, currentX];
                    maskIteratorCounter++;
                }
            }

            Array.Sort(newRedArray);
            Array.Sort(newGreenArray);
            Array.Sort(newBlueArray);

            newRed = newRedArray[newRedArray.Length / 2];
            newGreen = newGreenArray[newGreenArray.Length / 2];
            newBlue = newBlueArray[newBlueArray.Length / 2];

            byte[] newPixelRGB = new byte[3];
            newPixelRGB[0] = (byte)newRed;
            newPixelRGB[1] = (byte)newGreen;
            newPixelRGB[2] = (byte)newBlue;

            return newPixelRGB;
        }

        public void CalculateKuwaharFilter(int maskWidth, int maskHeight)
        {
            byte[,] modifiedRedValue = new byte[bitmapImage.Height, bitmapImage.Width];
            byte[,] modifiedGreenValue = new byte[bitmapImage.Height, bitmapImage.Width];
            byte[,] modifiedBlueValue = new byte[bitmapImage.Height, bitmapImage.Width];
            byte[] returnedPixelRGB = new byte[3];

            for (int i = 0; i < bitmapImage.Height; i++)
            {
                for (int j = 0; j < bitmapImage.Width; j++)
                {
                    int currentX = j;
                    int currentY = i;
                    returnedPixelRGB = this.CalculateKuwaharFilterPixel(maskWidth, maskHeight, currentX, currentY);
                    modifiedRedValue[i, j] = returnedPixelRGB[0];
                    modifiedGreenValue[i, j] = returnedPixelRGB[1];
                    modifiedBlueValue[i, j] = returnedPixelRGB[2];
                }
            }

            int counter = 0;

            for (int i = 0; i < bitmapImage.Height; i++)
            {
                for (int j = 0; j < bitmapImage.Width; j++)
                {
                    r_array[counter] = modifiedRedValue[i, j];
                    g_array[counter] = modifiedGreenValue[i, j];
                    b_array[counter] = modifiedBlueValue[i, j];
                    counter++;
                }
            }

            this.SetColorArrays();
            this.CreateEditableImage(bitmapImage);
        }

        public byte[] CalculateKuwaharFilterPixel(int maskWidth, int maskHeight, int x, int y)
        {
            int xRange = (maskWidth - 1) / 2;
            int yRange = (maskHeight - 1) / 2;
            int currentX = 0;
            int currentY = 0;
            byte[] newRedArray = new byte[maskHeight * maskWidth];
            byte[] newGreenArray = new byte[maskHeight * maskWidth];
            byte[] newBlueArray = new byte[maskHeight * maskWidth];
            double[] sumRed = new double[4];
            double[] sumGreen = new double[4];
            double[] sumBlue = new double[4];

            double[,] variance = new double[4,3];
            int divider = ((maskWidth / 2) + 1) * (((maskHeight / 2) + 1));

            for (int i = x - xRange; i <= x; i++)
            {
                for (int j = y - yRange; j <= y; j++)
                {
                    currentX = this.findX(i);
                    currentY = this.findY(j);
                    sumRed[0] += r_array_index[currentY, currentX];
                    sumGreen[0] += g_array_index[currentY, currentX];
                    sumBlue[0] += b_array_index[currentY, currentX];
                }
            }

            sumRed[0] /= divider;
            sumGreen[0] /= divider;
            sumBlue[0] /= divider;

            for (int i = x - xRange; i <= x; i++)
            {
                for (int j = y - yRange; j <= y; j++)
                {
                    currentX = this.findX(i);
                    currentY = this.findY(j);
                    variance[0, 0] += (Math.Pow(sumRed[0] - r_array_index[currentY, currentX], 2));
                    variance[0, 1] += (Math.Pow(sumGreen[0] - g_array_index[currentY, currentX], 2));
                    variance[0, 2] += (Math.Pow(sumBlue[0] - b_array_index[currentY, currentX], 2));
                }
            }

            variance[0, 0] /= divider;
            variance[0, 1] /= divider;
            variance[0, 2] /= divider;

            for (int i = x - xRange; i <= x; i++)
            {
                for (int j = y + yRange; j >= y; j--)
                {
                    currentX = this.findX(i);
                    currentY = this.findY(j);
                    sumRed[1] += r_array_index[currentY, currentX];
                    sumGreen[1] += g_array_index[currentY, currentX];
                    sumBlue[1] += b_array_index[currentY, currentX];

                }
            }

            sumRed[1] /= divider;
            sumGreen[1] /= divider;
            sumBlue[1] /= divider;

            for (int i = x - xRange; i <= x; i++)
            {
                for (int j = y + yRange; j >= y; j--)
                {
                    currentX = this.findX(i);
                    currentY = this.findY(j);
                    variance[1, 0] += (Math.Pow(sumRed[1] - r_array_index[currentY, currentX], 2));
                    variance[1, 1] += (Math.Pow(sumGreen[1] - g_array_index[currentY, currentX], 2));
                    variance[1, 2] += (Math.Pow(sumBlue[1] - b_array_index[currentY, currentX], 2));
                }
            }

            variance[1, 0] /= divider;
            variance[1, 1] /= divider;
            variance[1, 2] /= divider;

            for (int i = x + xRange; i >= x; i--)
            {
                for (int j = y - yRange; j <= y; j++)
                {
                    currentX = this.findX(i);
                    currentY = this.findY(j);
                    sumRed[2] += r_array_index[currentY, currentX];
                    sumGreen[2] += g_array_index[currentY, currentX];
                    sumBlue[2] += b_array_index[currentY, currentX];
                }
            }

            sumRed[2] /= divider;
            sumGreen[2] /= divider;
            sumBlue[2] /= divider;

            for (int i = x + xRange; i >= x; i--)
            {
                for (int j = y - yRange; j <= y; j++)
                {
                    currentX = this.findX(i);
                    currentY = this.findY(j);
                    variance[2, 0] += (Math.Pow(sumRed[2] - r_array_index[currentY, currentX], 2));
                    variance[2, 1] += (Math.Pow(sumGreen[2] - g_array_index[currentY, currentX], 2));
                    variance[2, 2] += (Math.Pow(sumBlue[2] - b_array_index[currentY, currentX], 2));

                }
            }

            variance[2, 0] /= divider;
            variance[2, 1] /= divider;
            variance[2, 2] /= divider;

            for (int i = x + xRange; i >= x; i--)
            {
                for (int j = y + yRange; j >= y; j--)
                {
                    currentX = this.findX(i);
                    currentY = this.findY(j);
                    sumRed[3] += r_array_index[currentY, currentX];
                    sumGreen[3] += g_array_index[currentY, currentX];
                    sumBlue[3] += b_array_index[currentY, currentX];
                }
            }

            sumRed[3] /= divider;
            sumGreen[3] /= divider;
            sumBlue[3] /= divider;


            for (int i = x + xRange; i >= x; i--)
            {
                for (int j = y + yRange; j >= y; j--)
                {
                    currentX = this.findX(i);
                    currentY = this.findY(j);
                    variance[3, 0] += (Math.Pow(sumRed[3] - r_array_index[currentY, currentX], 2));
                    variance[3, 1] += (Math.Pow(sumGreen[3] - g_array_index[currentY, currentX], 2));
                    variance[3, 2] += (Math.Pow(sumBlue[3] - b_array_index[currentY, currentX], 2));

                }
            }

            variance[3, 0] /= divider;
            variance[3, 1] /= divider;
            variance[3, 2] /= divider;

            double minimumRedVariance = variance[0, 0];
            double minimumRedAverage = sumRed[0];

            double minimumGreenVariance = variance[0, 1];
            double minimumGreenAverage = sumGreen[0];

            double minimumBlueVariance = variance[0, 2];
            double minimumBlueAverage = sumBlue[0];
            

            for (int i = 0; i < 4; i++)
            {
                if (variance[i, 0] < minimumRedVariance)
                {
                    minimumRedVariance = variance[i, 0];
                    minimumRedAverage = sumRed[i];
                }
                if (variance[i, 1] < minimumGreenVariance)
                {
                    minimumGreenVariance = variance[i, 1];
                    minimumGreenAverage = sumGreen[i];
                }
                if (variance[i, 2] < minimumBlueVariance)
                {
                    minimumBlueVariance = variance[i, 2];
                    minimumBlueAverage = sumBlue[i];
                }
            }

            byte[] newPixelRGB = new byte[3];
            newPixelRGB[0] = (byte)minimumRedAverage;
            newPixelRGB[1] = (byte)minimumGreenAverage;
            newPixelRGB[2] = (byte)minimumBlueAverage;

            return newPixelRGB;
        }

        public int findX (int value)
        {
            if (value < 0)
            {
                return 0;
            }
            else if (value >= bitmapImage.Width)
            {
                return (bitmapImage.Width - 1);
            }
            else
            {
                return value;
            }
        }

        public int findY(int value)
        {
            if (value < 0)
            {
                return 0;
            }
            else if (value >= bitmapImage.Height)
            {
                return (bitmapImage.Height - 1);
            }
            else
            {
                return value;
            }
        }
    }
}
