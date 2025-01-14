namespace ServiceWire.ZeroKnowledge
{
  using System.Text;

  public static class ZkExt
  {
    #region Methods

    public static byte[] ConvertToBytes(this string val)
    {
      return Encoding.Unicode.GetBytes(val);
    }

    public static string ConverToString(this byte[] bytes)
    {
      return Encoding.Unicode.GetString(bytes);
    }

    public static bool IsEqualTo(this byte[] a1, byte[] a2)
    {
      if (a1.Length != a2.Length) return false;

      for (var i = 0; i < a1.Length; i++)
        if (a1[i] != a2[i])
          return false;

      return true;
    }

    #endregion
  }
}
