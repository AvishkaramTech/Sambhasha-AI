namespace TeamsHackathonTest.Helper
{
    public class FileHelper:IFileHelper
    {
        private readonly string _filePath = AppDomain.CurrentDomain.BaseDirectory + "logFile.txt";
        public async Task<string> ReadFromFile()
        {
            string filePath = Path.Combine(_filePath);
            string lastDate = string.Empty;
            CreateFileIfNotExists();
            using (StreamReader reader = new StreamReader(filePath))
            {
                lastDate =await reader.ReadToEndAsync();
            }
            return lastDate;            
        }
        public async Task WriteToFile()
        {
            CreateFileIfNotExists();
            using (StreamWriter writer = new StreamWriter(_filePath))
            {
                await writer.WriteLineAsync(DateTime.UtcNow.ToString());
            }
        }
        private void CreateFileIfNotExists()
        {
            if (!File.Exists(_filePath))
            {
                // Create the file
                using (FileStream fileStream = File.Create(_filePath))
                {

                }               
            }
        }
    }
}
