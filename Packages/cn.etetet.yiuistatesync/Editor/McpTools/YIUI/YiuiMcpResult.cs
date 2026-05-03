namespace YIUIFramework.Editor
{
    public sealed class YiuiMcpResult
    {
        public bool Success { get; }
        public string Message { get; }
        public object Data { get; }

        private YiuiMcpResult(bool success, string message, object data)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static YiuiMcpResult Ok(string message, object data = null)
        {
            return new YiuiMcpResult(true, message, data);
        }

        public static YiuiMcpResult Fail(string message, object data = null)
        {
            return new YiuiMcpResult(false, message, data);
        }
    }
}
