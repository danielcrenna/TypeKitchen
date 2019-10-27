using System.ComponentModel.DataAnnotations;

namespace TypeKitchen
{
	public static class AccessorMembersExtensions
	{
		public static string DisplayName(this AccessorMembers members, string memberName)
		{
			return !members.TryGetValue(memberName, out var member)
				? memberName
				: member.Display.Name;
		}

		public static string Prompt(this AccessorMembers members, string memberName)
		{
			return !members.TryGetValue(memberName, out var member)
				? string.Empty
				: member.Display.Prompt;
		}

		public static bool IsDataType(this AccessorMembers members, string memberName, DataType dataType)
		{
			return members.TryGetValue(memberName, out var member) &&
			       member.TryGetAttribute(out DataTypeAttribute dataTypeAttribute) &&
			       dataTypeAttribute.DataType == dataType;
		}
	}
}