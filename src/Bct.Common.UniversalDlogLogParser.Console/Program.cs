using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bct.Common.LogParsing.DataAccess;
using Bct.Common.LogParsing.Reader;
using Bct.Common.LogParsing.Records;
using Bct.Common.LogParsing.Sinks;
using MongoDB.Bson;
using CommandLine;
using Microsoft.Extensions.Configuration;
using BCT.Common.Logging.Extensions;


namespace Bct.Common.LogParsing.Console
{
   class Program
   {
      /// <summary>
      /// TODO_CLG: Eventually MEF would be nice here
      /// </summary>
      private static Dictionary<String, Type> ms_InputSinks = new Dictionary<String, Type>()
      {
         { 
            Constants.DLOG, typeof(DlogInputReaderSink) 
         } ,
         // { mc_JSONTEXT, typeof(String) },
      };

      /// <summary>
      /// TODO_CLG: Eventually MEF would be nice here
      /// </summary>
      private static Dictionary<String, Type> ms_OutputSinks = new Dictionary<String, Type>()
      {
         { Constants.JSONTEXT, typeof(DlogToJsonFileOutputSink) },
         { Constants.MONGODB, typeof(DlogToMongoDbOutputSink) },
         { Constants.ELK, typeof(ElasticSearchOutputSink) },
      };

      /// <summary>
      /// 
      /// </summary>
      private volatile static Int32 ms_nFilesProcessed = 0;
      private volatile static Int32 ms_nFilesToProcess = 0;


      /// <summary>
      /// 
      /// </summary>
      /// <param name="args"></param>
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
               // TODO_CLG: .AddEnvironmentVariables()                                                                                                        
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
               ConnectionString = null,
               InputDirectory = startDir,
               InputFile = null,
               InputSink = Constants.DLOG,
               OutputDirectory = startDir,
               OutputSink = Constants.JSONTEXT,
               Overwrite = true,
               ParallelProcessingCount = 4, // TODO_CLG:
               ReadAllFiles = true,
               Verbose = false
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

