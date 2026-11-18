namespace TexaXRFramework.Core
{
    public class U3DNetLogger : INetLogger
    {
        public void Log(string info)
        {
            LBELogger.LogInfo(info);
        }

        public void LogError(string error)
        {
            LBELogger.LogError(error);
        }

        public void LogWarning(string warning)
        {
            LBELogger.LogWarning(warning);
        }
    }
}
