namespace ServiceWire.Common
{
  using System.Threading.Tasks;

  public interface IBiDirectionalContract
  {
    Task<bool> EstablishReverseConnection(EndPointBase reverseEndPoint);
    bool       TestConnection();
  }
}
