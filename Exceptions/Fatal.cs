using System;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    /// <summary>
    /// Exceptions of this category indicates that the adapter has failed.
    /// </summary>
    public abstract class Fatal : BaseClass
    {
        public const string Level = "Fatal";

        protected Fatal(string errorType, string message)
            : base(Level, errorType, message)
        {
        }

        protected Fatal(string errorType, string message, Exception exception)
            : base(Level, errorType, message, exception)
        {
        }
    }
}
