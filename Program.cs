var builder = WebApplication.CreateBuilder(args);

// Servicios necesarios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Esto sí es útil aunque no uses Swagger

var app = builder.Build();

// IMPORTANTE: Habilita el routing ANTES del mapeo de controladores
app.UseRouting();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();

// Mapea los controladores con su routing
app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/index.html");
});


// Configurar el puerto dinámico de Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

app.Run();
