namespace TypeKitchen.Languages
{
	public interface ILexlet<T> where T : struct
	{
		T? Map(char c);
		Token<T> ReadToken(Lexer<T> lexer, T type);
	}
}