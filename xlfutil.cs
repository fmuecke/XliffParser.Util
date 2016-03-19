namespace MUITool.xlf2resx
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CommandLine;

    internal class xlf2resx
    {
        private static int Main(string[] args)
        {
            try
            {
                return CommandLine.Parser.Default.ParseArguments<DefaultOptions, WriteOptions, UpdateOptions>(args)
                    .MapResult(
                    (DefaultOptions o) => RunDefault(o),
                    (WriteOptions o) => RunWrite(o),
                    (UpdateOptions o) => RunUpdate(o),
                    errs => 1);
            }
            catch (Exception e)
            {
                // ignore:
                // - file not found
                // - invalid resource file
                // - etc.
                Console.Error.WriteLine(e.Message);
            }

            return 1;
        }

        private static int RunUpdate(UpdateOptions o)
        {
            if (!File.Exists(o.XlfFile))
            {
                Console.Error.WriteLine("error, file not found: " + o.XlfFile);
                return 2;
            }
            var doc = new xlflib.XlfDocument(o.XlfFile);
            if (string.IsNullOrWhiteSpace(o.ResXFile))
            {
                o.ResXFile = Path.Combine(Path.GetDirectoryName(o.XlfFile), doc.Files[0].Original);
            }
            if (!File.Exists(o.ResXFile))
            {
                Console.Error.WriteLine("error, file not found: " + o.ResXFile);
                return 2;
            }
            var result = doc.UpdateFromResX(o.ResXFile);
            if (o.IsVerbose)
            {
                var msg = string.Empty;
                if (result.Item1 == 0 && result.Item2 == 0 && result.Item3 == 0)
                {
                    msg = "already up-to-date";
                }
                else
                {
                    if (result.Item1 > 0)
                    {
                        msg += string.Format("{0} item{1} updated", result.Item1, result.Item1 == 1 ? "" : "s");
                    }

                    if (result.Item2 > 0)
                    {
                        if (msg.Length > 0) msg += "/";
                        msg += string.Format("{0} item{1} added", result.Item2, result.Item2 == 1 ? "" : "s");
                    }

                    if (result.Item3 > 0)
                    {
                        if (msg.Length > 0) msg += "/";
                        msg += string.Format("{0} item{1} removed", result.Item3, result.Item3 == 1 ? "" : "s");
                    }
                }

                Console.WriteLine(msg);
            }

            doc.Save();
            return 0;
        }

        private static int RunWrite(WriteOptions o)
        {
            var doc = new xlflib.XlfDocument(o.XlfFile);
            doc.SaveAsResX(o.ResXFile, o.IsSorted ? xlflib.XlfDocument.SaveMode.Sorted : xlflib.XlfDocument.SaveMode.Default);
            return 0;
        }

        private static int RunDefault(DefaultOptions o)
        {
            var doc = new xlflib.XlfDocument(o.XlfFile);
            Console.WriteLine("file: " + doc.FileName);
            Console.WriteLine("XLIFF version: " + doc.Version);
            Console.WriteLine("file sections: " + doc.Files.Count);
            var n = 1;
            foreach (var f in doc.Files)
            {
                Console.WriteLine(">" + n + " data type: " + f.DataType);
                Console.WriteLine(">" + n + " original: " + f.Original);
                Console.WriteLine(">" + n + " source language: " + f.SourceLang);
                Console.WriteLine(">" + n + " target language: " + f.Optional.TargetLang);
                Console.WriteLine(">" + n + " tool id: " + f.Optional.ToolId);
                Console.WriteLine(">" + n + " translation units: " + f.TransUnits.Count);
            }

            return 0;
        }

        [Verb("info", HelpText = "Retrieves xlf file information.")]
        private class DefaultOptions
        {
            [Option('x', "xlf", Required = true, HelpText = "xlf input file")]
            public string XlfFile { get; set; }
        }

        [Verb("write", HelpText = "Write XLF translation data to specified .resx output file.")]
        private class WriteOptions
        {
            [Option('x', "xlf", Required = true, HelpText = "xlf input file")]
            public string XlfFile { get; set; }

            [Option('r', "resx", Required = true, HelpText = "output file")]
            public string ResXFile { get; set; }

            [Option('s', "sorted", HelpText = "sort data alphabetically by name")]
            public bool IsSorted { get; set; }
        }

        [Verb("update", HelpText = "Update XLF translation data from specified .resx input file.")]
        private class UpdateOptions
        {
            [Option('x', "xlf", Required = true, HelpText = "xlf file to be updated")]
            public string XlfFile { get; set; }

            [Option('r', "resx", HelpText = "resx source file")]
            public string ResXFile { get; set; }

            [Option('v', "verbose", HelpText = "dispay additional information")]
            public bool IsVerbose { get; set; }

            //[Option('s', "sorted", HelpText = "sort data alphabetically by name")]
            //public bool IsSorted { get; set; }
        }
    }
}