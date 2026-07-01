using System.Xml.Linq;
using PW.News8.Shared.Interfaces;

namespace PW.News8.API.Readers
{
    /// <summary>
    /// Lee una fuente XML/RSS. Soporta el formato RSS estándar (canal con
    /// nodos &lt;item&gt;) y, como respaldo genérico, cualquier XML tomando
    /// los hijos directos del primer nodo repetido que encuentre.
    /// </summary>
    public class XmlSourceReader : ISourceReader
    {
        public string SupportedType => "xml";

        private readonly IHttpClientFactory _httpClientFactory;

        public XmlSourceReader(IHttpClientFactory httpClientFactory)
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

                var content = await client.GetStringAsync(url);
                var document = XDocument.Parse(content);

                // Caso 1: RSS estándar -> <rss><channel><item>...</item></channel></rss>
                var rssItems = document.Descendants("item").ToList();
                if (rssItems.Count > 0)
                {
                    foreach (var item in rssItems)
                        result.Items.Add(ToDictionary(item));

                    result.Success = true;
                    return result;
                }

                // Caso 2: XML genérico -> tomamos el elemento que más se repite como "item"
                var root = document.Root;
                if (root is null)
                {
                    result.Success = false;
                    result.ErrorMessage = "El XML no tiene un elemento raíz.";
                    return result;
                }

                var mostCommonGroup = root.Elements()
                    .GroupBy(e => e.Name)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                if (mostCommonGroup is not null && mostCommonGroup.Count() > 1)
                {
                    foreach (var element in mostCommonGroup)
                        result.Items.Add(ToDictionary(element));
                }
                else
                {
                    // Si no hay elementos repetidos, tratamos el root como un único ítem
                    result.Items.Add(ToDictionary(root));
                }

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error al leer o parsear la fuente XML: {ex.Message}";
                return result;
            }
        }

        private static Dictionary<string, object> ToDictionary(XElement element)
        {
            var dict = new Dictionary<string, object>();
            foreach (var child in element.Elements())
            {
                // Si la clave se repite (ej. <category> múltiples veces), se concatena
                var key = child.Name.LocalName;
                var value = child.Value.Trim();

                if (dict.ContainsKey(key))
                    dict[key] = $"{dict[key]}, {value}";
                else
                    dict[key] = value;
            }

            if (dict.Count == 0 && !string.IsNullOrWhiteSpace(element.Value))
                dict["value"] = element.Value.Trim();

            return dict;
        }
    }
}