using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;

namespace TypeKitchen
{
	public static class AssemblyHash
	{
		public static byte[] GetBuildIndependent(Assembly assembly)
		{
			var definition = AssemblyDefinition.ReadAssembly(assembly.Location);
			return definition.MainModule.GetBuildIndependentHash(MD5.Create());
		}

		internal static byte[] GetBuildDependent(Assembly assembly)
		{
			var definition = AssemblyDefinition.ReadAssembly(assembly.Location);
			return definition.MainModule.GetBuildDependentHash(MD5.Create());
		}
	}
}
