using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyManage.Diff
{
    interface IReadable
    {
        string Name { get; }
        bool Open(string path);
        List<string> Read();
        List<Line> FindDifference(List<string> secondFile, int granularity);
    }
}
