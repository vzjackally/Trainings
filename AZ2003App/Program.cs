using System.Net;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async () =>
{
    // Get the local (container) IPv4 address
    var hostName = Dns.GetHostName();
    var hostEntry = await Dns.GetHostEntryAsync(hostName);
    var localIp = hostEntry.AddressList
        .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
        ?.ToString() ?? "Not found";

    // Get the WAN/Public IP by calling a simple external IP service
    using var httpClient = new HttpClient();
    string wanIp;
    try
    {
        // api.ipify.org returns the IP as plain text
        // Alternatives: ifconfig.me, checkip.amazonaws.com, etc.
        wanIp = await httpClient.GetStringAsync("https://api.ipify.org");
    }
    catch
    {
        wanIp = "Unable to retrieve public IP";
    }

    // Return a response showing both IPs
    var ipList = "";

    foreach (var ip in hostEntry.AddressList) 
    {
        ipList += ip + "\n";
    } 

    return $"Hello from .NET!\nLocal IP: {localIp}\nPublic IP: {wanIp}\n Other IPs: {ipList}";
});

app.Run();
