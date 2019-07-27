namespace JetBrains.ReSharper.Plugins.FSharp.Psi.LanguageService.Parsing

open FSharp.Compiler.Ast
open FSharp.Compiler.SourceCodeServices
open JetBrains.Annotations
open JetBrains.DocumentModel
open JetBrains.Lifetimes
open JetBrains.ProjectModel
open JetBrains.ReSharper.Psi
open JetBrains.ReSharper.Psi.Parsing
open JetBrains.ReSharper.Plugins.FSharp.Checker
open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree
open JetBrains.ReSharper.Plugins.FSharp.Psi.Parsing
open JetBrains.ReSharper.Plugins.FSharp.Psi.Resolve.SymbolsCache
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
open JetBrains.Util.Caches

type FSharpParser(lexer: ILexer, document: IDocument, path: FileSystemPath, sourceFile: IPsiSourceFile,
                  checkerService: FSharpCheckerService) =

    let solution = if isNull sourceFile then null else sourceFile.GetSolution()
    let symbolsCache = if isNull solution then null else solution.GetComponent<IFSharpResolvedSymbolsCache>()
    let parseCache = if isNull solution then null else solution.GetComponent<ParseTreeCache>()

    let tryCreateTreeBuilder lexer lifetime =
        Option.bind (fun (parseResults: FSharpParseFileResults) ->
            parseResults.ParseTree |> Option.map (function
            | ParsedInput.ImplFile(ParsedImplFileInput(_,_,_,_,_,decls,_)) ->
                FSharpImplTreeBuilder(lexer, document, decls, lifetime) :> FSharpTreeBuilderBase
            | ParsedInput.SigFile(ParsedSigFileInput(_,_,_,_,sigs)) ->
                FSharpSigTreeBuilder(lexer, document, sigs, lifetime) :> FSharpTreeBuilderBase))

    let createFakeBuilder lexer lifetime =
        { new FSharpTreeBuilderBase(lexer, document, lifetime) with
            override x.CreateFSharpFile() =
                x.FinishFile(x.Mark(), ElementType.F_SHARP_IMPL_FILE) }

    let parseFile () =
        use lifetimeDefinition = Lifetime.Define()
        let lifetime = lifetimeDefinition.Lifetime

        let defines = checkerService.GetDefines(sourceFile)
        let parsingOptions = checkerService.GetParsingOptions(sourceFile)

        let lexer = FSharpPreprocessedLexerFactory(defines).CreateLexer(lexer).ToCachingLexer()
        let parseResults = checkerService.ParseFile(path, document, parsingOptions)

        let language =
            match sourceFile with
            | null -> FSharpLanguage.Instance :> PsiLanguageType
            | sourceFile -> sourceFile.PrimaryPsiLanguage

        let treeBuilder =
            tryCreateTreeBuilder lexer lifetime parseResults
            |> Option.defaultWith (fun _ -> createFakeBuilder lexer lifetime)

        let parseResultsCachedValue =
            match sourceFile with
            | null -> CachedValues.CreateStrongParametrizedCachedValue(parseResults)
            | _ -> CachedValues.CreateWeakParametrizedCachedValue(parseCache.ParseFunc, parseCache.Cache, parseResults)

        treeBuilder.CreateFSharpFile(CheckerService = checkerService,
                                     ParseResultsCachedValue = parseResultsCachedValue,
                                     ResolvedSymbolsCache = symbolsCache,
                                     LanguageType = language)

    new (lexer, [<NotNull>] sourceFile: IPsiSourceFile, checkerService) =
        let document = if isNotNull sourceFile then sourceFile.Document else null
        let path = if isNotNull sourceFile then sourceFile.GetLocation() else null
        FSharpParser(lexer, document, path, sourceFile, checkerService)

    new (lexer, document, checkerService) =
        FSharpParser(lexer, document, FSharpParser.SandBoxPath, null, checkerService)

    static member val SandBoxPath = FileSystemPath.Parse("Sandbox.fs")

    interface IFSharpParser with
        member this.ParseFSharpFile() = parseFile ()
        member this.ParseFile() = parseFile () :> _

        member this.ParseExpression(chameleonExpr: IChameleonExpression, document) =
            let document = if isNotNull document then document else chameleonExpr.GetSourceFile().Document
            let projectedOffset = chameleonExpr.GetTreeStartOffset().Offset

            Lifetime.Using(fun lifetime ->
                // todo: cover error cases where fsImplFile or multiple expressions may be returned
                let treeBuilder = FSharpImplTreeBuilder(lexer, document, [], lifetime, projectedOffset)
                treeBuilder.ProcessTopLevelExpression(chameleonExpr.SynExpr)
                treeBuilder.GetTreeNode()) :?> ISynExpr
