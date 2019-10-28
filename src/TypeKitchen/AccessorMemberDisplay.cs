using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TypeKitchen
{
	public sealed class AccessorMemberDisplay
	{
		public string Name { get; private set; }
		public string Prompt { get; private set; }
		public string CustomDataType { get; private set; }
		public DataType DataType { get; private set; }
		public string DateFormat { get; private set; }
		public bool IsReadOnly { get; private set; }
		public bool IsVisible { get; private set; }

		public AccessorMemberDisplay(AccessorMember member, string profile)
		{
			member = MaybeUseMetadata(member, profile);

			ResolveName(member);
			ResolvePrompt(member);
			ResolveDataType(member);
			ResolveDateFormat(member);
			ResolveReadOnly(member);
			ResolveVisible(member);
		}

		private static AccessorMember MaybeUseMetadata(AccessorMember member, string profile)
		{
			if (member.DeclaringType == null || !member.HasAttribute<MetadataTypeAttribute>())
				return member;

			foreach (var attribute in member.DeclaringType.GetAttributes<MetadataTypeAttribute>())
			{
				if (attribute.Profile != profile)
					continue;

				var types = AccessorMemberTypes.None;
				types |= member.MemberType switch
				{
					AccessorMemberType.Field => AccessorMemberTypes.Fields,
					AccessorMemberType.Property => AccessorMemberTypes.Properties,
					AccessorMemberType.Method => AccessorMemberTypes.Methods,
					_ => throw new ArgumentOutOfRangeException()
				};

				var members = AccessorMembers.Create(attribute.MetadataType, types, member.Scope);
				foreach (var m in members)
				{
					if (m.Name != member.Name)
						continue;
					member = m;
					break;
				}
			}
			
			return member;
		}

		private void ResolveVisible(AccessorMember member)
		{
			if (member.TryGetAttribute(out BrowsableAttribute browsable))
			{
				IsVisible = browsable.Browsable;
			}
			else if(member.TryGetAttribute(out DesignTimeVisibleAttribute visible))
			{
				IsVisible = visible.Visible;
			}
			else if (!member.CanRead)
			{
				IsVisible = false;
			}
			else
			{
				IsVisible = true;
			}
		}

		private void ResolveReadOnly(AccessorMember member)
		{
			if (member.TryGetAttribute(out ReadOnlyAttribute readOnly))
			{
				IsReadOnly = readOnly.IsReadOnly;
			}
			else if (!member.CanWrite)
			{
				IsReadOnly = true;
			}
		}

		private void ResolveDateFormat(AccessorMember member)
		{
			if(DataType == DataType.Date || DataType == DataType.DateTime)
			{
				DateFormat = member.TryGetAttribute(out DisplayFormatAttribute displayFormat)
					? displayFormat.DataFormatString
					: "mm/dd/yyyy";
			}
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