using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Json;

namespace DMM
{
    public class CardList
    {
        public string Name { get; }
        public string Text { get; }
        public string Id { get; }
        public string Date { get; }
        public bool isLocked { get; }
        public CardList(Card card, long id)
        {
            Name = card.Name;
            Text = card.Text;
            Id = id.ToString();
            Date = card.Date.ToString("f");
            isLocked = card.Lock;
        }

    }
    public partial class MainPage : ContentPage
    {
        Data data;
        List<CardList> list;
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
        protected override void OnAppearing()
        {
            data = Data.FromJson(Data.Load(Path.Combine(path, "Data.json")));
            list = new List<CardList>();
            foreach (var card in data.Cards)
            {
                list.Add(new CardList(card.Value, card.Key));
            }
            listView.ItemsSource = null;
            listView.ItemsSource = list;
        }
        private async void ToCreatePage(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new Create());
        }        
        private async void ToSettingsPage(object sender, EventArgs args)
        {
            //File.Delete(Path.Combine(path, "Data.json"));
            await Navigation.PushAsync(new Settings());
        }

        private async void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            int index = (listView.ItemsSource as List<CardList>).IndexOf(e.Item as CardList);
            await Navigation.PushAsync(new Template(data.Cards.ElementAt(index).Key));
        }

        private void SearchBarClick(object sender, EventArgs e)
        {
            string text = Search.Text.ToLower();
            IEnumerable<CardList> results = list.Where(name => name.Name.ToLower().Contains(text));
            listView.ItemsSource = results;
            if (string.IsNullOrWhiteSpace(text)) listView.ItemsSource = list;
        }
    }
}
