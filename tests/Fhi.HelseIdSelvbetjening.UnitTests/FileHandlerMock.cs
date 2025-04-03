using Fhi.HelseIdSelvbetjening.Services;
using System.Collections.Concurrent;

namespace Fhi.HelseId.ClientSecret.App.Tests
{
    class FileHandlerMock : IFileHandler
    {
        public ConcurrentDictionary<string, string> _files = new ConcurrentDictionary<string, string>();

        public string ReadAllText(string path) => _files.ContainsKey(path) ? _files[path] : string.Empty;

        public void WriteAllText(string path, string content)
        {
            _files.TryAdd(path, content);
        }
    }
}
