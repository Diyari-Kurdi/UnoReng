using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace UnoReng
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ColorModel> _topColors = new();

        public MainViewModel()
        {
            TopColors.CollectionChanged += (sender, e) =>
            {
                OnPropertyChanged(nameof(TopColors));
            };
        }

        [ObservableProperty]
        private ColorModel _selectedColor = new(Color.FromArgb(73, 18, 73));

        [ObservableProperty]
        private ImageSource _imageSource = new BitmapImage(new Uri("ms-appx:///UnoReng/Assets/image.jpg"));

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GoCommand))]
        private string _imageURL;

        public async Task GetColors(Image image)
        {
            TopColors.Clear();
            await Task.Delay(500);
            //RenderTargetBitmap is not supported on WASM
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(image);

            Dictionary<Color, int> colorFrequency = new();

            double colorDistanceThreshold = 50;

            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();

            // Loop through each pixel in the image
            for (int i = 0; i < pixels.Length; i += 4)
            {
                Color pixelColor = Color.FromArgb(255, pixels[i + 2], pixels[i + 1], pixels[i]);

                // Check if the color is already in the dictionary or a similar color exists
                bool colorFound = false;
                foreach (Color color in colorFrequency.Keys)
                {
                    double colorDistance = ColorDistance(pixelColor, color);

                    if (colorDistance < colorDistanceThreshold)
                    {
                        colorFrequency[color]++;
                        colorFound = true;
                        break;
                    }
                }

                if (!colorFound)
                {
                    colorFrequency.Add(pixelColor, 1);
                }
            }
            int numColors = 20;

            var Colors = colorFrequency.OrderByDescending(c => c.Value).Take(numColors).Select(c => c.Key);
            foreach (Color color in Colors)
            {
                TopColors.Add(new(color));
            }
            //------------------------------------------------------------------------------------------------------
            //------------------------------------ Euclidean distance formula  -------------------------------------
            //------------------------------------------------------------------------------------------------------
            //----------------------------- sqrt((r₁ - r₂)² + (g₁ - g₂)² + (b₁ - b₂)²) -----------------------------
            //------------------------------------------------------------------------------------------------------
            static double ColorDistance(Color color1, Color color2)
            {
                double redDistance = color1.R - color2.R;
                double greenDistance = color1.G - color2.G;
                double blueDistance = color1.B - color2.B;

                //------------------------------------------------------------------------------------------------------
                //--------- sqrt --------(r₁ - r₂)²------- + ----------(g₁ - g₂)²--------- + ---------(b₁ - b₂)²--------
                //------------------------------------------------------------------------------------------------------
                return Math.Sqrt(redDistance * redDistance + greenDistance * greenDistance + blueDistance * blueDistance);
            }
        }


        [RelayCommand]
        private async void OpenImage(Image image)
        {
            var filePicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            filePicker.FileTypeFilter.Add(".png");
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            StorageFile PickedFile = await filePicker.PickSingleFileAsync();
            if (PickedFile != null)
            {
                using var stream = await PickedFile.OpenAsync(FileAccessMode.Read);
                var bitmap = new BitmapImage();
                bitmap.SetSource(stream);
                ImageSource = bitmap;
                await GetColors(image);
            }
        }

        private bool CanGo() => !string.IsNullOrEmpty(ImageURL);
        [RelayCommand(CanExecute = nameof(CanGo))]
        private async Task Go(Image image)
        {
            if (Uri.IsWellFormedUriString(ImageURL, UriKind.Absolute))
            {
                ImageSource = new BitmapImage(new(ImageURL));
                await GetColors(image);
            }
        }

        [RelayCommand]
        private void CopyHexToClipboard()
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(SelectedColor.HEX);
            Clipboard.SetContent(dataPackage);
        }
        [RelayCommand]
        private void CopyRgbToClipboard()
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(SelectedColor.RGB);
            Clipboard.SetContent(dataPackage);
        }
    }
}
