namespace Catalog.API
{
    public record DatabaseSettings (
        string ConnectionString, 
        string DatabaseName,
        string CollectionName)
    {
        
    }
}