            // Select Input Sink
            if (true == String.IsNullOrEmpty(o.InputSink))
            {
               o.InputSink = Constants.DLOG;
            }
            if (false == ms_InputSinks.Keys.Contains(o.InputSink))
            {
               System.Console.WriteLine("ERROR: Invalid Input sink selected: " + o.InputSink);
               return;
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
               if (false == o.ReadAllFiles)
               {
                  System.Console.WriteLine("ERROR: No input file.");
                  return;
               }
               else
               {
                  if (0 == o.ParallelProcessingCount)
                  {
                     o.ParallelProcessingCount = 4;
                  }

                  String[] files = Directory.GetFiles(o.InputDirectory, "*.dlog", SearchOption.TopDirectoryOnly);
                  if (null != files)
                  {
                     ms_nFilesToProcess += files.Count();
                     allFiles.TryAdd(o.InputDirectory, files);
                  }

                  System.Console.WriteLine("  -Reading all files in directory: ");
                  System.Console.WriteLine("  -Parallel processing: " + o.ParallelProcessingCount.ToString());

                  Task t = Task.Run(() => addDirectoriesToQueue(allFiles, Directory.GetDirectories(o.InputDirectory)));
               }
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

            System.Console.WriteLine("  -Input Sink : " + o.InputSink);
            System.Console.WriteLine("   Input Path : " + o.InputDirectory ?? o.ConnectionString);
            System.Console.WriteLine("  -Output Sink: " + o.OutputSink);
            System.Console.WriteLine("   Output Path: " + o.OutputDirectory ?? o.ConnectionString);
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
                         ParseOneFile(oNew);
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
                   String[] files = Directory.GetFiles(directory, "*.dlog", SearchOption.AllDirectories);
                   if (null != files)
                   {
                      allFiles.TryAdd(directory, files);
                      ms_nFilesToProcess += files.Count();
                      System.Console.WriteLine("- ADDED " + files.Count().ToString() + " files to Queue from " + directory);
                   }
                });
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="o"></param>
      private static void ParseOneFile(
         CommandLineOptions o)
      {
         String inputFileName = Path.Combine(o.InputDirectory, o.InputFile);

         if (false == File.Exists(inputFileName))
         {
            System.Console.WriteLine("ERROR: Invalid input file : " + inputFileName);
            return;
         }


         // TODO_CLG Validate MongoDb connection string
         IDlogParsingInputReaderSink inputReaderSink = null;
         try
         {
            // Validate output Sink
            if (true == String.IsNullOrEmpty(o.OutputSink))
            {
               o.OutputSink = Constants.JSONTEXT;
            }

            if (false == ms_OutputSinks.Keys.Contains(o.OutputSink))
            {
               System.Console.WriteLine("ERROR: No valid output sink selected.");
               return;
            }

            IDlogParsingOutputWriterSink parsingOutputSink = null;

            // TODO_CLG: USe the list ms_OutputSinks Activate 
            switch (o.OutputSink)
            {
               case Constants.JSONTEXT:
                  parsingOutputSink = new DlogToJsonFileOutputSink(o.OutputDirectory, o.InputFile);
                  break;
               case Constants.MONGODB:
                  parsingOutputSink = new DlogToMongoDbOutputSink(o.ConnectionString, Path.GetFileName(inputFileName));
                  break;
               case Constants.ELK:
                  parsingOutputSink = new ElasticSearchOutputSink(new Uri(o.ConnectionString), Path.GetFileName(inputFileName));
                  break;
               default:
                  System.Console.WriteLine("ERROR: Invalid output sink: " + o.OutputSink);
                  return;
            }

            switch (o.InputSink)
            {
               case Constants.FASTDLOG:
                  inputReaderSink = new DlogBufferedInputReaderSink(o.InputDirectory, o.InputFile, parsingOutputSink);
                  break;

               case Constants.DLOG:
                  inputReaderSink = new DlogInputReaderSink(o.InputDirectory, o.InputFile, parsingOutputSink);
                  break;
               default:
                  System.Console.WriteLine("ERROR: Invalid input sink: " + o.InputSink);
                  return;
            }

            Enums.ParsingResult result;
            Guid logId = Guid.Empty;
            if (false == o.Overwrite)
            {
               result = parsingOutputSink.GetProcessingStatus(inputReaderSink.SerialNumber, inputReaderSink.FileName, out logId);
               if (result != Enums.ParsingResult.Unknown)
               {
                  System.Console.WriteLine("- File Exists. Skipping : " + inputReaderSink.FileName);
                  return;
               }
            }
            else
            {
               if (false == parsingOutputSink.RemoveProcessingStatus(inputReaderSink.SerialNumber, inputReaderSink.FileName))
               {
                  System.Console.WriteLine("- Error removing processing status for " + inputReaderSink.FileName + " (overwrite)");
                  return;
               }
            }


            DateTime start = DateTime.Now;

            result = inputReaderSink.Read();
            DateTime end = DateTime.Now;
            TimeSpan ts = end - start;
            ++ms_nFilesProcessed;
            System.Console.WriteLine("Processed [" + o.InputFile + "], " + result.ToString() + ", pre-process time " + ts.ToString());
         }
         finally
         {
            if (null != inputReaderSink)
            {
               inputReaderSink.Dispose();
               inputReaderSink = null;
            }
         }
      }


