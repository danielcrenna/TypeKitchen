using TypeKitchen.Tests.Serialization.V1;
using TypeKitchen.ValueHash;
using Xunit;

namespace TypeKitchen.Tests.ValueHash
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

			Assert.Equal(DiffDocument.Empty, Delta.ObjectToObject(left, right));
			Assert.Equal(DiffDocument.Empty, Delta.ObjectToObject(right, left));
		}

		[Fact]
		public void Different_values_produce_a_diff()
		{
			var daniel = new Person {Name = "Daniel"};
			var george = new Person {Name = "George"};

			var left = Delta.ObjectToObject(daniel, george);
			Assert.NotEqual(DiffDocument.Empty, left);
			Assert.Single(left.Operations);
			Assert.Equal(george.Name, left.Operations[0].Value);
			
			var right = Delta.ObjectToObject(george, daniel);
			Assert.NotEqual(DiffDocument.Empty, right);
			Assert.Single(right.Operations);
			Assert.Equal(daniel.Name, right.Operations[0].Value);
		}
	}
}
