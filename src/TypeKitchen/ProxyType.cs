// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
namespace TypeKitchen
{
	public enum ProxyType
	{
		/// <summary>
		/// Proxy inherits from the prototype, and overrides all public virtual members. Fields are ignored.
		/// This type of proxy is suitable for interception or direct casting, but not duck casting.
		/// </summary>
		Pure,

		/// <summary>
		/// Proxy inherits from the prototype, and overrides all public virtual members.
		/// Fields are proxied through a property that accesses the field indirectly.
		/// This type of proxy is suitable for interception or duck casting, but not direct casting.
		/// </summary>
		Hybrid,

		/// <summary>
		/// Proxy does not inherit from the prototype, but retains the structure of all public members, including fields.
		/// This type of proxy is only suitable for duck casting.
		/// </summary>
		Mimic
	}
}