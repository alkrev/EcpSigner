using System;

namespace Portal
{
    public class NotLoggedInException : Exception
    {
        public NotLoggedInException(string message): base(message)
        {

        }
    }
}
