namespace PwshAudioExtraction.Extensions
{
    using System;

    public static class StringExtensions
    {
        public static bool IsValidUrl(this string uri)
        {
            return !string.IsNullOrEmpty(uri) && Uri.IsWellFormedUriString(uri, UriKind.Absolute);
        }
    }
}