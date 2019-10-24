namespace PwshAudioExtraction.Commands
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using Extensions;
    using SkiaSharp;

    [Cmdlet(VerbsCommon.New, Noun, DefaultParameterSetName = ParamSetPath)]
    public class NewSquareCroppedImageCommand : PSCmdlet
    {
        private const string Noun = "SquareCroppedImage";
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
        public int? Size { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter PreserveOriginal { get; set; }

        protected override void ProcessRecord()
        {
            var resolvedPaths = this.ResolvePaths(_shouldExpandWildcards, LiteralPath);
            foreach (var resolved in resolvedPaths)
            {
                var fileInfo = new FileInfo(resolved);

                // https://gist.github.com/xoofx/a9d08a37c43f692e65df80a1888c488b
                // https://stackoverflow.com/a/8143775
                var originalBitmap = SKBitmap.Decode(fileInfo.FullName);

                var isSquare = originalBitmap.Height == originalBitmap.Width;
                var bound = Size.HasValue
                    ? Math.Min(Math.Min(originalBitmap.Height, originalBitmap.Width), Size.Value)
                    : Math.Min(originalBitmap.Height, originalBitmap.Width);

                if (isSquare && originalBitmap.Height <= bound)
                {
                    WriteVerbose("Image is already square. Aborting");
                    continue;
                }

                WriteVerbose($"Cropping \"{resolved}\"");
                var croppedBitmap = new SKBitmap(bound, bound, originalBitmap.ColorType, originalBitmap.AlphaType);
                var cx = (bound - originalBitmap.Width) >> 1; // same as (...) / 2
                var cy = (bound - originalBitmap.Height) >> 1;

                var canvas = new SKCanvas(croppedBitmap);
                canvas.DrawBitmap(originalBitmap, cx, cy);
                canvas.Flush();

                var image = SKImage.FromBitmap(croppedBitmap);
                var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);

                var croppedPath =
                    $"{fileInfo.Directory}\\{System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name)}.jpg";
                var renamedPath =
                    $"{fileInfo.Directory}\\{System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name)}.orig{fileInfo.Extension}";
                if (PreserveOriginal.IsPresent && PreserveOriginal.ToBool())
                {
                    WriteVerbose($"Saving original \"{renamedPath}\"");
                    fileInfo.MoveTo(renamedPath);
                }
                else if (!new[] {".jpg", ".jpeg"}.Contains(fileInfo.Extension))
                {
                    WriteVerbose($"Deleting original \"{fileInfo.Name}\"");
                    fileInfo.Delete();
                }

                using (var stream = new FileStream(croppedPath, FileMode.OpenOrCreate,
                    FileAccess.Write))
                {
                    data.SaveTo(stream);
                }

                data.Dispose();
                image.Dispose();
                canvas.Dispose();
                croppedBitmap.Dispose();
                originalBitmap.Dispose();
            }
        }
    }
}