using System.Net;
using System.Net.Sockets;

internal class Program
{
    private static void Main(string[] args)
    {
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
            var ipListBuilder = new System.Text.StringBuilder();

            if (hostEntry?.AddressList == null)
                return "No IP addresses found";

            hostEntry.AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .ToList()
                .ForEach(ip => ipListBuilder.AppendLine(ip.ToString()));

            return $"""
    Hello from .NET!!!!!
    Local IP: {localIp}
    Public IP: {wanIp}
    Other IPs:
    {ipListBuilder}
    """;
        });

        app.Run();
    }
}