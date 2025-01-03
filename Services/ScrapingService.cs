
using BuscarRegistroSanitarioService.Enums;
using BuscarRegistroSanitarioService.DTO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using BuscarRegistroSanitarioService.Hubs;
using Microsoft.AspNetCore.SignalR;
using BuscarRegistroSanitarioService.Loggin;
using BuscarRegistroSanitarioService.Exceptions;
using OpenQA.Selenium.Remote;
using BuscarRegistroSanitarioCR.DTO;

namespace BuscarRegistroSanitarioService.services
{

    public class ScrapingService(IHubContext<NotificationHub> hubContext, ILoggerManager logger)
    {
        private readonly IHubContext<NotificationHub> _hubContext = hubContext;
        private readonly ILoggerManager _logger = logger;
        private static IWebDriver? driver;
        private List<(string Origin, string ResponseUrl, string ResponseBody)> respuestas = new List<(string, string, string)>();
        private HttpRequestMessage? interceptedRequest = null;
        private HttpResponseMessage? interceptedResponse = null;
        private INetwork? networkInterceptor;
        private WebDriverWait? wait;
        private readonly object lockObj = new object();
        public bool IsInitialized { get; private set; } = false;
        private ChromeDriverService? service;
        public event EventHandler? OnInitialized;

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1); // Semáforo para control de acceso

        public async Task InicializarAsync()
        {
            await semaphore.WaitAsync(); // Adquirir el semáforo

            try
            {
                if (!IsDriverAlive())
                {
                    try{
                        InitializedDriver();
                    } catch(Exception ex){
                        _logger.LogError($"Error al iniciar ChromeDriver: {ex.Message}", ex);
                    }
                }
            }
            finally
            {
                semaphore.Release();
                await EnsureDriverIsRunningAsync();
            }
        }

        private void InitializedDriver()
        {
            service = ChromeDriverService.CreateDefaultService();
            service.LogPath = "chromedriver.log";
            service.EnableVerboseLogging = true;

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--disable-dev-shm-usage");
            //chromeOptions.AddArgument("--remote-debugging-port=9230");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--blink-settings=imagesEnabled=false");

            try
            {
                driver = new ChromeDriver(service, chromeOptions);
                _logger.LogInfo("ChromeDriver iniciado exitosamente.");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al iniciar ChromeDriver: {ex.Message}", ex);
            }
        }
        private bool IsDriverAlive()
        {
            return service?.ProcessId > 0 && service.IsRunning && driver != null && driver.WindowHandles.Count > 0 && driver.Url.Contains("https://v2.registrelo.go.cr/reports/12");
        }

