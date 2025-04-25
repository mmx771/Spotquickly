using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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
            var accessToken = tokenResponse["access_token"];

            // Redirigir al frontend con el token
            return Redirect($"https://spotquickly.onrender.com/?token={accessToken}");

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

            var response = await httpClient.GetAsync("https://api.spotify.com/v1/me/top/tracks?limit=10");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new
                {
                    message = "Error al obtener las canciones más escuchadas.",
                    statusCode = (int)response.StatusCode,
                    errorDetail = content
                });
            }

            var topTracks = JsonConvert.DeserializeObject(content);
            return Ok(topTracks);
        }



    }
}
