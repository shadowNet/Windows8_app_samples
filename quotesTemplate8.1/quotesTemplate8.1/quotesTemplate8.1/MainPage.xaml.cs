using quotesTemplate81.DataModel;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace quotesTemplate81
{
    public sealed partial class MainPage : Page
    {
        //Holds all the quotes
        QuotesObject quotes;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Loads the quotes
            quotes = new QuotesDataSource().Quotes;
            //Binds the quotes to the UI
            BindCurrentQoute();
            //Shares the quote
            RegisterForShare();
        }

        //Binds the current quote
        private void BindCurrentQoute()
        {
            //Gets the current quote
            QuoteObject quote = quotes.CurrentQuote;

            //Binds the images and text
            BackgroundImage.ImageSource = new BitmapImage(quote.BackgroundUri);
            ForegroundImage.ImageSource = new BitmapImage(quote.ForegroundUri);
            QuotesTextBlock.Text = quote.Quote;
        }

        //Allows for sharing
        private void RegisterForShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                DataRequestedEventArgs>(this.ShareImageHandler);
        }

        //Builds the data to be shared
        private async void ShareImageHandler(DataTransferManager sender,
            DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            //Sets the title of the share request
            request.Data.Properties.Title = "Quote from QuotesTemplate";
            //Sets the text of whats actually being shared
            request.Data.SetText(quotes.CurrentQuote.Quote + "\n\n~" + quotes.CurrentQuote.Name + "\n");

            // Because we are making async calls in the DataRequested event handler,
            //  we need to get the deferral first.
            DataRequestDeferral deferral = request.GetDeferral();

            // Make sure we always call Complete on the deferral.
            try
            {
                //Renders the QuoteGrid as a bitmap
                var bmp = new RenderTargetBitmap();
                await bmp.RenderAsync(QuoteGrid);

                //Gets the pixels
                var pixelBuffer = await bmp.GetPixelsAsync();

                var reader = DataReader.FromBuffer(pixelBuffer);
                var bytes = new byte[pixelBuffer.Length];
                reader.ReadBytes(bytes);

                //Creates the image
                var randomAccessStream = new InMemoryRandomAccessStream();
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, randomAccessStream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, 96, 96, bytes);
                await encoder.FlushAsync();
                await randomAccessStream.FlushAsync();
                         
                //Sets the image of the share request
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromStream(randomAccessStream));
            }
            finally
            {
                deferral.Complete();
            }
        }

        //Goes to the previous quote
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            quotes.PrevQuote();
            BindCurrentQoute();
        }

        //Goes to the next quote
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            quotes.NextQuote();
            BindCurrentQoute();
        }

        //Reads keyboard input
        private void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Left || e.Key == VirtualKey.A)
                PrevButton_Click(this, new RoutedEventArgs());
            else if (e.Key == VirtualKey.Right || e.Key == VirtualKey.D)
                NextButton_Click(this, new RoutedEventArgs());
        }

        //Reads swipe input
        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.Velocities.Linear.X > 0)
                PrevButton_Click(this, new RoutedEventArgs());
            else if (e.Velocities.Linear.X < 0)
                NextButton_Click(this, new RoutedEventArgs());
        }

        //Shares the content
        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }
    }
}
