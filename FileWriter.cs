namespace MyShell.Core
{
    public static class FileWriter
    {
        public static void Write(string? output, string filePath, bool append = false)
        {
            try
            {
                using var writer = new StreamWriter(filePath, append);
                if (!string.IsNullOrEmpty(output))
                {
                    writer.WriteLine(output);
                }
                else
                {
                    writer.Write("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to file '{filePath}': {ex.Message}");
            }
        }
    }
}
