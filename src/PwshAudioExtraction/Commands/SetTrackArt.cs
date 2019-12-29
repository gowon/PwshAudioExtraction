namespace PwshAudioExtraction.Commands
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using Abstractions;
    using Extensions;
    using TagLib;
    using File = TagLib.File;

    [Cmdlet(VerbsCommon.Set, Noun, DefaultParameterSetName = ParamSetPath)]
    public class SetTrackArt : PSCmdlet
    {
        private const string Noun = "TrackArt";
        private const string ParamSetLiteral = "LiteralPathParameterSet";
        private const string ParamSetPath = "PathParameterSet";
        private bool _imageUsed;
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
            Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string ImagePath { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter DeleteImage { get; set; }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            var resolvedImagePath = this.ResolvePaths(_shouldExpandWildcards, ImagePath).SingleOrDefault();
            if (resolvedImagePath == null)
            {
                WriteVerbose($"Path \"{ImagePath}\" doesn't exist. Skipping");
                return;
            }

            var imageFileInfo = new FileInfo(resolvedImagePath);
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
                using (var imageFileStream = imageFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var file = File.Create(new FileStreamAbstraction(mp3FileStream)))
                {
                    WriteVerbose("Setting track art.");
                    var trackArt = new Picture(new FileStreamAbstraction(imageFileStream)) {Description = null};
                    file.Tag.Pictures = new IPicture[] {trackArt};
                    file.Save();
                    _imageUsed = true;
                }
            }
        }

        protected override void EndProcessing()
        {
            if (DeleteImage.IsPresent && DeleteImage.ToBool() && _imageUsed)
            {
                var imageFileInfo = new FileInfo(ImagePath);
                WriteVerbose($"Deleting \"{imageFileInfo.Name}\"");
                imageFileInfo.Delete();
            }
        }
    }
}