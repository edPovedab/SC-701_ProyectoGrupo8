using HtmlAgilityPack;
using PW.News8.Shared.Interfaces;

namespace PW.News8.API.Readers
{
    /// <summary>
    /// Lee una fuente HTML y extrae ítems genéricos buscando el contenedor
    /// repetido más común en la página (heurística simple: agrupa nodos por
    /// su combinación de tag + clase CSS y toma el grupo más numeroso).
    /// Pensado como fallback simple; fuentes HTML muy particulares podrían
    /// necesitar selectores CSS específicos configurados en AppSettings.
    /// </summary>
    public class HtmlSourceReader : ISourceReader
    {
        public string SupportedType => "html";

        private readonly IHttpClientFactory _httpClientFactory;

        public HtmlSourceReader(IHttpClientFactory httpClientFactory)
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

                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                // Candidatos típicos de "tarjeta de noticia" en sitios de medios
                var candidateSelectors = new[] { "//article", "//*[contains(@class,'card')]",
                                                  "//*[contains(@class,'post')]", "//*[contains(@class,'item')]" };

                HtmlNodeCollection? nodes = null;
                foreach (var selector in candidateSelectors)
                {
                    var found = doc.DocumentNode.SelectNodes(selector);
                    if (found is { Count: > 1 })
                    {
                        nodes = found;
                        break;
                    }
                }

                if (nodes is null)
                {
                    result.Success = false;
                    result.ErrorMessage = "No se encontraron elementos repetidos reconocibles (article/card/post/item) en el HTML.";
                    return result;
                }

                foreach (var node in nodes)
                {
                    var titleNode = node.SelectSingleNode(".//h1|.//h2|.//h3");
                    var linkNode = node.SelectSingleNode(".//a[@href]");
                    var imgNode = node.SelectSingleNode(".//img[@src]");

                    var item = new Dictionary<string, object>
                    {
                        ["title"] = titleNode?.InnerText.Trim() ?? string.Empty,
                        ["url"] = linkNode?.GetAttributeValue("href", string.Empty) ?? string.Empty,
                        ["imageUrl"] = imgNode?.GetAttributeValue("src", string.Empty) ?? string.Empty,
                        ["text"] = HtmlEntity.DeEntitize(node.InnerText).Trim()
                    };

                    result.Items.Add(item);
                }

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error al leer o parsear la fuente HTML: {ex.Message}";
                return result;
            }
        }
    }
}