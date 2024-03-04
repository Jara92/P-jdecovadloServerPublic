namespace PujcovadloServer.Areas.Admin.ViewModels;

public class CRUDModel<T> where T : class
{
    public string? action { get; set; }

    public string? table { get; set; }

    public string? keyColumn { get; set; }

    public object? key { get; set; }

    public T? value { get; set; }

    public List<T> added { get; set; } = new List<T>();

    public List<T> changed { get; set; } = new List<T>();

    public List<T> deleted { get; set; } = new List<T>();

    public IDictionary<string, object> @params { get; set; } = new Dictionary<string, object>();
}