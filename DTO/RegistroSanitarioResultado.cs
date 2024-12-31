using System.Text.Json.Serialization;
using BuscarRegistroSanitarioService.DTO;

namespace BuscarRegistroSanitarioCR.DTO;
public class RegistroSanitarioResultado<T>
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("errors")]
    public object Errors { get; set; }

    [JsonPropertyName("data")]
    public List<T> Data { get; set; }

    [JsonPropertyName("paginate")]
    public Paginate Paginate { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    public RegistroSanitarioResultado()
    {
        Message = string.Empty;
        Data = new List<T>();
        StatusCode = 200;
        Status = "OK";
        Errors = string.Empty;
        Paginate = new Paginate();

    }
}

