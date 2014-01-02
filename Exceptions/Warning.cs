using System;
using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    /// <summary>
    /// Exceptions of this category indicates that the request could not be carried out as specified.
    /// If the request is reformulated, then it can be carried out. 
    /// </summary>
    public abstract class Warning : BaseClass
    {
        protected Warning(FailureResponse.ErrorTypeEnum errorType, string message)
            : base(FailureResponse.ErrorLevelEnum.Warning, errorType, message)
        {
        }
    }
}
