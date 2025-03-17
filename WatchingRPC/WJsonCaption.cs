namespace WatchingRPC;

public class WJsonCaption
{
    public WJsonCaption(string caption, List<string> details)
    {
        Caption = caption;
        Details = details;
    }

    public string Caption { get; set; }
    public List<string> Details { get; set; }
}