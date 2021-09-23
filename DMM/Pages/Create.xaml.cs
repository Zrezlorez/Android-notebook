using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace DMM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Create : ContentPage
    {
        Json.Data data;
        Json.Card new_card;
        Random rnd = new Random();
        List<string> tagList;
        List<long> connectedList;
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        bool isClose = false;
        long diff = 0;
        public Create()
        {

            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            //кнопка закрытия
            TapGestureRecognizer close_tap = new TapGestureRecognizer();
            close_tap.Tapped += (s, e) =>
            {
                isClose = !isClose;
                Close.Source = isClose ? "locked.png" : "opened.png";
            };
            Close.GestureRecognizers.Add(close_tap);


            // нажатие на звезду
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

            // кнопка для картинки + в разделе тегов
            TapGestureRecognizer tag_tap = new TapGestureRecognizer();
            tag_tap.Tapped += (s, e) =>
            {
                Navigation.PushPopupAsync(new Tags(-1));
            };
            plus.GestureRecognizers.Add(tag_tap);

            // + в разделе соеденённые
            TapGestureRecognizer connect_tap = new TapGestureRecognizer();
            connect_tap.Tapped += (s, e) =>
            {
                Navigation.PushPopupAsync(new Connected(-1));
            };
            connectPlus.GestureRecognizers.Add(connect_tap);






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
        private async void CreateCard(object sender, EventArgs e)
        {
            data = Json.Data.FromJson(Json.Data.Load(Path.Combine(path, "Data.json")));
            new_card = new Json.Card()
            {
                Name = (Name.Text != null) ? Name.Text : " ",
                Text = (Text.Text != null) ? Text.Text : " ",
                Diff = diff,
                Date = DateTime.Now,
                Lock = isClose,
                Tags = (tagList != null) ? tagList : new List<string>(),
                ConnectedIdeas = (connectedList != null) ? connectedList : new List<long>()
            };
            data.Cards.Add(GetID(), new_card);
            Json.Data.Save(data, Path.Combine(path, "Data.json"));
            await Navigation.PopAsync();
        }
        // проверка на айди
        long GetID()
        {
            if (!string.IsNullOrWhiteSpace(Id.Text) && !long.TryParse(Id.Text, out long id)
                && !data.Cards.ContainsKey(id) && id < 0) return id;

            // если никакого нет либо введён неправильно то беру рандом
            long id_ = rnd.Next(0, 999999);
            while (data.Cards.ContainsKey(id_)) id_ = rnd.Next(0, 999999);
            return id_;
        }

        //кнопка назад
        private async void Back(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(Name.Text) || !string.IsNullOrWhiteSpace(Text.Text))
            {
                bool results = await DisplayAlert("Стоять!", "Если ты выйдешь, то идея не сохранится!", 
                    "Ну и ладно", "Я остаюсь");
                if (results)
                {
                    await Navigation.PopAsync();
                }
            }
            else await Navigation.PopAsync();
        }
    }
}