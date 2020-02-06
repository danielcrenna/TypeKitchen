using TypeKitchen.Differencing;
using TypeKitchen.Tests.Serialization.Fakes.V1;
using Xunit;

namespace TypeKitchen.Tests.Differencing
{
	public class DeltaTests
	{
		[Fact]
		public void Same_instance_has_no_diff()
		{
			var person = new Person { Name = "Daniel" };
			Assert.Equal(DiffDocument.Empty, Delta.ObjectToObject(person, person));
		}

		[Fact]
		public void Same_value_has_no_diff()
		{
			var left = new Person {Name = "Daniel"};
			var right = new Person {Name = "Daniel"};

			Assert.Same(DiffDocument.Empty, Delta.ObjectToObject(left, right));
			Assert.Same(DiffDocument.Empty, Delta.ObjectToObject(right, left));
		}

		[Fact]
		public void Different_values_produce_a_diff()
		{
			var daniel = new Person {Name = "Daniel"};
			var george = new Person {Name = "George"};

			var rightToLeft = Delta.ObjectToObject(daniel, george);
			Assert.NotSame(DiffDocument.Empty, rightToLeft);
			Assert.Single(rightToLeft.Operations);
			Assert.Equal(george.Name, rightToLeft.Operations[0].Value);

			rightToLeft.ApplyTo(daniel);
			Assert.Equal(daniel.Name, george.Name);
		}
	}
}
