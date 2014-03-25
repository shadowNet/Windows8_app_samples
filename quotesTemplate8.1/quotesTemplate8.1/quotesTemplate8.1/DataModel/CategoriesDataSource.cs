using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace quotesTemplate81.DataModel
{
    /// <summary>
    /// Stores all the quotes
    /// </summary>
    public class QuotesObject
    {
        public List<QuoteObject> Quotes { get; set; }
        private int _currentIndex { get; set; }
        public QuoteObject CurrentQuote { get { return Quotes[_currentIndex]; } }

        internal void NextQuote()
        {
            _currentIndex++;
            if (_currentIndex >= Quotes.Count)
                _currentIndex = 0;
        }

        internal void PrevQuote()
        {
            _currentIndex--;
            if (_currentIndex < 0)
                _currentIndex = Quotes.Count - 1;
        }
    }

    /// <summary>
    /// Stores the name and items of the quote
    /// </summary>
    public class QuoteObject
    {
        public string Name  { get; set; }
        public string Quote { get; set; }
        public string BackgroundPath { get; set; }
        public string ForegroundPath { get; set; }

        [JsonIgnore]
        public Uri BackgroundUri { get { return new Uri("ms-appx:///DataModel/Images/" + BackgroundPath); } }
        [JsonIgnore]
        public Uri ForegroundUri { get { return new Uri("ms-appx:///DataModel/Images/" + ForegroundPath); } }
    }

    /// <summary>
    /// Parses the JSON data into 
    /// </summary>
    public sealed class QuotesDataSource
    {
        private static QuotesDataSource quotesDataSource = new QuotesDataSource();
        private QuotesObject quotes = new QuotesObject();
        public QuotesObject Quotes
        {
            get { return quotesDataSource.quotes; }
        }

        public static async Task LoadQuotes()
        {
            await quotesDataSource.LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            //Checks to see if the quotes were already loaded
            if (quotes.Quotes != null && quotes.Quotes.Count != 0)
                return;

            //Loads the document
            Uri dataUri = new Uri("ms-appx:///DataModel/Quotes.json");

            //Deserializes it with JSON.NET
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);
            List<QuoteObject> jsonQuotes = JsonConvert.DeserializeObject<List<QuoteObject>>(jsonText);

            quotes = new QuotesObject();
            quotes.Quotes = jsonQuotes;
        }
    }
}