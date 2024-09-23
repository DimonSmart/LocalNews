namespace DimonSmart.WebScraper;

public interface IUrlRepository
{
    bool Contains(string url);
    void Add(string url);
}