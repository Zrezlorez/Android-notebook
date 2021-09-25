using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DMM
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IdeaManager : ContentPage
    {
        Data data;
        Card card;
        Random rnd = new Random();
        List<string> tagList;
        List<long> connectedList;
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        bool isClose = false;
        long diff = 0;
        long key;
        public IdeaManager(long key)
        {
            data = Data.FromJson(Data.Load(Path.Combine(path, "Data.json")));
            this.key = key;

            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            TapGestureRecognizer close_tap = new TapGestureRecognizer();
            if (key < 0)
            {
                // переключение доступности идеи
                close_tap.Tapped += (s, e) =>
                {
                    isClose = !isClose;
                    Close.Source = isClose ? "locked.png" : "opened.png";
                };
                InitializeTaps();
                delete.Source = "";
                Date.Text = "";
            }
            else
            {
                card = data.Cards[key];
                if (card.Lock) LoadLock();
                else
                {
                    Name.Text = card.Name;
                    Text.Text = card.Text;
                    close_tap.Tapped += (s, e) =>
                    {
                        LoadLock();
                    };
                }
                InitializeTaps();

                Date.Text = card.Date.ToString("f");

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

            }
            Close.GestureRecognizers.Add(close_tap);
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


        void InitializeTaps()
        {
            // + в разделе тегов
            TapGestureRecognizer tag_tap = new TapGestureRecognizer();
            tag_tap.Tapped += (s, e) =>
            {
                Navigation.PushAsync(new Tags(key));
            };
            plus.GestureRecognizers.Add(tag_tap);
            // + в разделе соеденённые
            TapGestureRecognizer connect_tap = new TapGestureRecognizer();
            connect_tap.Tapped += (s, e) =>
            {
                Navigation.PushAsync(new Connected(key));
            };
            connectPlus.GestureRecognizers.Add(connect_tap);
            //обработка нажатия на звезды
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

        private async void Back(object sender, EventArgs e)
        {
            bool backcheck;
            if (key >= 0) backcheck = (Name.Text != data.Cards[key].Name || Text.Text != data.Cards[key].Text);
            else backcheck = (!string.IsNullOrWhiteSpace(Name.Text) || !string.IsNullOrWhiteSpace(Text.Text));


            if (backcheck)
            {
                bool results = await DisplayAlert("Стоять!", "Если ты выйдешь, то все изменения не сохранятся!",
                    "Ну и ладно", "Я остаюсь");
                if (results)
                {
                    await Navigation.PopAsync();
                }
            }
            else await Navigation.PopAsync();
        }

        // методы template
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
            Close.Source = "locked.png";
            tagStack.Children.RemoveAt(2);
            for (int z = 1; z < 6; z++) important.Children[z].GestureRecognizers.Clear();
        }
        private async void DeleteCard(object sender, EventArgs args)
        {
            bool result = await DisplayAlert("Подтвердите действие",
                "Если вы удалите идею, то она исчезнет навсегда!",
                "Ладно", "Отмена!!!");
            if (result)
            {
                data = Data.FromJson(Data.Load(Path.Combine(path, "Data.json")));
                data.Cards.Remove(key);
                Data.Save(data, Path.Combine(path, "Data.json"));
                await Navigation.PopAsync();
            }
        }

        // методы create
        long GetID()
        {
            // если никакого нет либо введён неправильно то беру рандом
            long id_ = rnd.Next(0, 999999);
            while (data.Cards.ContainsKey(id_)) id_ = rnd.Next(0, 999999);
            return id_;
        }

        // общий метод создания
        private async void SaveCard(object sender, EventArgs e)
        {
            data = Data.FromJson(Data.Load(Path.Combine(path, "Data.json")));
            // сохранение
            if (key >= 0)
            {
                card = data.Cards[key];
                card.Name = Name.Text;
                card.Text = Text.Text;
                card.Diff = diff;
                card.Tags = tagList;
                card.ConnectedIdeas = connectedList;
            }
            //создание
            else
            {
                Card new_card = new Card()
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
            }
            Data.Save(data, Path.Combine(path, "Data.json"));
            await Navigation.PopAsync();
        }
    }
}