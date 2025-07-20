using System.Text.RegularExpressions;

namespace SpamBlocker.Utils
{
    public static class ValidationUtils
    {
        public static bool IsValidIpAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            var parts = ip.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out int value) || value < 0 || value > 255)
                    return false;

                if (part.Length > 1 && part.StartsWith("0"))
                    return false;
            }

            return true;
        }

        public static bool IsValidDomainFormat(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return false;

            if (!domain.Contains('.'))
                return false;

            if (!Regex.IsMatch(domain, @"[a-zA-Z]"))
                return false;

            if (Regex.IsMatch(domain, @"^[\d\.]+$"))
                return false;

            var parts = domain.Split('.');
            if (parts.Length < 2)
                return false;

            var tld = parts[parts.Length - 1];
            if (tld.Length < 2 || !Regex.IsMatch(tld, @"^[a-zA-Z]+$"))
                return false;

            return true;
        }

        public static bool IsNotRealUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            if (Regex.IsMatch(url, @"^\d+\.\d+$"))
                return true;

            if (Regex.IsMatch(url, @"^\d+\.\d+\.\d+$"))
                return true;

            if (url.Length < 4)
                return true;

            if (!Regex.IsMatch(url, @"[a-zA-Z]"))
                return true;

            return false;
        }

        public static bool IsAllowedProtocol(string url, List<string> allowedProtocols)
        {
            foreach (var protocol in allowedProtocols)
            {
                if (url.StartsWith($"{protocol}://", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            if (!url.Contains("://"))
                return true;

            return false;
        }

        public static bool IsInDomainList(string domain, List<string> domainList)
        {
            foreach (var listDomain in domainList)
            {
                if (string.IsNullOrWhiteSpace(listDomain))
                    continue;

                var cleanListDomain = listDomain.ToLowerInvariant().Replace("www.", "");

                if (domain == cleanListDomain)
                    return true;

                if (domain.EndsWith("." + cleanListDomain))
                    return true;
            }

            return false;
        }

        public static string ExtractDomain(string url)
        {
            try
            {
                if (IsNotRealUrl(url))
                    return string.Empty;

                if (!url.StartsWith("http://") && !url.StartsWith("https://") && !url.StartsWith("steam://"))
                    url = "http://" + url;

                var uri = new Uri(url);
                var domain = uri.Host.ToLowerInvariant();

                if (IsValidIpAddress(domain))
                    return string.Empty;

                if (!IsValidDomainFormat(domain))
                    return string.Empty;

                return domain;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}