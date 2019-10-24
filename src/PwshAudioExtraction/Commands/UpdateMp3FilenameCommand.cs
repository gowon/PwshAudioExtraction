namespace PwshAudioExtraction.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Text.RegularExpressions;
    using Abstractions;
    using Extensions;
    using File = TagLib.File;

    [Cmdlet(VerbsData.Update, Noun, DefaultParameterSetName = ParamSetPath)]
    [OutputType(typeof(FileInfo))]
    public class UpdateMp3FilenameCommand : PSCmdlet
    {
        private const string Noun = "Mp3Filename";
        private const string ParamSetLiteral = "LiteralPathParameterSet";
        private const string ParamSetPath = "PathParameterSet";
        private bool _shouldExpandWildcards;

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ParamSetLiteral)]
        [Alias("PSPath")]
        [ValidateNotNullOrEmpty]
        public string[] LiteralPath { get; set; }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ParamSetPath)]
        [ValidateNotNullOrEmpty]
        public string[] Path
        {
            get => LiteralPath;
            set
            {
                _shouldExpandWildcards = true;
                LiteralPath = value;
            }
        }

        [Parameter(Mandatory = false)]
        public int MaxLength { get; set; } = 100;

        protected override void BeginProcessing()
        {
            if (MaxLength < 5)
            {
                throw new ArgumentOutOfRangeException($"{nameof(MaxLength)} must be greater than 4.");
            }
        }

        protected override void ProcessRecord()
        {
            var resolvedPaths = this.ResolvePaths(_shouldExpandWildcards, LiteralPath);
            foreach (var resolved in resolvedPaths)
            {
                var fileInfo = new FileInfo(resolved);
                if (!fileInfo.Name.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    WriteVerbose($"File \"{resolved}\" is not an MP3. Skipping");
                    continue;
                }

                uint disc, track;
                string artist, title;
                using (var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                using (var file = File.Create(new FileStreamAbstraction(fileStream)))
                {
                    WriteVerbose($"Getting ID3 tags from \"{resolved}\"");
                    artist = file.Tag.FirstPerformer;
                    title = file.Tag.Title;
                    track = file.Tag.Track;
                    disc = file.Tag.Disc;
                }

                if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(title))
                {
                    WriteVerbose(
                        $"File \"{resolved}\" does not have enough metadata to create a new filename. Please set an Artist and Title ID3 tag. Skipping");
                    continue;
                }

                var trackString = string.Empty;
                if (track > 0)
                {
                    if (disc > 0)
                    {
                        trackString += $"{disc}";
                    }

                    trackString += $"{track:00}-";
                }

                const string pattern = @"([^a-zA-Z\d]+)";
                artist = Regex.Replace(artist, pattern, "_");
                title = Regex.Replace(title, pattern, "_");
                var filename = $"{trackString}{artist}-{title}".Trim('_');
                filename = filename.Substring(0, Math.Min(filename.Length, MaxLength - 4)) + ".mp3";

                if (string.Equals(fileInfo.Name, filename, StringComparison.OrdinalIgnoreCase))
                {
                    WriteVerbose($"File name is already \"{fileInfo.Name}\". Skipping");
                    continue;
                }

                var newPath = System.IO.Path.Combine(fileInfo.Directory?.FullName ?? string.Empty, filename);
                if (System.IO.File.Exists(newPath))
                {
                    WriteVerbose($"A file named \"{fileInfo.Name}\" already exists. Skipping");
                    continue;
                }

                WriteVerbose($"Renaming \"{fileInfo.Name}\" => \"{filename}\"");
                fileInfo.MoveTo(newPath);
                WriteObject(fileInfo);
            }
        }
    }
}