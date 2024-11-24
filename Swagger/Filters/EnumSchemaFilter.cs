
using BuscarRegistroSanitarioService.Enums;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuscarRegistroSanitarioService.Swagger.Filters;
public class EnumSchemaFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var parameter in operation.Parameters)
        {
            if (parameter.Name == "tipoProducto")
            {
                var enumValues = new List<IOpenApiAny>();
                enumValues.AddRange(Enum.GetNames(typeof(TipoProducto))
                   .Select(name => new OpenApiString(name) as IOpenApiAny));
                parameter.Schema.Enum = enumValues;
                parameter.Description += " Seleccione una opción.";
            }
        }
    }
}
