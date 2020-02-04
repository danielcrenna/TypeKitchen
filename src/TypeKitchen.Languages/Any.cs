namespace TypeKitchen.Languages
{
	internal class Any<T> : ILexlet<T> where T : struct
	{
		private readonly T _type;
		private readonly char[] _tokens;

		public Any(T type, params char[] tokens)
		{
			_type = type;
			_tokens = tokens;
		}

		public T? Map(char c)
		{
			foreach(var token in _tokens)
				if (c == token)
					return _type;

			return default;
		}

		public Token<T> ReadToken(Lexer<T> lexer, T type) => lexer.Concat(this, type);
	}
}