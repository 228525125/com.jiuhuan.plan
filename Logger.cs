namespace com.jiuhuan.plan.tools
{
    public static class Logger
    {
        private static ILogUtility _logUtility;
        private static LogUploadHelper logUploadHelper = null;

        public static void Initialize(ILogUtility logUtility)
        {
            _logUtility = logUtility;
            logUploadHelper = new LogUploadHelper();
            logUploadHelper.start();
        }

        public static void Info(string message)
        {
            _logUtility?.Log(message);
        }

        public static void Upload()
        {
            _logUtility?.UploadLog();
        }

        public static bool IsException()
        {
            return null != _logUtility.GetException();
        }
    }
}