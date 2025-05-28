using Fhi.HelseIdSelvbetjening.CLI.Tests.Utilities;

namespace Fhi.HelseIdSelvbetjening.CLI.AcceptanceTests
{
    /// <summary>
    /// Tests for path resolution in acceptance tests
    /// </summary>
    public class PathResolutionTests
    {
        [Test]        
        public void GetTestProjectDirectory_ShouldReturnValidPath()
        {
            // This test verifies that our path resolution logic works correctly
            var testProjectDirectory = TestPathHelper.GetTestProjectDirectory();
            
            // Verify that the directory exists
            Assert.That(Directory.Exists(testProjectDirectory), Is.True, 
                $"Test project directory should exist: {testProjectDirectory}");
            
            // Verify that it contains a .csproj file
            var csprojFiles = Directory.GetFiles(testProjectDirectory, "*.csproj");
            Assert.That(csprojFiles, Is.Not.Empty, 
                "Test project directory should contain a .csproj file");
            
            // Verify that TestData directory can be created/accessed
            var testDataDirectory = TestPathHelper.GetTestDataDirectory();
            
            // Create TestData directory if it doesn't exist (for the test)
            if (!Directory.Exists(testDataDirectory))
            {
                Directory.CreateDirectory(testDataDirectory);
            }
            
            Assert.That(Directory.Exists(testDataDirectory), Is.True,
                $"TestData directory should exist or be creatable: {testDataDirectory}");
        }
    }
}
