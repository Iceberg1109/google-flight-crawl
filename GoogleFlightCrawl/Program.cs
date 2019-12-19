using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GoogleFlightCrawl.CCsv;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Diagnostics;

namespace GoogleFlightCrawl
{
    class Program
    {
        static private int counter = 0;
        static public string FILEPATH;
        static public int thread_cnt = 0;
        static public string cur_airport = "";
        static public int THREAD_COUNT = 4;
        static void Main(string[] args)
        {
            FILEPATH = "C:/results/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".csv";

            Console.Write("\rStarting.");
            for (int i = 0; i < THREAD_COUNT; i++)
            {
                var process = new Thread(() => search_start(i));
                process.Start();
                Thread.Sleep(1000);
            }

            while (true)
            {
                if (thread_cnt == THREAD_COUNT)
                {
                    email_send();
                    break;
                }
                consoleSpiner();
                Thread.Sleep(2000);
            }
            Console.Write("\nDone ...");

        }
        public static void search_start(int airport_idx)
        {
            try
            {
                var airports = new CCsv().ReadCsv<Airport_Link>("Links.csv");
                List<Information> m_results = new List<Information>();
                CWebBrowser wb = new CWebBrowser();
                IWebDriver SearchDriver = wb.GoogleChrome();
                SearchDriver.Navigate().GoToUrl("https://www.google.com/ncr");
                Thread.Sleep(1000);

                for (int i = airport_idx; i < airports.Count; i = i + THREAD_COUNT)
                {
                    cur_airport = airports[i].Origin;
                    try
                    {
                        SearchDriver.Navigate().GoToUrl(airports[i].Link);
                        Thread.Sleep(5000);
                        var collection = SearchDriver.FindElements(By.XPath("//span[text() = 'Great value']"));
                        string current = SearchDriver.Url;
                        for (int index = 1; index <= collection.Count; index++)
                        {
                            //while (true)
                            {
                                try
                                {
                                    var discountprice = 0;
                                    var saleprice = 0;
                                    //while (true)
                                    {
                                        try
                                        {
                                            string path = "(//span[text() = 'Great value'])[" + index + "]//..//..//div";
                                            discountprice = int.Parse(SearchDriver.FindElement(By.XPath(path)).Text.Split(' ')[0].Split('$')[1], NumberStyles.AllowThousands);
                                            saleprice = int.Parse(SearchDriver.FindElements(By.XPath("(//span[text() = 'Great value'])[" + index + "]//..//span"))[1].Text.Replace("$", ""), NumberStyles.AllowThousands);
                                        }
                                        catch (Exception ex)
                                        {
                                            
                                        }
                                    }
                                    Information information = new Information();
                                    information.Origin = airports[i].Origin;
                                    information.OriginAirport = airports[i].Airport;
                                    information.SalePrice = saleprice.ToString();
                                    information.RegularPrice = (saleprice + discountprice).ToString();
                                    information.DiscountRate = ((float)saleprice * 100 / (saleprice + discountprice)).ToString("n2");
                                    // click one link
                                    int attempt = 0;
                                    while (attempt < 3)
                                    {
                                        var ele = SearchDriver.FindElement(By.XPath("(//span[text() = 'Great value'])[" + index + "]//..//..//..//..//.."));
                                        Actions actions = new Actions(SearchDriver);
                                        actions.MoveToElement(ele);
                                        actions.Perform();
                                        Thread.Sleep(1000);

                                        try
                                        {
                                            ele.Click();
                                            Thread.Sleep(1000);
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            
                                        }
                                        attempt++;
                                    }
                                    
                                    try
                                    {
                                        information.Destination = SearchDriver.FindElement(By.XPath("//h3[@class='GDWaad tD82ud']")).Text;
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    
                                    
                                    // var ele1 = SearchDriver.FindElement(By.XPath("//div[@class='ZAKz5d']//a"));
                                    // string link = ele1.GetAttribute("href");
                                    Flight_Detail detail = GetInfo_a(SearchDriver);
                                    information.DestinationAirport = detail.DestAirport;
                                    information.FlightDate = detail.FlightDate;
                                    information.BookingLink = detail.BookingLink;
                                    information.DatePosted = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
                                    // save information
                                    m_results.Add(information);
                                    // Back to results
                                    
                                    try
                                    {
                                        SearchDriver.FindElement(By.XPath("//button[@class='VfPpkd-BIzmGd OmoSvb OoEosd VfPpkd-BIzmGd-OWXEXe-yolsp']")).Click();
                                        Thread.Sleep(500);
                                    }
                                    catch (Exception ex)
                                    {
                                        if (SearchDriver.Url == information.BookingLink)
                                        {
                                            SearchDriver.Navigate().Back();
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    
                                }
                                catch (Exception ex)
                                {
                                    
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        
                    }
                    Thread.Sleep(2000);
                }
                new CCsv().AppendCsv(m_results, FILEPATH);
                thread_cnt ++;
                SearchDriver.Quit();
            }
            catch (Exception ex)
            {
                Console.Write("search_start end --" + ex.ToString());
            }
        }
        static private readonly Mutex m = new Mutex();
        static private Flight_Detail GetInfo_a(IWebDriver driver)
        {
            Flight_Detail detail = new Flight_Detail();

            try
            {
                var view_btn = driver.FindElement(By.XPath("//div[@class='ZAKz5d']//a"));
                var view_link = view_btn.GetAttribute("href");
                
                new CWebBrowser().NewTabWithUrl(driver, view_link);
                Thread.Sleep(5000);
                
                try
                {
                    // Get Departing flights
                    try
                    {
                        var destairport = driver.FindElement(By.XPath("//div[@class='gws-flights-results__airports flt-caption']"));
                        detail.DestAirport = destairport.Text.Split('–')[1];
                    }
                    catch (Exception ex)
                    {

                    }

                    // Get Departing flights Date
                    try
                    {
                        var dates = driver.FindElements(By.XPath("//span[@class='gws-flights-form__date-content']"));
                        detail.FlightDate = dates[0].Text + " - " + dates[1].Text;
                    }
                    catch (Exception ex)
                    {

                    }
                    detail.BookingLink = driver.Url;
                }
                catch (Exception ex)
                {
                    //Console.Write(ex.ToString());
                }

                new CWebBrowser().CloseLastTab(driver);
            }
            catch (Exception ex)
            {

            }

            return detail;
        }

        static private Flight_Detail GetInfo_hairline(IWebDriver driver, int index)
        {
            Flight_Detail detail = new Flight_Detail();


            try
            {
                new CWebBrowser().NewTabWithUrl(driver, driver.Url);
                Thread.Sleep(5000);
                int attempt = 0;
                while (attempt < 3)
                {
                    var ele = driver.FindElement(By.XPath("(//span[text() = 'Great value'])[" + index + "]//..//..//..//..//.."));
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(ele);
                    actions.Perform();
                    Thread.Sleep(1000);

                    try
                    {
                        ele.Click();
                        Thread.Sleep(1000);
                        break;
                    }
                    catch (Exception ex)
                    {

                    }
                    attempt++;
                }
                while(true) { 
                try
                {
                   /* var ele = driver.FindElement(By.XPath("//hairline-button[@class='yDhrce CpMx2b LZHEY flt-subhead2 RQn3j']"));
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(ele);
                    actions.Perform();
                    Thread.Sleep(1000);
                    ele.Click();
                    Thread.Sleep(1000);*/

                    driver.FindElement(By.XPath("//hairline-button[@data-flt-ve='view_flights']")).Click();
                    Thread.Sleep(2000);

                    // Get Departing flights
                    try
                    {
                        var destairport = driver.FindElement(By.XPath("//div[@class='gws-flights-results__airports flt-caption']"));
                        detail.DestAirport = destairport.Text.Split('–')[1];
                    }
                    catch (Exception ex)
                    {

                    }

                    // Get Departing flights Date
                    try
                    {
                        var dates = driver.FindElements(By.XPath("//span[@class='gws-flights-form__date-content']"));
                        detail.FlightDate = dates[0].Text + " - " + dates[1].Text;
                    }
                    catch (Exception ex)
                    {

                    }
                    detail.BookingLink = driver.Url;
                }
                catch (Exception ex)
                {
                    //Console.Write(ex.ToString());
                }
            }

                new CWebBrowser().CloseLastTab(driver);
            }
            catch (Exception ex)
            {

            }
            
            return detail;
        }

        static public void email_send()
        {
            var setting = File.ReadAllLines("setting.txt");
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            mail.From = new MailAddress(setting[0]);
            mail.To.Add(setting[2]);
            mail.Subject = "Scraping Completed";
            mail.Body = "mail with attachment";

            System.Net.Mail.Attachment attachment;
            attachment = new System.Net.Mail.Attachment(FILEPATH);
            mail.Attachments.Add(attachment);

            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential(setting[0], setting[1]);
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);

        }
        public static void consoleSpiner()
        {
            counter++;
            switch (counter % 4)
            {
                case 0:
                    Console.Write("\r" + cur_airport + " scraping.    ");
                    break;
                case 1:
                    Console.Write("\r" + cur_airport + " scraping..   ");
                    break;
                case 2:
                    Console.Write("\r" + cur_airport + " scraping...  ");
                    break;
                case 3:
                    Console.Write("\r" + cur_airport + " scraping.... ");
                    break;
            }

        }
    }
}
