using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PolicyManage.Diff
{
    class TSVFile : Repository, IReadable
    {

        //Override for the base class 'Read' method
        public override List<string> Read()
        {
            string fileContent = File.ReadAllText(FilePath);
            fileContent.Replace('\n', '\t');
            string[] fileContentArray = fileContent.Split('\t');

            //Removes the last line containing the last \t character
            List<string> fileContentList = new List<string>(fileContentArray);
            fileContentList.RemoveAt(fileContentList.Count() - 1);
 

            return fileContentList;
        }

        public override bool Create(string directoryPath)
        {


            FilePath = GetAbsolutePath(directoryPath + GetDateTime() + ".tsv");

            if (Exists(FilePath))
            {
                File.Create(FilePath);
            }

            return true;
        }

        //Override for the base class 'Write' method
        public override bool Write(string[] fileContent)
        {
            File.WriteAllText(FilePath, string.Join("\t",fileContent));
            return true;
        }

    }
}
