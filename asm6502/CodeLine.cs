using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib6502;

namespace asm6502
{
    public class CodeLine : IEquatable<CodeLine>
    {

        public enum Linetype
        {
            CODE,
            COMMENT,
            DIRECTIVE,
            LABEL,
            UNDEFINIED
        }

        public Linetype Type { get; }
        public bool ContainsComment { get; }
        public string Line { get; }

        public CodeLine(string line)
        {
            Line = line;
            if (line.StartsWith("\t;"))
                Type = Linetype.COMMENT;
            else if (line.StartsWith("\t."))
                Type = Linetype.DIRECTIVE;
            else if (line.StartsWith("\t"))
                Type = Linetype.CODE;
            else if (line.EndsWith(":"))
                Type = Linetype.LABEL;
            else
                Type = Linetype.UNDEFINIED;
            ContainsComment = line.Contains(";");
        }

        #region Implements
        public override string ToString() => Line;

        public override bool Equals(object obj)
        {
            return Equals(obj as CodeLine);
        }

        public bool Equals(CodeLine other)
        {
            return other != null &&
                   Line == other.Line;
        }

        public override int GetHashCode()
        {
            var hashCode = 261734979;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Line);
            return hashCode;
        }

        public static bool operator ==(CodeLine left, CodeLine right)
        {
            return EqualityComparer<CodeLine>.Default.Equals(left, right);
        }

        public static bool operator !=(CodeLine left, CodeLine right)
        {
            return !(left == right);
        }
        #endregion
    }
}
