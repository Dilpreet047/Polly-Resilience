# 🛡️ Resilient HTTP Client with Circuit Breaker (ASP.NET Core + Polly)

## 📌 Project Purpose

This project demonstrates how to build a **resilient external service client** in ASP.NET Core using the **Circuit Breaker pattern**.

Modern backend systems depend heavily on external APIs (payment gateways, microservices, third-party providers).
If one dependency fails repeatedly, it can bring down the entire application.

This repository shows how to **protect your API from cascading failures** using industry-standard resilience techniques.

---

## 🧠 Problem Statement

Consider an API that calls an external service:

```
Client → Our API → External Service
```

If the external service becomes slow or unavailable:

* Requests start failing
* Threads remain occupied
* Retries overload the dependency
* Application performance degrades
* Entire system may crash

We solve this using **Circuit Breaker**.

---

## ⚡ What is Circuit Breaker?

The Circuit Breaker pattern prevents continuous calls to a failing dependency.

It behaves like an electrical circuit.

### States

| State        | Meaning                                |
| ------------ | -------------------------------------- |
| ✅ Closed     | Requests flow normally                 |
| 🔴 Open      | Calls blocked due to repeated failures |
| 🟡 Half-Open | Test call allowed to check recovery    |

---

### Flow Diagram

```
Failures occur
      ↓
Circuit Opens
      ↓
Requests fail fast (no external call)
      ↓
Cooldown period
      ↓
Half-Open test request
      ↓
Success → Closed
Failure → Open again
```

---

## 🏗️ Project Architecture

```
ResilientApiDemo
│
├── Controllers
│     └── DemoController.cs
│
├── Services
│     └── ExternalApiService.cs
│
├── Resilience
│     └── ResiliencePolicies.cs
│
├── Models
│     └── ApiResponse.cs
│
└── Program.cs
```

---

## 📦 Technologies Used

* ASP.NET Core Web API
* HttpClientFactory
* Polly (Resilience Library)
* Circuit Breaker Pattern

---

## 🔧 Setup Instructions

### 1️⃣ Create Project

```bash
dotnet new webapi -n ResilientApiDemo
cd ResilientApiDemo
```

---

### 2️⃣ Install Dependencies

```bash
dotnet add package Polly
dotnet add package Microsoft.Extensions.Http.Polly
```

---

## 🧩 Core Components

---

### ✅ 1. Generic API Response Model

`Models/ApiResponse.cs`

Used to standardize service responses.

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}
```

---

### ✅ 2. External API Service

`Services/ExternalApiService.cs`

Responsible for communicating with downstream services.

```csharp
public class ExternalApiService
{
    private readonly HttpClient _client;

    public ExternalApiService(HttpClient client)
    {
        _client = client;
    }

    public async Task<ApiResponse<string>> GetDataAsync()
    {
        var response =
            await _client.GetAsync("https://jsonplaceholder.typicode.com/posts/1");

        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Error = $"Status Code: {response.StatusCode}"
            };
        }

        var data = await response.Content.ReadAsStringAsync();

        return new ApiResponse<string>
        {
            Success = true,
            Data = data
        };
    }
}
```

---

### ✅ 3. Circuit Breaker Policy

`Resilience/ResiliencePolicies.cs`

Central place for resilience configuration.

```csharp
using Polly;
using Polly.Extensions.Http;

public static class ResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(20),
                onBreak: (result, delay) =>
                    Console.WriteLine("Circuit OPEN"),
                onReset: () =>
                    Console.WriteLine("Circuit CLOSED"),
                onHalfOpen: () =>
                    Console.WriteLine("Circuit HALF-OPEN"));
}
```

---

### ✅ 4. HttpClient Registration

`Program.cs`

```csharp
builder.Services.AddHttpClient<ExternalApiService>()
    .AddPolicyHandler(ResiliencePolicies.CircuitBreakerPolicy);
```

This integrates Polly directly into the HttpClient pipeline.

---

### ✅ 5. API Controller

```csharp
[ApiController]
[Route("api/demo")]
public class DemoController : ControllerBase
{
    private readonly ExternalApiService _service;

    public DemoController(ExternalApiService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetDataAsync();

        if (!result.Success)
            return StatusCode(500, result);

        return Ok(result);
    }
}
```

---

## 🧪 How to Test Circuit Breaker

### Normal Behaviour

Call:

```
GET /api/demo
```

Response returns successfully.

---

### Simulate Failure

Change the external URL to an invalid endpoint:

```csharp
https://jsonplaceholder.typicode.com/invalid

OR

http://httpstat.us/200?sleep=5000

to simulate timeout
```

Call endpoint repeatedly.

Console output:

```
Circuit OPEN
```

Now requests will:

* Fail immediately
* Skip external calls
* Protect application resources

After cooldown:

```
Circuit HALF-OPEN
```

If request succeeds → circuit closes again.

---

## 🎯 Why This Pattern Matters

Without Circuit Breaker:

* Continuous retries overload failing systems
* Thread pool exhaustion
* Cascading failures
* System-wide outage

With Circuit Breaker:

✅ Fail fast
✅ Protect resources
✅ Improve stability
✅ Enable graceful recovery

---

## 🏢 Real-World Usage

This pattern is widely used in:

* Microservices architectures
* Payment processing systems
* Banking platforms
* Cloud-native APIs
* High-scale distributed systems

Companies using similar resilience strategies:

* Netflix
* Amazon
* Microsoft Azure
* Uber

---

## 💡 Key Engineering Takeaways

* Never trust external dependencies.
* Always isolate downstream failures.
* Resilience belongs in infrastructure, not business logic.
* HttpClientFactory + Polly is the recommended ASP.NET approach.

---

## 🚀 Possible Future Enhancements

* Retry with exponential backoff
* Timeout policy
* Fallback responses
* Bulkhead isolation
* Centralized resilience pipeline
* Observability & metrics

---

## 👨‍💻 Learning Outcome

This project demonstrates how to move from:

> “Calling APIs”

to

> **Designing reliable distributed systems.**

---

## Polly Resilience Policies State Diagram

<img width="1536" height="1024" alt="image" src="https://github.com/user-attachments/assets/ff183ffd-a661-4343-883b-de9f7b76bc65" />


## ⭐ If You Found This Useful

Consider ⭐ starring the repository to help others learn resilience patterns in ASP.NET Core.
