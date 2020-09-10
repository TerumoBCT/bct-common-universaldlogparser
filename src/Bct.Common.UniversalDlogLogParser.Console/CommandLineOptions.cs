using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace Bct.Common.LogParsing.Console
{
   /// <summary>
   /// 
   /// </summary>
   public class CommandLineOptions
   {
      /// <summary>
      /// method for cloning object 
      /// </summary>
      /// <returns></returns>
      public Object Shallowcopy()
      {
         return this.MemberwiseClone();
      }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'v', longName: "verbose", Required = false, HelpText = "Set output to verbose messages.")]
      public Boolean Verbose { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'i', longName: "inputSink", Required = true, HelpText = "Input Sinks: " + Constants.DLOG)]
      public String InputSink { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'o', longName: "outputSink", Required = true, HelpText = "Valid Output Format: " + Constants.JSONTEXT + "," + Constants.MONGODB)]
      public String OutputSink { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'q', longName: "inputDirectory", Required = false, HelpText = "Directory path for Input Files (Only valid if output sink is " + Constants.JSONTEXT + ")")]
      public String InputDirectory { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'q', longName: "outputDirectory", Required = false, HelpText = "Directory path for Output Files (Only valid if output sink is " + Constants.JSONTEXT + ")")]
      public String OutputDirectory { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'w', longName: "inputFile", Required = false, HelpText = "Input file name (Valid for input sinks of type: " + Constants.DLOG + ", " + Constants.JSONTEXT + ")")]
      public String InputFile { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'c', longName: "connectionString", Required = false, HelpText = "Connection String for output sink of types: " + Constants.MONGODB)]
      public String ConnectionString { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'r', longName: "readAllFiles", Required = false, HelpText = "When an inputdirectory is passed but no single file name is passed, to read the whole directory set this flag")]
      public Boolean ReadAllFiles { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'p', longName: "parallelProcessingCount", Required = false, HelpText = "Number of DLOGs to process in parallel, only valid when the flag readAllFiles is used.")]
      public Int32 ParallelProcessingCount { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'x', longName: "overWrite", Required = false, HelpText = "Overwrite the existing output data (re-process) if it exists.")]
      public Boolean Overwrite { get; set; }

   }
}
