using System;
using System.Collections.Generic;

namespace TypeKitchen.Languages
{
    /// <summary>
    /// A top down infix using Pratt's precedence
    /// This is an idiomatic port of the infix parser at https://github.com/munificent/bantam
    /// </summary>
    /// <seealso href="https://github.com/munificent/bantam" />
    /// <seealso href="http://en.wikipedia.org/wiki/Pratt_parser" />
    /// <seealso href="http://en.wikipedia.org/wiki/Recursive_descent_parser" />
    /// <seealso href="http://effbot.org/zone/simple-top-down-parsing.htm" />
    public class Parser<T> where T : struct
    {
	    private readonly IEnumerator<Token<T>> _tokens;
	    private readonly T _eof;
	    private readonly T _line;
	    private readonly IList<Token<T>> _readTokens = new List<Token<T>>();

	    private readonly IDictionary<T, IPrefixParselet<T>> _prefix = new Dictionary<T, IPrefixParselet<T>>();
	    private readonly IDictionary<T, IInfixParselet<T>> _infix = new Dictionary<T, IInfixParselet<T>>();

        public Parser(IEnumerator<Token<T>> tokens, T eof, T line)
        {
	        _tokens = tokens;
	        _eof = eof;
	        _line = line;
        }

        public void Ignore(T token)
        {
	        _prefix.Add(token, new SkipPrefixParselet<T>());
        }

        public void Register(T token, IPrefixParselet<T> prefix)
        {
            _prefix.Add(token, prefix);
        }

        public void Register(T token, IInfixParselet<T> infix)
        {
            _infix.Add(token, infix);
        }

        public List<IExpression> ParseModule()
        {
	        var expressions = new List<IExpression>();
	        while (!EndOfFile())
	        {
		        var expression = ParseStatement();
		        expressions.Add(expression);
		        if(!LookAhead().Type.Equals(_eof))
			        Consume(_line);
	        }
	        return expressions;

	        bool EndOfFile() => LookAhead().Type.Equals(_eof);
        }

        private IExpression ParseStatement()
		{
			return ParseExpression(0);
		}

        public IExpression ParseExpression(Precedence precedence = 0)
        {
            var token = Consume();

			if(!_prefix.TryGetValue(token.Type, out var prefix))
				throw new ParseException($"Could not parse \"{token.Value}\".");

            var left = prefix.Parse(this, token);

            while (precedence < GetPrecedence())
            {
                token = Consume();
                var infix = _infix[token.Type];
                left = infix.Parse(this, left, token);
            }

            return left;
        }
        public bool Match(T expected)
        {
            if (!LookAhead().Type.Equals(expected))
	            return false;
            Consume();
            return true;
        }

        public Token<T> Consume(T expected)
        {
	        var token = LookAhead();
	        return !token.Type.Equals(expected)
		        ? throw new ApplicationException($"Expected token {expected} and found {token.Type}")
		        : Consume();
        }

        public Token<T> Consume()
        {
            LookAhead();
            var token = _readTokens[0];
            _readTokens.RemoveAt(0);
            return token;
        }

        private Token<T> LookAhead(int distance = 0)
        {
            while (distance >= _readTokens.Count)
            {
                _tokens.MoveNext();
                _readTokens.Add(_tokens.Current);
            }
            return _readTokens[distance];
        }

        private Precedence GetPrecedence()
        {
	        var type = LookAhead().Type;
	        if(!_infix.TryGetValue(type, out var infix))
				return Precedence.Unknown;
			return infix?.Precedence ?? Precedence.Unknown;
        }
    }
}
