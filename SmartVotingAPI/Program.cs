using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartVotingAPI.Data;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "Smart Voting API", Version = "v1" });
    o.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token_value}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    o.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddCors(o =>
{
    o.AddPolicy("corspolicy",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000", "https://smartvoting.cc").AllowAnyHeader().AllowAnyMethod();
        });
});
builder.Services.AddDbContext<PostgresDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("RDS_Postgres")));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings").GetSection("TokenSignature").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });


AWSOptions awsOptions = new AWSOptions
{
    Credentials = new BasicAWSCredentials(builder.Configuration.GetSection("AppSettings").GetValue<string>("AmazonAWS:Username"), builder.Configuration.GetSection("AppSettings").GetValue<string>("AmazonAWS:Password")),
    Region = RegionEndpoint.USEast1
};
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddApiVersioning(o =>
{
    o.ReportApiVersions = true;
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ApiVersionReader = new HeaderApiVersionReader("X-API-Version");
});

builder.Services.AddAWSService<IAmazonSimpleEmailService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Voting API");
    c.RoutePrefix = "";
});

app.UseHttpsRedirection();

app.UseCors("corspolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
