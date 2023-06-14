namespace TeamsHackathonTest.Helper
{
    public interface IFileHelper
    {
        Task<string> ReadFromFile();
        Task WriteToFile();
    }
}
