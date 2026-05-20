using System.Net.Http.Json;
using ClientApp.Models;

namespace ClientApp.Services;

public class ProductService
{
    private readonly HttpClient _http;
    private Product[]? _cachedProducts;
    private DateTime _cacheExpiration;
    private const int CACHE_DURATION_MINUTES = 5;
    private const string CACHE_KEY = "products_cache";
    private const string CACHE_TIME_KEY = "products_cache_time";

    public ProductService(HttpClient http)
    {
        _http = http;
    }

    public async Task<Product[]> GetProductsAsync()
    {
        // Check memory cache first
        if (_cachedProducts != null && DateTime.UtcNow < _cacheExpiration)
        {
            return _cachedProducts;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            _cachedProducts = await _http.GetFromJsonAsync<Product[]>(
                "http://localhost:5174/api/productsList", 
                cts.Token) ?? Array.Empty<Product>();
            
            _cacheExpiration = DateTime.UtcNow.AddMinutes(CACHE_DURATION_MINUTES);
            return _cachedProducts;
        }
        catch (OperationCanceledException)
        {
            throw new InvalidOperationException("The request timed out.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Failed to fetch products.", ex);
        }
    }

    public void ClearCache()
    {
        _cachedProducts = null;
        _cacheExpiration = DateTime.MinValue;
    }
}
