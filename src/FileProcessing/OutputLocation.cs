using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ringo.FileProcessing
{
    public class OutputLocation
    {
        public string Path { get; set; }
        public Func<string, string> NamingConvention { get; set; }
    }
}
