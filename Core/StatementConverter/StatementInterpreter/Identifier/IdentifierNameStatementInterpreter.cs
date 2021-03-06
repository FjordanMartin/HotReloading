﻿using System;
using System.Collections.Generic;
using HotReloading.Core;
using HotReloading.Core.Statements;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StatementConverter.Extensions;

namespace StatementConverter.StatementInterpreter
{
    public class IdentifierNameStatementInterpreter : IStatementInterpreter
    {
        private readonly IdentifierNameSyntax identifierNameSyntax;
        private readonly List<Parameter> parameters;
        private readonly Statement parent;
        private readonly SemanticModel semanticModel;

        public IdentifierNameStatementInterpreter(IdentifierNameSyntax identifierNameSyntax,
            SemanticModel semanticModel,
            List<Parameter> parameters, Statement parent = null)
        {
            this.identifierNameSyntax = identifierNameSyntax;
            this.semanticModel = semanticModel;
            this.parameters = parameters;
            this.parent = parent;
        }

        public Statement GetStatement()
        {
            var varName = identifierNameSyntax.Identifier.Text;

            if (varName == "nameof")
                return new NameOfStatement();

            if(parent is BaseStatement)
            {
                //Find HotReloadingBaseCall method
                var callerMethod = GetCallingMethod(identifierNameSyntax);
                var callerSymbol = semanticModel.GetDeclaredSymbol(callerMethod);
                var methodSymbol = semanticModel.GetSymbolInfo(identifierNameSyntax).Symbol as IMethodSymbol;
                return new InstanceMethodMemberStatement
                {
                    Name = "HotReloadingBase_" + methodSymbol.Name,
                    ParentType = callerSymbol.ContainingType.GetClassType(),
                    Parent = parent ?? new ThisStatement(),
                    AccessModifier = AccessModifier.Private
                };
            }

            var symbolInfo = semanticModel.GetSymbolInfo(identifierNameSyntax);
            return GetStatement(symbolInfo, varName);
        }

        private MethodDeclarationSyntax GetCallingMethod(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax)
                return syntaxNode as MethodDeclarationSyntax;

            if (syntaxNode.Parent == null)
                return null;

            return GetCallingMethod(syntaxNode.Parent);
        }

        private Statement GetStatement(SymbolInfo symbolInfo, string varName)
        {
            Statement statement;
            switch (symbolInfo.Symbol)
            {
                case IFieldSymbol fs:
                    statement = GetStatement(fs, varName);
                    break;
                case IPropertySymbol ps:
                    statement = GetStatement(ps, varName);
                    break;
                case ILocalSymbol ls:
                    statement = GetStatement(ls, varName);
                    break;
                case IParameterSymbol paraS:
                    statement = GetStatement(paraS, varName);
                    break;
                case IMethodSymbol ms:
                    statement = GetStatement(ms, varName);
                    break;
                case INamedTypeSymbol nts:
                    statement = GetStatement(nts, varName);
                    break;
                case INamespaceSymbol nts:
                    statement = GetStatement(nts, varName);
                    break;
                default:
                    throw new NotSupportedException($"{symbolInfo.Symbol.GetType()} is not supported yet.");
            }

            return statement;
        }

        private Statement GetStatement(IFieldSymbol fs, string varName)
        {
            if (fs.IsStatic)
                return new StaticFieldMemberStatement
                {
                    Name = varName,
                    ParentType = fs.ContainingType.GetClassType()
                };
            return new InstanceFieldMemberStatement
            {
                Name = varName,
                Parent = parent ?? new ThisStatement()
            };
        }

        private Statement GetStatement(IPropertySymbol ps, string varName)
        {
            if (ps.IsStatic)
                return new StaticPropertyMemberStatement
                {
                    Name = varName,
                    ParentType = ps.ContainingType.GetClassType()
                };
            return new InstancePropertyMemberStatement
            {
                Name = varName,
                Parent = parent ?? new ThisStatement()
            };
        }

        private Statement GetStatement(IMethodSymbol ms, string varName)
        {
            if (ms.IsStatic)
                return new StaticMethodMemberStatement
                {
                    Name = ms.Name,
                    ParentType = ms.ContainingType.GetClassType(),
                    AccessModifier = GetAccessModifier(ms)
                };
            return new InstanceMethodMemberStatement
            {
                Name = ms.Name,
                ParentType = ms.ContainingType.GetClassType(),
                Parent = parent ?? new ThisStatement(),
                AccessModifier = GetAccessModifier(ms)
            };
        }

        private AccessModifier GetAccessModifier(IMethodSymbol ms)
        {
            switch(ms.DeclaredAccessibility)
            {
                case Accessibility.Public:
                    return AccessModifier.Public;
                case Accessibility.Private:
                    return AccessModifier.Private;
                case Accessibility.Protected:
                    return AccessModifier.Protected;
                case Accessibility.Internal:
                    return AccessModifier.Internal;
                case Accessibility.ProtectedOrInternal:
                    return AccessModifier.ProtectedInternal;
                default:
                    throw new Exception("Accessibility is unknown");
            }
        }

        private static Statement GetStatement(ILocalSymbol ls, string varName)
        {
            return new LocalIdentifierStatement
            {
                Name = varName,
                Type = ls.Type.GetClassType()
            };
        }

        private static Statement GetStatement(IParameterSymbol paraS, string varName)
        {
            return new ParameterIdentifierStatement
            {
                Name = varName,
                Type = paraS.Type.GetClassType()
            };
        }

        private static Statement GetStatement(INamedTypeSymbol ns, string varName)
        {
            return new NamedTypeStatement
            {
                Name = varName,
                Type = ns.GetClassType()
            };
        }

        private static Statement GetStatement(INamespaceSymbol ns, string varName)
        {
            return new NamedTypeStatement
            {
                Name = varName,
                Type = null
            };
        }
    }
}