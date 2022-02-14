using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyManage.Diff
{
    class Block
    {

        public List<Block> SubBlocks { get; set; }
        
        public bool ContainsSubBlock { get; set; }

        public int File1Start { get; set; }

        public int File2Start { get; set; }

        public int Length { get; set; }

    }
}
