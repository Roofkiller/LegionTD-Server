using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegionTDServerReborn.Responses
{
    public class FailureMessage
    {
        public int FailureCode { get; set; } = 0;
        public string Failure { get; set; } = "Unkown Error";
    }


    public class InvalidRequestFailure : FailureMessage
    {
        public InvalidRequestFailure()
        {
            Failure = "Invalid request.";
            FailureCode = 2;
        }
    }

    public class ConflictFailure : FailureMessage
    {
        public ConflictFailure()
        {
            Failure = "Conflict.";
            FailureCode = 3;
        }
    }

    public class NotFoundFailure : FailureMessage
    {
        public NotFoundFailure()
        {
            Failure = "Not found.";
            FailureCode = 4;
        }
    }

    public class MissingArgumentFailure : FailureMessage
    {
        public MissingArgumentFailure()
        {
            Failure = "Missing Arguments.";
            FailureCode = 5;
        }
    }

    public class NoPermissionFailure : FailureMessage {
        public NoPermissionFailure() {
            Failure = "You have no permission to do that.";
            FailureCode = 6;
        }
    }
}
