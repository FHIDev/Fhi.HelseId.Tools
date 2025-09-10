using Fhi.HelseIdSelvbetjening.CLI.Services;
using System.Collections.Concurrent;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup
{
    internal class FileHandlerMock : IFileHandler
    {
        public ConcurrentDictionary<string, string> Files = new();

        public void CreateDirectory(string path)
        {
        }

        public bool PathExists(string path)
        {
            return true;
        }

        public string ReadAllText(string path)
        {
            if (Files.TryGetValue(path, out string? value))
                return value;

            return string.Empty;
        }

        public void WriteAllText(string path, string content)
        {
            Files.TryAdd(path, content);
        }
        
        public void WriteAllBytes(string path, byte[] content)
        {
            Files.TryAdd(path, System.Text.Encoding.UTF8.GetString(content));
        }
    }
}
