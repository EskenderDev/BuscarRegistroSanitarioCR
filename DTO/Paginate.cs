using System.Text.Json.Serialization;

namespace BuscarRegistroSanitarioService.DTO;


public class Paginate
{
    [JsonPropertyName("skip")]
    public string Skip { get; set; }

    [JsonPropertyName("limit")]
    public string Limit { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}