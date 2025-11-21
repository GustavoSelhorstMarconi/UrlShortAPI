namespace UrlShort.Domain.Interfaces;

public interface IUrlShortRepository
{
    Task AddAsync(Domain.Entities.UrlShort urlShort);

    Task<Domain.Entities.UrlShort?> GetByShortUrl(string hashUrl);
    
    Task UpdateClicksAmount(Domain.Entities.UrlShort urlShort);

    Task<long> GetNextId();
}