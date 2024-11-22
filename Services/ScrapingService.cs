
using BuscarRegistroSanitarioService.models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using System.Net;
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
        public bool IsInitialized { get; private set; } = false;
        public void inicializar()
        {
            if (driver == null)
            {
                lock (lockObj)
                {
                    if (driver == null)
                    {
                        var chromeOptions = new ChromeOptions();
                        chromeOptions.AddArgument("--headless");
                        //chromeOptions.AddArgument("--blink-settings=imagesEnabled=false");
                 

                        driver = new ChromeDriver(chromeOptions);
                        driver.Navigate().GoToUrl("https://v2.registrelo.go.cr/reports/12");

                        networkInterceptor = driver?.Manage().Network;
                        networkInterceptor?.AddRequestHandler(networkRequestHandler());
                        networkInterceptor?.AddResponseHandler(networkResponseHandler());

                        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        iniciarCampos();
                        IsInitialized = true;
                        OnInitialized?.Invoke(this, EventArgs.Empty);
                    }

                }
            }
        }

        public event EventHandler? OnInitialized;

        private NetworkRequestHandler networkRequestHandler()
        {
            return new NetworkRequestHandler
            {
                RequestMatcher = request => request.Url.Contains("/reports/v1/publicreports/getPublicReport") && request.Headers.ContainsKey("api-token") && request.Method == HttpMethod.Get.Method,

                RequestTransformer = request =>
                {
                    interceptedRequest = new HttpRequestMessage(HttpMethod.Get, request.Url);
                    foreach (var header in request.Headers)
                    {
                        interceptedRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                    return request;
                }
            };

        }
        private NetworkResponseHandler networkResponseHandler()
        {
            return new NetworkResponseHandler()
            {
                ResponseMatcher = response => response.StatusCode == 200 && response.Url.Contains("https://gateway.registrelo.go.cr/reports/v1/publicreports/getPublicReport"),
                ResponseTransformer = response =>
                {


                    interceptedResponse = new HttpResponseMessage((System.Net.HttpStatusCode)response.StatusCode);
                    foreach (var header in response.Headers)
                    {
                        interceptedResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        interceptedResponse.Content = new StringContent(response.Content.ReadAsString());
                    }

                    return response;

                }

            };
        }
        private void iniciarCampos()
        {
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").ToString() == "complete");
            var advanceSearch = EsperarQueElementoSeaClickable(By.ClassName("advance-search"));
            wait.Until(d => advanceSearch.Displayed);
            advanceSearch.Click();

            if (driver?.FindElements(By.ClassName("times-icon")).Count > 0)
            {
                var fechas = wait.Until(d => d.FindElements(By.ClassName("times-icon")));
                wait.Until(d => fechas.First().Displayed);
                fechas?.First().Click();
                fechas?.Last().Click();

            }

            var tipo = wait.Until(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(2) > div:nth-child(2) > div > div:nth-child(1) > input")));
            var estado = wait.Until(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(12) > div:nth-child(2) > div > div:nth-child(1) > input")));


            tipo?.Click();
            var listaTipo = wait.Until(d => d.FindElement(By.Id("downshift-1-item-6")));
            listaTipo?.Click();
            estado?.Click();
            var listaEstado = EsperarQueElementoSeaClickable(By.Id("downshift-3-item-0"));
            listaEstado.Click();
            IsInitialized = true;
           
            OnInitialized?.Invoke(this, EventArgs.Empty);

        }

        public ApiResponse<ProductData> CambiarTipo(TipoProducto tipoProducto)
        {
            ApiResponse<ProductData>? payload = new ApiResponse<ProductData>();
            try
            {
                var tipo = wait.Until(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(2) > div:nth-child(2) > div > div:nth-child(1) > input")));
                tipo?.Click();
                var listaTipo = wait.Until(d => d.FindElement(By.Id($"downshift-1-item-{((int)tipoProducto)}")));
                listaTipo?.Click();
                payload.Status = HttpStatusCode.OK.ToString();
                payload.StatusCode = (int)HttpStatusCode.OK;
                payload.Message = "El tipo de producto se ha Actualizado";
            }
            catch (Exception err)
            {
                payload.Status = HttpStatusCode.InternalServerError.ToString();
                payload.StatusCode = (int)HttpStatusCode.InternalServerError;
                payload.Errors = err.Message;
                payload.Message = "Error al cambiar el tipo de producto.";
            }
            return payload;
        }
        public async Task<ApiResponse<ProductData>> BuscarRegistroSanitario(string nombreProducto)
        {
            interceptedRequest = null;
            interceptedResponse = null;
            respuestas.Clear();
            ApiResponse<ProductData>? payload = new ApiResponse<ProductData>();

            try
            {

                await networkInterceptor.StartMonitoring();

                var nombre = wait.Until(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(3) > div:nth-child(2) > div > input")));
                nombre?.Clear();
                nombre?.SendKeys(nombreProducto);

                var reportButton = wait.Until(d => d.FindElement(By.CssSelector("#reportFilterForm button[type='submit']")));

                reportButton?.Click();

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
        }
        private IWebElement EsperarQueElementoSeaClickable(By by)
        {
            return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
        }

        private async Task<ApiResponse<ProductData>?> ManejadorInterceptorRespuesta()
        {
            ApiResponse<ProductData> payload = new ApiResponse<ProductData>();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var responseBody = await interceptedResponse.Content.ReadAsStringAsync();
            payload = JsonSerializer.Deserialize<ApiResponse<ProductData>>(responseBody, options);

            return payload;
        }


        public async Task<ApiResponse<ProductData>> paginar(BotonesPaginador boton)
        {
            interceptedRequest = null;
            interceptedResponse = null;
            respuestas.Clear();
            ApiResponse<ProductData>? payload = new ApiResponse<ProductData>();
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
            var emptyState = false;
            try {
                emptyState = driver.FindElement(By.ClassName("empty-state")).Displayed;

            } catch (NoSuchElementException) {
               emptyState = false;
            } catch (Exception) {
                emptyState = false;
            }
             if(emptyState) {
                    payload.StatusCode = 204;
                    payload.Message = "Aun no hay datos que mostrar.";
                    return payload;
                };
            try
            {
                var botonPaginacion = EsperarQueElementoSeaClickable(By.ClassName(clase));
                var botonEstaDeshabilitado = botonPaginacion.GetAttribute("style").Contains("cursor: not-allowed;");

                if (botonEstaDeshabilitado && payload.Data.Count == 0)
                {
                    payload.StatusCode = 204;
                    return payload;
                };
                if (botonEstaDeshabilitado)
                {
                    payload.StatusCode = 200;
                    payload.Message = "Última página alcanzada.";
                    return payload;
                }
                botonPaginacion.Click();
                await networkInterceptor.StartMonitoring();

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
                    if (botonPaginacion.GetAttribute("style").Contains("cursor: not-allowed;"))
                    {
                        payload.Message = "Última página alcanzada.";
                    }
                }
                await networkInterceptor.StopMonitoring();


            }
            catch (Exception ex)
            {
                payload.Errors = ex.Message;
                payload.StatusCode = 500;
                payload.Message = "Ocurrió un error en la paginación.";

            }

            return payload;
        }
        public void Dispose()
        {
            driver?.Quit();
            driver = null;
        }

        internal ApiResponse<string> ObtenerTipos()
        {
            ApiResponse<string> payload = new ApiResponse<string>();
            try
            {
                var tipo = wait.Until(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(2) > div:nth-child(2) > div > div:nth-child(1) > input")));
                tipo?.Click();
                var listaTipo = wait.Until(d => d.FindElement(By.Id("downshift-1-menu")));
                var tipos = listaTipo?.FindElements(By.CssSelector("li"));
                payload.Data = tipos.Select(t => t.Text).ToList();
                tipo?.Click();
                payload.StatusCode = 200;
                payload.Message = "Tipos obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                payload.StatusCode = 500;
                payload.Message = $"Error al obtener los tipos: {ex.Message}";
            }
            return payload;
        }
    }
}

public enum BotonesPaginador
{
    siguiente,
    anterior
}

public enum TipoProducto
{
    Alimento = 0,
    Cosmético = 1,
    Químico = 2,
    EquipoYMaterialBiomédico = 3,
    MedicamentosBiológicos = 4,
    MedicamentosBiológicosHomologados = 5,
    Medicamentos = 6,
    MedicamentosHomologados = 7,
    Plaguicidas = 8,
    MateriasPrimas = 9,
    ProductosHigiénicos = 10,
    ProductosNaturales = 11
}