namespace DimonSmart.WebScraper;

public class ScraperWorkerStatus
{
    public string CurrentPageUrl { get; private set; }
    public int ProcessedFiles { get; private set; }
    public long TotalBytesProcessed { get; private set; }
    public WorkerStatus Status { get; private set; }

    public ScraperWorkerStatus()
    {
        Status = WorkerStatus.Waiting;
    }

    public void SetWorkingStatus(string currentPageUrl)
    {
        Status = WorkerStatus.Working;
        CurrentPageUrl = currentPageUrl;
        Console.WriteLine($"Processing started for URL: {currentPageUrl}");
    }

    public void SetWaitStatus()
    {
        Status = WorkerStatus.Waiting;
        Console.WriteLine("Processing finished. Now waiting for a new task.");
    }

    public void AddProcessedData(long bytesProcessed)
    {
        ProcessedFiles++;
        TotalBytesProcessed += bytesProcessed;
    }

    public override string ToString()
    {
        return $"Status: {Status}, Page: {CurrentPageUrl}, Files: {ProcessedFiles}, Bytes: {TotalBytesProcessed}";
    }
}
