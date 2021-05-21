using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;

namespace MSWaitLibrary
{
   public class ActionsForElement
    {

        internal IWebDriver WebDriver { get; set; }
        private OpenQA.Selenium.Interactions.Actions DriverActions { get; set; }
        public WaitElement WaitElement { get; private set; }
        public Alerts Alert { get; private set; }
        public DelayTime Delay { get; private set; }
        private readonly IJavaScriptExecutor JS;
        private IWebElement lastElementClicked;
        public const float Factor = 1;
        public const int TimedOutSecond = 120;
        public const int DelayForActions = 250;

        public ActionsForElement(IWebDriver driver)
        {
            WebDriver = driver;
            DriverActions = new OpenQA.Selenium.Interactions.Actions(driver);
            WaitElement = new WaitElement(driver, this);
            Alert = new Alerts(driver);
            Delay = new DelayTime();
            JS = (IJavaScriptExecutor)driver;
        }

        /// <summary>
        /// Allows tester to pass a IWebElement or By object for most actions
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private IWebElement FindElement(object element) =>
            element.GetType() == typeof(By)
                ? FindElement((By)element)
                : (IWebElement)element;

        public List<IWebElement> FindElements(By element, bool throwException = false) =>
            FindElements(element, null, null, null, null, null, throwException);

        /// <summary>
        /// Looks for the element. Provide child By locators to look for elements under elements.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="childElement1"></param>
        /// <param name="childElement2"></param>
        /// <param name="childElement3"></param>
        /// <param name="childElement4"></param>
        /// <param name="childElement5"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        public List<IWebElement> FindElements(
            By element,
            By childElement1,
            By childElement2 = null,
            By childElement3 = null,
            By childElement4 = null,
            By childElement5 = null,
            bool throwException = false
        )
        {
            Log.Debug($"Searching for '{element}'");
            List<IWebElement> elements = new List<IWebElement>();

            try
            {
                elements = WebDriver.FindElements(element).ToList();

                // search for child element(s) if needed
                if (childElement1 != null)
                {
                    Log.Debug($"=> Searching for child element '{childElement1}'");
                    elements = elements.First().FindElements(childElement1).ToList();
                }

                if (childElement2 != null)
                {
                    Log.Debug($"==> Searching for child element '{childElement2}'");
                    elements = elements.First().FindElements(childElement2).ToList();
                }

                if (childElement3 != null)
                {
                    Log.Debug($"===> Searching for child element '{childElement3}'");
                    elements = elements.First().FindElements(childElement3).ToList();
                }

                if (childElement4 != null)
                {
                    Log.Debug($"====> Searching for child element '{childElement4}'");
                    elements = elements.First().FindElements(childElement4).ToList();
                }

                if (childElement5 != null)
                {
                    Log.Debug($"=====> Searching for child element '{childElement5}'");
                    elements = elements.First().FindElements(childElement5).ToList();
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString(), true, WebDriver, throwException);
            }

            // check results
            if (!elements.Any())
            {
                Log.Error($"No elements found", true, WebDriver, throwException, new NoSuchElementException());
                return null;
            }

            // return found elements
            Log.Debug($"{elements.Count()} element(s) found");
            return elements;
        }

        /// <summary>
        /// Finds the element in the browser. Returns the first found within `FindElements` method. Can search for child elements.
        /// </summary>
        /// <returns></returns>
        public IWebElement FindElement(By element, bool throwException = false) => FindElements(element, throwException: throwException).First();

        public IWebElement FindElement(
            By element,
            By childElement1,
            By childElement2 = null,
            By childElement3 = null,
            By childElement4 = null,
            By childElement5 = null,
            bool throwException = false
            ) => FindElements(element, childElement1, childElement2, childElement3, childElement4, childElement5, throwException).First();

        public IWebElement FindLastElement(By element, bool throwException = true) =>
            FindElements(element, null, null, null, null, null, throwException).Last();

