namespace StreetRacer.Application.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
    public int TotalCount { get; set; }
}

public class Cursor
{
    public DateTime? CreatedAt { get; set; }
    public Guid? Id { get; set; }
    public int Limit { get; set; } = 20;
    
    public static Cursor FromBase64(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor))
            return new Cursor();
            
        try
        {
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            return System.Text.Json.JsonSerializer.Deserialize<Cursor>(json) ?? new Cursor();
        }
        catch
        {
            return new Cursor();
        }
    }
    
    public string ToBase64()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(this);
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
    }
}