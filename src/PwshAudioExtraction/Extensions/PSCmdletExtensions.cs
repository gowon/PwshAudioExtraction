namespace PwshAudioExtraction.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.PowerShell.Commands;

    // ReSharper disable once InconsistentNaming
    public static class PSCmdletExtensions
    {
        public static List<string> ResolvePaths(this PSCmdlet cmdlet, bool shouldExpandWildcards, params string[] paths)
        {
            var resolvedPaths = new List<string>();
            foreach (var path in paths)
            {
                // this contains the paths to process for this iteration of the
                // loop to resolve and optionally expand wildcards.
                var filePaths = new List<string>();
                try
                {
                    // This will hold information about the provider containing
                    // the items that this path string might resolve to.                
                    ProviderInfo provider;
                    if (shouldExpandWildcards)
                    {
                        // Turn *.txt into foo.txt,foo2.txt etc.
                        // if path is just "foo.txt," it will return unchanged.
                        filePaths.AddRange(cmdlet.GetResolvedProviderPathFromPSPath(path, out provider));
                    }
                    else
                    {
                        // no wildcards, so don't try to expand any * or ? symbols.                    
                        filePaths.Add(cmdlet.SessionState.Path.GetUnresolvedProviderPathFromPSPath(
                            path, out provider, out _));
                    }

                    // ensure that this path (or set of paths after wildcard expansion)
                    // is on the filesystem. A wildcard can never expand to span multiple
                    // providers.
                    if (provider.ImplementingType != typeof(FileSystemProvider))
                    {
                        // create a .NET exception wrapping our error text
                        var ex = new ArgumentException(path +
                                                       " does not resolve to a path on the FileSystem provider.");
                        // wrap this in a powershell errorrecord
                        var error = new ErrorRecord(ex, "InvalidProvider",
                            ErrorCategory.InvalidArgument, path);
                        // write a non-terminating error to pipeline
                        cmdlet.WriteError(error);

                        // no, so skip to next path in _paths.
                        continue;
                    }
                }
                catch (Exception exception)
                {
                    // wrap this in a powershell errorrecord
                    var error = new ErrorRecord(exception, "InvalidPath",
                        ErrorCategory.InvalidArgument, path);
                    // write a non-terminating error to pipeline
                    cmdlet.WriteError(error);
                    continue;
                }

                resolvedPaths.AddRange(filePaths);
            }

            return resolvedPaths;
        }
    }
}