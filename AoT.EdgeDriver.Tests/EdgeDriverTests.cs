using System;
using System.Globalization;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Edge.SeleniumTools;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace AoT.EdgeDriver.Tests
{
    public class EdgeDriverTests
    {
        private IWebDriver driver1;
        private IWebDriver driver2;
        private EdgeOptions edgeOptions;

        [SetUp]
        public void SetUp()
        {
            edgeOptions = new Microsoft.Edge.SeleniumTools.EdgeOptions();
            edgeOptions.UseChromium = true;
        }

        [Test]
        public void CanLaunchEdgeDriver()
        {
            driver1 = new Microsoft.Edge.SeleniumTools.EdgeDriver(edgeOptions);
            driver1.Url = "http://example.com";
            driver1.Title.Should().Be("Example Domain");
        }
        
        [Test]
        public void CanLaunchMultipleEdgeDrivers()
        {
            edgeOptions.AddArgument("headless");

            driver1 = new Microsoft.Edge.SeleniumTools.EdgeDriver(edgeOptions);
            driver1.Url = "http://example.com";

            driver2 = new Microsoft.Edge.SeleniumTools.EdgeDriver(edgeOptions);
            driver2.Url = "http://example.com";

            //assert that both drivers are still active
            using (new AssertionScope())
            {
                driver1.Title.Should().Be("Example Domain");
                driver2.Title.Should().Be("Example Domain");
            }
        }

        [Test]
        public void CanLaunchEdgeDriverInHeadlessMode()
        {
            edgeOptions.AddArgument("headless");
            driver1 = new Microsoft.Edge.SeleniumTools.EdgeDriver(edgeOptions);
            driver1.Url = "http://example.com";

            var executor = (IJavaScriptExecutor) driver1;
            string userAgent = executor.ExecuteScript("return window.navigator.userAgent").ToString();

            using (new AssertionScope())
            {
                driver1.Title.Should().Be("Example Domain");
                userAgent.ToLower().Should().Contain("headless");
            }
        }

        [Test]
        public void CanLaunchEdgeDriverWithAPreferredLanguageCulture()
        {
            CultureInfo cultureInfo = new CultureInfo("de");
            edgeOptions.AddUserProfilePreference("intl.accept_languages", cultureInfo.ToString());
            driver1 = new Microsoft.Edge.SeleniumTools.EdgeDriver(edgeOptions);
            driver1.Url = "https://manytools.org/http-html-text/browser-language/";

            var executor = (IJavaScriptExecutor) driver1;
            string language = executor.ExecuteScript("return window.navigator.userlanguage || window.navigator.language").ToString();

            using (new AssertionScope())
            {
                driver1.Title.Should().Be("Browser language - display the list of languages your browser says you prefer");
                language.Should().BeEquivalentTo(cultureInfo.ToString());
            }
        }

        [Test]
        public void CannotLaunchHeadlessEdgeDriverWithAPreferredLanguageCulture()
        {
            CultureInfo cultureInfo = new CultureInfo("nl");
            edgeOptions.AddUserProfilePreference("intl.accept_languages", cultureInfo.ToString());
            edgeOptions.AddArgument("headless");
            driver1 = new Microsoft.Edge.SeleniumTools.EdgeDriver(edgeOptions);
            driver1.Url = "https://manytools.org/http-html-text/browser-language/";

            var executor = (IJavaScriptExecutor) driver1;
            string language = executor.ExecuteScript("return window.navigator.userlanguage || window.navigator.language").ToString();

            using (new AssertionScope())
            {
                driver1.Title.Should().Be("Browser language - display the list of languages your browser says you prefer");
                //unless your system language is nl!
                language.Should().NotBeEquivalentTo(cultureInfo.ToString());
            }
        }

        [Test]
        public void RequestingRemoteChromiumEdgeDriverThrowsWebDriverException()
        {
            edgeOptions.PlatformName = PlatformType.Windows.ToString();
            // This does not work on either Selenium 3 or Selenium 4 grids in my testing
            Uri gridUri = new Uri("http://192.168.0.200:4444/wd/hub");
            
            Action action = () => driver1 = new RemoteWebDriver(gridUri, edgeOptions);

            action.Should().Throw<OpenQA.Selenium.WebDriverException>();
        }

        [TearDown]
        public void TearDown(){
            driver1?.Quit();
            driver1?.Dispose();
            driver2?.Quit();
            driver1?.Dispose();
        }
    }
}