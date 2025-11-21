using UrlShort.Application.Dto;

namespace UrlShort.Application.Interfaces;

public interface IUrlShortService
{
    Task<ApiResponse<UrlShortDto>> ShortenUrl(ShortenUrlDto dto);

    Task<ApiResponse<string>> GetShortUrl(string shortUrl);
}