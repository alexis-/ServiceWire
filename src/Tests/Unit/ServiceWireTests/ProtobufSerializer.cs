namespace ServiceWireTests
{
  using System;
  using System.IO;
  using ProtoBuf;
  using ServiceWire;

  public class ProtoBufSerializer : ISerializer
  {
    #region Methods Impl

    public T Deserialize<T>(byte[] bytes)
    {
      if (bytes.Length == 0) return default(T);
      using (var ms = new MemoryStream(bytes))
        return Serializer.Deserialize<T>(ms);
    }

    public object Deserialize(byte[] bytes, string typeConfigName)
    {
      if (null == typeConfigName) throw new ArgumentNullException(nameof(typeConfigName));

      var type = typeConfigName.ToType();
      if (null == typeConfigName || null == bytes || bytes.Length == 0) return type.GetDefault();
      using (var ms = new MemoryStream(bytes))
        return Serializer.Deserialize(type, ms);
    }

    public byte[] Serialize<T>(T obj)
    {
      if (null == obj) return null;

      using (var ms = new MemoryStream())
      {
        try
        {
          Serializer.Serialize<T>(ms, obj);
          var bytes = ms.ToArray();
          return bytes;
        }
        catch (Exception e)
        {
          Console.Write(e);
        }

        return null;
      }
    }

    public byte[] Serialize(object obj, string typeConfigName)
    {
      if (null == typeConfigName) throw new ArgumentNullException(nameof(typeConfigName));

      if (null == obj) return null;

      using (var ms = new MemoryStream())
      {
        try
        {
          Serializer.Serialize(ms, obj);
          var bytes = ms.ToArray();
          return bytes;
        }
        catch (Exception e)
        {
          Console.Write(e);
        }

        return null;
      }
    }

    #endregion
  }
}