        public IWebElement FindLastElement(
            By element,
            By childElement1,
            By childElement2 = null,
            By childElement3 = null,
            By childElement4 = null,
            By childElement5 = null,
            bool throwException = false
        ) => FindElements(element, childElement1, childElement2, childElement3, childElement4, childElement5, throwException).Last();

        /// <summary>
        /// Checks the visibility of the element and brings it into view if needed
        /// </summary>
        public void EnsureVisible(object element)
        {
            if (!IsVisibleInViewPort(element))
                ScrollToElement(element);
        }

        /// <summary>
        /// Performs a click action on the target element
        /// </summary>
        public void Click(object element, bool ensureVisible = false, bool log = true)
        {
            if (log) Log.Debug($"Clicking '{element}'");
            Delay.Milliseconds(DelayForActions, true, false);
            CloseErrorDialogIfExist();

            IWebElement foundElement = FindElement(element);
            if (ensureVisible) EnsureVisible(foundElement);

            try
            {
                foundElement.Click();
                Log.Success($"Clicked '{element}'");
            }
            catch (ElementClickInterceptedException e)
            {
                // Retry previous click if click is intercepted
                Log.Warn("Click intercepted, trying to click the last element clicked again, then this target element", true, WebDriver, false, e);
                lastElementClicked?.Click();
                Delay.Seconds(3);
                foundElement.Click();
                Log.Success($"Clicked '{element}'");
            }

            lastElementClicked = foundElement;
        }

