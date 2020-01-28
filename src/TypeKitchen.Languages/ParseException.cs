using System;

namespace TypeKitchen.Languages
{
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {

        }
    }
}
