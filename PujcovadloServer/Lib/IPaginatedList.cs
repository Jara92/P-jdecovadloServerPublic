namespace PujcovadloServer.Lib;

public interface IPaginatedList
{
    public int PageIndex { get; }

    public int TotalPages { get; }

    public bool HasPreviousPage { get; }

    public bool HasNextPage { get; }

    public int TotalCount { get; }
}