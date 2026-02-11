using AITranslatorWebApp.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<AITranslatorService>();
builder.Services.AddSingleton<EmailParserService>();
builder.Services.AddScoped<EmailSenderService>();
builder.Services.AddControllersWithViews();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
var hfToken = builder.Configuration["HuggingFace:Token"];
// Register HttpClient
builder.Services.AddHttpClient();

builder.Services.AddHttpClient<AITranslatorService>();
// Add services
builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
