using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TypeKitchen.Languages
{
    /// <summary>
    /// A very primitive lexer; it takes a string and splits it into a series of
    /// Tokens. Operators and punctuation are mapped to unique keywords. Names,
    /// which can be any series of letters, are turned into NAME tokens, and all other
    /// characters are ignored (except to separate names). Numbers and strings are
    /// not supported. This is really just the bare minimum to give the parser
    /// something to work with.
    /// </summary>
    public class Lexer<T> : IEnumerator<Token<T>> where T : struct
    {
	    private readonly List<ILexlet<T>> _lexlets;

	    public Lexer(string text, T eof)
        {
            _index = 0;
            _text = text;
            _lexlets = new List<ILexlet<T>>();
			_eofToken = new Token<T> { Type = eof, Value = string.Empty};
        }

        private readonly string _text;
        private int _index;
        private readonly Token<T> _eofToken;

        public void Register(ILexlet<T> lexlet)
        {
			_lexlets.Add(lexlet);
        }

        public void Any(T type, params char[] tokens)
        {
	        _lexlets.Add(new Any<T>(type, tokens));
        }

        public char? Peek()
        {
	        return _index < _text.Length - 1 ? _text[_index + 1] : default(char?);
        }

        public bool MoveNext()
        {
            while (_index < _text.Length - 1)
            {
                var c = _text[_index++];

                foreach (var lexlet in _lexlets)
                {
	                var type = lexlet.Map(c);
	                if (!type.HasValue)
		                continue;
	                Current = lexlet.ReadToken(this, type.Value);
					return true;
                }
            }

            Current = _eofToken;
            return true;
        }

        public void Reset()
        {
            _index = 0;
        }

        public Token<T>  Current { get; private set; }

        object IEnumerator.Current => Current;

        public Token<T> Concat(ILexlet<T> lexlet, T type)
        {
	        var sb = Pooling.StringBuilderPool.Get();
	        try
	        {
		        sb.Append(_text[_index]);

		        while (true)
		        {
			        var c = Peek();
			        if (!c.HasValue)
				        return new Token<T> { Type = type, Value = sb.ToString() };

			        var t = lexlet.Map(c.Value);
			        if (!t.HasValue || !t.Value.Equals(type))
				        return new Token<T> { Type = type, Value = sb.ToString() };

			        sb.Append(c);
			        if(_index < _text.Length)
						_index++;
		        }
	        }
	        finally
	        {
		        Pooling.StringBuilderPool.Return(sb);
	        }
        }

        public void Dispose() { }
    }
}