namespace affolterNET.Web.Core.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Combines URL segments with proper slash handling
    /// </summary>
    public static string UrlCombine(this string baseUrl, params string[] segments)
    {
        if (string.IsNullOrEmpty(baseUrl))
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));

        var url = baseUrl.TrimEnd('/');

        foreach (var segment in segments.Where(s => !string.IsNullOrEmpty(s)))
        {
            url = url + "/" + segment.TrimStart('/');
        }

        return url;
    }
}