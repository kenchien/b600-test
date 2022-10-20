using System;
using System.Runtime.Serialization;

namespace TldcFare.Dal.Common
{
    [Serializable]
    public class CustomException : Exception
    {
        public CustomException()
        {
        }

        public CustomException(string message) : base(message)
        {
        }
    }
}