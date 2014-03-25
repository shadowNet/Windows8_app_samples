using quotesTemplate81.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace quotesTemplate81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        QuotesObject quotes;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            quotes = new QuotesDataSource().Quotes;
            BindCurrentQoute();
            RegisterForShare();
        }

        private void BindCurrentQoute()
        {
            QuoteObject quote = quotes.CurrentQuote;

            BackgroundImage.ImageSource = new BitmapImage(quote.BackgroundUri);
            ForegroundImage.ImageSource = new BitmapImage(quote.ForegroundUri);
            QuotesTextBlock.Text = quote.Quote;
        }

        private void RegisterForShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                DataRequestedEventArgs>(this.ShareImageHandler);
        }

        private async void ShareImageHandler(DataTransferManager sender,
            DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            request.Data.Properties.Title = "Quote from QuotesTemplate";
            request.Data.SetText(quotes.CurrentQuote.Quote + "\n\n~" + quotes.CurrentQuote.Name + "\n");

            // Because we are making async calls in the DataRequested event handler,
            //  we need to get the deferral first.
            DataRequestDeferral deferral = request.GetDeferral();

            // Make sure we always call Complete on the deferral.
            try
            {
                var bmp = new RenderTargetBitmap();
                await bmp.RenderAsync(QuoteGrid);

                var pixelBuffer = await bmp.GetPixelsAsync();

                var reader = DataReader.FromBuffer(pixelBuffer);
                var bytes = new byte[pixelBuffer.Length];
                reader.ReadBytes(bytes);

                var randomAccessStream = new InMemoryRandomAccessStream();
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, randomAccessStream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, 96, 96, bytes);
                await encoder.FlushAsync();
                await randomAccessStream.FlushAsync();
                         
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromStream(randomAccessStream));
            }
            finally
            {
                deferral.Complete();
            }
        }

        async Task SaveGridToFile(Grid grid, StorageFile file)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(grid, (int)grid.Width, (int)grid.Height);
            var pixels = await renderTargetBitmap.GetPixelsAsync();

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await
                    BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                byte[] bytes = pixels.ToArray();
                encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                     BitmapAlphaMode.Ignore,
                                     (uint)grid.Width, (uint)grid.Height,
                                     96, 96, bytes);

                await encoder.FlushAsync();
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            quotes.PrevQuote();
            BindCurrentQoute();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            quotes.NextQuote();
            BindCurrentQoute();
        }

        private void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Left)
                PrevButton_Click(this, new RoutedEventArgs());
            else if (e.Key == VirtualKey.Right)
                NextButton_Click(this, new RoutedEventArgs());
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.Velocities.Linear.X > 0)
                PrevButton_Click(this, new RoutedEventArgs());
            else if (e.Velocities.Linear.X < 0)
                NextButton_Click(this, new RoutedEventArgs());
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }
    }
}
