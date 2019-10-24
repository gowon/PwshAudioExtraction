namespace PwshAudioExtraction.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using Abstractions;
    using Extensions;
    using TagLib;
    using File = TagLib.File;

    [Cmdlet(VerbsCommon.Get, Noun, DefaultParameterSetName = ParamSetPath)]
    [OutputType(typeof(Tag))]
    public class GetId3TagCommand : PSCmdlet
    {
        private const string Noun = "Id3Tag";
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

                using (var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                using (var file = File.Create(new FileStreamAbstraction(fileStream)))
                {
                    WriteVerbose($"Getting ID3 tags from \"{resolved}\"");
                    WriteObject(file.Tag);
                }
            }
        }
    }
}