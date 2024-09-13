using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDb"))
);

var app = builder.Build();

app.MapGet("/db-name", async (AppDbContext dbContext) =>
{
    // Wait a little before and after, so we know two requests are handled at the same time.
    await Task.Delay(2500);
    var dbName = await dbContext.Database.SqlQueryRaw<string>("SELECT DB_NAME() as Value").SingleAsync();
    await Task.Delay(2500);
    return Results.Text(dbName);
});

app.Run();

public partial class Program;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options);
