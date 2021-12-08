namespace ServiceWire
{
  using System;
  using System.Reflection.Emit;

  internal sealed class ProxyBuilder
  {
    #region Properties & Fields - Public

    public string          ProxyName       { get; set; }
    public Type            InterfaceType   { get; set; }
    public Type            CtorType        { get; set; }
    public AssemblyBuilder AssemblyBuilder { get; set; }
    public ModuleBuilder   ModuleBuilder   { get; set; }
    public TypeBuilder     TypeBuilder     { get; set; }

    #endregion
  }
}
