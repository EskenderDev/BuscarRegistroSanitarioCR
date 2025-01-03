using BuscarRegistroSanitarioService;
using BuscarRegistroSanitarioService.Hubs;
using BuscarRegistroSanitarioService.Loggin;
using BuscarRegistroSanitarioService.services;
using BuscarRegistroSanitarioService.Swagger.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var serviceType = Environment.GetEnvironmentVariable("SERVICE_TYPE");

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });
          services.AddCors(options =>
        {
            options.AddPolicy("MobilePolicy", app =>
            {
                app
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
        });

        services.AddSingleton<ILoggerManager, LogManager>();

        services.AddAuthorization();

        if(serviceType == "Worker") {
            services.AddHostedService<Worker>();
        }

        services.AddControllers();
        services.AddSignalR();
        services.AddSingleton<ScrapingService>();
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
        services.AddHealthChecks();

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                       ForwardedHeaders.XForwardedProto |
                       ForwardedHeaders.XForwardedHost
        });
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/error");
            app.UseHsts();
        }
        app.UseCors("MobilePolicy");
        app.UseStatusCodePagesWithReExecute("/Home/error", "?statusCode={0}");
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
            endpoints.MapHub<NotificationHub>("/notifications");
            endpoints.MapHealthChecks("/health");
        });
    }
}