        /// <summary>
        /// Grabs the current text of the element
        /// </summary>
        /// <returns>Text of element</returns>
        public string GetText(object element, bool log = true)
        {
            if (log) Log.Debug($"Getting text of '{element}'");

            const int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    string text = FindElement(element).Text;
                    Log.Debug($"\tText found: {text}");
                    return text;
                }
                catch (StaleElementReferenceException ex)
                {
                    Log.Warn($"Stale element exception. Trying action again (try {i + 1} of {retryCount})\n\t{ex}");
                }
            }

            return null;
        }

        public string GetUrl(bool log = true)
        {
            string url = WebDriver.Url;
            if (log) Log.Debug($"Getting Driver URL. Currently: {url}");
            return url;
        }

        /// <summary>
        /// Checks if an element is currently visible
        /// </summary>
        public bool IsDisplayed(object element, bool log = true)
        {
            if (log) Log.Debug($"Checking visibility of '{element}'");

            // NED 3/18 - Retry method as a temp fix for TC_8943
            const int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    return FindElement(element).Displayed;
                }
                catch (StaleElementReferenceException ex)
                {
                    Log.Warn($"Stale element exception. Trying action again (try {i + 1} of {retryCount})\n\t{ex}");
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if an element within the current viewport
        /// https://stackoverflow.com/questions/45243992/verification-of-element-in-viewport-in-selenium
        /// </summary>
        public bool IsVisibleInViewPort(object element)
        {
            Log.Debug($"Checking if '{element}' is visible in view port - WARNING: FLAKY");

            const int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    var foundElement = FindElement(element);
                    return (bool)(JS.ExecuteScript(
                        "var elem = arguments[0],                 " +
                        "  box = elem.getBoundingClientRect(),    " +
                        "  cx = box.left + box.width / 2,         " +
                        "  cy = box.top + box.height / 2,         " +
                        "  e = document.elementFromPoint(cx, cy); " +
                        "for (; e; e = e.parentElement) {         " +
                        "  if (e === elem)                        " +
                        "    return true;                         " +
                        "}                                        " +
                        "return false;                            "
                        , foundElement));
                }
                catch (StaleElementReferenceException ex)
                {
                    Log.Warn($"Stale element exception. Trying action again (try {i + 1} of {retryCount})\n\t{ex}");
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if element exists
        /// </summary>
        public bool Exist(By element, bool log = true)
        {
            if (log) Log.Debug($"Checking existence of '{element}'");
            bool exist = FindElements(element) != null;
            return exist;
        }

        /// <summary>
        /// Checks if element is enabled
        /// </summary>
        public bool Enabled(By by)
        {
            Log.Debug($"Checking existence of '{by}'");
            return FindElement(by).Enabled;
        }

        /// <summary>
        /// Clicks the element if it exist
        /// </summary>
        /// <returns>`True` if element is clicked; else `False`</returns>
        public bool ClickIfExist(By element, bool log = true)
        {
            if (log) Log.Debug($"Clicking {element} if it exist");
            if (!Exist(element)) return false;
            Log.Debug($"Element exist, clicking '{element}'");
            Click(element);
            return true;
        }

        /// <summary>
        /// Types the text within the element
        /// </summary>
        /// <param name="text">Text to type</param>
        public void Type(object element, string text, bool mask = false)
        {
            Log.Debug(mask ? $"Typing '*********' (masked) in '{element}'" : $"Typing '{text}' in '{element}'");
            Delay.Milliseconds(DelayForActions, true, false);
            CloseErrorDialogIfExist();
            FindElement(element).SendKeys(text);
        }
        public void ClearField(object element)
        {
            Log.Debug($"Clearing '{element}'");
            Delay.Milliseconds(DelayForActions, true, false);
            IWebElement foundElement = FindElement(element);

            foundElement.SendKeys(Keys.Control + "a");
            foundElement.SendKeys(Keys.Delete);
            foundElement.Clear(); // backup method
        }

        /// <summary>
        /// Clears the field (if enabled), clicks the field, then types the input into the field
        /// </summary>
        public void Input(object element, string text, bool clearField = true, bool mask = false)
        {
            if (clearField) ClearField(element);
            Click(element);
            Type(element, text, mask);
        }

        /// <summary>
        /// Selects the option via the visible text option
        /// </summary>
        public void SelectByText(object element, string optionText)
        {
            Log.Debug($"Selecting option value by text '{optionText}' in '{element}'");
            Delay.Milliseconds(DelayForActions, true, false);
            CloseErrorDialogIfExist();
            SelectElement selectElement = new SelectElement(FindElement(element));
            selectElement.SelectByText(optionText);
        }

        public string GetAttributeValue(object element, string attributeName)
        {
            Log.Debug($"Getting attribute value of '{attributeName}' from '{element}'");
            return FindElement(element).GetAttribute(attributeName);
        }

        public string GetCssValue(object element, string styleName)
        {
            Log.Debug($"Getting Css style value of '{styleName}' from '{element}'");
            return FindElement(element).GetCssValue(styleName);
        }

        /// <summary>
        /// Scrolls to the target element within the viewport
        /// </summary>
        public void ScrollToElement(object element)
        {
            Log.Debug($"Scrolling to element '{element}'");
            Delay.Milliseconds(DelayForActions, true, false);

            WebDriver.ExecuteJavaScript("arguments[0].scrollIntoView();", FindElement(element));
        }

        /// <summary>
        /// Scrolls to the target element within the viewport
        /// </summary>
        /// <param name="element"></param>
        /// <param name="throwException"></param>
        [Obsolete("Use 'By' overload method, this method only here until old code is refactored")]
        public void ScrollToElement(IWebElement element, bool throwException = false)
        {
            Log.Debug($"Scrolling to element '{element}'");
            Delay.Milliseconds(DelayForActions, true, false);

            const int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    WebDriver.ExecuteJavaScript("arguments[0].scrollIntoView();", element);
                    return;
                }
                catch (StaleElementReferenceException ex)
                {
                    Log.Warn($"Stale element exception. Trying action again (try {i + 1} of {retryCount})\n\t{ex}");
                }
            }

            Log.Error("Failed to scroll to element", throwException: throwException);
        }

        /// <summary>
        /// Scrolls to the bottom of the page, regardless of height
        /// </summary>
        public void ScrollToBottom()
        {
            Log.Debug($"Scrolling to bottom of page");
            Delay.Milliseconds(DelayForActions, true, false);

            WebDriver.ExecuteJavaScript("window.scrollTo(0, document.body.scrollHeight)");
        }

        /// <summary>
        /// Drags an element from one location to another
        /// </summary>
        /// <param name="fromBy"></param>
        /// <param name="toBy"></param>
        public void DragElementTo(By fromBy, By toBy)
        {
            Log.Debug($"Dragging dropping '{fromBy}' -> '{toBy}'");
            Delay.Milliseconds(DelayForActions, true, false);
            DriverActions.DragAndDrop(FindElement(fromBy), FindElement(toBy)).Build().Perform();
        }

        [Obsolete("Use 'By' overload method, this method only here until old code is refactored")]
        public void DragElementTo(IWebElement from, IWebElement to)
        {
            Log.Debug($"Dragging dropping '{from}' -> '{to}'");
            Delay.Milliseconds(DelayForActions, true, false);
            DriverActions.DragAndDrop(from, to).Build().Perform();
        }

        public void AcceptAlert()
        {
            Log.Debug($"Accepting JS Alert");
            Delay.Milliseconds(DelayForActions, true, false);
            WebDriver.SwitchTo().Alert().Accept();
        }

        public void CropImage(By fromBy, int x, int y)
        {
            Log.Debug($"Cropping image with DragAndDropToOffset method. Element: '{fromBy}'; Offset: '{x},{y}'");
            DriverActions.DragAndDropToOffset(FindElement(fromBy), x, y).Build().Perform();
        }

        [Obsolete("Use 'By' overload method, this method only here until old code is refactored")]
        public void CropImage(IWebElement from, int x, int y)
        {
            Log.Debug($"Cropping image with DragAndDropToOffset method. Element: '{from}'; Offset: '{x},{y}'");
            DriverActions.DragAndDropToOffset(from, x, y).Build().Perform();
        }

        [Obsolete("2/11/21 Not used currently, may need to be refactored before using")]
        public void ErrorPopup(By errorPopUpClass, By errorPopUp)
        {
            if (IsDisplayed(errorPopUpClass) || Exist(errorPopUp))
                Assert.Fail("Error Pop Up is shown on MY matter page, Please see the logs");
        }

        public IWebElement FindChildElementContainingText(By element, By childElement, string text)
        {
            return FindElements(element).FirstOrDefault(x => x.FindElement(childElement).Text.Contains(text));
        }

        public string GetMostRecentChatMessageText(bool log = true)
        {
            int retryCount = 3;
            Delay.Milliseconds(DelayForActions, true, false);
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    if (log) Log.Debug($"Reading most recent chat message text (attempt {i + 1}/{retryCount})");
                    return FindLastElement(By.XPath("//div[@class='message__content']//*[contains(@class, 'message__content__text') and not(contains(@class, '__mention')) or contains(@class, 'deleted-message')]")).Text;
                }
                catch (Exception e)
                {
                    Log.Warn($"\tAction failed: {e}");
                }
            }

            Log.Error("Failed to perform action");
            return null;
        }

        /// <summary>
        /// Closes the error dialog if found
        /// </summary>
        private void CloseErrorDialogIfExist()
        {
            return; // Causes test to delay implicit wait time every time
                    //ClickIfExist(By.XPath("//*[name()='svg']/*[name()='title' and .='Close dialog']"), false);
        }

        /// <summary>
        /// Scrolls to the target element within the viewport
        /// </summary>
        public void RefreshView()
        {
            Log.Debug("Refresh View");
            WebDriver.Navigate().Refresh();
            Delay.Seconds(4, true, false);

        }

        public void ClickOutOfBetaModalGotItButton(By element, int times)
        {
            if (Exist(element))
            {
                for (int i = 0; i < times; i++)
                {
                    ClickIfExist(element);

                }
            }
        }

        public string GetFileFullPath(string fileName, string folder = "InputData")
        {
            string current_directory = System.AppContext.BaseDirectory;
            string file = Path.Combine(current_directory, $"{folder}/{fileName}");
            var fullPath = Path.GetFullPath(file);
            Log.Info($"{fullPath} is the file uploaded");
            Log.Debug($"Checking existence of file '{fileName}' in {folder} folder");
            Assert.IsTrue(File.Exists(fullPath), $"File {fileName} does not exist on folder {folder}");
            return fullPath;
        }

    }
}
