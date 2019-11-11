using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleFlightCrawl
{
    class CCsv
    {
        public class Information
        {
            public string DatePosted { get; set; }
            public string Origin { get; set; }
            public string OriginAirport { get; set; }
            public string Destination { get; set; }
            public string DestinationAirport { get; set; }
            public string RegularPrice { get; set; }
            public string SalePrice { get; set; }
            public string DiscountRate { get; set; }
            public string FlightDate { get; set; }
            public string BookingLink { get; set; }
        }
        public class Airport_Link
        {
            public string Link { get; set; }
            public string Origin { get; set; }
            public string Airport { get; set; }
        }
        public class Flight_Detail
        {
            public string FlightDate { get; set; }
            public string DestAirport { get; set; }
            public string Price { get; set; }
            public string BookingLink { get; set; }
        }
        public List<T> ReadCsv<T>(string path)
        {
            List<T> list = new List<T>();
            try
            {
                using (var textReader = File.OpenText(path))
                {
                    var csv = new CsvReader(textReader);
                    while (csv.Read())
                    {
                        var record = csv.GetRecord<T>();
                        list.Add(record);
                    }
                    textReader.Close();
                }
            }
            catch (Exception)
            {
            }
            return list;
        }
        public void AppendCsv<T>(List<T> list, string path)
        {
            List<T> orig_list = new List<T>();
            orig_list.AddRange(ReadCsv<T>(path));
            orig_list.AddRange(list);
            SaveCsv<T>(orig_list, path);
        }
        public void SaveCsv<T>(List<T> list, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            using (CsvWriter cw = new CsvWriter(sw))
            {
                cw.WriteHeader<T>();
                cw.NextRecord();
                foreach (T item in list)
                {
                    cw.WriteRecord<T>(item);
                    cw.NextRecord();
                }
            }
        }
    }
}
