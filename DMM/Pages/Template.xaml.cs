using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DMM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Template : ContentPage
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        long key, diff;
        List<string> tagList;
        List<long> connectedList;
        Json.Data data;
        Json.Card card;
        public Template(long key)
        {

            data = Json.Data.FromJson(Json.Data.Load(Path.Combine(path, "Data.json")));
            this.key = key;
            card = data.Cards[key];
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            // проверка открыта/закрыта карта
            if (card.Lock) LoadLock();
            else
            {
                Name.Text = card.Name;
                Text.Text = card.Text;
                //кнопка для картинки + в разделе тегов
                TapGestureRecognizer tag_tap = new TapGestureRecognizer();
                tag_tap.Tapped += async (s, e) =>
                {
                    await Navigation.PushAsync(new Tags(key));
                };
                plus.GestureRecognizers.Add(tag_tap);

                // нажатие на звзеду
                TapGestureRecognizer star_tap = new TapGestureRecognizer();
                star_tap.Tapped += (s, e) =>
                {
                    var view_element = (View)s;
                    diff = important.Children.IndexOf(view_element);
                    for (int x = 1; x < 6; x++)
                    {
                        var y = (Image)important.Children[x];

                        if (x > diff) y.Source = "gray_star.png";
                        else y.Source = "star.png";
                    }
                };
                for (int z = 1; z < 6; z++) important.Children[z].GestureRecognizers.Add(star_tap);
            }
            // + в разделе соеденённые
            TapGestureRecognizer connect_tap = new TapGestureRecognizer();
            connect_tap.Tapped += async (s, e) =>
            {
                await Navigation.PushAsync(new Connected(key));
            };
            connectPlus.GestureRecognizers.Add(connect_tap);

            // строка из тегов
            string tags = "";
            if (card.Tags != null) 
                foreach (string i in card.Tags) 
                    tags += i + ", ";
            if (!string.IsNullOrWhiteSpace(tags)) tags = tags.Remove(tags.Length - 2);
            tagLabel.Text = tags;

            // строка из присоеденённых
            string connected = "";
            if (card.ConnectedIdeas != null) 
                foreach (long i in card.ConnectedIdeas)
                    connected += data.Cards[i].Name + ", ";
            if (!string.IsNullOrWhiteSpace(connected)) connected = connected.Remove(connected.Length - 2);
            connectedLabel.Text = connected;


            Date.Text = card.Date.ToString("f");

            // отображение важности
            for (int i = 1; i < card.Diff+1; i++)
            {
                var image = (Image)important.Children[i];
                image.Source = "star.png";
            }
        }

        protected override void OnAppearing()
        {
            // принимаю и обрабатываю список из tag.xaml.cs
            MessagingCenter.Subscribe<object, List<Model>>(this, "tag", (obj, result) => {

                string tagString = "";
                List<string> tagSList = new List<string>();
                foreach (var model in result)
                {
                    tagString += model.Text + ", ";
                    tagSList.Add(model.Text);
                }
                if (!string.IsNullOrWhiteSpace(tagString)) tagString = tagString.Remove(tagString.Length - 2);
                tagList = tagSList;
                tagLabel.Text = tagString;
            });
            // принимаю и обрабатываю список из connected.xaml.cs
            MessagingCenter.Subscribe<object, List<Model>>(this, "connected", (obj, result) => {
                string connectString = "";
                List<long> connectSList = new List<long>();
                foreach (var model in result)
                {
                    connectString += model.Text + ", ";
                    connectSList.Add(model.Id);
                }
                if (!string.IsNullOrWhiteSpace(connectString)) connectString = connectString.Remove(connectString.Length - 2);
                connectedList = connectSList;
                connectedLabel.Text = connectString;
            });
        }
        private async void DeleteCard(object sender, EventArgs args)
        {
            bool result = await DisplayAlert("Подтвердите действие", 
                "Если вы удалите идею, то она исчезнет навсегда!", 
                "Ладно", "Отмена!!!");
            if (result)
            {
                data = Json.Data.FromJson(Json.Data.Load(Path.Combine(path, "Data.json")));
                data.Cards.Remove(key);
                Json.Data.Save(data, Path.Combine(path, "Data.json"));
                await Navigation.PopAsync();
            }
        }
        private async void SaveCard(object sender, EventArgs args)
        {
            data = Json.Data.FromJson(Json.Data.Load(Path.Combine(path, "Data.json")));
            card.Name = Name.Text;
            card.Text = Text.Text;
            card.Diff = diff;
            card.Tags = tagList;
            card.ConnectedIdeas = connectedList;
            Json.Data.Save(data, Path.Combine(path, "Data.json"));
            await Navigation.PopAsync();
        }
        private async void LockCard(object sender, EventArgs args)
        {
            if (!card.Lock)
            {
                bool result = await DisplayAlert("Подтвердите действие", "Если вы закроете идею, то вы больше не сможете её редактировать", "Закрыть", "Отмена");
                if (result)
                {
                    card.Lock = true;
                    LoadLock();
                }
            }
            else await DisplayAlert("Ошибка!", "Идея уже закрыта", "Ладно");
        }
        private async void Back(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        //функция полностью работает с интерфейсом. По названию всё понятно
        void LoadLock()
        {
            NameStack.Children.RemoveAt(0);
            NameStack.Children.Insert(0, new Label()
            {
                Text = card.Name,
                WidthRequest = 200,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold
            });
            TextFrame.Content = new Label()
            {
                Text = card.Text,
                HeightRequest = 185,
                FontSize = 14
            };
            Open.Source = "locked.png";
            tagStack.Children.RemoveAt(2);
            for (int z = 1; z < 6; z++) important.Children[z].GestureRecognizers.Clear();
        }


    }
}