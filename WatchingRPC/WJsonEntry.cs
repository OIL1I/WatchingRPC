namespace WatchingRPC;

public class WJsonEntry
{
    public WJsonEntry(string title, List<WJsonCaption> captions)
    {
        Title = title;
        Captions = captions;
    }

    public string Title { get; set; }
    public List<WJsonCaption> Captions { get; set; }
}