using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyManage.Diff
{
    class Line
    {
        public List<Phrase> Phrases = new List<Phrase>();

        public int File1Number { get; set; }
        public int File2Number { get; set; }
    }
}
