namespace PwshAudioExtraction.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using Abstractions;
    using Extensions;
    using File = TagLib.File;

    [Cmdlet(VerbsCommon.Set, Noun, DefaultParameterSetName = ParamSetPath)]
    public class SetId3Tag : PSCmdlet
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

        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string Property { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 2,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public object Value { get; set; }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            var resolvedPaths = this.ResolvePaths(_shouldExpandWildcards, LiteralPath);
            foreach (var resolved in resolvedPaths)
            {
                var mp3FileInfo = new FileInfo(resolved);
                if (!mp3FileInfo.Name.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    WriteVerbose($"File \"{resolved}\" is not an MP3. Skipping");
                    continue;
                }

                using (var mp3FileStream = mp3FileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                using (var file = File.Create(new FileStreamAbstraction(mp3FileStream)))
                {
                    WriteVerbose($"Setting ID3 Tag {Property}: {Value}");
                    file.Tag.SetProperty(Property, Value);
                    file.Save();
                }
            }
        }
    }
}