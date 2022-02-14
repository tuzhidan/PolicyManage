using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyManage.Diff
{
    interface IWritable
    {
        string FilePath { get; set; }
        bool Create(string directoryPath);
        string[] CreateDiffForFile(int granularity, List<Line> lines = null);
        bool Write(string[] fileContents);
        List<Line> FindDifference(List<string> secondFile, int granularity);
    }
}
