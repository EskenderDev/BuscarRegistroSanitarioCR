
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
                        chromeOptions.AddArgument("--blink-settings=imagesEnabled=false");//"--headless");
                        driver = new ChromeDriver(chromeOptions);
                        driver.Navigate().GoToUrl("https://v2.registrelo.go.cr/reports/12");
                        
                        networkInterceptor = driver?.Manage().Network;
                        networkInterceptor?.AddRequestHandler(networkRequestHandler());

                        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        iniciarCampos();
                    }

                }
            }
        }

        private NetworkRequestHandler networkRequestHandler() {
            return new NetworkRequestHandler
                        {
                            RequestMatcher = request => request.Url.Contains("/reports/v1/publicreports/getPublicReport") && request.Headers.ContainsKey("api-token") && request.Method == HttpMethod.Get.Method,

                            RequestTransformer = request =>
                            {                       
                                //request.Url = request.Url.Replace("skip=0", $"skip={skip}");
                                interceptedRequest = new HttpRequestMessage(HttpMethod.Get, request.Url);
                                foreach (var header in request.Headers)
                                {
                                    interceptedRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                                }
                                return request;
                            }
                        };

        }
        private void iniciarCampos(){
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
                while (interceptedRequest == null && stopwatch.Elapsed < maxWaitTime)
                {
                    await Task.Delay(500);  
                }
                stopwatch.Stop();
                if (interceptedRequest != null)
                {
                    payload = await ManejadorInterceptorSolicitud();
                }
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
        private async Task<ApiResponse> ManejadorInterceptorSolicitud()
        {
            ApiResponse? payload = new ApiResponse();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.SendAsync(interceptedRequest);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    var responseBody = await response.Content.ReadAsStringAsync();
                    payload = JsonSerializer.Deserialize<ApiResponse>(responseBody, options);
                }
            }

            return payload;
        }

        
        public async Task<ApiResponse> paginar(BotonesPaginador boton ){

              ApiResponse? payload = new ApiResponse();
              string clase = "next";
            switch (boton) {
                case BotonesPaginador.siguiente:
                    clase="next";
                    break;
                case BotonesPaginador.anterior:
                    clase="prev";
                    break;
            }
            try
            {
                var siguiente = WaitForElementToBeVisible(By.ClassName(clase));

                await networkInterceptor.StartMonitoring();
                if(!siguiente.GetAttribute("style").Contains("cursor: not-allowed;")) {
                    siguiente.Click();
                }

                var maxWaitTime = TimeSpan.FromSeconds(60);
                var stopwatch = Stopwatch.StartNew();
                while (interceptedRequest == null && stopwatch.Elapsed < maxWaitTime)
                {
                    await Task.Delay(500);  
                }
                stopwatch.Stop();

                if (interceptedRequest != null)
                {
                    payload = await ManejadorInterceptorSolicitud();
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

public enum BotonesPaginador {
            siguiente,
            anterior
}