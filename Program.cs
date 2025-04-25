var builder = WebApplication.CreateBuilder(args);

// Servicios necesarios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Esto s� es �til aunque no uses Swagger

var app = builder.Build();

// IMPORTANTE: Habilita el routing ANTES del mapeo de controladores
app.UseRouting();
app.UseStaticFiles();
app.UseAuthorization();

// Mapea los controladores con su routing
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Configurar el puerto din�mico de Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

app.Run();
