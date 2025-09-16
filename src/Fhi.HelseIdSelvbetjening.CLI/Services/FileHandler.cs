namespace Fhi.HelseIdSelvbetjening.CLI.Services
{
    internal interface IFileHandler
    {
        bool PathExists(string path);
        void CreateDirectory(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string content);
        void WriteAllBytes(string path, byte[] content);
    }
    internal class FileHandler : IFileHandler
    {
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public bool PathExists(string path)
        {
            return Directory.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }
        
        public void WriteAllBytes(string path, byte[] content)
        {
            File.WriteAllBytes(path, content);
        }
    }
}
