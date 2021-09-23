using System.Collections.Generic;
using System.IO;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DMM
{
    public partial class App : Application
    {
        public App()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //File.Delete(Path.Combine(path, "Data.json"));
            if (!File.Exists(Path.Combine(path, "Data.json")))
            {
                var clear_data = new Json.Data
                {
                    Cards = new Dictionary<long, Json.Card>(1)
                    {
                        [0] = new Json.Card() { Name = "Первая идея", 
                            Text = "Вот и ваша первая запись!", 
                            Diff = 0, 
                            Lock = false, 
                            Tags = new List<string>() { "Self" },
                            ConnectedIdeas = new List<long>()
                        }
                    },
                    Tags = new List<string>() { "Self", "Idea" }
                };
                Json.Data.Save(clear_data, Path.Combine(path, "Data.json"));
            }
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
