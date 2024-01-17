using System.Text.RegularExpressions;

namespace PujcovadloServer.Helpers;

public class UrlHelper
{
    public static string CreateUrlStub(string input)
    {
        // TODO: Replace all czech characters first
        
        // Replace spaces with dashes, remove special characters, and convert to lowercase
        string urlStub = Regex.Replace(input, @"[^a-zA-Z0-9]", "-").ToLower();

        // Remove consecutive dashes
        urlStub = Regex.Replace(urlStub, @"-+", "-");

        // Remove leading and trailing dashes
        urlStub = urlStub.Trim('-');

        return urlStub;
    }
}