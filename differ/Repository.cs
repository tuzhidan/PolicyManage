using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PolicyManage.Diff
{

    abstract class Repository : IReadable, IWritable
    {

        public string FilePath { get; set; }
        public string Name { get; set; }

        public static IReadable RENAME(string file1Type)
        {
            IReadable file1;
            if (file1Type == "csv")
            {
                file1 = new CSVFile();
            }
            else if (file1Type == "tsv")
            {
                file1 = new TSVFile();
            }
            else
            {
                file1 = new TXTFile();
                if (file1Type != "txt")
                {
                    Console.WriteLine("The analyser doesn't currently support '." + file1Type + "' types.");
                    Console.WriteLine("Parameter 1 will be treated as a .txt, this may lead to unwanted results.");
                }
            }
            return file1;
        }

        public static string GetFileName(string filePath)
        {
            return filePath.Split('\\')[filePath.Split('\\').Length - 1];
        }

        public static string GetFileExtention(string filePath)
        {
            return Repository.GetFileName(filePath).Split('.').Last();
        }

        public static string GetDateTime()
        {
            DateTime currentDateTime = DateTime.Now;
            return currentDateTime.ToString("yyyy/MM/dd HH:mm:ss");
        }

        public static string GetAbsolutePath(string filePath)
        {
            return Path.IsPathRooted(filePath) ? filePath : Path.GetFullPath(filePath);
        }

        public static bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }


        //Available for read and write
        public bool Open(string filePath)
        {

            //Checking if a path has been inputted
            if (filePath.Length == 0)
            {
                throw new ArgumentNullException();
            }

            FilePath = GetAbsolutePath(filePath);

            Name = GetFileName(FilePath);

            return Exists(FilePath);

        }

        //Forces derived classes to implement some kind of 'create file' method
        public abstract bool Create(string directoryPath);

        //Forces derived classes to implement some kind of 'read' method
        public abstract List<string> Read();

        //Forces derived classes to implement some kind of 'write' method
        public abstract bool Write(string[] fileOutput);

        public string[] CreateDiffForFile(int granularity,List<Line> lines = null)
        {

            int currentVariant;
            string currentString;
            float differences = 0;
            float similarities = 0;

            List<string> fileOutput = new List<string>
            {
                "POLICY FILE DIFFERENCE ANALYSER SUMMARY",
                "Timestamp:   " + GetDateTime()
            };

            

            if (granularity == 0) { fileOutput.Add("Granularity: Per Line"); }
            else if (granularity == 1) { fileOutput.Add("Granularity: Per Word"); }
            else { fileOutput.Add("Granularity: Per Character"); }
            fileOutput.Add("");

            string fileExtention = GetFileExtention(FilePath);

            if (lines != null)
            {

                foreach (Line l in lines)
                {

                    currentString = "";
                    currentVariant = -2;

                    foreach (Phrase p in l.Phrases)
                    {

                        if (p.Variant == -1 && currentVariant != -1)
                        {
                            currentString += fileExtention == "csv" ? "<" : "-";
                        }
                        else if (p.Variant == 0 && currentVariant != 0)
                        {
                            currentString += fileExtention == "csv" ? "^" : "=";
                        }
                        else if (p.Variant == 1 && currentVariant != 1)
                        {
                            currentString += fileExtention == "csv" ? ">" : "+";
                        }

                        if (p.Variant == -1 || p.Variant == 1)
                        {
                            differences++;
                        }
                        else
                        {
                            similarities++;
                        }

                        currentVariant = p.Variant;

                        currentString += p.Text;

                    }

                    fileOutput.Add(currentString);
                }
                fileOutput.Insert(3, "Difference Rating: " + Math.Round((differences / (similarities + differences) * 100), 2).ToString() + "%");
            }
            else
            {
                fileOutput.Add("The two files were identical.");
            }
            fileOutput.Insert(3, "Similarities size: " + similarities);
            fileOutput.Insert(4, "Differences  size: " + differences);

            return fileOutput.ToArray();
        }

        //Returns the longest common subarray between two specified lists
        private Block GetLongestSublist(List<string> list1, List<string> list2, int list1Start, int list1End, int list2Start, int list2End)
        {
            int file1Pos;
            int file2Pos;
            int length;

            //Creating the block that will contain indicies of the largest sublist
            Block largestBlock = new Block();
   
            //Looping through each item in list1, comparing each to every item in list2
            for (int i = list1Start; i < list1End; i++)
            {
                for (int j = list2Start; j < list2End; j++)
                {

                    file1Pos = i;
                    file2Pos = j;
                    length = 0;

                    //Attempt to extend the current list while there are still elements in both lists
                    while (file1Pos < list1End && file2Pos < list2End)
                    {
                        if (list1[file1Pos] == list2[file2Pos])
                        {
                            file1Pos++;
                            file2Pos++;
                            length++;
                        }
                        else { break; }
                    }

                    //If the length of the current block exceeds the length of the currently stored block
                    if (length > largestBlock.Length)
                    {
                        largestBlock.File1Start = i;
                        largestBlock.File2Start = j;
                        largestBlock.Length = length;
                    }
                }
            }

            return largestBlock;
        }

        //Splits a string down to a specified granularity (either words or characters)
        private List<string> SplitString(string stringToSplit, int granularity)
        {
            List<string> stringArray;

            //Splits a string into a series of words seperated by spaces
            if (granularity == 1) { stringArray = stringToSplit.Split(' ').ToList(); }

            //Splits a string into a series of characters
            else { stringArray = stringToSplit.Select(curChar => curChar.ToString()).ToList(); }

            return stringArray;
        }

        //
        private List<Block> GetSubSimilarities(List<string> list1, List<string> list2, int index1Start, int index1End, int index2Start, int index2End, int granularity)
        {

            List<Block> similarityBlocks = new List<Block>();

            for (int i = index1Start; i < index1End; i++)
            {
                for (int j = index2Start; j < index2End; j++)
                {
          
                    List<string> sentence1 = SplitString(list1[i], granularity);
                    List<string> sentence2 = SplitString(list2[j], granularity);

                    List<Block> sentenceSimilarities = GetSimilarityBlocks(sentence1, sentence2, 1, granularity);

                    if (sentenceSimilarities.Count() > 0)
                    {

                        Block newSentence = new Block
                        {
                            File1Start = i,
                            File2Start = j,
                            Length = 1,
                            ContainsSubBlock = true,
                            SubBlocks = sentenceSimilarities
                        };

                        similarityBlocks.Add(newSentence);

                        i++;
                        index2Start = j;
                            


                    }
                    
                }
            }

            return similarityBlocks;
        }

        //Returns a list of indices showing the similarities between two lists
        private List<Block> GetSimilarityBlocks(List<string> list1, List<string> list2, int index1Start, int index1End, int index2Start, int index2End, int recLevel, int granularity)
        {
            List<Block> similarityBlocks = new List<Block>();

            Block currentSimilarity;

            currentSimilarity = GetLongestSublist(list1, list2, index1Start, index1End, index2Start, index2End);

            if (currentSimilarity.Length == 0)
            {

                if (recLevel < 1 && granularity > 0)
                {
                    similarityBlocks.AddRange(GetSubSimilarities(list1, list2, index1Start, index1End, index2Start, index2End, granularity));
                }

                return similarityBlocks;
            }

            similarityBlocks.Add(currentSimilarity);

            int lowerFile1Start = currentSimilarity.File1Start + currentSimilarity.Length;
            int lowerFile2Start = currentSimilarity.File2Start + currentSimilarity.Length;

            similarityBlocks.AddRange(GetSimilarityBlocks(list1, list2, index1Start, currentSimilarity.File1Start, index2Start, currentSimilarity.File2Start, 0, granularity));
            similarityBlocks.AddRange(GetSimilarityBlocks(list1, list2, lowerFile1Start, index1End, lowerFile2Start, index2End, 0, granularity));


            return similarityBlocks;             
        }

        public List<Block> GetSimilarityBlocks(List<string> list1, List<string> list2, int recLevel, int granularity)
        {
            return GetSimilarityBlocks(list1, list2, 0, list1.Count(), 0, list2.Count(), recLevel, granularity);
        }

        //Creates a new Line object, each with a single phrase
        public Line CreateLine(List<string> currentList, int listNumber, int listPosition, int variant)
        {
            Line l = new Line();

            if (listNumber == 1)
            {
                l.File1Number = listPosition;
            }
            else
            {
                l.File2Number = listPosition;
            }

            Phrase p = new Phrase
            {
                Variant = variant,
                Text = currentList[listPosition]
            };

            l.Phrases.Add(p);
            return l;
        }

        //Translates a list of similarity blocks into the physical text 
        public List<Line> GetLines(List<Block> similarityBlocks, List<string> list1, List<string> list2, int recLevel, int granularity)
        {
            List<Line> lines = new List<Line>();
            
            similarityBlocks = similarityBlocks.OrderBy(Block => Block.File1Start).ToList();

            int lastFile1BlockEnd = -1;
            int lastFile2BlockEnd = -1;

            for (int i = 0; i < similarityBlocks.Count(); i++)
            {
                if (i == 0)
                {
                    for (int j = 0; j < similarityBlocks[i].File1Start; j++)
                    {
                        lines.Add(CreateLine(list1, 1, j, -1));
                    }

                    for (int j = 0; j < similarityBlocks[i].File2Start; j++)
                    {
                        lines.Add(CreateLine(list2, 2, j, 1));
                    }
                }
                else
                {
                    for (int j = similarityBlocks[i - 1].File1Start + similarityBlocks[i - 1].Length; j < similarityBlocks[i].File1Start; j++)
                    {
                        lines.Add(CreateLine(list1, 1, j, -1));
                    }

                    for (int j = similarityBlocks[i - 1].File2Start + similarityBlocks[i - 1].Length; j < similarityBlocks[i].File2Start; j++)
                    {
                        lines.Add(CreateLine(list2, 2, j, 1));
                    }
                }

                if (recLevel < 1 && similarityBlocks[i].ContainsSubBlock)
                {
                    Line l = new Line
                    {
                        File1Number = similarityBlocks[i].File1Start,
                        File2Number = similarityBlocks[i].File2Start
                    };

                    List<string> sentence1 = SplitString(list1[similarityBlocks[i].File1Start], granularity);
                    List<string> sentence2 = SplitString(list2[similarityBlocks[i].File2Start], granularity);

                    foreach (Line a in GetLines(similarityBlocks[i].SubBlocks, sentence1, sentence2, 1, granularity))
                    {
                        foreach (Phrase b in a.Phrases)
                        {
                            Phrase tempPhrase = new Phrase
                            {
                                Variant = b.Variant,
                                Text = b.Text
                            };

                            if (granularity == 1)
                            {
                                tempPhrase.Text += " ";
                            }

                            l.Phrases.Add(tempPhrase);
                        }
                    }

                    lines.Add(l);

                }
                else
                {
                    //Adds the current similarity block
                    for (int j = 0; j < (similarityBlocks[i].File1Start + similarityBlocks[i].Length) - similarityBlocks[i].File1Start; j++)
                    {

                        Line l = new Line
                        {
                            File1Number = similarityBlocks[i].File1Start + j,
                            File2Number = similarityBlocks[i].File2Start + j
                        };

                        Phrase p = new Phrase
                        {
                            Variant = 0,
                            Text = list1[similarityBlocks[i].File1Start + j]
                        };
                        l.Phrases.Add(p);

                        lines.Add(l);

                    }
                }
            }

            if (similarityBlocks.Count() == 0)
            {
                similarityBlocks.Add(new Block { File1Start = -1, File2Start = -1, Length = 1 });
            }

            lastFile1BlockEnd = similarityBlocks.Last().File1Start + similarityBlocks.Last().Length;
            lastFile2BlockEnd = similarityBlocks.Last().File2Start + similarityBlocks.Last().Length;

            for (int i = lastFile1BlockEnd; i < list1.Count(); i++)
            {
                lines.Add(CreateLine(list1, 1, i, -1));
            }

            for (int i = lastFile2BlockEnd; i < list2.Count(); i++)
            {
                lines.Add(CreateLine(list2, 1, i, 1));
            }

            return lines;
        }

        public List<Line> GetLines(List<Block> similarityBlocks, List<string> list1, List<string> list2, int granularity)
        {
            return GetLines(similarityBlocks, list1, list2, 0, granularity);
        }

        //Controller method 
        public List<Line> FindDifference(List<string> file2Content, int granularity)
        {

            //Finds the indices of every similar section in the two files
            //A higher granularity rating, the more in-depth the analysis goes (lines => words => characters)
            List<Block> similarityBlocks = GetSimilarityBlocks(Read(), file2Content, 0, granularity);

            //Translates the above similarity blocks into 
            List<Line> lines = GetLines(similarityBlocks, Read(), file2Content, granularity);

            return lines;

        }
    }
}
