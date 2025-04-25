using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Spotquickly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
        private readonly string clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");
        private readonly string redirectUri = Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI");

        [HttpGet("login")]
        public IActionResult Login()
        {
            // Validación por si faltan variables de entorno
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return StatusCode(500, "Spotify client credentials are not configured.");
            }

            var scopes = "user-read-email user-top-read";
            var spotifyUrl = $"https://accounts.spotify.com/authorize?response_type=code&client_id={clientId}&scope={Uri.EscapeDataString(scopes)}&redirect_uri={Uri.EscapeDataString(redirectUri)}";
            return Redirect(spotifyUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return StatusCode(500, "Spotify client credentials are not configured.");
            }
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Code parameter is missing");
            }

            var httpClient = new HttpClient();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            });

            var response = await httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            // Extraer el token de la respuesta de Spotify
            var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            //var accessToken = tokenResponse["access_token"];
            var accessToken = tokenResponse["access_token"];
            var refreshToken = tokenResponse.ContainsKey("refresh_token") ? tokenResponse["refresh_token"] : "";

            return Redirect($"https://spotquickly.onrender.com/?token={accessToken}&refresh_token={refreshToken}");


            // Redirigir al frontend con el token
            //return Redirect($"https://spotquickly.onrender.com/?token={accessToken}");
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetSpotifyProfile([FromQuery] string token)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync("https://api.spotify.com/v1/me");
            var content = await response.Content.ReadAsStringAsync();

            return Content(content, "application/json");
        }

        [HttpGet("top-tracks")]
        public async Task<IActionResult> GetTopTracks([FromQuery] string token)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync("https://api.spotify.com/v1/me/top/tracks?limit=50");

            var content = await response.Content.ReadAsStringAsync();

            // Agregar log para ver la respuesta cruda de Spotify
            Console.WriteLine($"Spotify Response: {content}");

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new
                {
                    message = "Error al obtener las canciones más escuchadas.",
                    statusCode = (int)response.StatusCode,
                    errorDetail = content
                });
            }

            // Deserializar la respuesta de Spotify
            var topTracksResponse = JsonConvert.DeserializeObject<TopTracksResponse>(content);

            // Verificar si hay canciones en la respuesta
            if (topTracksResponse?.Items == null || topTracksResponse.Items.Count == 0)
            {
                return NotFound("No se encontraron canciones.");
            }

            return Ok(topTracksResponse.Items);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromQuery] string refreshToken)
        {
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return StatusCode(500, "Spotify client credentials are not configured.");
            }

            try
            {
                var httpClient = new HttpClient();
                var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
                request.Content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken)
        });

                var response = await httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new { message = "Error al renovar el token de acceso", errorDetail = json });
                }

                var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                var newAccessToken = tokenResponse["access_token"];

                return Ok(new { access_token = newAccessToken });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error inesperado al renovar el token", error = ex.Message });
            }
        }



    }

    // Clase para mapear la respuesta de los top tracks de Spotify
    public class TopTracksResponse
    {
        [JsonProperty("items")]
        public List<Track> Items { get; set; }
    }

    // Clase para representar cada canción
    public class Track
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("artists")]
        public List<Artist> Artists { get; set; }

        [JsonProperty("album")]
        public Album Album { get; set; }
    }

    // Clase para representar el artista
    public class Artist
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("external_urls")]
        public ExternalUrls ExternalUrls { get; set; }
    }

    // Clase para representar la URL externa (Spotify)
    public class ExternalUrls
    {
        [JsonProperty("spotify")]
        public string Spotify { get; set; }
    }

    // Clase para representar el álbum
    public class Album
    {
        [JsonProperty("external_urls")]
        public ExternalUrls ExternalUrls { get; set; }
    }
}
