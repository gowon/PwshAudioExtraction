namespace PwshAudioExtraction.Commands
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using Extensions;
    using NYoutubeDL;
    using NYoutubeDL.Options;

    // https://github.com/ytdl-org/youtube-dl/issues/5957#issuecomment-207357604
    // https://stackoverflow.com/a/48829530
    // http://id3.org/id3v2.3.0#Attached_picture
    [Cmdlet(VerbsCommunications.Read, Noun, DefaultParameterSetName = ParamSetNoThumbnail)]
    public class ReadSourceAudioCommand : PSCmdlet
    {
        private const string Noun = "SourceAudio";
        private const string ParamSetNoThumbnail = "NoThumbnailParameterSet";
        private const string ParamSetConfigFile = "ConfigFileParameterSet";
        private const string ParamSetConfigJson = "ConfigJsonParameterSet";

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Url { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ParamSetConfigFile)]
        [ValidateNotNullOrEmpty]
        public string ConfigurationFile { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ParamSetConfigJson)]
        [ValidateNotNullOrEmpty]
        public string ConfigurationJson { get; set; }

        [Parameter(Mandatory = false,
            ParameterSetName = ParamSetNoThumbnail)]
        public SwitchParameter NoThumbnail { get; set; }

        protected override void BeginProcessing()
        {
            // we cannot set the working directory of the process created in the YoutubeDL object
            // so it defaults to the current directory of the application that called it.
            WriteVerbose($"Setting the working directory to \"{SessionState.Path.CurrentFileSystemLocation.Path}\"");
            Directory.SetCurrentDirectory(SessionState.Path.CurrentFileSystemLocation.Path);
        }

        protected override void ProcessRecord()
        {
            if (!Url.IsValidUrl())
            {
                WriteVerbose($"The given url: \"{Url}\" is not valid.");
                return;
            }

            var client = new YoutubeDL {VideoUrl = Url};
            if (!string.IsNullOrWhiteSpace(ConfigurationFile))
            {
                var resolvedPath = this.ResolvePaths(false, ConfigurationFile).SingleOrDefault()
                                   ?? throw new ArgumentException(
                                       "Cannot find the configuration file at the given path.",
                                       nameof(ConfigurationFile));

                WriteVerbose($"Using \"{resolvedPath}\"");
                client.Options = Options.Deserialize(File.ReadAllText(resolvedPath));
            }
            else if (!string.IsNullOrWhiteSpace(ConfigurationJson))
            {
                WriteVerbose("Using json configuration");
                client.Options = Options.Deserialize(ConfigurationJson);
            }
            else
            {
                WriteVerbose("Using default media options");
                var includeThumbnail = !(NoThumbnail.IsPresent && NoThumbnail.ToBool());
                client.ApplyDefaultMediaOptions(includeThumbnail);
            }

            WriteVerbose("Executing Youtubedl using the following parameters:");
            WriteVerbose(client.Options.ToCliParameters());

            if (MyInvocation.BoundParameters.ContainsKey("Verbose"))
            {
                client.StandardOutputEvent += (sender, output) => WriteLineExternal(output, ConsoleColor.Yellow);

            }

            client.StandardErrorEvent += (sender, errorOutput) => WriteLineExternal(errorOutput, ConsoleColor.Red);
            client.Download();
        }

        public static void WriteLineExternal(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}