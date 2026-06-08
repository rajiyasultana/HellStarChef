// Instead of taking a 'path', take the fully loaded text.
public class CsvCompatibilityRepository : ICompatibilityRepository
{
    private readonly string _csvContent;

    public CsvCompatibilityRepository(string csvContent)
    {
         _csvContent = csvContent;
         // Parse the _csvContent here instead of using File.ReadAllText(path)...
    }
}