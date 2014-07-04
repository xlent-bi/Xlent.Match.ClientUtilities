using System;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class SilentFailOnlyForTestingException : Exception
    {
        public SilentFailOnlyForTestingException(string message = "")
            : base(message)
        {
        }
    }
}