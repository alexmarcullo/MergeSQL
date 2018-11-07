using System.Collections.Generic;
using System.IO;

namespace MergeSQL
{
    public class GenSqlFile
    {
        protected GenSqlFile() { }

        public GenSqlFile(FileInfo fileInfo) : this()
        {
            FileInfo = fileInfo;
            Dependencies = new List<GenSqlFile>();
            Depends = new List<GenSqlFile>();
        }

        public string Name
        {
            get
            {
                return FileInfo.Name.Replace(FileInfo.Extension, "");
            }
        }
        public List<GenSqlFile> Dependencies { get; private set; }
        public List<GenSqlFile> Depends { get; private set; }

        public FileInfo FileInfo { get; private set; }


        public void AddDependency(GenSqlFile genSqlFile)
        {
            Dependencies.Add(genSqlFile);
        }

        public void AddDepends(GenSqlFile genSqlFile)
        {
            Depends.Add(genSqlFile);
        }
    }
}
