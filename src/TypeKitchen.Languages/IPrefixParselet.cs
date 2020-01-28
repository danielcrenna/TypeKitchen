namespace TypeKitchen.Languages
{
    /// <summary>
    /// One of the two interfaces used by the Pratt parser. A PrefixParselet is
    /// associated with a token that appears at the beginning of an expression. Its
    /// Parse() method will be called with the consumed leading token, and the
    /// parselet is responsible for parsing anything that comes after that token.
    /// This interface is also used for single-token expressions like variables, in
    /// which case Parse() simply doesn't consume any more tokens.
    /// </summary>
    public interface IPrefixParselet<T> where T : struct
    {
        IExpression Parse(Parser<T> parser, Token<T> token);
    }
}
