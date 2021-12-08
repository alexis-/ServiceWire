﻿namespace ServiceWire
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.IO;

  public abstract class LoggerBase
  {
    #region Constants & Statics

    protected const string TimeStampPattern = "yyyy-MM-ddTHH:mm:ss.fff";

    private const string ToDateOnlyPattern = "yyyyMMdd";
    private const string ToDateHourPattern = "yyyyMMdd-HH";

    private const string FileNameFormatSizePattern = "00000000";

    #endregion




    #region Properties & Fields - Non-Public

    protected object                    _syncRoot          = new object();
    protected string                    _logDirectory      = null;
    protected string                    _logFilePrefix     = null;
    protected string                    _logFileExtension  = null;
    protected bool                      _useUtcTimeStamp   = false;
    protected ConcurrentQueue<string[]> _logQueue          = new ConcurrentQueue<string[]>();
    protected LogOptions                _options           = LogOptions.LogOnlyToFile;
    protected LogRollOptions            _rollOptions       = LogRollOptions.Daily;
    protected string                    _logFileNameFormat = null;
    protected int                       _messageBufferSize = 32;
    protected int                       _rollMaxMegaBytes  = 1024;

    private string _currentFileName    = null;
    private int    _currentFileOffset  = -1;
    private int    _currentWritesCount = 0;

    #endregion




    #region Properties & Fields - Public

    public LogOptions LogOptions
    {
      get => _options;
      set
      {
        _options = value;
        if (_options != LogOptions.LogOnlyToConsole)
        {
          if (!_logFileExtension.StartsWith(".")) _logFileExtension = "." + _logFileExtension;
          _logFileNameFormat = Path.Combine(_logDirectory, _logFilePrefix + "{0}" + _logFileExtension);
        }
      }
    }

    #endregion




    #region Methods

    public virtual void FlushLog()
    {
      WriteBuffer(int.MaxValue);
    }

    protected string GetTimeStamp()
    {
      return _useUtcTimeStamp
        ? DateTime.UtcNow.ToString(TimeStampPattern)
        : DateTime.Now.ToString(TimeStampPattern);
    }

    protected string GetTimeStamp(DateTime dt)
    {
      return _useUtcTimeStamp
        ? dt.ToUniversalTime().ToString(TimeStampPattern)
        : dt.ToString(TimeStampPattern);
    }

    protected void WriteBuffer(int count)
    {
      var list = new List<string>();
      for (var i = 0; i < count; i++)
      {
        string[] msg;
        _logQueue.TryDequeue(out msg);
        if (null == msg) break;

        list.AddRange(msg);
      }

      if (list.Count == 0) return; //nothing to log

      var lines = list.ToArray();
      if (_options == LogOptions.LogOnlyToConsole || _options == LogOptions.LogToBoth)
        Console.Write(lines);
      if (_options == LogOptions.LogOnlyToFile || _options == LogOptions.LogToBoth)
        WriteToFile(lines);
    }

    private void WriteToFile(string[] lines)
    {
      lock (_syncRoot)
        try
        {
          var fileName = GetFileName();
          File.AppendAllLines(fileName, lines);
        }
        catch
        {
          //todo ?
        }
    }

    private string GetFileName()
    {
      //by size
      if (_rollOptions == LogRollOptions.Size)
      {
        if (_currentFileOffset < 0) _currentFileOffset = GetCurrentFileOffset();
        if (null == _currentFileName)
        {
          _currentFileName = GetSizeFileName(_currentFileOffset);
          return _currentFileName;
        }

        //should we check size?
        if (_currentWritesCount * _messageBufferSize >= 3200) //100 writes at 32 per
        {
          _currentWritesCount = 0; //reset
          var fi = new FileInfo(_currentFileName);
          if (fi.Length > _rollMaxMegaBytes * 1024 * 1024)
          {
            _currentFileOffset++;
            _currentFileName = GetSizeFileName(_currentFileOffset);
          }
        }

        return _currentFileName;
      }

      //based on roll options
      if (_rollOptions == LogRollOptions.Hourly)
      {
        if (_useUtcTimeStamp) return string.Format(_logFileNameFormat, DateTime.UtcNow.ToString(ToDateHourPattern));

        return string.Format(_logFileNameFormat, DateTime.Now.ToString(ToDateHourPattern));
      }

      if (_useUtcTimeStamp) return string.Format(_logFileNameFormat, DateTime.UtcNow.ToString(ToDateOnlyPattern));

      return string.Format(_logFileNameFormat, DateTime.Now.ToString(ToDateOnlyPattern));
    }

    private int GetCurrentFileOffset()
    {
      return Directory.GetFiles(_logDirectory, _logFilePrefix + "*").Length;
    }

    private string GetSizeFileName(int offset)
    {
      return string.Format(_logFileNameFormat, offset.ToString(FileNameFormatSizePattern));
    }

    #endregion
  }
}
