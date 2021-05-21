using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MSWaitLibrary
{
    class Log
    {
        public enum Level { Debug, Info, Success, Warn, Error }

        private static void LogMessage(Level level, string message, bool takeScreenShot = false, IWebDriver driver = null, bool throwException = false, Exception exception = null)
        {
            // generate log line
            string logMessage = string.Empty;
            string tcName = TestContext.CurrentContext.Test.Name;
            logMessage += $"{level.ToString().ToUpper().PadRight(7).Substring(0, 7)}";
            logMessage += $" | {Metrics.ElapsedSinceStart().PadRight(5).Substring(0, 5)}";
            logMessage += $" | {Metrics.ElapsedSinceLast().PadRight(5).Substring(0, 5)}";
            if (tcName.Contains("_"))
                logMessage += $" | {tcName.Substring(0, tcName.LastIndexOf('_'))}";
            else
                logMessage += $" | {tcName}";
            logMessage += $" | {GetCallingMethodName()}";
            logMessage += $" | {message} ";
            if (driver != null) logMessage += GetDriverURL(driver);
            if (exception != null)
                logMessage += Environment.NewLine + exception;
            if (driver != null && takeScreenShot)
                logMessage += TakeScreenshot(driver, tcName);

            // output message
            TestContext.Progress.WriteLine(logMessage);
            WriteToFile(logMessage, tcName);

            // throw exception
            switch (throwException)
            {
                case true when exception == null:
                    Assert.Fail($"{message}");
                    break;
                case true when exception != null:
                    Assert.Fail($"{message}\nException: {exception}");
                    break;
            }
        }

        public static void Debug(string message, bool takeScreenShot = false, IWebDriver driver = null) => LogMessage(Level.Debug, message, takeScreenShot, driver);
        public static void Info(string message, bool takeScreenShot = false, IWebDriver driver = null) => LogMessage(Level.Info, message, takeScreenShot, driver);
        public static void Success(string message, bool takeScreenShot = false, IWebDriver driver = null) => LogMessage(Level.Success, message, takeScreenShot, driver);
        public static void Warn(string message, bool takeScreenShot = false, IWebDriver driver = null, bool throwException = false, Exception exception = null) => LogMessage(Level.Warn, message, takeScreenShot, driver, throwException, exception);
        public static void Error(string message, bool takeScreenShot = true, IWebDriver driver = null, bool throwException = true, Exception exception = null) => LogMessage(Level.Error, message, takeScreenShot, driver, throwException, exception);

        private static string TakeScreenshot(IWebDriver driver, string fileName)
        {
            string logMessage = Environment.NewLine;
            //to avoid errors when the TC name is large
            fileName = $"{fileName.Substring(0, fileName.LastIndexOf('_'))}_{DateTime.Now.ToFileTimeUtc()}.png";
            //fileName = $"{fileName}_{DateTime.Now.ToFileTimeUtc()}.png";
            logMessage += $"\tCapturing screenshot ({fileName})... ";

            try
            {
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                fileName = MakeValidFileName(fileName);
                string screenshotFile = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName);
                screenshot.SaveAsFile(screenshotFile, ScreenshotImageFormat.Png);

                // Add that file to NUnit results
                TestContext.AddTestAttachment(screenshotFile, $"Attaching screenshot: {fileName}");
                logMessage += $"Done!";
            }
            catch (Exception ex)
            {
                logMessage += $"Failed!\n\t{ex.Message}";
            }

            return logMessage;
        }

        /// <summary>
        /// Sanitizes filename
        /// https://stackoverflow.com/questions/53813397/sanitizing-a-file-path-in-c-sharp-without-compromising-the-drive-letter
        /// </summary>
        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        private static void WriteToFile(string message, string tcName)
        {
            string fileName = $"Logs_{tcName}_{DateTime.Today}.txt";
            File.AppendAllText(Path.Combine(TestContext.CurrentContext.WorkDirectory, MakeValidFileName(fileName)),
                message + Environment.NewLine);
        }

        public static string LogFileName()
        {
            string tcName = TestContext.CurrentContext.Test.Name;
            string fileName = $"Logs_{tcName}_{DateTime.Today}.txt";
            return Path.Combine(TestContext.CurrentContext.WorkDirectory, MakeValidFileName(fileName));
        }

        /// <summary>
        /// Gets the drivers URL
        /// Wrapped in a try/catch since sometimes it throws an exception (e.g. window is closed)
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        private static string GetDriverURL(IWebDriver driver)
        {
            try
            {
                return $" [{driver.Url}]";
            }
            catch (Exception)
            {
                // ignore
            }

            return "";
        }

        private static string GetCallingMethodName(int maxDepth = 9)
        {
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            string methods = string.Empty;

            // get depth to TC
            int depth = 0;
            while (true)
            {
                if (depth > maxDepth || stackTrace.GetFrame(depth).GetMethod().Name.Contains("_"))
                    break;

                depth++;
            }

            // build stack string
            for (int i = depth - 1; i >= 3; i--)
            {
                methods += "." + stackTrace.GetFrame(i).GetMethod().Name;
            }

            return methods;
        }
    }
}
