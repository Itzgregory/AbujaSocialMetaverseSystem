namespace AbujaSocialMetaverse.Infrastructure.Caching;

public class CacheException : Exception 
{
    public string CacheKey {get;}
    public string Operation {get;}

    public CacheException(string operation, string key, string message, Exception? inner = null) : base(
        $"Cache operation {operation} failed for key '{key}': {message}", inner)
    {
        Operation = operation;
        CacheKey = key;
    }

    
}