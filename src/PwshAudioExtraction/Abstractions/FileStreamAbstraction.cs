namespace PwshAudioExtraction.Abstractions
{
    using System.IO;
    using File = TagLib.File;

    // http://www.geekchamp.com/articles/reading-and-writing-metadata-tags-with-taglib
    public class FileStreamAbstraction : File.IFileAbstraction
    {
        public FileStreamAbstraction(FileStream stream)
        {
            Name = stream.Name;
            FileStream = stream;
        }

        public FileStream FileStream { get; }

        public string Name { get; }

        public Stream ReadStream => FileStream;

        public Stream WriteStream => FileStream;

        public void CloseStream(Stream stream)
        {
            stream.Position = 0;
        }
    }
}