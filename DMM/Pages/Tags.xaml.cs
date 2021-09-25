using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;

namespace DMM
{
    //класс для отображения тегов и присоеденённых идей
    public class Model : INotifyPropertyChanged
    {
        // хз зачем это надо но оно надо
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Model()
        {
            IsChecked = false;
        }

        public string Text { get; set; }
        public long Id { get; set; }
        public Color Color { get; set; }

        private bool isChecked;
        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                NotifyPropertyChanged();
            }
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Tags : ContentPage
    {
        Json.Data data;
        List<Model> list = new List<Model>();
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        long key;
        public Tags(long key)
        {
            InitializeComponent();
            data = Json.Data.FromJson(Json.Data.Load(Path.Combine(path, "Data.json")));
            this.key = key;
            // создаю список из тегов
            if (data.Tags != null) 
                foreach (string element in data.Tags) 
                    list.Add(new Model() { Text = element, IsChecked = isHave(element) });
            listView.ItemsSource = list;
        }

        private async void Done(object sender, EventArgs e)
        {
            await Navigation.PopAsync();

            var result = list.Where(w => w.IsChecked == true).ToList();
            // отправляю список из тегов с галочкой
            MessagingCenter.Send<object, List<Model>>(this, "tag", result);
        }

        private async void CreateTag(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Добавление тега", "Введите названия нового тега");
            if (!string.IsNullOrWhiteSpace(result))
            {
                if (data.Tags.Contains(result)) await DisplayAlert("Упс...", "Такой тег уже существует!", "Ладно");
                else
                {
                    data.Tags.Add(result);
                    list.Add(new Model() { Text = result });
                    listView.ItemsSource = null;
                    listView.ItemsSource = list;
                    Json.Data.Save(data, Path.Combine(path, "Data.json"));
                }
            }
        }        
        private void DeleteTags(object sender, EventArgs e)
        {
            var result = list.Where(w => w.IsChecked == true).ToList();
            foreach(var model in result)
            {
                data.Tags.Remove(model.Text);
                list.Remove(model);
            }
            listView.ItemsSource = null;
            listView.ItemsSource = list;
            Json.Data.Save(data, Path.Combine(path, "Data.json"));
        }

        bool isHave(string tag)
        {
            if (key>=0 && data.Cards[key].Tags.Contains(tag)) return true;
            return false;
        }
    }
}