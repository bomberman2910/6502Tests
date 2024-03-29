﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace asm6502
{
    public class CodeLine : IEquatable<CodeLine>
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum Linetype
        {
            CODE,
            COMMENT,
            DIRECTIVE,
            LABEL,
            VARIABLE,
            UNDEFINED
        }

        public CodeLine(string line)
        {
            Line = line.Trim();
            if (Line.StartsWith(";"))
                Type = Linetype.COMMENT;
            else if (Line.StartsWith("."))
                Type = Linetype.DIRECTIVE;
            else if (Line.Contains(":"))
                Type = Linetype.LABEL;
            else if (Line.Contains("=") && !line.Contains(";") || Line.Contains(";") && Line.Split(';')[0].Contains("="))
                Type = Linetype.VARIABLE;
            else
                Type = Linetype.CODE;
            ContainsComment = Line.Contains(";");
        }

        public Linetype Type { get; }
        public bool ContainsComment { get; }
        public string Line { get; set; }

        /// <summary>
        ///     Returns the CodeLine without comments and leading or trailing whitespaces
        /// </summary>
        /// <returns>clean CodeLine</returns>
        public string Clean() => ContainsComment ? Line.Split(';')[0].Trim() : Line.Trim();

        public override string ToString() => Line;

        public override bool Equals(object obj) => Equals(obj as CodeLine);

        public bool Equals(CodeLine other) =>
            other != null &&
            Line == other.Line;

        public override int GetHashCode()
        {
            var hashCode = 261734979;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(CodeLine left, CodeLine right) => EqualityComparer<CodeLine>.Default.Equals(left, right);

        public static bool operator !=(CodeLine left, CodeLine right) => !(left == right);
    }
}