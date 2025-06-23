namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup
{
    internal class FileHandlerBuilder
    {
        private readonly FileHandlerMock _fileHandlerMock = new FileHandlerMock();

        public FileHandlerBuilder WithExistingPrivateJwk(string path, string? content = null)
        {
            _fileHandlerMock.Files[path] = content ?? path;
            return this;
        }

        public FileHandlerBuilder WithNewPublicJwk(string path, string? content = null)
        {
            _fileHandlerMock.Files[path] = content ?? path;
            return this;
        }

        public FileHandlerMock Build() => _fileHandlerMock;
    }
}
