using N2K_BackboneBackEnd.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using N2K_BackboneBackEnd.Services;
using N2K_BackboneBackEnd.Models;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using N2K_BackboneBackEnd.Helpers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using N2K_BackboneBackEnd.Hubs;


var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<ISiteChangesService, SiteChangesService>();
builder.Services.AddScoped<ISiteDetailsService, SiteDetailsService>();
builder.Services.AddScoped<IHarvestedService, HarvestedService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IEULoginService, EULoginService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<IUnionListService, UnionListService>();
builder.Services.AddScoped<IReleaseService, ReleaseService>();
builder.Services.AddScoped<ISiteLineageService, SiteLineageService>();
builder.Services.AddScoped<ISDFService, SDFService>();
builder.Services.AddScoped<IReportingPeriodService, ReportingPeriodService>();
builder.Services.AddScoped<IDownloadService, DownloadService>();
builder.Services.AddScoped<IExtractionService, ExtractionService>();
builder.Services.AddScoped<IHostedService, BackgroundTasks>();

builder.Services.AddTransient<IFireForgetRepositoryHandler, FireForgetRepositoryHandler>();
//builder.Services.AddHostedService<FMELongRunningService>();
builder.Services.AddSingleton<IBackgroundSpatialHarvestJobs, BackgroundSpatialHarvestJobs>();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[] { "text/plain", "application/json", "text/json" };
});
builder.Services.Configure<GzipCompressionProviderOptions>
   (opt =>
   {
       opt.Level = CompressionLevel.SmallestSize;
   }
);


builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddDbContext<N2KBackboneContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("N2K_BackboneBackEndContext"));
});

builder.Services.AddDbContext<N2KReleasesContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("N2K_ReleasesBackEndContext"));
});

builder.Services.AddDbContext<N2K_VersioningContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("N2K_VersioningBackEndContext"));
});


builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});


builder.Services.AddControllersWithViews();
builder.Services.Configure<ConfigSettings>(builder.Configuration.GetSection("GeneralSettings"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "N2KBacboneAPI", Version = "v1" });
    c.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                new List<string>()
            }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "EULoginSchema";
})
.AddScheme<ChangeDetectionHashAuthenticationSchemeOptions, ChangeDetectionHashAuthenticationHandler>("EULoginSchema", null);

builder.Services.AddMemoryCache();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddRouting(options =>
{
    options.ConstraintMap.Add("string", typeof(RouteAlphaNumericConstraint));
    options.ConstraintMap.Add("Status", typeof(RouteStatusConstraint));
    options.ConstraintMap.Add("level", typeof(RouteLevelConstraint));
});

var app = builder.Build();
// <snippet_UseWebSockets>
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};
app.UseWebSockets(webSocketOptions);

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .WithExposedHeaders("Content-Disposition")
    .AllowCredentials()); // allow credentials

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}
app.UseResponseCompression();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
    RequestPath = new PathString("/Resources")
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/ws");

app.Run();
