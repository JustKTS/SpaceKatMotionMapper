namespace SpaceKat.Shared.Functions;

public static class FileOccupiedChecker
{
    public static bool IsOccupied(string filePath)
    {
        FileStream? stream = null;
        try
        {
            stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch
        {
            return true;
        }
        finally
        {
            stream?.Close();
        }
    }
}