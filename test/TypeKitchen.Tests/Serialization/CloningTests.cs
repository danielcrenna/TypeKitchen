using TypeKitchen.Differencing;
using TypeKitchen.Serialization;
using TypeKitchen.Tests.Serialization.Fakes;
using Xunit;

namespace TypeKitchen.Tests.Serialization
{
	public class CloningTests
	{
		[Theory]
		[ClassData(typeof(ValueTypes))]
		[ClassData(typeof(ReferenceTypes))]
		public void BasicTests_round_trips(object source)
		{
			var target = Cloning.ShallowCopy(source, new ReflectionTypeResolver());
			AssertEqualByValue(source, target);
		}

		private static void AssertEqualByValue(object x, object y)
		{
			Assert.Equal(ValueHash.ComputeHash(x), ValueHash.ComputeHash(y));
		}
	}
}
