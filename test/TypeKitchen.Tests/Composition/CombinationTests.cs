using System.Linq;
using TypeKitchen.Composition;
using Xunit;

namespace TypeKitchen.Tests.Composition
{
	public class CombinationTests
	{
		[Fact]
		public void Can_get_k_combinations_of_n()
		{
			var combinations = new[] { "A", "B", "C"}.GetCombinations(1);
			Assert.NotNull(combinations);
			Assert.Equal(3, combinations.Distinct().Count());
		}
	}
}

