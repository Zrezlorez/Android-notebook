using System.IO;
using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Json
{
    // файл с помощью которого работаю с json, тут всё очевидно
    public partial class Data
    {
        public static void Save(Data text, string path)
        {
            using (StreamWriter write = new StreamWriter(path, false, System.Text.Encoding.UTF8)) write.WriteLine(JsonConvert.SerializeObject(text));
        }
        public static string Load(string path)
        {
            using (StreamReader read = new StreamReader(path, System.Text.Encoding.UTF8)) return read.ReadToEnd();
        }

        [JsonProperty("cards")]
        public Dictionary<long, Card> Cards { get; set; }        
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
    }

    public partial class Card
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("diff")]
        public long Diff { get; set; }   
        
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }        
        [JsonProperty("ideas")]
        public List<long> ConnectedIdeas { get; set; }

        [JsonProperty("Lock")]
        public bool Lock { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }
    }

    public partial class Data
    {
        public static Data FromJson(string json) => JsonConvert.DeserializeObject<Data>(json, Json.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Data self) => JsonConvert.SerializeObject(self, Json.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
