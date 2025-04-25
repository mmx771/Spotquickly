var builder = WebApplication.CreateBuilder(args);

// Agregar servicios si los ten�s
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Desactivar redirecci�n HTTPS en Render
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// Eliminar cualquier intento de redirecci�n HTTPS
// app.UseHttpsRedirection();  <-- Esta l�nea debe estar comentada o eliminada

app.UseAuthorization();

app.MapControllers();

// Configurar el puerto din�mico de Render (esto es importante)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

app.Run();

