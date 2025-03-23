namespace Fhi.HelseIdSelvbetjening.Services
{
    public interface IFileHandler
    {
        void WriteAllText(string path, string content);

        string ReadAllText(string path);
    }
    public class FileHandler : IFileHandler
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
