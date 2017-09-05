using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace LegionTDServerReborn.Utils {
    public class IpAddressRange {
        readonly AddressFamily addressFamily;
        readonly byte[] lowerBytes;
        readonly byte[] upperBytes;

        public IpAddressRange(string lowerInclusive, string upperInclusive)
            :this(IPAddress.Parse(lowerInclusive), IPAddress.Parse(upperInclusive)){

        }

        public IpAddressRange(IPAddress lowerInclusive, IPAddress upperInclusive)
        {
            this.addressFamily = lowerInclusive.AddressFamily;
            this.lowerBytes = lowerInclusive.GetAddressBytes();
            this.upperBytes = upperInclusive.GetAddressBytes();
        }

        public bool IsInRange(IPAddress address)
        {
            if (address.AddressFamily != addressFamily)
            {
                return false;
            }

            byte[] addressBytes = address.GetAddressBytes();

            bool lowerBoundary = true, upperBoundary = true;

            for (int i = 0; i < this.lowerBytes.Length && 
                (lowerBoundary || upperBoundary); i++)
            {
                if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                    (upperBoundary && addressBytes[i] > upperBytes[i]))
                {
                    return false;
                }

                lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                upperBoundary &= (addressBytes[i] == upperBytes[i]);
            }

            return true;
        }

        public static IpAddressRange Parse(string content) {
            var ips = content.Split('-');
            if (ips.Length > 1) {
                return new IpAddressRange(ips[0], ips[1]);
            }
            ips = content.Split('/');
            if (ips.Length > 1) {
                var ip = ips[0];
                return new IpAddressRange(ip, ip.Substring(ip.LastIndexOf(".") + 1) + ips[1]);
            }
            return new IpAddressRange(content, content);
        }
    }
}