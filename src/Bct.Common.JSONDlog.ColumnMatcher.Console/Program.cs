using BCT.Common.Logging.Extensions;
using CommandLine;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bct.Common.JSONDlog.ColumnMatcher.Console
{
   class Program
   {
      private volatile static Int32 ms_nFilesProcessed = 0;
      private volatile static Int32 ms_nFilesToProcess = 0;

      private const String JSON_EXTENSION = ".dlog.json";


      static void Main(string[] args)
      {
         System.Console.WriteLine("TerumoBCT Universal DLOG Parser.");

         String currentDirectory = System.AppContext.BaseDirectory;
         System.Console.WriteLine("  - Current Directory: " + currentDirectory);

         // Set logger
         try
         {
            var configurationBuilder = new ConfigurationBuilder()
               .SetBasePath(currentDirectory)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
               .Build();
            configurationBuilder.ConfigureBCTLogging();
         }
         catch (Exception ex)
         {
            System.Console.WriteLine("  - Exception while trying to configure: " + ex.Message);
         }

         if (args?.Length == 0)
         {
            String startDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            System.Console.WriteLine("TerumoBCT Universal DLOG Parser. No parameters passed. Entering local directory mode.");

            CommandLineOptions cmdOptions = new CommandLineOptions()
            {
               InputDirectory = startDir,
               InputFile = null,
               OutputDirectory = startDir,
               Overwrite = true,
               ParallelProcessingCount = 4,
               Verbose = false,
               ColumnFileName = null
            };

            processCommandLineOptions(cmdOptions);
         }
         else
         {
            CommandLine.ParserResult<CommandLineOptions> resultOptions = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
               .WithParsed<CommandLineOptions>(o => processCommandLineOptions(o));
         }
      }

      private static void processCommandLineOptions(
         CommandLineOptions o)
      {
         {
            if (true == o.Verbose)
            {
               System.Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
            }

            // Validate input directory
            if (true == String.IsNullOrEmpty(o.InputDirectory))
            {
               o.InputDirectory = System.Environment.CurrentDirectory;
            }
            else if (false == Directory.Exists(o.InputDirectory))
            {
               System.Console.WriteLine("ERROR: Invalid input directory : " + o.InputDirectory);
               return;
            }

            // Validate output directory
            if (true == String.IsNullOrEmpty(o.OutputDirectory))
            {
               o.OutputDirectory = System.Environment.CurrentDirectory;
            }
            else if (false == Directory.Exists(o.OutputDirectory))
            {
               System.Console.WriteLine("ERROR: Invalid output directory : " + o.OutputDirectory);
               return;
            }

            // Validate Input FileName
            ConcurrentDictionary<String, String[]> allFiles = new ConcurrentDictionary<String, String[]>();
            if ((true == String.IsNullOrEmpty(o.InputFile)) &&
                (false == String.IsNullOrEmpty(o.InputDirectory)))
            {

               if (0 == o.ParallelProcessingCount)
               {
                  o.ParallelProcessingCount = 4;
               }

               String[] files = Directory.GetFiles(o.InputDirectory, String.Concat("*", JSON_EXTENSION), SearchOption.TopDirectoryOnly);
               if (null != files)
               {
                  ms_nFilesToProcess += files.Count();
                  allFiles.TryAdd(o.InputDirectory, files);
               }

               System.Console.WriteLine("  -Reading all files in directory: ");
               System.Console.WriteLine("  -Parallel processing: " + o.ParallelProcessingCount.ToString());

               Task t = Task.Run(() => addDirectoriesToQueue(allFiles, Directory.GetDirectories(o.InputDirectory)));
            }
            else
            {
               allFiles.TryAdd(o.InputDirectory, new String[] { o.InputFile });
               ms_nFilesToProcess += 1;
            }

            if (0 >= o.ParallelProcessingCount)
            {
               o.ParallelProcessingCount = 1;
            }


            List<String> columnNames = new List<string>();
            if (!String.IsNullOrWhiteSpace(o.ColumnFileName))
            {
               if (!File.Exists(o.ColumnFileName))
               {
                  System.Console.WriteLine("ERROR: Invalid column file name : " + o.ColumnFileName);
                  return;
               }

               columnNames = GetColumnList(o.ColumnFileName);
            }

            System.Console.WriteLine("   Input Path : " + o.InputDirectory);
            System.Console.WriteLine("   Output Path: " + o.OutputDirectory);
            System.Console.WriteLine("...");

            // TODO_CLG: we shouldnt wait a random number, we just want to wait until the task adding dlogs to the queue starts
            Thread.Sleep(2000);

            DateTime start = DateTime.Now;
            while (0 != allFiles.Count)
            {
               String key = allFiles.First().Key;
               if (true == allFiles.TryRemove(key, out string[] bag))
               {
                  Parallel.ForEach(bag,
                      new ParallelOptions()
                      {
                         MaxDegreeOfParallelism = o.ParallelProcessingCount
                      },
                      file =>
                      {
                         CommandLineOptions oNew = (CommandLineOptions)o.Shallowcopy();
                         oNew.InputDirectory = Path.GetDirectoryName(Path.Combine(key, file));
                         oNew.InputFile = Path.GetFileName(file);
                         ParseOneFile(oNew, columnNames);
                      });
               }
            }

            DateTime end = DateTime.Now;
            TimeSpan ts = end - start;
            TimeSpan averageTs = (ms_nFilesProcessed == 0) ? new TimeSpan(0) : ts.Divide(ms_nFilesProcessed);
            System.Console.WriteLine("- Files Processed:" + ms_nFilesProcessed.ToString() + ", total: " + ts.ToString() + ", average: " + averageTs.ToString());
         }
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="allFiles"></param>
      /// <param name="directoryList"></param>
      private static void addDirectoriesToQueue(
         ConcurrentDictionary<String, String[]> allFiles,
         String[] directoryList)
      {
         if (null != directoryList)
         {
            Parallel.ForEach(directoryList,
                new ParallelOptions()
                {
                   MaxDegreeOfParallelism = 3
                },
                directory =>
                {
                   String[] files = Directory.GetFiles(directory, String.Concat("*", JSON_EXTENSION), SearchOption.AllDirectories);
                   if (null != files)
                   {
                      allFiles.TryAdd(directory, files);
                      ms_nFilesToProcess += files.Count();
                      System.Console.WriteLine("- ADDED " + files.Count().ToString() + " files to Queue from " + directory);
                   }
                });
         }
      }

      private static void ParseOneFile(
         CommandLineOptions o,
         List<String> columnNames)
      {
         String inputFileName = Path.Combine(o.InputDirectory, o.InputFile);

         if (false == File.Exists(inputFileName))
         {
            System.Console.WriteLine("ERROR: Invalid input file : " + inputFileName);
            return;
         }

         DateTime start = DateTime.Now;

         // TODO_CLG: PROCESSING OF FILE HERE
         JsonFileProcessor.ProcessFile(inputFileName, columnNames, o.Overwrite);

         DateTime end = DateTime.Now;
         TimeSpan ts = end - start;
         ++ms_nFilesProcessed;
         System.Console.WriteLine("Processed [" + o.InputFile + "] in " + ts.ToString());

      }


      private static List<string> GetColumnList(
         String columnFileName)
      {
         List<string> retVal = new List<String>();

         using (FileStream fs = File.Open(columnFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
         {
            using (BufferedStream bs = new BufferedStream(fs))
            {
               using (StreamReader sr = new StreamReader(bs))
               {
                  string line;
                  while ((line = sr.ReadLine()) != null)
                  {
                     retVal.Add(line);
                  }
               }
            }
         }

         return (retVal);
      }
   }
}
