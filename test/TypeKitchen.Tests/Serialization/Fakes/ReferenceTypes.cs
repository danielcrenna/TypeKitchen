using System;
using System.Collections;
using System.Collections.Generic;

namespace TypeKitchen.Tests.Serialization.Fakes
{
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