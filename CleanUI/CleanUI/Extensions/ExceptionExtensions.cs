using System;

namespace CleanUI.Extensions
{
    public static class ExceptionExtensions
    {
        public static string BuildException(this Exception exception)
        {
            string result = exception.Message;
            if (exception.InnerException != null)
                result += "\n" + BuildException(exception.InnerException);
            return result;
        }
    }
}