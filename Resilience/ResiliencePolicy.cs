

using System.Net;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;

static class ResiliencePolicy
{
    public static IAsyncPolicy<HttpResponseMessage> RetryPolicy =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .OrResult(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                3,
                retry => TimeSpan.FromSeconds(1),
                (outcome, delay, retry, ctx) =>
                {
                    Console.WriteLine($"Retry attempt {retry}");
                });
    
    public static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => !r.IsSuccessStatusCode)
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(20),
                onBreak: (_, _) => Console.WriteLine("Circuit OPEN"),
                onReset: () => Console.WriteLine("Circuit CLOSED"),
                onHalfOpen: () => Console.WriteLine("Circuit HALF OPEN"));

    public static IAsyncPolicy<HttpResponseMessage> FallbackPolicy =>
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<BrokenCircuitException>()
            .Or<TimeoutRejectedException>()
            .OrResult(r => !r.IsSuccessStatusCode)
            .FallbackAsync(
                fallbackAction: (outcome, ct) => 
                {
                    Console.WriteLine("Fallback executed");

                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            "{ \"message\": \"Fallback response - service unavailable\" }")
                    };

                    return Task.FromResult(response);
                },
                onFallbackAsync: (outcome, context) =>
                {
                    Console.WriteLine("Executing fallback logic");
                    return Task.CompletedTask;
                });

    public static IAsyncPolicy<HttpResponseMessage> TimeoutPolicy =>
        Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(2),
            TimeoutStrategy.Optimistic,
            onTimeoutAsync: (context, timespan, task, exception) =>
            {
                Console.WriteLine($"Timeout after {timespan.TotalSeconds}s");
                return Task.CompletedTask;
            });


}