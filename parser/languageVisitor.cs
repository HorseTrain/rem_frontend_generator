//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from language.g4 by ANTLR 4.13.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="languageParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.2")]
[System.CLSCompliant(false)]
public interface IlanguageVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.sourceFile"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSourceFile([NotNull] languageParser.SourceFileContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.topLevelDeclarations"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTopLevelDeclarations([NotNull] languageParser.TopLevelDeclarationsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.line"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLine([NotNull] languageParser.LineContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.semiColin"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSemiColin([NotNull] languageParser.SemiColinContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.functionCallLine"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionCallLine([NotNull] languageParser.FunctionCallLineContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.caseStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCaseStatement([NotNull] languageParser.CaseStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.loopStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLoopStatement([NotNull] languageParser.LoopStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.switchStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSwitchStatement([NotNull] languageParser.SwitchStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.returnStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReturnStatement([NotNull] languageParser.ReturnStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.lValueSet"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLValueSet([NotNull] languageParser.LValueSetContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.ifStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfStatement([NotNull] languageParser.IfStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.elseStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitElseStatement([NotNull] languageParser.ElseStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.scope"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitScope([NotNull] languageParser.ScopeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.identifierExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIdentifierExpression([NotNull] languageParser.IdentifierExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.baseExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBaseExpression([NotNull] languageParser.BaseExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.unaryExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnaryExpression([NotNull] languageParser.UnaryExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.parenthesis"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParenthesis([NotNull] languageParser.ParenthesisContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.genericImpl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGenericImpl([NotNull] languageParser.GenericImplContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.functionCall"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionCall([NotNull] languageParser.FunctionCallContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpression([NotNull] languageParser.ExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.fixedLengthInstructionOperand"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFixedLengthInstructionOperand([NotNull] languageParser.FixedLengthInstructionOperandContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.fixedLengthInstructionOperands"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFixedLengthInstructionOperands([NotNull] languageParser.FixedLengthInstructionOperandsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.fixedLengthInstruction"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFixedLengthInstruction([NotNull] languageParser.FixedLengthInstructionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.instructionDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInstructionDeclaration([NotNull] languageParser.InstructionDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.functionParameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionParameter([NotNull] languageParser.FunctionParameterContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.functionParameters"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionParameters([NotNull] languageParser.FunctionParametersContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.functionBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionBody([NotNull] languageParser.FunctionBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.variableType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVariableType([NotNull] languageParser.VariableTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.functionDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionDeclaration([NotNull] languageParser.FunctionDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.identifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIdentifier([NotNull] languageParser.IdentifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.number"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNumber([NotNull] languageParser.NumberContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.runtimeOperandType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRuntimeOperandType([NotNull] languageParser.RuntimeOperandTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.compileTimeIntegerType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCompileTimeIntegerType([NotNull] languageParser.CompileTimeIntegerTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.runtimeTypeCase"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRuntimeTypeCase([NotNull] languageParser.RuntimeTypeCaseContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.runtimeTypeSwitch"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRuntimeTypeSwitch([NotNull] languageParser.RuntimeTypeSwitchContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.runtimeTypeReference"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRuntimeTypeReference([NotNull] languageParser.RuntimeTypeReferenceContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.operandTypeDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperandTypeDeclaration([NotNull] languageParser.OperandTypeDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.undefinedVariableDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUndefinedVariableDeclaration([NotNull] languageParser.UndefinedVariableDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.definedVariableDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDefinedVariableDeclaration([NotNull] languageParser.DefinedVariableDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="languageParser.variableDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVariableDeclaration([NotNull] languageParser.VariableDeclarationContext context);
}