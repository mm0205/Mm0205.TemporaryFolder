using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Mm0205.TemporaryFolder
{
    /// <summary>
    /// Temporary Folder.
    /// </summary>
    public class TemporaryFolder : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Path to the temporary folder.<br/>
        /// The path is computed according to following code:
        /// <code>
        /// FolderPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), tempFolderName).
        /// </code>
        /// The <c>tempFolderName</c> is the argument of the constructor.<br/>
        /// If folder name is not passed to constructor, the folder name will be
        /// <c>tempFolderName = Guid.NewGuid().ToString().ToLowerInvariant()</c>.
        /// </summary>
        public string FolderPath { get; }

        private readonly IFileSystem _fileSystem;

        private TemporaryFolder(string folderPath, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            FolderPath = folderPath;
        }

        /// <summary>
        /// Creates a new instance of <see cref="TemporaryFolder"/>.
        /// <br/>
        /// This method throws exceptions when failed to create temp folder.
        /// See [MS docs](https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.createdirectory).
        /// </summary>
        /// <param name="tempFolderName">
        /// Temporary folder name. see <see cref="FolderPath"/>.<br/>
        /// If this parameter is null or empty, this method uses <c>Guid.NewGuid().ToString().ToLowerInvariant()</c>.
        /// </param>
        /// <param name="fileSystem">
        /// The file system object.<br/>
        /// This parameter is for unit testing. So should not pass the parameter in actual operation.<br/>
        /// If fileSystem isn't passed, this method uses the default <see cref="System.IO.Abstractions.FileSystem">FileSystem</see> object.<br/>
        /// </param>
        // ReSharper disable once MemberCanBePrivate.Global
        public static TemporaryFolder Create(
            string? tempFolderName = null,
            IFileSystem? fileSystem = null)
        {
            tempFolderName = string.IsNullOrEmpty(tempFolderName)
                ? Guid.NewGuid().ToString().ToLowerInvariant()
                : tempFolderName;
            fileSystem ??= new FileSystem();

            var folderPath = Path.Combine(Path.GetTempPath(), tempFolderName);
            fileSystem.Directory.CreateDirectory(folderPath);
            return new TemporaryFolder(folderPath, fileSystem);
        }

        #region IDisposable, IAsyncDisposable

        /// <summary>
        /// Release unmanaged resources.
        /// <br/>
        /// This method recursively delete the temp folder created by this instance. 
        /// </summary>
        private void ReleaseUnmanagedResources()
        {
            if (_fileSystem.Directory.Exists(FolderPath))
            {
                _fileSystem.Directory.Delete(FolderPath, true);
            }
        }

        // ReSharper disable once UnusedParameter.Global
        protected virtual void Dispose(bool _)
        {
            // This class has no managed resources
            // so release only unmanaged (the directory created by this instance).
            ReleaseUnmanagedResources();
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The finalizer.
        /// </summary>
        ~TemporaryFolder()
        {
            Dispose(false);
        }

        /// <summary>
        /// This is for <c>await using</c>.
        /// </summary>
        /// <returns>The task.</returns>
        public ValueTask DisposeAsync()
        {
            Dispose(false);
            
            // code from https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
            
            return ValueTask.CompletedTask;
        }

        #endregion
    }
}