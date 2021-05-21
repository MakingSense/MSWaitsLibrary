using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;
namespace MSWaitLibrary
{
    public class WaitElement
    {
        private readonly ActionsForElement _actions;
        private readonly WebDriverWait _wait;
        public const float DelayFactor = 1;
        public const int WaitForTimeoutInSeconds = 120;
        public const int DelayMsBetweenActions = 250;

        public WaitElement(IWebDriver driver, ActionsForElement actions)
        {
            _actions = actions;
            _wait = new WebDriverWait(driver, TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Updates the _wait timeout for methods that use this
        /// </summary>
        /// <param name="seconds"></param>
        private void UpdateTimeout(int seconds) => _wait.Timeout = TimeSpan.FromSeconds(seconds);

        public bool Exist(By by, int timeoutInSeconds = -1, bool throwException = true, bool log = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            if (log) Log.Debug($"Waiting for '{by}' to exist (timeout={timeoutInSeconds}s)");
            UpdateTimeout(timeoutInSeconds);

            try
            {
                _wait.Until(ExpectedConditions.ElementExists(by));
                return true;
            }
            catch (Exception)
            {
                if (throwException)
                {
                    Log.Error($"Wait for failed after {timeoutInSeconds} seconds.");
                    throw;
                }
            }
            return false;
        }

        public bool NotExist(By by, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for '{by}' to not exist (timeout={timeoutInSeconds}s)");
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                if (!_actions.Exist(by, false))
                {
                    Log.Success($"'{by}' no longer exist");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{by}' still exist after {timeoutInSeconds} seconds", true, _actions.WebDriver, throwException);
            return false;
        }
        public bool NotExistForElement(object element, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for {element} of '{element}' (timeout: {timeoutInSeconds}s)");
            string currentValue = string.Empty;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                if (_actions.FindElement((By)element) != null)
                {
                    Log.Success($"'{element}' exist");
                    return false;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{element}'  after {timeoutInSeconds} seconds was not found" +
                        $"\n\tCurrent: {currentValue}" +
                        $"\n\tExpected: {element}", true, _actions.WebDriver, throwException);
            return true;
        }
        public bool AttributeEqual(object element, string attributeName, string value, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for {attributeName} of '{element}' to equal '{value}' (timeout: {timeoutInSeconds}s)");
            string currentValue = string.Empty;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                currentValue = _actions.GetAttributeValue(element, attributeName).ToLowerInvariant();
                if (currentValue == value.ToLowerInvariant())
                {
                    Log.Success($"{attributeName} does equal {value}");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{element}' {attributeName} did not equal '{value}' after {timeoutInSeconds} seconds" +
                        $"\n\tCurrent: {currentValue}" +
                        $"\n\tExpected: {value}", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool AttributeNotEqual(By by, string attributeName, string value, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for {attributeName} of '{by}' to not equal '{value}' (timeout: {timeoutInSeconds}s)");
            string currentValue = string.Empty;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                currentValue = _actions.GetAttributeValue(by, attributeName).ToLowerInvariant();
                if (currentValue != value.ToLowerInvariant())
                {
                    Log.Success($"{attributeName} does not equal '{value}' (Current: {currentValue})");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{by}' {attributeName} did equal '{value}' after {timeoutInSeconds} seconds" +
                        $"\n\tCurrent: {currentValue}" +
                        $"\n\tExpected: {value}", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool AttributeContains(object element, string attributeName, string value, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for {attributeName} of '{element}' to contain '{value}' (timeout: {timeoutInSeconds}s)");
            string currentValue = string.Empty;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                currentValue = _actions.GetAttributeValue(element, attributeName).ToLowerInvariant();
                if (currentValue.Contains(value.ToLowerInvariant()))
                {
                    Log.Success($"{attributeName} does contain {value}");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{element}' {attributeName} did not contain '{value}' after {timeoutInSeconds} seconds" +
                        $"\n\tCurrent: {currentValue}" +
                        $"\n\tExpected: {value}", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool AttributeNotContains(object element, string attributeName, string value, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for {attributeName} of '{element}' to not contain '{value}' (timeout: {timeoutInSeconds}s)");
            string currentValue = string.Empty;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                currentValue = _actions.GetAttributeValue(element, attributeName).ToLowerInvariant();
                if (!currentValue.Contains(value.ToLowerInvariant()))
                {
                    Log.Success($"{attributeName} does not contain {value}");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{element}' {attributeName} did contain '{value}' after {timeoutInSeconds} seconds");
            return false;
        }

        public bool CssStyleContains(object element, string styleName, string styleValue, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for style '{styleName}' of '{element}' to contain '{styleValue}' (timeout: {timeoutInSeconds}s)");
            string currentValue = string.Empty;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                currentValue = _actions.GetCssValue(element, styleName).ToLowerInvariant();
                if (currentValue.Contains(styleValue.ToLowerInvariant()))
                {
                    Log.Success($"{styleName} does contain {styleValue}");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{element}' '{styleName}' did not contain '{styleValue}' after {timeoutInSeconds} seconds" +
                        $"\n\tCurrent: {currentValue}" +
                        $"\n\tExpected: {styleValue}", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool Displayed(object element, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for '{element}' to be visible (timeout: {timeoutInSeconds}s)");
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                if (_actions.IsDisplayed(element))
                {
                    Log.Success($"'{element}' is visible");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{element}' is not visible after {timeoutInSeconds} seconds", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool NotDisplayed(By by, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for '{by}' to be not be visible (timeout: {timeoutInSeconds}s)");
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                if (!_actions.IsDisplayed(by))
                {
                    Log.Success($"'{by}' is not visible");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{by}' is still visible after {timeoutInSeconds} seconds", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool TextEqual(object element, string text, bool caseSensitive = false, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for '{element}' text to equal '{text}' (timeout: {timeoutInSeconds}s)");

            string currentText = string.Empty;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                currentText = _actions.GetText(element);
                if (!caseSensitive)
                {
                    text = text.ToLowerInvariant();
                    currentText = currentText.ToLowerInvariant();
                }
                if (currentText == text)
                {
                    Log.Success($"'{element}' text equals '{text}'");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{element}' text does not equal '{text}' after {timeoutInSeconds} seconds (currently: '{currentText}')", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool TextNotEqual(By by, string text, bool caseSensitive = false, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for '{by}' text to not equal '{text}' (timeout: {timeoutInSeconds}s)");

            string currentText = string.Empty;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                currentText = _actions.GetText(by);
                if (!caseSensitive)
                {
                    text = text.ToLowerInvariant();
                    currentText = currentText.ToLowerInvariant();
                }
                if (currentText != text)
                {
                    Log.Success($"'{by}' text does not equal '{text}' (currently: '{currentText}')");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);

            }

            Log.Error($"'{by}' text still equals '{text}' after {timeoutInSeconds} seconds", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool TextContains(object element, string text, bool caseSensitive = false, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for '{element}' text to contain '{text}' (timeout: {timeoutInSeconds}s)");

            string currentText = string.Empty;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                currentText = _actions.GetText(element);
                if (currentText == null)
                    continue;
                if (!caseSensitive)
                {
                    text = text.ToLowerInvariant();
                    currentText = currentText.ToLowerInvariant();
                }
                if (currentText.Contains(text))
                {
                    Log.Success($"'{element}' text '{currentText}' does contain '{text}'");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{element}' text '{currentText}' does not contain '{text}' after {timeoutInSeconds} seconds", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool TextNotContains(By by, string text, bool caseSensitive = false, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for '{by}' text to not contain '{text}' (timeout: {timeoutInSeconds}s)");

            string currentText = string.Empty;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                currentText = _actions.GetText(by);
                if (!caseSensitive)
                {
                    text = text.ToLowerInvariant();
                    currentText = currentText.ToLowerInvariant();
                }
                if (!currentText.Contains(text))
                {
                    Log.Success($"'{by}' text does not contain '{text}' (currently: '{currentText}')");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{by}' text '{currentText}' still contains '{text}' after {timeoutInSeconds} seconds", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool UrlContains(string url, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for URL of browser to contain '{url}' (timeout: {timeoutInSeconds}s)");

            string currentUrl = string.Empty;
            for (int i = 0; i < timeoutInSeconds * 2; i++)
            {
                currentUrl = _actions.GetUrl(false).ToLowerInvariant();
                if (currentUrl.Contains(url.ToLowerInvariant()))
                {
                    Log.Success($"Browser's URL does contain '{url}'");
                    return true;
                }

                _actions.Delay.Milliseconds(500, true, false);
            }

            Log.Error($"Browser's URL does not contain '{url}' after {timeoutInSeconds} seconds (currently: '{currentUrl}')", true, _actions.WebDriver, throwException);
            return false;
        }

        public void NewChatMessage(string lastMessageTime, By by, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for new chat message (timeout: {timeoutInSeconds}s)");

            for (int i = 0; i < 60; i++)
            {
                string currentLastMessageTime = _actions.FindElements(by).Last().Text;
                if (lastMessageTime != currentLastMessageTime)
                {
                    Log.Success($"New chat message found");
                    return;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"New chat message not found", true, _actions.WebDriver, throwException);
        }

        public bool ElementCountEqual(By by, int count, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for '{by}' to return '{count}' element(s) (timeout: {timeoutInSeconds}s)");

            int currentCount = -1;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                currentCount = _actions.FindElements(by).Count;
                if (currentCount == count)
                {
                    Log.Success($"'{by}' returns expected '{currentCount}' element(s)");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{by}' returning '{currentCount}' element(s) (expected: {count}) after {timeoutInSeconds} seconds", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool ElementCountNotEqual(By by, int count, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for '{by}' to return != '{count}' element(s) (timeout: {timeoutInSeconds}s)");

            int currentCount = -1;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                currentCount = _actions.FindElements(by).Count;
                if (currentCount != count)
                {
                    Log.Success($"'{by}' returns '{currentCount}' element(s) (expected != {count})");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{by}' returning '{currentCount}' element(s) (expected != {count}) after {timeoutInSeconds} seconds", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool Click(By by, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for '{by}' to be clicked (timeout: {timeoutInSeconds}s)");

            for (int i = 0; i < timeoutInSeconds; i++)
            {
                if (_actions.ClickIfExist(by))
                {
                    Log.Success($"'{by}' clicked");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{by}' is not clicked after {timeoutInSeconds} seconds", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool Enabled(By by, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for '{by}' to be Enabled (timeout: {timeoutInSeconds}s)");

            for (int i = 0; i < timeoutInSeconds; i++)
            {
                if (_actions.Enabled(by))
                {
                    Log.Success($"'{by}' is enabled");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"'{by}' is not enabled after {timeoutInSeconds} seconds", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool NotEnabled(By by, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for '{by}' to be not Enabled (timeout: {timeoutInSeconds}s)");

            for (int i = 0; i < timeoutInSeconds; i++)
            {
                if (!_actions.Enabled(by))
                {
                    Log.Success($"'{by}' is not enabled");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }


            Log.Error($"'{by}' is still enabled after {timeoutInSeconds} seconds", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool NewChatMessageContain(string chatMessage, bool caseSensitive = false, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for last chat message to contain '{chatMessage}' (timeout: {timeoutInSeconds}s)");

            string lastChatMessage = string.Empty;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                lastChatMessage = _actions.GetMostRecentChatMessageText();
                if (!caseSensitive)
                {
                    chatMessage = chatMessage.ToLowerInvariant();
                    lastChatMessage = lastChatMessage.ToLowerInvariant();
                }
                if (lastChatMessage.Contains(chatMessage))
                {
                    Log.Success($"'{chatMessage}' found");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"Last chat message '{lastChatMessage}' does not contain '{chatMessage}' after {timeoutInSeconds} seconds.", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool NewChatMessageNotContain(string chatMessage, bool caseSensitive = false, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            Log.Debug($"Waiting for last chat message to contain '{chatMessage}' (timeout: {timeoutInSeconds}s)");

            string lastChatMessage = string.Empty;
            for (int i = 0; i < timeoutInSeconds; i++)
            {
                lastChatMessage = _actions.GetMostRecentChatMessageText();
                if (!caseSensitive)
                {
                    chatMessage = chatMessage.ToLowerInvariant();
                    lastChatMessage = lastChatMessage.ToLowerInvariant();
                }
                if (!lastChatMessage.Contains(chatMessage))
                {
                    Log.Success($"'{chatMessage}' found");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"Last chat message '{lastChatMessage}' does not contain '{chatMessage}' after {timeoutInSeconds} seconds.", true, _actions.WebDriver, throwException);
            return false;
        }

        public bool FileExist(string fileName, string filePath = null, int timeoutInSeconds = -1, bool throwException = true)
        {
            if (timeoutInSeconds < 0) timeoutInSeconds = WaitForTimeoutInSeconds;
            if (filePath is null)
                filePath = TestContext.CurrentContext.TestDirectory;

            Log.Debug($"Waiting for file '{fileName}' to exist in '{filePath}' (timeout: {timeoutInSeconds}s)");

            for (int i = 0; i < timeoutInSeconds; i++)
            {
                if (Directory.GetFiles(filePath, fileName).Length > 0)
                {
                    Log.Success($"'{fileName}' found");
                    return true;
                }

                _actions.Delay.Seconds(1, true, false);
            }

            Log.Error($"File '{fileName}'did not exist in '{filePath}' after {timeoutInSeconds} seconds.", true, _actions.WebDriver, throwException);
            return false;
        }

    }
}
