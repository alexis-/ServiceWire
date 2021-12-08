namespace ServiceWire
{
  internal class NullStats : IStats
  {
    #region Methods Impl

    public void Log(string name, float value) { }

    public void Log(string category, string name, float value) { }

    #endregion




    #region Methods

    public void LogSys() { }

    #endregion
  }
}
