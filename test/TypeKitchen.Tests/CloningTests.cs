using System;
using System.Collections;
using System.Collections.Generic;
using TypeKitchen.Serialization;
using Xunit;

namespace TypeKitchen.Tests
{
	public class CloningTests
	{
		[Theory]
		[ClassData(typeof(ValueTypes))]
		[ClassData(typeof(ReferenceTypes))]
		public void BasicTests_round_trips(object source)
		{
			var target = Cloning.ShallowCopy(source);
			AssertEqualByValue(source, target);
		}

		private static void AssertEqualByValue(object x, object y)
		{
			Assert.Equal(ValueHash.ValueHash.ComputeHash(x), ValueHash.ValueHash.ComputeHash(y));
		}
	}

	public class Nested
	{
		public Dictionary<string, NestedType> Map { get; set; }
	}

	public class NestedType
	{
		public DateTimeOffset TimeStamp { get; set; }
	}

	public class ValueTypes : IEnumerable<object[]>
	{
		private static object[] Single(object instance) => new[] { instance };

		public IEnumerator<object[]> GetEnumerator()
		{
			yield return Single((byte) 255);
			yield return Single((sbyte) -128);
			yield return Single(true);
			yield return Single(false);
			yield return Single(123);
			yield return Single((short) 123);
			yield return Single((ushort) 123);
			yield return Single(12345678L);
			yield return Single(12345678UL);
			yield return Single(123.456f);
			yield return Single(123.456m);
			yield return Single(123.456d);

			yield return Single(default(TimeSpan));
			yield return Single(default(DateTime));
			yield return Single(default(DateTimeOffset));

			yield return Single(default(TimeSpan?));
			yield return Single(default(DateTime?));
			yield return Single(default(DateTimeOffset?));
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class ReferenceTypes : IEnumerable<object[]>
	{
		private static object[] Single(object instance) => new[] { instance };

		public IEnumerator<object[]> GetEnumerator()
		{
			yield return Single("rosebud");
			yield return Single(new Nested
			{
				Map = new Dictionary<string, NestedType>
				{
					{
						"Key", new NestedType { TimeStamp = DateTimeOffset.UtcNow }
					}
				}
			});
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
