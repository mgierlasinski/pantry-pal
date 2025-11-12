using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using PantryPal.Api.Endpoints;
using PantryPal.Api.Exceptions;
using PantryPal.Api.Extensions;
using PantryPal.Api.Repositories;
using PantryPal.Api.Services;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<ArgumentExceptionHandler>();
builder.Services.AddExceptionHandler<InvalidOperationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(configure =>
{
    configure.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
    };
});

builder.Services.AddSupabase();
builder.Services.AddOpenRouter(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

if (builder.Environment.IsEnvironment("Test"))
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    var secret = builder.Configuration["Supabase:Auth:JwtSecret"]!;
    var bytes = Encoding.UTF8.GetBytes(secret);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(bytes),
        ValidIssuer = builder.Configuration["Supabase:Auth:Issuer"],
        ValidAudience = "authenticated"
    };
});

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Register repositories
builder.Services.AddScoped<IPantryRepository, PantryRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
builder.Services.AddScoped<IRecipesGenerationsRepository, RecipesGenerationsRepository>();
builder.Services.AddScoped<IRecipeRejectReasonsRepository, RecipeRejectReasonsRepository>();
builder.Services.AddScoped<IDietTypesRepository, DietTypesRepository>();
builder.Services.AddScoped<IPreferredCuisinesRepository, PreferredCuisinesRepository>();

// Register services
builder.Services.AddScoped<IPantryService, PantryService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<IDietTypesService, DietTypesService>();
builder.Services.AddScoped<IPreferredCuisinesService, PreferredCuisinesService>();
builder.Services.AddScoped<IRecipeRejectReasonsService, RecipeRejectReasonsService>();
builder.Services.AddScoped<IAIRecipeGeneratorService, AIRecipeGeneratorService>();

var app = builder.Build();
app.UseExceptionHandler();

// Enable authentication/authorization middleware
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => "Hello World!");
app.MapHealthChecks("/healthz");

app.RegisterPantryItemsEndpoints();
app.RegisterRecipesEndpoints();
app.RegisterUserPreferencesEndpoints();
app.RegisterDictionariesEndpoints();

app.Run();
