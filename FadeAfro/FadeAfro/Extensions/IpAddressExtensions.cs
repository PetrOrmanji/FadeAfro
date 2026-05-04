using System.Net;
using System.Net.Sockets;

namespace FadeAfro.Extensions;

public static class IpAddressExtensions
{
    public static string ToIpV4AddressString(this IPAddress? address)
    {
        return address?.AddressFamily is AddressFamily.InterNetworkV6
            ? address.MapToIPv4().ToString()
            : address?.ToString() ?? "unknown";
    }
}