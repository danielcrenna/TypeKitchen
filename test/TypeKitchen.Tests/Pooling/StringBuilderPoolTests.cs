using Xunit;

namespace TypeKitchen.Tests.Pooling
{
	public class StringBuilderPoolTests
	{
		[Fact]
		public void Can_use_scoped_builder()
		{
			var result = TypeKitchen.Pooling.StringBuilderPool.Scoped(sb =>
			{
				sb.Append("This is a line.");
			});
			Assert.Equal("This is a line.", result);

			result = TypeKitchen.Pooling.StringBuilderPool.Scoped(sb =>
			{
				sb.Append("This is a line.");
			}, 10, 4);
			Assert.Equal("line", result);
		}
	}
}