        private async Task EnsureDriverIsRunningAsync()
        {
            if (!IsDriverAlive())
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        if (driver != null && !IsDriverAlive())
                        {
                            _logger.LogWarning("ChromeDriver ha dejado de funcionar. Reiniciando...");
                            InitializedDriver();

                            // Esperamos a que el driver esté listo de nuevo antes de continuar
                            await driver.Navigate().GoToUrlAsync("https://v2.registrelo.go.cr/reports/12");
                            _logger.LogInfo("Página cargada exitosamente.");

                            networkInterceptor = driver?.Manage().Network;
                            //networkInterceptor?.AddRequestHandler(networkRequestHandler());
                            networkInterceptor?.AddResponseHandler(networkResponseHandler());

                            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                            await iniciarCampos();

                            IsInitialized = true;
                            OnInitialized?.Invoke(this, EventArgs.Empty);
                            _logger.LogInfo("ChromeDriver reiniciado exitosamente.");

                        }
                        Thread.Sleep(20000);
                    }
                }
                );
            }
        }
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
        private async Task iniciarCampos()
        {
            if (wait != null)
            {
                await wait.UntilAsync(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").ToString() == "complete");
                var advanceSearch = await EsperarQueElementoSeaClickableAsync(By.ClassName("advance-search"));
                await wait.UntilAsync(d => advanceSearch.Displayed);
                advanceSearch.Click();

                if (driver?.FindElements(By.ClassName("times-icon")).Count > 0)
                {
                    var fechas = await wait.UntilAsync(d => d.FindElements(By.ClassName("times-icon")));
                    if (fechas != null)
                    {
                        await wait.UntilAsync(d => fechas.First().Displayed);
                    }
                    fechas?.First().Click();
                    fechas?.Last().Click();

                }

                var tipo = await wait.UntilAsync(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(2) > div:nth-child(2) > div > div:nth-child(1) > input")));
                var estado = await wait.UntilAsync(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(12) > div:nth-child(2) > div > div:nth-child(1) > input")));


                tipo?.Click();
                var listaTipo = await wait.UntilAsync(d => d.FindElement(By.Id("downshift-1-item-6")));
                listaTipo?.Click();
                estado?.Click();
                var listaEstado = await EsperarQueElementoSeaClickableAsync(By.Id("downshift-3-item-0"));
                listaEstado.Click();
                IsInitialized = true;

                OnInitialized?.Invoke(this, EventArgs.Empty);
            }

        }

        public RegistroSanitarioResultado<string>? CambiarTipo(TipoProducto tipoProducto)
        {
            RegistroSanitarioResultado<string>? payload = new RegistroSanitarioResultado<string>();
            payload.Data = new List<string>();
            try
            {
                var tipo = wait?.Until(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(2) > div:nth-child(2) > div > div:nth-child(1) > input")));
                tipo?.Click();
                var listaTipo = wait?.Until(d => d.FindElement(By.Id($"downshift-1-item-{((int)tipoProducto)}")));
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
        public async Task<RegistroSanitarioResultado<ProductData>> BuscarRegistroSanitario(string nombreProducto)
        {
            interceptedRequest = null;
            interceptedResponse = null;
            respuestas.Clear();
            RegistroSanitarioResultado<ProductData>? payload = new RegistroSanitarioResultado<ProductData>();

            payload.Data = new List<ProductData>();

            try
            {

                if (networkInterceptor != null)
                {
                    await networkInterceptor.StartMonitoring();

                    var nombre = await wait.UntilAsync(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(3) > div:nth-child(2) > div > input")));
                    nombre?.Clear();
                    nombre?.SendKeys(nombreProducto);

                    var reportButton = wait?.Until(d => d.FindElement(By.CssSelector("#reportFilterForm button[type='submit']")));

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
                        payload = await ManejadorInterceptorRespuestaAsync();
                    }

                    await networkInterceptor.StopMonitoring();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}", ex);

                if (ex.Message.Contains("Object reference not set to an instance of an object."))
                {
                    throw new DriverException("Problemas al iniciar el networkInterceptor", ex);
                }

            }
            return payload;
        }
        private async Task<IWebElement> EsperarQueElementoSeaClickableAsync(By by)
        {
            if (wait == null)
            {
                throw new InvalidOperationException("WebDriverWait instance is not initialized.");
            }
            return await Task.Run(() => wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by)));
        }

        private async Task<RegistroSanitarioResultado<ProductData>> ManejadorInterceptorRespuestaAsync()
        {
            RegistroSanitarioResultado<ProductData> payload = new RegistroSanitarioResultado<ProductData>();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var responseBody = interceptedResponse != null ? await interceptedResponse.Content.ReadAsStringAsync() : string.Empty;
            payload = JsonSerializer.Deserialize<RegistroSanitarioResultado<ProductData>>(responseBody, options) ?? new RegistroSanitarioResultado<ProductData>();

            return payload;
        }


        public async Task<RegistroSanitarioResultado<ProductData>> paginar(BotonesPaginador boton)
        {
            interceptedRequest = null;
            interceptedResponse = null;
            respuestas.Clear();
            RegistroSanitarioResultado<ProductData>? payload = new RegistroSanitarioResultado<ProductData>();
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
            try
            {
                emptyState = driver?.FindElement(By.ClassName("empty-state"))?.Displayed ?? false;

            }
            catch (NoSuchElementException)
            {
                emptyState = false;
            }
            catch (Exception)
            {
                emptyState = false;
            }
            if (emptyState)
            {
                payload.StatusCode = 204;
                payload.Message = "Aun no hay datos que mostrar.";
                return payload;
            };
            try
            {
                var botonPaginacion = await EsperarQueElementoSeaClickableAsync(By.ClassName(clase));
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
                if (networkInterceptor != null)
                {
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
                        payload = await ManejadorInterceptorRespuestaAsync();
                        if (botonPaginacion.GetAttribute("style").Contains("cursor: not-allowed;"))
                        {
                            payload.Message = "Última página alcanzada.";
                        }
                    }
                    await networkInterceptor.StopMonitoring();
                }


            }
            catch (Exception ex)
            {
                payload.Errors = ex.Message;
                payload.StatusCode = 500;
                payload.Message = "Ocurrió un error en la paginación.";

            }

            return payload;
        }
        internal RegistroSanitarioResultado<string> ObtenerTipos()
        {
            RegistroSanitarioResultado<string> payload = new RegistroSanitarioResultado<string>();
            try
            {
                var tipo = wait?.Until(d => d.FindElement(By.CssSelector("#reportFilterForm > div > div:nth-child(2) > div:nth-child(2) > div > div:nth-child(1) > input")));
                tipo?.Click();
                var listaTipo = wait?.Until(d => d.FindElement(By.Id("downshift-1-menu")));
                var tipos = listaTipo?.FindElements(By.CssSelector("li"));
                payload.Data = tipos?.Select(t => t.Text).ToList() ?? new List<string>();
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
        public async Task Dispose()
        {
            driver?.Quit();
            driver = null;
            await _hubContext.Clients.All.SendAsync("ReceiveStatus", "ChromeDriver cerrado.");
        }

    }
}

public static class WebDriverWaitExtensions
{
    public static async Task<T> UntilAsync<T>(this WebDriverWait wait, Func<IWebDriver, T> condition)
    {
        return await Task.Run(() => wait.Until(condition));
    }
}

