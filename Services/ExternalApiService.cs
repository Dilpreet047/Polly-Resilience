public class ExternalApiService
{
    private readonly HttpClient _client;

    public ExternalApiService(HttpClient client)
    {
        _client = client;
    }

    public async Task<ApiResponse<string>> GetDataAsync(string serviceURL)
    {
        var response = await _client.GetAsync(serviceURL);

        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Error = $"Status code: {response.StatusCode}"
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