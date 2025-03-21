using Fhi.HelseId.ClientSecret.App.Services;
using System.Collections.Concurrent;

namespace Fhi.HelseId.ClientSecret.App.Tests
{
    class FileHandlerMock : IFileHandler
    {
        public ConcurrentDictionary<string, string> _files = new ConcurrentDictionary<string, string>();

        public string ReadAllText(string path)
        {
            if (_files.ContainsKey(path))
                return _files[path];

            return string.Empty;
        }

        public void WriteAllText(string path, string content)
        {
            _files.TryAdd(path, content);
        }
    }
}
