﻿namespace ServiceWire
{
  internal class NullLogger : ILog
  {
    #region Methods Impl

    public void Debug(string formattedMessage, params object[] args) { }

    public void Info(string formattedMessage, params object[] args) { }

    public void Warn(string formattedMessage, params object[] args) { }

    public void Error(string formattedMessage, params object[] args) { }

    public void Fatal(string formattedMessage, params object[] args) { }

    #endregion
  }
}
