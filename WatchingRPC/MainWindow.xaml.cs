using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text.Json;
using NetDiscordRpc;
using NetDiscordRpc.Core.Logger;
using NetDiscordRpc.RPC;
using Newtonsoft.Json;
using Button = NetDiscordRpc.RPC.Button;
using Path = System.IO.Path;

namespace WatchingRPC;

public partial class MainWindow : Window
{
    private DiscordRPC discordRpc;
    private int windowHeight;
    private int windowWidth;
    private bool firstlaunch;
    private string rpcId;
    private string entryDir;
    private WJsonEntry currentEntry;

    public MainWindow()
    {
        InitializeComponent();

        try
        {
            LoadConfig();
        }
        catch (WatchingRPC.ConfigurationException ex)
        {
            var inner = ex.InnerException == null ? "" : "\n" + ex.InnerException.Message;
            MessageBox.Show(ex.Message + inner, "Config Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
            return;
        }

        discordRpc = new DiscordRPC(rpcId);
        discordRpc.Logger = new NullLogger();
        discordRpc.Initialize();

        Initialize();
    }

    private void LoadConfig()
    {
        try
        {
            windowHeight = int.Parse(ConfigurationManager.AppSettings["HEIGHT"]);
            windowWidth = int.Parse(ConfigurationManager.AppSettings["WIDTH"]);
            firstlaunch = bool.Parse(ConfigurationManager.AppSettings["FIRST_LAUNCH"]);
        }
        catch (NullReferenceException nullex)
        {
            throw new WatchingRPC.ConfigurationException("App.config is missing a value.", nullex);
        }

        rpcId = ConfigurationManager.AppSettings["RPC_CLIENT_ID"];
        var relativeEntryDir = ConfigurationManager.AppSettings["RELATIVE_ENTRY_DIR"];

        if (string.IsNullOrEmpty(rpcId))
        {
            throw new WatchingRPC.ConfigurationException("App.config: The app's RPC client ID is missing.");
        }

        if (string.IsNullOrEmpty(relativeEntryDir))
        {
            throw new WatchingRPC.ConfigurationException("App.config: The app's entry directory is missing");
        }

        entryDir = Environment.CurrentDirectory + relativeEntryDir;
    }

    private void Initialize()
    {
        if (firstlaunch)
        {
            MessageBox.Show("This is the first launch of the app. \nTo get help using this program, click on the \"Get Help\" button or got to https://github.com/OIL1I/WatchingRPC/wiki", "First Launch", MessageBoxButton.OK, MessageBoxImage.Information);
            ConfigurationManager.AppSettings.Set("FIRST_LAUNCH", "false");
        }
        
        App.Current.MainWindow.Width = windowWidth;
        App.Current.MainWindow.Height = windowHeight;

        if (!Directory.Exists(entryDir))
        {
            Directory.CreateDirectory(entryDir);
        }

        foreach (var file in Directory.GetFiles(entryDir, "*.json").Select(Path.GetFileNameWithoutExtension))
        {
            lview_entries.Items.Add(file);
        }
    }

    private void Lview_entries_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        cbox_caption.Items.Clear();
        lview_detail.Items.Clear();

        var fs = new FileStream(entryDir + "\\" + lview_entries.SelectedItem + ".json", FileMode.Open);
        StreamReader r = new StreamReader(fs);

        var entryString = r.ReadToEnd();
        currentEntry = JsonConvert.DeserializeObject<WJsonEntry>(entryString);
        r.Close();
        fs.Dispose();
        
        foreach (var caption in currentEntry.Captions)
        {
            cbox_caption.Items.Add(caption.Caption);
        }

        cbox_caption.SelectedIndex = 0;
    }

    private void Btn_update_lview_OnClick(object sender, RoutedEventArgs e)
    {
        lview_entries.Items.Clear();
        cbox_caption.Items.Clear();
        lview_detail.Items.Clear();
        
        foreach (var file in Directory.GetFiles(entryDir, "*.json").Select(Path.GetFileNameWithoutExtension))
        {
            lview_entries.Items.Add(file);
        }
    }

    private void Btn_get_entries_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Cbox_caption_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        lview_detail.Items.Clear();
        var index = cbox_caption.SelectedIndex;
        
        foreach (var detail in currentEntry.Captions[index].Details)
        {
            lview_detail.Items.Add(detail);
        }
    }

    private void Btn_update_rpc_OnClick(object sender, RoutedEventArgs e)
    {
        discordRpc.SetPresence(new RichPresence()
        {
            Details = $"Watching {lview_entries.SelectedItem} {cbox_caption.SelectedItem}",
            State = lview_detail.SelectedItem.ToString(),
            Buttons = new Button[] { new Button() { Label = "Get the App", Url = "https://github.com/OIL1I/WatchingRPC" } }
        });
        discordRpc.Invoke();
    }

    private void Btn_help_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        discordRpc.ClearPresence();
        discordRpc.Dispose();
    }
}