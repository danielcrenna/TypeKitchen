// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using TypeKitchen.Serialization;

namespace TypeKitchen.Differencing
{
	public class DiffDocument : IReadObjectSink, IDeltaStream
	{
		public static readonly DiffDocument Empty = new DiffDocument();

		private AccessorMembers _leftMembers;
		private AccessorMembers _rightMembers;

		public void StartedReadingObject(Type type, AccessorMembers members)
		{
			if (_leftMembers == null)
				_leftMembers = members;
			else if (_rightMembers == null)
				_rightMembers = members;
			else
				throw new InvalidOperationException("Can only create diff for types");
		}

		private Dictionary<string, object> _leftMemberValues;
		private Dictionary<string, object> _rightMemberValues;

		public void ReadMember(Type parentType, string memberName, Type memberType, object memberValue)
		{
			if (_leftMembers.DeclaringType == parentType)
			{
				_leftMemberValues ??= new Dictionary<string, object>();

				if (!_leftMemberValues.ContainsKey(memberName))
				{
					_leftMemberValues[memberName] = memberValue;
					return;
				}
			}
			
			if (_rightMembers.DeclaringType == parentType)
			{
				_rightMemberValues ??= new Dictionary<string, object>();

				if (!_rightMemberValues.ContainsKey(memberName))
				{
					_rightMemberValues[memberName] = memberValue;
				}
			}
		}

		public IReadOnlyList<Operation> Operations
		{
			get
			{
				var list = new List<Operation>();

				if (_leftMemberValues == null)
					return list;

				foreach(var name in _leftMemberValues.Keys)
				{
					if (!_rightMemberValues.TryGetValue(name, out var replaceWith))
					{
						list.Add(RemoveObjectMember(name));
						continue;
					}

					_leftMemberValues.TryGetValue(name, out var replaceThis);
					if(!replaceWith.Equals(replaceThis))
						list.Add(ReplaceMemberValue(name, replaceWith));
				}

				foreach (var name in _rightMemberValues.Keys)
				{
					if (_leftMemberValues.ContainsKey(name))
						continue;
					list.Add(AddObjectMember(name, _rightMemberValues[name]));
				}

				return list;
			}
		}

		public void ApplyTo(object instance)
		{
			foreach (var operation in Operations)
			{
				switch (operation.Type)
				{
					case OperationTypes.Add:
					case OperationTypes.Remove:
						throw new NotSupportedException("Cannot mutate static types in C#");

					case OperationTypes.Replace:
					{
						foreach (var member in AccessorMembers.Create(instance, AccessorMemberTypes.Properties))
						{
							if (!operation.Path.Equals($"/{member.Name}", StringComparison.OrdinalIgnoreCase))
								continue;

							WriteAccessor.Create(instance, AccessorMemberTypes.Properties)
								.TrySetValue(instance, member.Name, operation.Value);
						}
						break;
					}
				}
			}
		}

		private static Operation AddObjectMember(string name, object value) => new Operation(OperationTypes.Add, $"/{name}", null, value);
		private static Operation ReplaceMemberValue(string name, object value) => new Operation(OperationTypes.Replace, $"/{name}", null, value);
		private static Operation RemoveObjectMember(string name) => new Operation(OperationTypes.Remove, $"/{name}", null, null);
	}
}