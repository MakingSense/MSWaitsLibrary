using NLog.Fluent;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace MSWaitLibrary
{
    public class Alerts
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private OpenQA.Selenium.Interactions.Actions DriverActions { get; set; }


        public Alerts(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            DriverActions = new OpenQA.Selenium.Interactions.Actions(driver);
        }

        private IAlert FindAlert(bool throwExceptionIfNotExist, int waitForExistSeconds)
        {
            Log.Info("Looking for JS Alert");
            // (2/12/21 - Ned) Alternative method:
            //_wait.Timeout = TimeSpan.FromSeconds(waitForExistSeconds);
            //IAlert alert = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());

            try
            {
                IAlert alert = _driver.SwitchTo().Alert();
                return alert;
            }
            catch (Exception e)
            {
                Log.Error("JS Alert does not exist", false, _driver, throwExceptionIfNotExist, e);
                return null;
            }
        }

        public void Dismiss(bool throwExceptionIfNotExist = true, int waitForExistSeconds = 5)
        {
            Log.Info("Dismissing JS Alert");
            FindAlert(throwExceptionIfNotExist, waitForExistSeconds)?.Dismiss();
        }
        public void Accept(bool throwExceptionIfNotExist = true, int waitForExistSeconds = 5)
        {
            Log.Info("Accepting JS Alert");
            FindAlert(throwExceptionIfNotExist, waitForExistSeconds)?.Accept();
        }
        public string Text(bool throwExceptionIfNotExist = true, int waitForExistSeconds = 5)
        {
            Log.Info("Getting text of JS Alert");
            return FindAlert(throwExceptionIfNotExist, waitForExistSeconds)?.Text;
        }
        public void Input(string input, bool throwExceptionIfNotExist = true, int waitForExistSeconds = 5)
        {
            Log.Info($"Typing '{input}' in JS Alert");
            FindAlert(throwExceptionIfNotExist, waitForExistSeconds)?.SendKeys(input);
        }
    }
}
