using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bct.Common.JSONDlog.ColumnMatcher.Console
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
      [Option(shortName: 'i', longName: "inputDirectory", Required = false, HelpText = "Directory path for Input Files.")]
      public String InputDirectory { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'o', longName: "outputDirectory", Required = false, HelpText = "Directory path for Output Files.")]
      public String OutputDirectory { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'f', longName: "inputFile", Required = false, HelpText = "Input file name.")]
      public String InputFile { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'w', longName: "overWrite", Required = false, HelpText = "Overwrite the existing output data (re-process) if it exists.")]
      public Boolean Overwrite { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'p', longName: "parallelProcessingCount", Required = false, HelpText = "Number of files to process in parallel, only valid when the flag readAllFiles is used.")]
      public Int32 ParallelProcessingCount { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [Option(shortName: 'c', longName: "columnFileName", Required = false, HelpText = "Column list filename (FQN).")]
      public String ColumnFileName { get; set; }
   }
}
