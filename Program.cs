using BuscarRegistroSanitarioService;
using BuscarRegistroSanitarioService.services;
using BuscarRegistroSanitarioService.Swagger.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var scrapingService = host.Services.GetRequiredService<ScrapingService>();
        scrapingService.inicializar();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine("Interrupción Ctrl+C detectada. Cerrando la aplicación...");
            scrapingService.Dispose();
            eventArgs.Cancel = true;
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
        {
            Console.WriteLine("Señal de cierre del sistema recibida. Cerrando la aplicación...");
            scrapingService.Dispose();
        };


        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(() =>
        {
            scrapingService.Dispose();
            Console.WriteLine("ScrapingService liberado al detener la aplicación.");
        });

        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddHostedService<Worker>();
        services.AddControllers();
        services.AddSingleton<ScrapingService>();
        services.AddSwaggerGen();
        services.AddSwaggerGen(c =>
        {   
            c.OperationFilter<EnumSchemaFilter>();
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Registro Sanitario API",
                Version = "v1",
                Description = "API para consultar registros sanitarios de productos.",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Alejandro Miranda Rodríguez",
                    Email = "",
                    Url = new Uri("https://github.com/EskenderDev/BuscarRegistroSanitarioCR")
                },
                License = new Microsoft.OpenApi.Models.OpenApiLicense
                {
                    Name = "Licencia MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/error");
            app.UseHsts();
        }
        app.UseStatusCodePagesWithReExecute("/Home/error", "?statusCode={0}");
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Registro Sanitario API V1");
            c.RoutePrefix = string.Empty;
            c.DefaultModelExpandDepth(2);
            c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            c.DefaultModelsExpandDepth(-1);
        });
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}