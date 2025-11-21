namespace UrlShort.Application.Dto;

public class UrlShortDto
{
    public string BaseUrl { get; set; }

    public string HashUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ClicksCount { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; }
}