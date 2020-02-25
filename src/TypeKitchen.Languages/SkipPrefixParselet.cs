namespace TypeKitchen.Languages
{
	internal class SkipPrefixParselet<T> : IPrefixParselet<T> where T : struct
	{
		private static readonly SkipExpression SkipExpression = new SkipExpression();

		public IExpression Parse(Parser<T> parser, Token<T> token)
		{
			return SkipExpression;
		}
	}
}