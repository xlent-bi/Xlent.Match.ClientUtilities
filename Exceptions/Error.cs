using System;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    /// <summary>
    /// Exceptions of this category indicates that the caller made an error.
    /// </summary>
    public abstract class Error : BaseClass
    {
        public const string Level = "Error";

        protected Error(string errorType, string message)
            : base(Level, errorType, message)
        {
        }
    }
}