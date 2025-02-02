﻿namespace ServiceWire.ZeroKnowledge
{
  using System;
  using System.Linq;
  using System.Security.Cryptography;

  /// <summary>Zero knowledge protocol loosely based on secure remote password protocol v6.</summary>
  public class ZkProtocol
  {
    #region Properties & Fields - Non-Public

    private readonly SHA256CryptoServiceProvider _sha;
    private readonly Random                      _random;
    private readonly BigInteger                  _n;

    #endregion




    #region Constructors

    public ZkProtocol()
    {
      _sha    = new SHA256CryptoServiceProvider();
      _random = new Random(DateTime.Now.Millisecond);
      _n      = new BigInteger(ZkSafePrimes.N4);
    }

    #endregion




    #region Methods

    /// <summary>Server must generate password hash and store only username and hash values.</summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public ZkPasswordHash HashCredentials(string username, string password)
    {
      var salt = ComputeHash(CryptRand());
      var key  = ComputeHash(salt, username.ConvertToBytes(), password.ConvertToBytes());
      var ver  = CryptRand();
      return new ZkPasswordHash
      {
        Salt     = salt,
        Key      = key,
        Verifier = ver
      };
    }


    /// <summary>Step 1. Client sends username and ephemeral hash of random number.</summary>
    /// <param name="aRand">Crypto randum generated by CryptRand() method.</param>
    /// <returns></returns>
    public byte[] GetClientEphemeralA(byte[] aRand)
    {
      var aval = ComputeHash(aRand);
      return aval;
    }

    /// <summary>
    ///   Step 2. Server looks up username, gets pwd hash, and sends client salt and ephemeral
    ///   hash of params.
    /// </summary>
    /// <param name="salt"></param>
    /// <param name="verifier"></param>
    /// <param name="bRand">Crypto randum generated by CryptRand() method.</param>
    /// <returns></returns>
    public byte[] GetServerEphemeralB(byte[] salt, byte[] verifier, byte[] bRand)
    {
      var bval = ComputeHash(salt, verifier, bRand);
      return bval;
    }

    /// <summary>
    ///   Step 3. Client and server calculate random scramble of ephemeral hash values
    ///   exchanged.
    /// </summary>
    /// <param name="ephemeralA"></param>
    /// <param name="ephemeralB"></param>
    /// <returns></returns>
    public byte[] CalculateRandomScramble(byte[] ephemeralA, byte[] ephemeralB)
    {
      return ComputeHash(ephemeralA, ephemeralB);
    }

    /// <summary>Step 4. Client computes session key</summary>
    /// <param name="salt"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="aEphemeral"></param>
    /// <param name="bEphemeral"></param>
    /// <param name="uScramble"></param>
    /// <returns></returns>
    public byte[] ClientComputeSessionKey(byte[] salt,
                                          string username,
                                          string password,
                                          byte[] aEphemeral,
                                          byte[] bEphemeral,
                                          byte[] uScramble)
    {
      var key = ComputeHash(salt, username.ConvertToBytes(), password.ConvertToBytes());
      var kc  = ComputeHash(salt, key, aEphemeral, bEphemeral, uScramble);
      return kc;
    }

    /// <summary>Step 5. Server computes session key</summary>
    /// <param name="salt"></param>
    /// <param name="key"></param>
    /// <param name="aEphemeral"></param>
    /// <param name="bEphemeral"></param>
    /// <param name="uScramble"></param>
    /// <returns></returns>
    public byte[] ServerComputeSessionKey(byte[] salt, byte[] key, byte[] aEphemeral, byte[] bEphemeral, byte[] uScramble)
    {
      var ks = ComputeHash(salt, key, aEphemeral, bEphemeral, uScramble);
      return ks;
    }

    /// <summary>
    ///   Step 6. Client creates hash of session key and sends to server. Server creates same
    ///   key and verifies.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="salt"></param>
    /// <param name="aEphemeral"></param>
    /// <param name="bEphermeral"></param>
    /// <param name="sessionKey"></param>
    /// <returns></returns>
    public byte[] ClientCreateSessionHash(string username, byte[] salt, byte[] aEphemeral, byte[] bEphermeral, byte[] sessionKey)
    {
      var mc = ComputeHash(username.ConvertToBytes(), salt, aEphemeral, bEphermeral, sessionKey);
      return mc;
    }

    /// <summary>
    ///   Step 7. Server creates hash of session key and sends to client. Client creates same
    ///   key and verifies.
    /// </summary>
    /// <param name="aEphemeral"></param>
    /// <param name="clientHash"></param>
    /// <param name="sessionKey"></param>
    /// <returns></returns>
    public byte[] ServerCreateSessionHash(byte[] aEphemeral, byte[] clientHash, byte[] sessionKey)
    {
      var ms = ComputeHash(aEphemeral, clientHash, sessionKey);
      return ms;
    }

    /// <summary>Generate crypto safe, pseudo random number.</summary>
    /// <param name="bits">max value supported is 4096</param>
    /// <returns></returns>
    public byte[] CryptRand(int bits = 4096)
    {
      var rb = new byte[256];
      _random.NextBytes(rb);
      var bigrand = new BigInteger(rb);
      var crand   = (bigrand % ZkSafePrimes.GetSafePrime(_random.Next(0, 2047))) ^ _n;
      var bytes   = BigInteger.ToByteArray(crand);
      if (bits >= 4096) return bytes;

      var count = bits / 8;
      var skip  = _random.Next(0, bytes.Length - count);
      return bytes.Skip(skip).Take(count).ToArray();
    }

    public byte[] ComputeHash(params byte[][] items)
    {
      var buf = Combine(items);
      return _sha.ComputeHash(buf);
    }

    public byte[] Combine(params byte[][] arrays)
    {
      var ret    = new byte[arrays.Sum(x => x.Length)];
      var offset = 0;
      foreach (var data in arrays)
      {
        Buffer.BlockCopy(data, 0, ret, offset, data.Length);
        offset += data.Length;
      }

      return ret;
    }

    #endregion
  }
}
