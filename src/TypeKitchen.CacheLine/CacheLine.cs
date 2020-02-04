/*
MIT License

Copyright (c) 2017 Nick Strupat

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace TypeKitchen.CacheLine
{
	[SuppressMessage("ReSharper", "IdentifierTypo")]
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "StringLiteralTypo")]
	public static class CacheLine
	{
		public static readonly int Size;

		static CacheLine()
		{
			Size = GetSize();
		}

		private static int GetSize()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return GetSize_Windows();
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				return GetSize_Linux();
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return GetSize_macOS();
			throw new NotSupportedException();
		}

		#region Linux

		public static int GetSize_Linux() => (int) sysconf(_SC_LEVEL1_DCACHE_LINESIZE);

		[DllImport("libc")]
		private static extern long sysconf(int name);

		private const int _SC_LEVEL1_DCACHE_LINESIZE = 190;

		#endregion

		#region OSX

		public static int GetSize_macOS()
		{
			var sizeOfLineSize = (IntPtr) IntPtr.Size;
			sysctlbyname("hw.cachelinesize", out var lineSize, ref sizeOfLineSize, IntPtr.Zero, IntPtr.Zero);
			return lineSize.ToInt32();
		}

		[DllImport("libc")]
		private static extern int sysctlbyname(string name, out IntPtr oldp, ref IntPtr oldlenp, IntPtr newp, IntPtr newlen);
		
		#endregion

		#region Windows

		public static int GetSize_Windows()
		{
			var info = ManagedGetLogicalProcessorInformation();
			if (info == null)
				throw new Exception("Could not retrieve the cache line size.");
			return info.First(x => x.Relationship == LOGICAL_PROCESSOR_RELATIONSHIP.RelationCache).ProcessorInformation.Cache.LineSize;
		}

		// http://stackoverflow.com/a/6972620/232574

		[StructLayout(LayoutKind.Sequential)]
		struct PROCESSORCORE
		{
			public byte Flags;
		};

		[StructLayout(LayoutKind.Sequential)]
		struct NUMANODE
		{
			public uint NodeNumber;
		}

		enum PROCESSOR_CACHE_TYPE
		{
			CacheUnified,
			CacheInstruction,
			CacheData,
			CacheTrace
		}

		[StructLayout(LayoutKind.Sequential)]
		struct CACHE_DESCRIPTOR
		{
			public byte Level;
			public byte Associativity;
			public ushort LineSize;
			public uint Size;
			public PROCESSOR_CACHE_TYPE Type;
		}

		[StructLayout(LayoutKind.Explicit)]
		struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION_UNION
		{
			[FieldOffset(0)]
			public PROCESSORCORE ProcessorCore;
			[FieldOffset(0)]
			public NUMANODE NumaNode;
			[FieldOffset(0)]
			public CACHE_DESCRIPTOR Cache;
			[FieldOffset(0)]
			private UInt64 Reserved1;
			[FieldOffset(8)]
			private UInt64 Reserved2;
		}

		enum LOGICAL_PROCESSOR_RELATIONSHIP
		{
			RelationProcessorCore,
			RelationNumaNode,
			RelationCache,
			RelationProcessorPackage,
			RelationGroup,
			RelationAll = 0xffff
		}

		private struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION
		{
#pragma warning disable 0649
			public UIntPtr ProcessorMask;
			public LOGICAL_PROCESSOR_RELATIONSHIP Relationship;
			public SYSTEM_LOGICAL_PROCESSOR_INFORMATION_UNION ProcessorInformation;
#pragma warning restore 0649
		}

		[DllImport(@"kernel32.dll", SetLastError = true)]
		private static extern bool GetLogicalProcessorInformation(IntPtr Buffer, ref uint ReturnLength);

		private const int ERROR_INSUFFICIENT_BUFFER = 122;

		private static SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] ManagedGetLogicalProcessorInformation()
		{
			uint ReturnLength = 0;
			GetLogicalProcessorInformation(IntPtr.Zero, ref ReturnLength);
			if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
				return null;
			IntPtr Ptr = Marshal.AllocHGlobal((int)ReturnLength);
			try
			{
				if (GetLogicalProcessorInformation(Ptr, ref ReturnLength))
				{
					int size = Marshal.SizeOf<SYSTEM_LOGICAL_PROCESSOR_INFORMATION>();
					int len = (int)ReturnLength / size;
					SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] Buffer = new SYSTEM_LOGICAL_PROCESSOR_INFORMATION[len];
					IntPtr Item = Ptr;
					for (int i = 0; i < len; i++)
					{
						Buffer[i] = Marshal.PtrToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION>(Item);
						Item += size;
					}
					return Buffer;
				}
			}
			finally
			{
				Marshal.FreeHGlobal(Ptr);
			}
			return null;
		}

		#endregion
	}
}