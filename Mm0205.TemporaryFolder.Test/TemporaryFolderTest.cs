using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;

namespace Mm0205.TemporaryFolder.Test
{
    public class TemporaryFolderTest
    {
        [Fact(DisplayName = "The temporary folder should exist.")]
        public void TestCreation()
        {
            var mockFileSystem = new MockFileSystem();
            var tempFolder = TemporaryFolder.Create("test-temp-folder", mockFileSystem);
            Assert.True(mockFileSystem.Directory.Exists(tempFolder.FolderPath));
        }
        
        [Fact(DisplayName = "The temporary folder must not exist after disposing.")]
        public void TestDispose()
        {
            var mockFileSystem = new MockFileSystem();
            var tempFolder = TemporaryFolder.Create("test-temp-folder", mockFileSystem);
            Assert.True(mockFileSystem.Directory.Exists(tempFolder.FolderPath));
            tempFolder.Dispose();
            Assert.False(mockFileSystem.Directory.Exists(tempFolder.FolderPath));
        }
        
        [Fact(DisplayName = "The file should be created in the temporary folder.")]
        public void TestUsing()
        {
            var mockFileSystem = new MockFileSystem();
            string? filePath = null;
            
            using (var tempFolder = TemporaryFolder.Create("test-temp-folder", mockFileSystem))
            {
                Assert.True(mockFileSystem.Directory.Exists(tempFolder.FolderPath));

                filePath = Path.Combine(tempFolder.FolderPath, "test-file-name");

                const string expectedFileContent = "test message";
                mockFileSystem.File.WriteAllText(filePath, expectedFileContent);
                
                Assert.True(mockFileSystem.File.Exists(filePath));
                
                Assert.Equal(expectedFileContent, mockFileSystem.File.ReadAllText(filePath));
            }

            Assert.False(mockFileSystem.File.Exists(filePath));
            Assert.False(mockFileSystem.Directory.Exists(Path.GetDirectoryName(filePath)));
        }
        
        [Fact(DisplayName = "All file should be deleted after disposing.")]
        public void TestAllFileShouldDeleted()
        {
            var mockFileSystem = new MockFileSystem();

            string? folderPath = null;
            using (var tempFolder = TemporaryFolder.Create("test-temp-folder", mockFileSystem))
            {
                folderPath = tempFolder.FolderPath;
                foreach (var i in Enumerable.Range(1, 10))
                {
                    var filePath = Path.Combine(tempFolder.FolderPath, i.ToString());
                    mockFileSystem.File.WriteAllText(filePath, i.ToString());
                }
            }

            Assert.NotNull(folderPath);
            Assert.False(mockFileSystem.Directory.Exists(folderPath));
        }
    }
}