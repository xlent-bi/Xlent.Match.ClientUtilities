using System;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    /// <summary>
    /// Exceptions of this category indicates that the request could not be carried out as specified.
    /// If the request is reformulated, then it can be carried out. 
    /// </summary>
    public abstract class Warning : BaseClass
    {
        public const string Level = "Warning";

        protected Warning(string errorType, string message)
            : base(Level, errorType, message)
        {
        }
    }
}
