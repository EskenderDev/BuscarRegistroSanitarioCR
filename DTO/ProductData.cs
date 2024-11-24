using System.Text.Json.Serialization;

namespace BuscarRegistroSanitarioService.DTO;

 public class ProductData
    {
        [JsonPropertyName("lineNumber")]
        public int LineNumber { get; set; }
        
        [JsonPropertyName("registerNumber")]
        public string RegisterNumber { get; set; }
        
        [JsonPropertyName("type")]
        public int Type { get; set; }
        
        [JsonPropertyName("typeName")]
        public string TypeName { get; set; }
        
        [JsonPropertyName("subType")]
        public int SubType { get; set; }
        
        [JsonPropertyName("subTypeName")]
        public string SubTypeName { get; set; }
        
        [JsonPropertyName("productName")]
        public string ProductName { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; }
        
        [JsonPropertyName("countrySourceId")]
        public int CountrySourceId { get; set; }
        
        [JsonPropertyName("countrySource")]
        public string CountrySource { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("expiredAt")]
        public DateTime ExpiredAt { get; set; }
        
        [JsonPropertyName("status")]
        public int Status { get; set; }
        
        [JsonPropertyName("statusName")]
        public string StatusName { get; set; }
        
        [JsonPropertyName("isSimplified")]
        public int IsSimplified { get; set; }
        
        [JsonPropertyName("isRegional")]
        public int IsRegional { get; set; }
        
        [JsonPropertyName("containGluten")]
        public bool? ContainGluten { get; set; }
        
        [JsonPropertyName("isSupplement")]
        public int IsSupplement { get; set; }
        
        [JsonPropertyName("brand")]
        public string Brand { get; set; }
        
        [JsonPropertyName("parentId")]
        public int ParentId { get; set; }
        
        [JsonPropertyName("parentName")]
        public string ParentName { get; set; }
        
        [JsonPropertyName("manufacturers")]
        public string Manufacturers { get; set; }
        
        [JsonPropertyName("headline")]
        public string Headline { get; set; }
        
        [JsonPropertyName("importers")]
        public string Importers { get; set; }
        
        [JsonPropertyName("useRegistry")]
        public object UseRegistry { get; set; }
        
        [JsonPropertyName("therapeuticEquivalences")]
        public int TherapeuticEquivalences { get; set; }
        
        [JsonPropertyName("isRecognitionClinicalStudies")]
        public int IsRecognitionClinicalStudies { get; set; }
    }