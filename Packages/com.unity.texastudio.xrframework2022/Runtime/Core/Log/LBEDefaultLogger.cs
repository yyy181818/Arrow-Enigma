using System;
namespace TexaXRFramework.Core
{
    public class LBEDefaultLogger : INetLogger
    {
        public string Header;
        public LBEDefaultLogger(string header)
        {
            this.Header = header;
        }
        public void Log(string info)
        {
            Console.WriteLine($"[{Header}][log:]{info}");
        }

        public void LogError(string error)
        {
            Console.WriteLine($"[{Header}][error:]{error}");
        }

        public void LogWarning(string warning)
        {
            Console.WriteLine($"[{Header}][warning:]{warning}");
        }
    }
}