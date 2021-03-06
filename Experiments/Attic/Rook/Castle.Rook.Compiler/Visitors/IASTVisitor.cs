// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Rook.Compiler.Visitors
{
	using System;

	using Castle.Rook.Compiler.AST;

	public interface IASTVisitor
	{
		void VisitCompilationUnit(CompilationUnit compilationUnit);
		bool VisitSourceUnit(SourceUnit unit);

		// Members

		bool VisitNamespace(NamespaceDescriptor ns);
		bool VisitEnter(NamespaceDescriptor ns);
		bool VisitLeave(NamespaceDescriptor ns);
		bool VisitTypeDefinitionStatement(TypeDefinitionStatement typeDef);
		bool VisitEnter(TypeDefinitionStatement typeDef);
		bool VisitLeave(TypeDefinitionStatement typeDef);
		bool VisitMethodDefinitionStatement(MethodDefinitionStatement methodDef);
		bool VisitEnter(MethodDefinitionStatement methodDef);
		bool VisitLeave(MethodDefinitionStatement methodDef);

		// References

//		bool VisitTypeReference(TypeReference reference);
//		bool VisitIdentifier(Identifier identifier);
//		bool VisitParameterIdentifier(ParameterIdentifier parameterIdentifier);
		void VisitOpaqueIdentifier(OpaqueIdentifier opaqueIdentifier);
		void VisitParameterVarIdentifier(ParameterVarIdentifier varIdentifier);
		void VisitTypeReference(TypeReference reference);

		// Statements

		void VisitExpressionStatement(ExpressionStatement statement);
		void VisitConstructorDefinitionStatement(ConstructorDefinitionStatement statement);

//		bool VisitMultipleVariableDeclarationStatement(MultipleVariableDeclarationStatement varDecl);
//		bool VisitRepeatStatement(RepeatStatement statement);
//		bool VisitPostfixCondition(PostfixCondition postfixCondition);
//		bool VisitSingleVariableDeclarationStatement(SingleVariableDeclarationStatement declarationStatement);
//		bool VisitReturnStatement(ReturnStatement statement);
//		bool VisitRequireStatement(RequireStatement statement);

		// Expressions

		void VisitMethodInvocationExpression(MethodInvocationExpression invocationExpression);
		void VisitVariableReferenceExpression(VariableReferenceExpression expression);
		void VisitConstExpression(ConstExpression expression);
		void VisitBlockExpression(BlockExpression expression);
		void VisitMemberAccessExpression(MemberAccessExpression accessExpression);

//		bool VisitAssignmentExpression(AssignmentExpression assignExp);
//		bool VisitAugAssignmentExpression(AugAssignmentExpression auAssignExp);
//		bool VisitYieldExpression(YieldExpression yieldExpression);
//		bool VisitVariableReferenceExpression(VariableReferenceExpression variableReferenceExpression);
//		bool VisitUnaryExpression(UnaryExpression unaryExpression);
//		bool VisitRetryExpression(RetryExpression expression);
//		bool VisitNextExpression(NextExpression expression);
//		bool VisitRedoExpression(RedoExpression expression);
//		bool VisitRangeExpression(RangeExpression rangeExpression);
//		bool VisitRaiseExpression(RaiseExpression expression);
//		bool VisitMethodInvocationExpression(MethodInvocationExpression invocationExpression);
//		bool VisitMemberAccessExpression(MemberAccessExpression accessExpression);
//		bool VisitLiteralReferenceExpression(LiteralReferenceExpression expression);
//		bool VisitListExpression(ListExpression expression);
//		bool VisitLambdaExpression(LambdaExpression expression);
//		bool VisitIfStatement(IfStatement ifStatement);
//		bool VisitForStatement(ForStatement statement);
//		bool VisitExpressionStatement(ExpressionStatement statement);
//		bool VisitDictExpression(DictExpression expression);
//		bool VisitCompoundExpression(CompoundExpression expression);
//		bool VisitBreakExpression(BreakExpression breakExpression);
//		bool VisitBlockExpression(BlockExpression expression);
//		bool VisitBinaryExpression(BinaryExpression expression);
//		bool VisitTypeDeclarationExpression(TypeDeclarationExpression typeDeclarationExpression);
//		bool VisitBaseReferenceExpression(BaseReferenceExpression expression);
//		bool VisitSelfReferenceExpression(SelfReferenceExpression expression);
//		bool VisitNullCheckExpression(NullCheckExpression expression);
	}
}
