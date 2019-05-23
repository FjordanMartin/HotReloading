﻿using System;
using System.Linq;
using HotReloading.Core;
using HotReloading.Core.Statements;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StatementConverter.Extensions;

namespace StatementConverter.StatementInterpreter
{
    internal class LamdaStatementInterpreter : IStatementInterpreter
    {
        private StatementInterpreterHandler statementInterpreterHandler;
        private ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax;
        private readonly Microsoft.CodeAnalysis.SemanticModel semanticModel;

        public LamdaStatementInterpreter(StatementInterpreterHandler statementInterpreterHandler, ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax, Microsoft.CodeAnalysis.SemanticModel semanticModel)
        {
            this.statementInterpreterHandler = statementInterpreterHandler;
            this.parenthesizedLambdaExpressionSyntax = parenthesizedLambdaExpressionSyntax;
            this.semanticModel = semanticModel;
        }

        public Statement GetStatement()
        {
            var lamdaStatement = new LamdaStatement();

            var parameters = parenthesizedLambdaExpressionSyntax.ParameterList.Parameters
                .Select(x => statementInterpreterHandler.GetStatement(x));
            lamdaStatement.Parameters = parameters.Cast<Parameter>().ToArray();

            lamdaStatement.Body = statementInterpreterHandler.GetStatement(parenthesizedLambdaExpressionSyntax.Body);

            var typeInfo = semanticModel.GetTypeInfo(parenthesizedLambdaExpressionSyntax);

            lamdaStatement.Type = typeInfo.ConvertedType.GetClassType();

            return lamdaStatement;
        }
    }
}