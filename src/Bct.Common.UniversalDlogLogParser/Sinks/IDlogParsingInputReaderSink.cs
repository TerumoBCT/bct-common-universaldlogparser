using System;
using System.Collections.Generic;
using System.Text;

namespace Bct.Common.LogParsing.Sinks
{
   /// <summary>
   /// 
   /// </summary>
   public interface IDlogParsingInputReaderSink : IDisposable
   {
      /// <summary>
      /// The original DLOG File name
      /// </summary>
      String FileName { get; }

      /// <summary>
      /// For a
      ///   File: this property is a FQN of a directory
      ///   Database: the connection String 
      /// </summary>
      String InputPath { get;  }

      /// <summary>
      /// Reads the DLOG from the input Sink (the implementation uses dependency injection to pass the writer sink)
      /// </summary>
      /// <returns></returns>
      Enums.ParsingResult Read();

      /// <summary>
      /// 
      /// </summary>
      String SerialNumber { get; }
   }
}
