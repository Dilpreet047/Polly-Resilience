var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddHttpClient<ExternalApiService>()
    .AddPolicyHandler(ResiliencePolicy.FallbackPolicy)
    .AddPolicyHandler(ResiliencePolicy.RetryPolicy)
    .AddPolicyHandler(ResiliencePolicy.CircuitBreakerPolicy)
    .AddPolicyHandler(ResiliencePolicy.TimeoutPolicy);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.UseHttpsRedirection();

app.Run();
