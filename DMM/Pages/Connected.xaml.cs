using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Json;

namespace DMM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Connected : ContentPage
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        long key;
        List<Model> list = new List<Model>();
        Data data;
        public Connected(long key)
        {
            this.key = key;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            data = Data.FromJson(Data.Load(Path.Combine(path, "Data.json")));
            // добавляю в список все названия идей кроме активной
            list.AddRange(from card in data.Cards
                          where key != card.Key
                          select new Model()
                          {
                              Text = card.Value.Name,
                              IsChecked = isConnect(key, card.Key),
                              Color = isRed(key),
                              Id = card.Key
                          });
            listView.ItemsSource = list;
        }
        bool isConnect(long key, long name)
        {
            // присоеденена ли идея уже
            if (key >= 0)
            {
                Card card = data.Cards[key];
                if (card.ConnectedIdeas != null && card.ConnectedIdeas.Contains(name))
                    return true;
            }
            return false;

        }
        Color isRed(long key)
        {
            // если идея источник то возвращается красный цвет
            foreach (var cards in data.Cards)
                if(cards.Key!=key)
                    if(cards.Value.ConnectedIdeas.Contains(key)) return Color.Red;
            return Color.Default;
        }
        private async void Done(object sender, EventArgs e)
        {
            await Navigation.PopAsync();

            var result = list.Where(w => w.IsChecked == true).ToList();
            // отправляю список выбранных идей
            MessagingCenter.Send<object, List<Model>>(this, "connected", result);
        }

        private void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            int index = (listView.ItemsSource as List<Model>).IndexOf(e.Item as Model);
            list.ElementAt(index).IsChecked = !list.ElementAt(index).IsChecked;
            listView.ItemsSource = list;
        }
    }
}