using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace zallods.Formats
{
    class FormatException : Exception
    {
        public FormatException() : base() { }
        public FormatException(String message) : base(message) { }
    }
}
