﻿namespace HotReloading.Core.Statements
{
    public class DelegateObjectCreationStatement : Statement
    {
        public Statement Method { get; set; }
        public ClassType Type { get; set; }
    }
}