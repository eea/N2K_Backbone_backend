using N2K_BackboneBackEnd.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using N2K_BackboneBackEnd.Services;
using N2K_BackboneBackEnd.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<ISiteChangesService, SiteChangesService>();
builder.Services.AddScoped<IHarvestedService, HarvestedService>();
builder.Services.AddScoped<IEULoginService, EULoginService>();

builder.Services.AddCors();
builder.Configuration.AddJsonFile("appsettings.json");


builder.Services.AddDbContext<N2KBackboneContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("N2K_BackboneBackEndContext"));

});


builder.Services.AddDbContext<N2KBackboneReadOnlyContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("N2K_BackboneBackEndContext"));
});


builder.Services.AddDbContext<N2K_VersioningContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("N2K_VersioningBackEndContext"));
});



//builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews();
builder.Services.Configure<ConfigSettings>(builder.Configuration.GetSection("GeneralSettings"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "N2KBacboneAPI", Version = "v1" });
});



builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials()); // allow credentials
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
