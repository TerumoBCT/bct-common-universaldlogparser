using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bct.Common.JSONDlog.ColumnMatcher.Console
{
   /// <summary>
   /// 
   /// </summary>
   public class JsonFileProcessor
   {
      private const String NEW_FILE_EXTENSION = "POST-OUTPUT";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="inputFileName"></param>
      /// <param name="columnNames"></param>
      /// <param name="overWrite"></param>
      /// <returns></returns>
      public static Boolean ProcessFile(
         String inputFileName,
         List<String> columnNames,
         Boolean overWrite)
      {
         using (FileStream fsInput = File.Open(inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
         {
            using (BufferedStream bsInput = new BufferedStream(fsInput))
            {
               using (StreamReader srInput = new StreamReader(bsInput))
               {
                  String outputFilename = String.Concat(inputFileName, NEW_FILE_EXTENSION);
                  String tempOutputFilename = String.Concat(inputFileName, NEW_FILE_EXTENSION, ".temp");
                  if (File.Exists(tempOutputFilename))
                  {
                     File.Delete(tempOutputFilename);
                  }

                  if (File.Exists(outputFilename))
                  {
                     if (false == overWrite)
                     {
                        return (false);
                     }

                     File.Delete(outputFilename);
                  }

                  using (FileStream fsOutput = File.Open(tempOutputFilename, FileMode.Create, FileAccess.Write, FileShare.None))
                  {
                     using (BufferedStream bsOutput = new BufferedStream(fsOutput))
                     {
                        using (StreamWriter srOutput = new StreamWriter(bsOutput))
                        {
                           String line;
                           while ((line = srInput.ReadLine()) != null)
                           {
                              // TODO_CLG:  PROCESS LINE And WRITE IF NEEDED.
                              Boolean bKeepLine = false;
                              foreach (var colName in columnNames)
                              {
                                 if (line.Contains("\"" + colName + "\""))
                                 {
                                    bKeepLine = true;
                                    break;
                                 }
                              }

                              if (bKeepLine)
                              {
                                 srOutput.WriteLine(line);
                              }
                           }
                        }
                     }
                  }

                  File.Move(tempOutputFilename, outputFilename);
               }
            }
         }

         return (true);
      }
   }
}
