﻿using HotReloading.Core.Statements;

namespace StatementConverter.StatementInterpreter
{
    public interface IStatementInterpreter
    {
        Statement GetStatement();
    }
}