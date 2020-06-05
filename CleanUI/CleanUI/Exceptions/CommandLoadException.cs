using System;

namespace CleanUI.Exceptions
{
    public class CommandLoadException : ApplicationException
    {
        public CommandLoadException(Exception exception) : base("Unable to load command", exception)
        {
        }
    }
}