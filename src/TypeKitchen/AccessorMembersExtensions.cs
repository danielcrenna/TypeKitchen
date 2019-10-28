using System.ComponentModel.DataAnnotations;

namespace TypeKitchen
{
	public static class AccessorMembersExtensions
	{
		public static string DisplayName(this AccessorMembers members, string memberName, string profile = "Default")
		{
			return !members.TryGetValue(memberName, out var member)
				? memberName
				: member.Display(profile).Name;
		}

		public static string Prompt(this AccessorMembers members, string memberName, string profile = "Default")
		{
			return !members.TryGetValue(memberName, out var member)
				? string.Empty
				: member.Display(profile).Prompt;
		}

		public static string DateFormat(this AccessorMembers members, string memberName, string profile = "Default")
		{
			return !members.TryGetValue(memberName, out var member)
				? string.Empty
				: member.Display(profile).DateFormat;
		}

		public static bool IsReadOnly(this AccessorMembers members, string memberName, string profile = "Default")
		{
			return members.TryGetValue(memberName, out var member) &&
			       member.Display(profile).IsReadOnly;
		}
		
		public static bool IsDataType(this AccessorMembers members, string memberName, DataType dataType, string profile = "Default")
		{
			return members.TryGetValue(memberName, out var member) &&
			       member.Display(profile).DataType == dataType;
		}
	}
}