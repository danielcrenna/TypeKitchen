using Xunit;

namespace TypeKitchen.Tests
{
	public class DuckTests
	{
		[Fact]
		public void Can_duck_cast_as_interface()
		{
			var foo = new Foo { Bar = "Baz" }.QuackLike<IFoo>();
			Assert.Equal("Baz", foo.Bar);
		}

		public class Foo
		{
			public string Bar { get; set; }
		}

		public interface IFoo
		{
			string Bar { get; }
		}
	}
}
