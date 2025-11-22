using Microsoft.Extensions.Configuration;
using UrlShort.Application.Dto;
using UrlShort.Application.Interfaces;
using UrlShort.Application.Mappers;
using UrlShort.Domain.Interfaces;

namespace UrlShort.Application.Services;

public class UrlShortService : IUrlShortService
{
    private readonly IUrlShortRepository _urlShortRepository;
    private readonly char[] BASE62ALPHABET;
    private readonly string ROUTE;

    public UrlShortService(IUrlShortRepository urlShortRepository, IConfiguration configuration)
    {
        _urlShortRepository = urlShortRepository;
        BASE62ALPHABET = configuration.GetSection("Base62Alphabet").Value.ToCharArray();
        ROUTE = configuration.GetSection("Route").Value;
    }
    
    public async Task<ApiResponse<UrlShortDto>> ShortenUrl(ShortenUrlDto dto)
    {
        var urlShortId = await _urlShortRepository.GetNextId();
        
        var hashUrl = Base62Conversor(urlShortId);
        
        var entity = new Domain.Entities.UrlShort(urlShortId, dto.BaseUrl, hashUrl, 0, dto.ExpiresAt, true);
        
        await _urlShortRepository.AddAsync(entity);

        var response = GenericMapper<Domain.Entities.UrlShort, UrlShortDto>.ToDto(entity);
        response.HashUrl = ROUTE + hashUrl;
        
        return new ApiResponse<UrlShortDto>(response, 200);
    }

    public async Task<ApiResponse<string>> GetShortUrl(string shortUrl)
    {
        if (string.IsNullOrEmpty(shortUrl))
            return new ApiResponse<string>("Short url is required.", 400);
        
        var entity = await _urlShortRepository.GetByShortUrl(shortUrl);
        
        if (entity is null)
            return new ApiResponse<string>("Url not found.", 400);
        
        await _urlShortRepository.UpdateClicksAmount(entity);

        var baseUrl = entity.BaseUrl;
        if (!baseUrl.StartsWith("http://") && !baseUrl.StartsWith("https://"))
            baseUrl = "https://" + baseUrl;
        
        return new ApiResponse<string>(baseUrl, 200);
    }

    private string Base62Conversor(long urlId)
    {
        char[] hashUrl = new char[11];
        int counter = 0;
        
        do
        {
            var index = (int)(urlId % 62);
            
            hashUrl[counter++] = BASE62ALPHABET[index];
            
            urlId /= 62;
        } while (urlId > 0);

        return new string(hashUrl, 0, counter);
    }
}