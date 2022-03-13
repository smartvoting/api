using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartVotingAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Smart Voting API", Version = "v1" });
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


AWSOptions awsOptions = new AWSOptions
{
    Credentials = new BasicAWSCredentials(builder.Configuration.GetSection("AccessKeys").GetValue<string>("AWS:AccessID"), builder.Configuration.GetSection("AccessKeys").GetValue<string>("AWS:SecretKey")),
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

app.UseAuthorization();

app.MapControllers();

app.Run();
