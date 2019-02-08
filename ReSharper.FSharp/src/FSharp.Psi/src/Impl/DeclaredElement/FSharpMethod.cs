﻿using JetBrains.Annotations;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using Microsoft.FSharp.Compiler.SourceCodeServices;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.DeclaredElement
{
  internal class FSharpMethod<TDeclaration> : FSharpMethodBase<TDeclaration>
    where TDeclaration : FSharpDeclarationBase, IFSharpDeclaration, IAccessRightsOwnerDeclaration,
    IModifiersOwnerDeclaration
  {
    public FSharpMethod([NotNull] ITypeMemberDeclaration declaration, [NotNull] FSharpMemberOrFunctionOrValue mfv,
      [CanBeNull] IFSharpTypeDeclaration typeDeclaration) : base(declaration, mfv, typeDeclaration)
    {
    }
  }

  internal class FSharpTypePrivateMethod : FSharpMethodBase<TopPatternDeclarationBase>
  {
    public FSharpTypePrivateMethod([NotNull] ITypeMemberDeclaration declaration,
      [NotNull] FSharpMemberOrFunctionOrValue mfv, [CanBeNull] IFSharpTypeDeclaration typeDeclaration)
      : base(declaration, mfv, typeDeclaration)
    {
    }

    public override AccessRights GetAccessRights() => AccessRights.INTERNAL;
    public override bool IsStatic => false;
  }
}