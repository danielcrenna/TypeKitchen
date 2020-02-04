namespace TypeKitchen.Languages
{
	internal class SkipPrefixParselet<T> : IPrefixParselet<T> where T : struct
	{
		public IExpression Parse(Parser<T> parser, Token<T> token)
		{
			return new SkipExpression();
		}
	}
}