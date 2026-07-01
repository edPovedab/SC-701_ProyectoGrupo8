using System.Text.Json;
using PW.News8.Shared.Interfaces;

namespace PW.News8.API.Readers
{
    /// <summary>
    /// Lee una fuente cuyo contenido es JSON (array de objetos o un objeto
    /// contenedor con un array adentro) y lo convierte a una lista genérica
    /// de diccionarios para que pueda mostrarse en el frontend sin acoplar
    /// el reader a un modelo de noticia específico.
    /// </summary>
    public class JsonSourceReader : ISourceReader
    {
        public string SupportedType => "json";

        private readonly IHttpClientFactory _httpClientFactory;

        public JsonSourceReader(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SourceReadResult> ReadAsync(string url, string? secret = null)
        {
            var result = new SourceReadResult();
            try
            {
                var client = _httpClientFactory.CreateClient();

                if (!string.IsNullOrWhiteSpace(secret))
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {secret}");

                using var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    result.Success = false;
                    result.ErrorMessage = $"La fuente respondió con estado {(int)response.StatusCode}.";
                    return result;
                }

                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);

                // Soporta tanto un array raíz [...] como un objeto { "items": [...] }
                var root = document.RootElement;
                var arrayElement = root.ValueKind == JsonValueKind.Array
                    ? root
                    : FindFirstArrayProperty(root);

                if (arrayElement is null)
                {
                    // No es un array: tratamos el objeto raíz como un único ítem
                    result.Items.Add(ToDictionary(root));
                }
                else
                {
                    foreach (var element in arrayElement.Value.EnumerateArray())
                        result.Items.Add(ToDictionary(element));
                }

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error al leer o parsear la fuente JSON: {ex.Message}";
                return result;
            }
        }

        private static JsonElement? FindFirstArrayProperty(JsonElement obj)
        {
            if (obj.ValueKind != JsonValueKind.Object)
                return null;

            foreach (var property in obj.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                    return property.Value;
            }
            return null;
        }

        private static Dictionary<string, object> ToDictionary(JsonElement element)
        {
            var dict = new Dictionary<string, object>();
            if (element.ValueKind != JsonValueKind.Object)
            {
                dict["value"] = element.ToString();
                return dict;
            }

            foreach (var property in element.EnumerateObject())
            {
                dict[property.Name] = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                    JsonValueKind.Number => property.Value.GetRawText(),
                    JsonValueKind.True or JsonValueKind.False => property.Value.GetBoolean(),
                    JsonValueKind.Null => string.Empty,
                    _ => property.Value.GetRawText() // objetos/arrays anidados se guardan como texto
                };
            }
            return dict;
        }
    }
}