var builder = WebApplication.CreateBuilder(args);

// Agregar servicios si los tenés
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Desactivar redirección HTTPS en Render
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// Eliminar cualquier intento de redirección HTTPS
// app.UseHttpsRedirection();  <-- Esta línea debe estar comentada o eliminada

app.UseAuthorization();

app.MapControllers();

// Configurar el puerto dinámico de Render (esto es importante)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

app.Run();

