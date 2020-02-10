using System;
using System.Linq;
using System.Reflection;

namespace TypeKitchen.Creation
{
	public static class TypeExtensions
	{
		public static ConstructorInfo GetWidestConstructor(this Type implementationType)
		{
			return GetWidestConstructor(implementationType, out _);
		}

		public static ConstructorInfo GetWidestConstructor(this Type implementationType, out ParameterInfo[] parameters)
		{
			var allPublic = implementationType.GetConstructors();
			var constructor = allPublic.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
			if (constructor == null)
			{
				parameters = new ParameterInfo[0];
				return implementationType.GetConstructor(Type.EmptyTypes);
			}
			parameters = constructor.GetParameters();
			return constructor;
		}
	}
}
