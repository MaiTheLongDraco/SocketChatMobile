using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class AppMath 
{
	public static void ConvertStructeToByteArr<T>(T structure, ref int size, ref byte[] output)
	{
		size = Marshal.SizeOf(structure);
		if (size <= 0) return;
		IntPtr ptr = Marshal.AllocHGlobal(size);
		Marshal.StructureToPtr(structure, ptr, false);
		Marshal.Copy(ptr, output, 0, size);
		Marshal.FreeHGlobal(ptr);
	}
	public static void ConvertByteArrToStructure<T>(byte[] input, int size, ref T output)
	{
		try
		{
			size = Marshal.SizeOf(typeof(T));
			if (size <= 0) return;
			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.Copy(input, 0, ptr, size);
			output = (T)Marshal.PtrToStructure(ptr, output.GetType());
			Marshal.FreeHGlobal(ptr);
		}
		catch
		(Exception e)
		{
			Console.WriteLine($" cannot convert to {typeof(T).Name} due to {e.Message}");
		}

	}
}
