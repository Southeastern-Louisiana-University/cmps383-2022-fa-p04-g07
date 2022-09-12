using FA22.P04.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// sets up our database connection
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")));

// ensures that controllers can be resolved
builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// migrates and seeds the database when starting up
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    MigrateAndSeed.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// maps the URLs on controllers to the methods on those classes
app.MapControllers();

app.Run();

//see: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0
// Hi 383 - this is added so we can test our web project automatically. More on that later
public partial class Program { }