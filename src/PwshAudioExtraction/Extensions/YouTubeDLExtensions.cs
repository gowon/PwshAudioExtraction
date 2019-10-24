namespace PwshAudioExtraction.Extensions
{
    using NYoutubeDL;
    using NYoutubeDL.Helpers;

    public static class YouTubeDlExtensions
    {
        public static void ApplyDefaultMediaOptions(this YoutubeDL client, bool includeThumbnail = true)
        {
            // -C -i --add-metadata --audio-format mp3 -x --postprocessor-args "-id3v2_version 3" --write-thumbnail -f bestaudio[ext=m4a]/bestaudio/best
            client.Options.GeneralOptions.IgnoreErrors = true;
            client.Options.FilesystemOptions.Continue = true;
            client.Options.ThumbnailImagesOptions.WriteThumbnail = includeThumbnail;
            client.Options.PostProcessingOptions.AddMetadata = true;
            client.Options.PostProcessingOptions.ExtractAudio = true;
            client.Options.PostProcessingOptions.AudioFormat = Enums.AudioFormat.mp3;
            client.Options.PostProcessingOptions.PostProcessorArgs = "-id3v2_version 3";
            client.Options.VideoFormatOptions.FormatAdvanced = "bestaudio[ext=m4a]/bestaudio/best";
        }
    }
}