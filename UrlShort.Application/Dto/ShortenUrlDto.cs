namespace UrlShort.Application.Dto;

public class ShortenUrlDto
{
    public string BaseUrl { get; set; }

    public DateTime? ExpiresAt { get; set; }
}