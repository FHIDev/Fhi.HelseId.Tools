namespace Fhi.HelseIdSelvbetjening.Services
{
    internal interface IFileHandler
    {
        void WriteAllText(string path, string content);

        string ReadAllText(string path);
    }
    internal class FileHandler : IFileHandler
    {
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }
    }
}
