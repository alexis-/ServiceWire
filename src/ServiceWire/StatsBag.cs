﻿namespace ServiceWire
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;

  internal class StatsBag
  {
    #region Constants & Statics

    protected const string TimeStampPattern = "yyyy-MM-ddTHH:mm:ss.fff";

    #endregion




    #region Properties & Fields - Non-Public

    private readonly bool _useUtcTime = false;

    private readonly DateTime _started = DateTime.Now;
    private          int      _count   = 0;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, StatInfo>> _bag
      = new ConcurrentDictionary<string, ConcurrentDictionary<string, StatInfo>>();

    #endregion




    #region Constructors

    public StatsBag(bool useUtcTime)
    {
      _useUtcTime = useUtcTime;
      _started    = _useUtcTime ? DateTime.UtcNow : DateTime.Now;
    }

    #endregion




    #region Properties & Fields - Public

    public int Count => _count;

    #endregion




    #region Methods

    public void Clear()
    {
      _bag.Clear();
    }

    protected string GetTimeStamp()
    {
      return _useUtcTime
        ? DateTime.UtcNow.ToString(TimeStampPattern)
        : DateTime.Now.ToString(TimeStampPattern);
    }

    protected string GetTimeStamp(DateTime dt)
    {
      return _useUtcTime
        ? dt.ToUniversalTime().ToString(TimeStampPattern)
        : dt.ToString(TimeStampPattern);
    }

    public void Add(string category, string name, float value)
    {
      var val       = value;
      var container = _bag.GetOrAdd(category, new ConcurrentDictionary<string, StatInfo>());
      container.AddOrUpdate(name, s => new StatInfo(val), (s, t) =>
      {
        t.Increment(val);
        return t;
      });
      Interlocked.Increment(ref _count);
    }

    public string[] GetDump()
    {
      var dumped = _useUtcTime ? DateTime.UtcNow : DateTime.Now;
      var ts     = dumped - _started;
      var tsTxt  = ts.TotalSeconds.ToString("######.000000");
      var lines  = new List<string>();
      //add header
      lines.Add(string.Format("<entry fr=\"{0}\" to=\"{1}\" cnt=\"{2}\" secs=\"{3}\">", GetTimeStamp(_started), GetTimeStamp(dumped),
                              _count, tsTxt));

      foreach (var cat in _bag)
      {
        var catCount = (from n in cat.Value select n.Value.Count).Sum();
        lines.Add(string.Format("  <cat nm=\"{0}\" cnt=\"{1}\">", cat.Key, catCount));

        foreach (var stat in cat.Value)
        {
          if (null == stat.Value) continue;

          var total = stat.Value.Total;
          var avg = total == 0.0f
            ? 0.0f
            : total / stat.Value.Count;
          lines.Add(string.Format("    <stat nm=\"{0}\" cnt=\"{1}\" tot=\"{2}\" avg=\"{3}\" />",
                                  stat.Key, stat.Value.Count, total, avg));
        }

        lines.Add("  </cat>");
      }

      lines.Add("</entry>");
      return lines.ToArray();
    }

    #endregion
  }

  internal class StatInfo
  {
    #region Properties & Fields - Non-Public

    private readonly ConcurrentBag<float> _bag = new ConcurrentBag<float>();

    #endregion




    #region Constructors

    public StatInfo(float value)
    {
      _bag.Add(value);
    }

    #endregion




    #region Properties & Fields - Public

    public int Count => _bag.Count;

    public float Total => _bag.Sum();

    #endregion




    #region Methods

    public void Increment(float value)
    {
      _bag.Add(value);
    }

    #endregion
  }
}
