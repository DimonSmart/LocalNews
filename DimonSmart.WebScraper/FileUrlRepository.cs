namespace DimonSmart.WebScraper;

public class FileUrlRepository : IUrlRepository
{
    private readonly string _filePath;
    private readonly HashSet<string> _urls;

    public FileUrlRepository(string filePath)
    {
        _filePath = filePath;
        _urls = new HashSet<string>();

        if (File.Exists(_filePath))
        {
            foreach (var line in File.ReadAllLines(_filePath))
            {
                _urls.Add(line);
            }
        }
    }

    public bool Contains(string url)
    {
        return _urls.Contains(url);
    }

    public void Add(string url)
    {
        if (_urls.Add(url))
        {
            File.AppendAllText(_filePath, url + "\n");
        }
    }
}