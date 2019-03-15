using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Microsoft.Office.Interop.Excel;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace DataLogic
{
    class MongoKVPair
    {
        [BsonId]
        [Key]
        public long QuestionNumber { get; set; }
        public string Fst { get; set; }
        public string Snd { get; set; }
        public override bool Equals(object obj)
        {
            return Fst.Equals((obj as MongoKVPair).Fst);
        }

        public override int GetHashCode()
        {
            return Fst.GetHashCode();
        }
    }
    public static class DataLogic
    {
        private static readonly IMongoDatabase _database;
        static DataLogic()
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            _database = client.GetDatabase("BotData");
        }
        private static IEnumerable<IEnumerable<string>> GetTable()
        {
            var webGet = new HtmlWeb();
            webGet.OverrideEncoding = Encoding.UTF8;
            var count = 0;
            var doc = webGet.Load("https://baza-otvetov.ru/categories/view/17");
            var returning = new List<IEnumerable<string>>();
            for (int i = 0; i < 45; ++i)
            {
                returning.AddRange(
                    doc.DocumentNode.SelectSingleNode(
                            "//table[@class='q-list__table']").Descendants("tr")
                        .Skip(1)
                        .Where(x => x.Elements("td").Count() > 1)
                        .Select(x => x.Elements("td")
                            .Select(td => td.InnerText.Trim()))
                );
                count += 10;
                doc = webGet.Load("https://baza-otvetov.ru/categories/view/17/" + count);
            }

            return returning;
        }

        private static void Export(IEnumerable<MongoKVPair> input, string adress)
        {
            _database.DropCollection(adress);
            _database.GetCollection<MongoKVPair>(adress).InsertMany(input);
        }

        private static MongoKVPair Map(IEnumerable<string> input)
        {
            var arr = input.ToArray();
            return new MongoKVPair()
            {
                Fst=arr[1],
                Snd=arr[2],
                QuestionNumber = Convert.ToInt64(arr[0])
            };
        }

        private static IEnumerable<MongoKVPair> Import(string adress)
        {
            return _database.GetCollection<MongoKVPair>(adress).AsQueryable().AsEnumerable();
        }

        public static string ClearString(string input)
        {
            using (var fs = new FileStream("input.txt", FileMode.OpenOrCreate))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(input);
                }
            }

            Process mystem = new Process();
            mystem.StartInfo.FileName = "mystem.exe";
            mystem.StartInfo.Arguments = "-l input.txt output.txt";
            mystem.StartInfo.UseShellExecute = false;
            mystem.StartInfo.RedirectStandardInput = true;
            mystem.StartInfo.RedirectStandardOutput = true;

            mystem.Start();
            mystem.WaitForExit();
            mystem.Close();

            using (var fs = new FileStream("output.txt", FileMode.Open))
            {
                using (var sw = new StreamReader(fs))
                {
                    return (string.Concat(sw.ReadToEnd().Split('{')).Replace('}', ' ').Trim());
                }
            }

        }

        public static void GetAndExportData()
        {
            var data = GetTable();
            
            Export(data.Select(Map), "rawData");
        }

        public static void ClearAndExportData()
        {
            var data = Import("rawData");
            Export(data.Select(x=>new MongoKVPair()
            {
                QuestionNumber =  x.QuestionNumber,
                Fst=ClearString(x.Fst),
                Snd = x.Snd
            }), "clearData");
        }

        public static IEnumerable<KeyValuePair<string, string>> GetClearData()
        {
            return Import("clearData").Select(x=>new KeyValuePair<string, string>(x.Fst, x.Snd));
        }
    }
}
