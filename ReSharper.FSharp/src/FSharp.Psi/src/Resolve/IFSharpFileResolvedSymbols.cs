using System.Collections.Generic;
using FSharp.Compiler.SourceCodeServices;
using JetBrains.Annotations;
using JetBrains.ReSharper.Plugins.FSharp.Checker;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Resolve
{
  public interface IFSharpFileResolvedSymbols
  {
    [CanBeNull]
    FSharpSymbolUse GetSymbolUse(int offset);

    [CanBeNull]
    FSharpSymbolUse GetSymbolDeclaration(int offset);

    [NotNull]
    IReadOnlyList<FSharpResolvedSymbolUse> GetAllDeclaredSymbols();

    [NotNull]
    IReadOnlyList<FSharpResolvedSymbolUse> GetAllResolvedSymbols();

    [CanBeNull]
    FSharpSymbol GetSymbol(int offset);
  }
}
