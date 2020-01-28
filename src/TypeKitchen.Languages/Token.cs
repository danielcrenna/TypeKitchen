using System.Diagnostics;

namespace TypeKitchen.Languages
{
    [DebuggerDisplay("{Type}:{Value}")]
    public class Token<T> where T : struct
    {
        public T Type { get; set; }
        public string Value { get; set; }
    }
}
