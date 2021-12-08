﻿namespace ServiceWire
{
  using System;
  using System.IO;
  using System.IO.Compression;
  using System.Text.RegularExpressions;

  public static class NetExtensions
  {
    #region Methods

    public static string ToConfigName(this Type t)
    {
      var assName = t.Assembly.GetName().Name;

      // Do not qualify types from mscorlib/System.Private.CoreLib otherwise calling between process running with different frameworks won't work
      // i.e. "System.String, mscorlib" (.NET FW) != "System.String, System.Private.CoreLib" (.NET CORE)
      if (assName == "mscorlib" || assName == "System.Private.CoreLib")
        return t.FullName;

      var name = t.AssemblyQualifiedName;

      name = Regex.Replace(name, @", Version=\d+.\d+.\d+.\d+", string.Empty);
      name = Regex.Replace(name, @", Culture=\w+", string.Empty);
      name = Regex.Replace(name, @", PublicKeyToken=\w+", string.Empty);

      return name;
    }

    public static Type ToType(this string configName)
    {
      try
      {
        var result = Type.GetType(configName);
        return result;
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }

      return null;
    }

    /// <summary>Returns true if Type inherits from baseType.</summary>
    /// <param name="t">The Type extended by this method.</param>
    /// <param name="baseType">The base type to find in the inheritance hierarchy.</param>
    /// <returns>True if baseType is found. False if not.</returns>
    public static bool InheritsFrom(this Type t, Type baseType)
    {
      var cur = t.BaseType;
      while (cur != null)
      {
        if (cur.Equals(baseType)) return true;

        cur = cur.BaseType;
      }

      return false;
    }

    public static object GetDefault(this Type t)
    {
      var tm = new DefaultTypeMaker();
      return tm.GetDefault(t);
    }

    public static byte[] ToGZipBytes(this byte[] data)
    {
      using (var msCompressed = new MemoryStream())
      {
        using (var msObj = new MemoryStream(data))
        using (var gzs = new GZipStream(msCompressed, CompressionMode.Compress))
          msObj.CopyTo(gzs);
        return msCompressed.ToArray();
      }
    }

    public static byte[] FromGZipBytes(this byte[] compressedBytes)
    {
      using (var msObj = new MemoryStream())
      {
        using (var msCompressed = new MemoryStream(compressedBytes))
        using (var gzs = new GZipStream(msCompressed, CompressionMode.Decompress))
          gzs.CopyTo(msObj);
        msObj.Seek(0, SeekOrigin.Begin);
        return msObj.ToArray();
      }
    }

    public static string Flatten(this string src)
    {
      return src.Replace("\r", ":").Replace("\n", ":");
    }

    #endregion
  }
}
