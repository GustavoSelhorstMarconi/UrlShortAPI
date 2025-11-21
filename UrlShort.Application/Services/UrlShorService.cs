using UrlShort.Application.Dto;
using UrlShort.Application.Interfaces;
using UrlShort.Application.Mappers;
using UrlShort.Domain.Interfaces;

namespace UrlShort.Application.Services;

public class UrlShorService : IUrlShortService
{
    private readonly IUrlShortRepository _urlShortRepository;
    private static readonly char[] BASE62ALPHABET =
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();

    public UrlShorService(IUrlShortRepository urlShortRepository)
    {
        _urlShortRepository = urlShortRepository;
    }
    
    public async Task<ApiResponse<UrlShortDto>> ShortenUrl(ShortenUrlDto dto)
    {
        var urlShortId = await _urlShortRepository.GetNextId();
        
        var hashUrl = Base62Conversor(urlShortId);
        
        var entity = new Domain.Entities.UrlShort(urlShortId, dto.BaseUrl, hashUrl, 0, dto.ExpiresAt, true);
        
        await _urlShortRepository.AddAsync(entity);
        
        return new ApiResponse<UrlShortDto>(GenericMapper<Domain.Entities.UrlShort, UrlShortDto>.ToDto(entity), 200);
    }

    public async Task<ApiResponse<string>> GetShortUrl(string shortUrl)
    {
        if (string.IsNullOrEmpty(shortUrl))
            return new ApiResponse<string>("Short url is required.", 400);
        
        var entity = await _urlShortRepository.GetByShortUrl(shortUrl);
        
        if (entity is null)
            return new ApiResponse<string>("Url not found.", 400);
        
        await _urlShortRepository.UpdateClicksAmount(entity);
        
        return new ApiResponse<string>(entity.BaseUrl, 200);
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