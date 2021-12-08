namespace ServiceWireTests
{
  using System;
  using System.Text;
  using Newtonsoft.Json;
  using ServiceWire;

  public class NewtonsoftSerializer : ISerializer
  {
    #region Properties & Fields - Non-Public

    private readonly JsonSerializerSettings settings = new JsonSerializerSettings
    {
      ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    #endregion




    #region Methods Impl

    public T Deserialize<T>(byte[] bytes)
    {
      if (null == bytes || bytes.Length == 0) return default(T);

      var json = Encoding.UTF8.GetString(bytes);
      return JsonConvert.DeserializeObject<T>(json, settings);
    }

    public object Deserialize(byte[] bytes, string typeConfigName)
    {
      if (null == typeConfigName) throw new ArgumentNullException(nameof(typeConfigName));

      var type = typeConfigName.ToType();
      if (null == typeConfigName || null == bytes || bytes.Length == 0) return type.GetDefault();

      var json = Encoding.UTF8.GetString(bytes);
      return JsonConvert.DeserializeObject(json, type, settings);
    }

    public byte[] Serialize<T>(T obj)
    {
      if (null == obj) return null;

      var json = JsonConvert.SerializeObject(obj, settings);
      return Encoding.UTF8.GetBytes(json);
    }

    public byte[] Serialize(object obj, string typeConfigName)
    {
      if (null == obj) return null;

      var type = typeConfigName.ToType();
      var json = JsonConvert.SerializeObject(obj, type, settings);
      return Encoding.UTF8.GetBytes(json);
    }

    #endregion
  }
}
