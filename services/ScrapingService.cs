
using BuscarRegistroSanitarioService.models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using System.Text.Json;

namespace BuscarRegistroSanitarioService.services
{

    public class ScrapingService
    {
        private static IWebDriver? driver;
        private List<(string Origin, string ResponseUrl, string ResponseBody)> respuestas = new List<(string, string, string)>();
        private HttpRequestMessage? interceptedRequest = null;
        private HttpResponseMessage? interceptedResponse = null;
        private INetwork? networkInterceptor;
        private WebDriverWait wait;
        private readonly object lockObj = new object();
        public void inicializar()
        {
            if (driver == null)
            {
                lock (lockObj)
                {
                    if (driver == null)
                    {
                        var chromeOptions = new ChromeOptions();
                        //chromeOptions.AddArgument("--headless");
                        chromeOptions.AddArgument("--blink-settings=imagesEnabled=false");//"--headless");

                        driver = new ChromeDriver(chromeOptions);
                        driver.Navigate().GoToUrl("https://v2.registrelo.go.cr/reports/12");

                        networkInterceptor = driver?.Manage().Network;
                        networkInterceptor?.AddResponseHandler(networkResponseHandler());

                        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        iniciarCampos();
                    }

                }
            }
        }

        private NetworkResponseHandler networkResponseHandler()
        {
            int count = 0;
            return new NetworkResponseHandler()
            {
                ResponseMatcher = response => response.StatusCode == 200 && response.Url.Contains("https://gateway.registrelo.go.cr/reports/v1/publicreports/getPublicReport?reportDefinitionCode"),
                ResponseTransformer = response =>
                {
                    Console.WriteLine(count++);
                    if (interceptedRequest?.RequestUri.ToString() == response.Url)
                    {
                        interceptedResponse = new HttpResponseMessage((System.Net.HttpStatusCode)response.StatusCode);
                        foreach (var header in response.Headers)
                        {
                            interceptedResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
                            interceptedResponse.Content = new StringContent(response.Content.ReadAsString());
                        }
                    }
                    return response;
                }
            };

        }
        private void iniciarCampos()
        {
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").ToString() == "complete");
            var advanceSearch = WaitForElementToBeVisible(By.ClassName("advance-search"));
            wait.Until(d => advanceSearch.Displayed);
            advanceSearch.Click();

            if (driver.FindElements(By.ClassName("times-icon")).Count > 0)
            {
                var fechas = wait.Until(d => d.FindElements(By.ClassName("times-icon")));
                wait.Until(d => fechas.First().Displayed);
                fechas.First().Click();
                fechas.Last().Click();

            }

            var tipo = wait.Until(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(2) > div:nth-child(2) > div > div:nth-child(1) > input")));
            var estado = wait.Until(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(12) > div:nth-child(2) > div > div:nth-child(1) > input")));


            tipo.Click();
            var listaTipo = wait.Until(d => d.FindElement(By.Id("downshift-1-item-6")));
            listaTipo.Click();
            estado.Click();
            var listaEstado = wait.Until(d => d.FindElement(By.Id("downshift-3-item-0")));
            listaEstado.Click();


        }
        public async Task<ApiResponse> BuscarRegistroSanitario(string? nombreProducto, int? skip = 0)
        {
            interceptedResponse = null;
            respuestas.Clear();
            ApiResponse? payload = new ApiResponse();

            try
            {

                await networkInterceptor.StartMonitoring();

                var nombre = wait.Until(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(3) > div:nth-child(2) > div > input")));
                nombre.Clear();
                nombre.SendKeys(nombreProducto);

                var reportButton = wait.Until(d => d.FindElement(By.CssSelector("#reportFilterForm button[type='submit']")));

                reportButton.Click();

                var maxWaitTime = TimeSpan.FromSeconds(60);
                var stopwatch = Stopwatch.StartNew();
                while (interceptedResponse == null && stopwatch.Elapsed < maxWaitTime)
                {
                    await Task.Delay(500);
                }
                stopwatch.Stop();
                if (interceptedResponse != null)
                {
                    payload = await ManejadorInterceptorRespuesta();
                }
                // while (interceptedRequest == null && stopwatch.Elapsed < maxWaitTime)
                // {
                //     await Task.Delay(500);  
                // }

                // if (interceptedRequest != null)
                // {
                //     payload = await ManejadorInterceptorSolicitud();
                // }
                await networkInterceptor.StopMonitoring();


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

            }

            return payload;
        }
        private IWebElement WaitForElementToBeVisible(By by)
        {
            return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
        }

        private async Task<ApiResponse> ManejadorInterceptorRespuesta()
        {
            ApiResponse? payload = new ApiResponse();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var responseBody = await interceptedResponse.Content.ReadAsStringAsync();
            payload = JsonSerializer.Deserialize<ApiResponse>(responseBody, options);

            return payload;
        }


        public async Task<ApiResponse> paginar(BotonesPaginador boton)
        {
            interceptedResponse = null;
            respuestas.Clear();
            ApiResponse? payload = new ApiResponse();
            string clase = "next";
            switch (boton)
            {
                case BotonesPaginador.siguiente:
                    clase = "next";
                    break;
                case BotonesPaginador.anterior:
                    clase = "prev";
                    break;
            }
            try
            {
                var siguiente = WaitForElementToBeVisible(By.ClassName(clase));

                await networkInterceptor.StartMonitoring();
                if (!siguiente.GetAttribute("style").Contains("cursor: not-allowed;"))
                {
                    siguiente.Click();
                }

                var maxWaitTime = TimeSpan.FromSeconds(60);
                var stopwatch = Stopwatch.StartNew();
                while (interceptedResponse == null && stopwatch.Elapsed < maxWaitTime)
                {
                    await Task.Delay(500);
                }
                stopwatch.Stop();

                if (interceptedResponse != null)
                {
                    payload = await ManejadorInterceptorRespuesta();
                }
                await networkInterceptor.StopMonitoring();


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

            }

            return payload;
            //
        }
        public void Dispose()
        {
            driver?.Quit();
            driver = null;
        }
    }
}

public enum BotonesPaginador
{
    siguiente,
    anterior
}