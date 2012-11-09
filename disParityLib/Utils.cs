﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace disParity
{

  public class Utils
  {

    public static string MakeFullPath(string path, string name)
    {
      if (path == "")
        return name;
      if (path[path.Length - 1] == '\\')
        return path + name;
      else
        return path + "\\" + name;
    }

    public static string StripRoot(string root, string path)
    {
      if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        return path;
      path = path.Remove(0, root.Length);
      while (path.Length > 0 && path[0] == Path.DirectorySeparatorChar)
        path = path.Remove(0, 1);
      return path;
    }

    public static bool HashCodesMatch(byte[] h1, byte[] h2)
    {
      if (h1.Length != h2.Length)
        return false;
      for (int i = 0; i < h1.Length; i++)
        if (h1[i] != h2[i])
          return false;
      return true;
    }

    public static unsafe void FastXOR(byte[] buf1, byte[] buf2)
    {
      fixed (byte* p1 = buf1)
      fixed (byte* p2 = buf2) {
        long* lp1 = (long*)p1;
        long* lp2 = (long*)p2;
        for (int i = 0; i < (buf1.Length / 8); i++) {
          *lp1 ^= *lp2;
          lp1++;
          lp2++;
        }
      }
    }

    public static string SmartSize(long size)
    {
      const long KB = 1024;
      const long MB = KB * 1024;
      const long GB = MB * 1024;
      const long TB = GB * 1024;

      string units;
      double result;

      if (size < KB) {
        if (size == 1)
          return "1 byte";
        else
          return size.ToString() + " bytes";
      }
      else if (size < MB) {
        result = (double)size / KB;
        units = "KB";
      }
      else if (size < GB) {
        result = (double)size / MB;
        units = "MB";
      }
      else if (size < TB) {
        result = (double)size / GB;
        units = "GB";
      }
      else {
        result = (double)size / TB;
        units = "TB";
      }
      return String.Format("{0:F1} {1}", result, units);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
      public uint dwLength;
      public uint dwMemoryLoad;
      public ulong ullTotalPhys;
      public ulong ullAvailPhys;
      public ulong ullTotalPageFile;
      public ulong ullAvailPageFile;
      public ulong ullTotalVirtual;
      public ulong ullAvailVirtual;
      public ulong ullAvailExtendedVirtual;

      public MEMORYSTATUSEX()
      {
        this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
      }
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    private const ulong ONE_GB = 1073741824;

    public static ulong TotalSystemRAM()
    {
      MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
      if (GlobalMemoryStatusEx(memStatus)) {
        // round up to nearest GB
        ulong RAM = memStatus.ullTotalPhys;
        ulong remainder = RAM % ONE_GB;
        if (remainder == 0)
          return RAM;
        else
          return (RAM + ONE_GB - remainder);
      } 
      else
        return 0;
    }

  }


}
