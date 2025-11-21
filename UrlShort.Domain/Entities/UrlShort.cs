using System.Buffers.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UrlShort.Domain.Entities;

public class UrlShort
{
    public UrlShort(long id, string baseUrl, string hashUrl, int clicksCount, DateTime? expiresAt, bool isActive)
    {
        Id = id;
        BaseUrl = baseUrl;
        HashUrl = hashUrl;
        ClicksCount = clicksCount;
        ExpiresAt = expiresAt;
        IsActive = isActive;
    }

    protected UrlShort()
    {
    }

    [BsonId]
    public long Id { get; private set; }
    
    [BsonElement("BaseUrl")]
    [BsonRequired]
    public string BaseUrl { get; private set; }

    [BsonElement("HashUrl")]
    [BsonRequired]
    public string HashUrl { get; private set; }

    [BsonElement("CreatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    [BsonElement("ClicksCount")]
    public int ClicksCount { get; private set; }

    [BsonElement("ExpiresAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ExpiresAt { get; private set; }

    [BsonElement("IsActive")]
    public bool IsActive { get; private set; }

    public void Update(DateTime? expiresAt, bool isActive, int clicksCount)
    {
        ExpiresAt = expiresAt;
        IsActive = isActive;
        ClicksCount = clicksCount;
    }
}