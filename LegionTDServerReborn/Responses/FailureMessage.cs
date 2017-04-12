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

    public class NotFoundFailure : FailureMessage
    {
        public NotFoundFailure()
        {
            Failure = "Not found.";
            FailureCode = 4;
        }
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

    public class MissingArgumentFailure : FailureMessage
    {
        public MissingArgumentFailure()
        {
            Failure = "Missing Arguments.";
            FailureCode = 5;
        }
    }
}
