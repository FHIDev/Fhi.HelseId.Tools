using Fhi.HelseIdSelvbetjening.CLI.Services;
using System.Collections.Concurrent;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup
{
    internal class FileHandlerMock : IFileHandler
    {
        public ConcurrentDictionary<string, string> _files = new();

        public void CreateDirectory(string path)
        {
        }

        public bool PathExists(string path)
        {
            return true;
        }

        public string ReadAllText(string path)
        {
            if (_files.TryGetValue(path, out string? value))
                return value;

            return string.Empty;
        }

        public void WriteAllText(string path, string content)
        {
            _files.TryAdd(path, content);
        }
    }
}
