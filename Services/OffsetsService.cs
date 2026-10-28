using Bikuida.Models;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace Bikuida.Services
{
    public class OffsetsService
    {
        public async Task<JsonBinResponse> GetOffsetsAsync()
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                string url = "https://api.jsonbin.io/v3/b/6a50a521f5f4af5e297b85b9/latest";
                using var client = new HttpClient(handler);
                string json = await client.GetStringAsync(url);
                return JsonSerializer.Deserialize<JsonBinResponse>(json, options);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Bikuida", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

    }

}
