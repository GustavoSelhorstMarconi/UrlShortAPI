using Microsoft.EntityFrameworkCore;
using UrlShort.Domain.Interfaces;
using UrlShort.Infra.Context;

namespace UrlShort.Infra.Repository;

public class UrlShortRepository : IUrlShortRepository
{
    private readonly ApplicationDbContext _context;

    public UrlShortRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Domain.Entities.UrlShort urlShort)
    {
        if (urlShort is null)
            throw new ArgumentNullException(nameof(urlShort));
        
        await _context.AddAsync(urlShort);
        
        await _context.SaveChangesAsync();
    }

    public async Task<Domain.Entities.UrlShort?> GetByShortUrl(string shortUrl)
    {
        return await _context.UrlShorts.FirstOrDefaultAsync(urlShort => urlShort.HashUrl == shortUrl);
    }

    public async Task UpdateClicksAmount(Domain.Entities.UrlShort urlShort)
    {
        urlShort.Update(urlShort.ExpiresAt, urlShort.IsActive, urlShort.ClicksCount + 1);
        
        await _context.SaveChangesAsync();
    }

    public async Task<long> GetNextId()
    {
        var lastEntity = await _context.UrlShorts
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (lastEntity is null)
            return 999999;
        
        return lastEntity.Id + 1;
    }
}