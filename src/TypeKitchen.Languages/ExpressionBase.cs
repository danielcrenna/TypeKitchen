using System.Text;

namespace TypeKitchen.Languages
{
    public abstract class ExpressionBase : IExpression
    {
        public override string ToString()
        {
	        return Pooling.StringBuilderPool.Scoped(Print);
        }

        public abstract void Print(StringBuilder sb);
    }
}