using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MergeSQL
{
    static class Program
    {
        static void Main(string[] args)
        {
            Run(Environment.CurrentDirectory, Configuration.EXTENSION);
            var folders = Directory.GetDirectories(Environment.CurrentDirectory, "*", SearchOption.AllDirectories).ToList();
            folders.ForEach(x => Run(x, Configuration.EXTENSION));

            Console.WriteLine("Press any key to exit ...");
            Console.ReadLine();
        }

        private static void Run(string folder, string extension)
        {
            var directoryInfo = new DirectoryInfo(folder);
            Log($@"SEARCHING ON FOLDER => {directoryInfo.Name}");

            var ls = new List<GenSqlFile>();

            #region Searching for sql files
            Log(@"SEARCHING FOR SQL FILES");
            var files = Directory.GetFiles(folder, extension);
            foreach (var file in files)
            {
                var genSqlFile = new GenSqlFile(new FileInfo(file));
                if (!genSqlFile.Name.Contains("_MERGED"))
                {
                    ls.Add(genSqlFile);
                    Log($"FILE FOUNDED => {genSqlFile.FileInfo.Name}");
                } 
            }


            if (ls.Count == 0)
            {
                Log($"NO {extension} FILES ON THIS DIRECTORY");
                return;
            }
            #endregion


            #region Searching for sql dependencies
            Log(@"SEARCHING FOR DEPENDENCIES");
            ls.ForEach(search =>
            {
                ls.ForEach(dependency =>
                {
                    if (search.Name != dependency.Name && VerifyDependency(search.FileInfo, dependency.Name))
                    {
                        search.AddDependency(dependency);
                        dependency.AddDepends(search);
                        Log($@"FILE DEPENDENCY FOUND => {search.FileInfo.Name} DEPENDS {dependency.FileInfo.Name}");
                    }
                });
            });
            #endregion

            #region Ordering files
            Log(@"ORDERING FILES");
            foreach (var file in ls.OrderByDescending(x => x.Depends.Count).OrderBy(x => x.Dependencies.Count))
            {
                Log($@"FILE {file.Name} HAVE {file.Depends.Count} DEPENDS AND {file.Dependencies.Count} DEPENDENCIES");
                file.Depends.ForEach(x => Log($@"= = => DEPENDS {x.Name}"));
                file.Dependencies.ForEach(x => Log($@"= = => DEPENDENCY {x.Name}"));
            }
            #endregion

            #region Generate merged file
            Log(@"GENERATE MERGED FILE");
            var sb = new StringBuilder();

            foreach (var file in ls.OrderByDescending(x => x.Depends.Count).OrderBy(x => x.Dependencies.Count))
            {
                sb.Append($"/* MERGED BY GenSQL => {file.FileInfo.Name}*/");
                sb.Append(Environment.NewLine);
                sb.Append(File.ReadAllText(file.FileInfo.FullName));
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }

            string generatedFile = $@"{folder}\{DateTime.Now.ToString("yyyyMMddHHmmss")}_MERGED.sql";

            using (var streamWrite = new StreamWriter(generatedFile))
                streamWrite.Write(sb.ToString());
            #endregion
        }

        private static bool VerifyDependency(FileInfo searchingFile, string sqlDependency)
        {
            var depends = false;
            string[] lines = File.ReadAllLines(searchingFile.FullName);
            lines.ToList().ForEach(line =>
            {
                if (line.ToLower().Contains($".{sqlDependency.ToLower()} "))
                    depends = true;
            });

            return depends;
        }

        private static void Log(string text)
        {
            Console.WriteLine(text);
        }
    }
}
