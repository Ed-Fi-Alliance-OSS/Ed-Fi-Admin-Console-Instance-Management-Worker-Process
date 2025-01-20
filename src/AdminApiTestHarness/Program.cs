var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet(
        "/adminconsole/instances",
        static () =>
        {
            return Results.Ok(new[] { new Tenant(1, "test #1", "Pending"), new Tenant(1, "test #2", "Pending") });
        }
    )
    .WithName("GetInstances");

app.Run();

record Tenant(int tenantId, string instanceName, string status);
