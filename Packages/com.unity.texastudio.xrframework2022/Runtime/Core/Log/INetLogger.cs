namespace TexaXRFramework.Core
{
    public interface INetLogger
    {
        void Log(string info);
        void LogError(string error);
        void LogWarning(string warning);
    }
}