namespace ServiceWire.ZeroKnowledge
{
  public interface IZkRepository
  {
    ZkPasswordHash GetPasswordHashSet(string username);
  }

  public class ZkNullRepository : IZkRepository
  {
    #region Methods Impl

    public ZkPasswordHash GetPasswordHashSet(string username)
    {
      return null;
    }

    #endregion
  }
}
