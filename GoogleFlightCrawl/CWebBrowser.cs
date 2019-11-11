using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Drawing;
using System.Threading;

namespace GoogleFlightCrawl
{
    class CWebBrowser
    {
        public IWebDriver GoogleChrome()
        {
            ChromeOptions option = new ChromeOptions();
            option.AddArguments("disable-infobars");               //disable test automation message
            option.AddArguments("--disable-notifications");        //disable notifications
            option.AddArguments("--disable-web-security");         //disable save password windows
            option.AddArgument("--headless");                      //do not show browser

            option.AddUserProfilePreference("credentials_enable_service", false);
            option.AddUserProfilePreference("browser.download.manager.showWhenStarting", false);
            option.AddUserProfilePreference("browser.helperApps.neverAsk.saveToDisk", "text/csv");
            option.AddUserProfilePreference("disable-popup-blocking", "true");
            option.AddUserProfilePreference("safebrowsing.enabled", true);
            
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            IWebDriver driver = new ChromeDriver(driverService, option);
            //driver.Manage().Window.Minimize();
            return driver;
        }

        public bool NewTabWithUrl(IWebDriver driver, string url)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("window.open()");
                driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count - 1]);
                driver.Navigate().GoToUrl(url);
                Thread.Sleep(500);
                return true;
            }
            catch (Exception)
            {
                if (driver.WindowHandles.Count == 2)
                {
                    driver.Close();
                    driver.SwitchTo().Window(driver.WindowHandles.First());
                }
                return false;
            }

        }
        public void CloseTab(IWebDriver driver)
        {
            driver.Close();
            driver.SwitchTo().Window(driver.WindowHandles.First());
        }

        public void CloseLastTab(IWebDriver driver)
        {
            driver.SwitchTo().Window(driver.WindowHandles.Last());
            driver.Close();
            driver.SwitchTo().Window(driver.WindowHandles.First());
        }

        public void PageDown(IWebDriver driver)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                Thread.Sleep(500);
            }
            catch (Exception)
            {
            }
        }
        public void PageUp(IWebDriver driver)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("window.scrollTo(0, 0);");
                Thread.Sleep(500);
            }
            catch (Exception)
            {
            }
        }
        public void Scrolldown(IWebDriver driver, IWebElement element, int hight)
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollTop = arguments[1];", element, hight);
                Thread.Sleep(1000);
            }
            catch (Exception)
            {
            }
        }
        public void ButtonClick(IWebDriver driver, string element, string txt)
        {
            try
            {
                var buttons = driver.FindElements(By.XPath(element));
                foreach (var item in buttons)
                {
                    if (item.Text.ToLower().Contains(txt))
                    {
                        item.Click();
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
