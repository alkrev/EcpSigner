using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcpSigner.Domain.Exceptions
{
    ///<summary>Исключение, которое возникает когда полученный токен 
    ///не действует (по любой причине)</summary>
    public class IsNotLoggedInException : Exception
    {
        public IsNotLoggedInException(string message) : base(message) { }
    }
    ///<summary>Исключение, возникновение которого означает, что дальнейшая 
    ///работа невозможна</summary>
    public class BreakWorkException : Exception
    {
        public BreakWorkException(string message) : base(message) { }
    }
    ///<summary>Исключение, при котором действие можно повторить</summary>
    public class RetryableException : Exception
    {
        public RetryableException(string message) : base(message) { }
    }
    ///<summary>Исключение, которое сигнализирует об остановке работы</summary>
    public class StopWorkException : Exception { }
    ///<summary>Исключение, при котором можно завершить выполнение текущей работы (например, нечего делать)</summary>
    public class ContinueException : Exception
    {
        public ContinueException(string message) : base(message) { }
    }
    ///<summary>Исключение, возникающее при ошибке подписания документа</summary>
    public class DocumentSigningException : Exception
    {
        public DocumentSigningException(string message) : base(message) { }
    }
}
