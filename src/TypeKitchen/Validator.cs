// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TypeKitchen
{
	public static class Validator
	{
		public static bool ValidateObject(object instance, out List<ValidationResult> validationResults)
		{
			MaybeUseMetadata(ref instance);
			var validationContext = new ValidationContext(instance, null, null);
			validationResults = new List<ValidationResult>();
			return System.ComponentModel.DataAnnotations.Validator.TryValidateObject(instance, validationContext, validationResults, true);
		}

		public static bool ValidateMember(object instance, string fieldName, out List<ValidationResult> validationResults)
		{
			MaybeUseMetadata(ref instance);

			validationResults = new List<ValidationResult>();

			var accessor = ReadAccessor.Create(instance, AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var members);
			if (!members.TryGetValue(fieldName, out var member) || !accessor.TryGetValue(instance, member.Name, out var value))
				return false;

			var validationContext = new ValidationContext(instance) {MemberName = member.Name};
			return System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(value, validationContext, validationResults);
		}

		private static void MaybeUseMetadata(ref object instance)
		{
			var type = instance.GetType();
			if (!type.TryGetAttribute(false, out MetadataTypeAttribute metadata))
				return;

			var metadataType = metadata.MetadataType;

			var reads = ReadAccessor.Create(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public,
				out var readMembers);
			var writes = WriteAccessor.Create(metadataType, AccessorMemberTypes.Properties, AccessorMemberScope.Public,
				out var writeMembers);

			var surrogate = Instancing.CreateInstance(metadataType);
			foreach (var member in readMembers)
			{
				if (!writeMembers.ContainsKey(member.Name))
					continue;

				if (reads.TryGetValue(instance, member.Name, out var value))
					writes.TrySetValue(surrogate, member.Name, value);
			}

			instance = surrogate;
		}
	}
}