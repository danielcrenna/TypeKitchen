using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TypeKitchen
{
	public class AccessorMemberDisplay
	{
		public string Name { get; set; }
		public string Prompt { get; set; }
		public string CustomDataType { get; set; }
		public DataType DataType { get; set; }

		public AccessorMemberDisplay(AccessorMember member)
		{
			ResolveName(member);
			ResolvePrompt(member);
			ResolveDataType(member);
		}

		private void ResolvePrompt(AccessorMember member)
		{
			Prompt = member.TryGetAttribute(out DisplayAttribute display) ? display.GetPrompt() : string.Empty;
		}

		private void ResolveDataType(AccessorMember member)
		{
			if (!member.TryGetAttribute(out DataTypeAttribute dataType))
			{
				DataType = DataType.Text;
				return;
			}
			DataType = dataType.DataType;
			if (DataType == DataType.Custom)
				CustomDataType = dataType.CustomDataType;
		}

		private void ResolveName(AccessorMember member)
		{
			if (member.TryGetAttribute(out DisplayAttribute display))
			{
				Name = display.GetName() ?? display.GetShortName();
				if (Name != null)
					return;
			}
			
			if (member.TryGetAttribute(out DisplayNameAttribute displayName))
			{
				Name = displayName.DisplayName;
				if (Name != null)
					return;
			}

			Name = member.Name;
		}
	}
}