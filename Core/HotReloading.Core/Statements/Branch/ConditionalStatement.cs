﻿using System;
namespace HotReloading.Core.Statements
{
    public class ConditionalStatement : Statement
    {
        public Statement Condition { get; set; }
        public Statement IfTrue { get; set; }
        public Statement IfFalse { get; set; }
    }
}