      //private static void ProcessByConfigFile()
      //{
      //   DateTime start = DateTime.Now;
      //   String[] filesOrion = new String[0]; // Directory.GetFiles(ConfigurationManager.AppSettings["ParseTopDirectory"], "1S*.dlog", SearchOption.AllDirectories);
      //   String[] filesTrima = Directory.GetFiles(ConfigurationManager.AppSettings["ParseTopDirectory"], "1T*.dlog", SearchOption.AllDirectories);
      //   String[] filesReveos = new String[0]; // Directory.GetFiles(ConfigurationManager.AppSettings["ParseTopDirectory"], "1W*.dlog", SearchOption.AllDirectories);
      //   String[] filesCES = new String[0]; // Directory.GetFiles(ConfigurationManager.AppSettings["ParseTopDirectory"], "1C*.dlog", SearchOption.AllDirectories);
      //   String[] filesAtreus = new String[0]; // Directory.GetFiles(ConfigurationManager.AppSettings["ParseTopDirectory"], "1A*.dlog", SearchOption.AllDirectories);
      //   filesTrima = filesTrima.Take(500).ToArray<String>();
      //   DateTime end = DateTime.Now;
      //   TimeSpan ts = end - start;
      //   System.Console.WriteLine("Orion Files found  : " + filesOrion.Count() + " (" + ts.ToString() + ")");
      //   System.Console.WriteLine("Trima Files found  : " + filesTrima.Count() + " (" + ts.ToString() + ")");
      //   System.Console.WriteLine("Reveos Files found : " + filesReveos.Count() + " (" + ts.ToString() + ")");
      //   System.Console.WriteLine("Quantum Files found: " + filesCES.Count() + " (" + ts.ToString() + ")");
      //   System.Console.WriteLine("Atreus Files found : " + filesAtreus.Count() + " (" + ts.ToString() + ")");
      //   System.Console.WriteLine("Total files found: " + ( filesOrion.Count() + filesTrima.Count() + filesReveos.Count() + filesCES.Count() + filesAtreus.Count() + filesAtreus.Count() ).ToString());
      //   System.Console.WriteLine("   Time: " + ts.ToString());
      //   start = DateTime.Now;
      //   List<String> bag = new List<String>();
      //   bag.AddRange(filesOrion);
      //   bag.AddRange(filesTrima);
      //   bag.AddRange(filesReveos);
      //   bag.AddRange(filesCES);
      //   bag.AddRange(filesAtreus);

      //   if (AppSettingsConfiguration.ParallelProcessingCount > 0)
      //   {
      //      Parallel.ForEach(bag,
      //          new ParallelOptions()
      //          {
      //             MaxDegreeOfParallelism = AppSettingsConfiguration.ParallelProcessingCount
      //          },
      //          f => ParseFile(f));
      //   }
      //   else
      //   {
      //      foreach (var i in bag)
      //      {
      //         ParseFile(i);
      //      }
      //   }

      //   end = DateTime.Now;
      //   ts = end - start;
      //   System.Console.WriteLine("   Total Time: " + ts.ToString());
      //   TimeSpan averageTs = ts.Divide(ms_nFilesProcessed);
      //   System.Console.WriteLine("   Average File Time: " + averageTs.ToString());
      //   System.Console.ReadKey();
      //}


      //private static void ParseFiles(
      //    String[] fileNames)
      //{
      //   foreach (String file in fileNames)
      //   {
      //      ParseFile(file);
      //   }
      //}

      //private static void ParseFile(
      //    String file)
      //{
      //   IDlogParsingOutputWriterSink parsingOutputSink = null;
      //   try
      //   {
      //      if (true == AppSettingsConfiguration.MongoDbWrite)
      //      {
      //         parsingOutputSink = new DlogToMongoDbOutputSink(ConfigurationManager.ConnectionStrings["DLOG_MONGODB"].ConnectionString, Path.GetFileName(file));
      //      }
      //      else if (false == String.IsNullOrEmpty(AppSettingsConfiguration.OutputFilePath))
      //      {
      //         parsingOutputSink = new DlogToJsonFileOutputSink(AppSettingsConfiguration.OutputFilePath,
      //                                                      Path.GetFileName(file));
      //      }

      //      using (IDlogParsingInputReaderSink dlogReader = new DlogInputReaderSink(Path.GetDirectoryName(file), Path.GetFileName(file), parsingOutputSink))
      //      {
      //         Enums.ParsingResult result = dlogReader.Read();
      //         System.Console.WriteLine("Parsed file " + file + " (" + ( ++ms_nFilesProcessed ).ToString() + ") :: Result: " + result.ToString());
      //      }
      //   }
      //   finally
      //   {
      //      if (null != parsingOutputSink)
      //      {
      //         parsingOutputSink.Dispose();
      //      }
      //   }
      //}
   }
}

