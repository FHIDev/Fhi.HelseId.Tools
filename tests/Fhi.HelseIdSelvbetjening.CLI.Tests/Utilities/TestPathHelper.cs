namespace Fhi.HelseIdSelvbetjening.CLI.Tests.Utilities
{
    /// <summary>
    /// Helper class for resolving test paths and directories
    /// </summary>
    public static class TestPathHelper
    {
        /// <summary>
        /// Gets the test project directory (not the bin output directory)
        /// </summary>
        public static string GetTestProjectDirectory()
        {
            // Start from the current directory (bin/Debug/net9.0) and navigate back to the test project root
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            
            // Navigate up until we find the .csproj file or reach a reasonable limit
            while (currentDirectory != null && currentDirectory.Parent != null)
            {
                var csprojFiles = currentDirectory.GetFiles("*.csproj");
                if (csprojFiles.Length > 0)
                {
                    return currentDirectory.FullName;
                }
                currentDirectory = currentDirectory.Parent;
            }
            
            // Fallback: assume we're in bin/Debug/net9.0 and go up 3 levels
            var fallbackPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "..");
            return Path.GetFullPath(fallbackPath);
        }        
        /// <summary>
        /// Gets the TestData directory path for the test project
        /// </summary>
        public static string GetTestDataDirectory()
        {
            return Path.Combine(GetTestProjectDirectory());
        }

        /// <summary>
        /// Gets the path to a file within the TestData directory
        /// </summary>
        /// <param name="fileName">The name of the file within TestData</param>
        /// <returns>The full path to the file</returns>
        public static string GetTestDataFilePath(string fileName)
        {
            return Path.Combine(GetTestDataDirectory(), fileName);
        }
    }
}
