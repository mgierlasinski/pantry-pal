using Supabase;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(provider =>
{
    var url = builder.Configuration["Supabase:Url"]!;
    var key = builder.Configuration["Supabase:AnonKey"]!;
    var options = new SupabaseOptions { AutoConnectRealtime = true };

    return new Client(url, key, options);
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
