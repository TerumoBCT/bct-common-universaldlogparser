using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bct.Common.LogParsing.Records;

namespace Bct.Common.LogParsing.Models
{
   /// <summary>
   /// Db Status record
   /// </summary>
   [BsonIgnoreExtraElements]
   public class RunDataProcessingStatusRecord
   {
      /// <summary>
      /// Database Id
      /// </summary>
      [BsonIgnore]
      public ObjectId _id;

      /// <summary>
      /// 
      /// </summary>
      [BsonElement(Constants.DB_RecordGuid)]
      [BsonRepresentation(BsonType.String)]
      public Guid LogId { get; set; }

      /// <summary>
      /// String representation of the time this record was created
      /// </summary>
      [BsonElement(Constants.DB_UTCTimeStamp)]
      [BsonRepresentation(BsonType.DateTime)]
      public DateTime UTCTimeStamp
      {
         set;
         get;
      }

      /// <summary>
      /// Device Serial Number
      /// </summary>
      [BsonElement(Constants.DB_SerialNumber)]
      [BsonRepresentation(BsonType.String)]
      public String SerialNumber;

      /// <summary>
      /// Rundata filename without extenstion
      /// </summary>
      [BsonElement(Constants.DB_LogName)]
      [BsonRepresentation(BsonType.String)]
      public String LogName;

      /// <summary>
      /// String representation of Enums.e_DataLogParsingResult
      /// </summary>
      [BsonElement(Constants.DB_ParsingStatusStr)]
      [BsonRepresentation(BsonType.String)]
      public String ParsingStatusStr;

      /// <summary>
      /// 
      /// </summary>
      [BsonElement(Constants.DB_ParsingStatus)]
      [BsonRepresentation(BsonType.Int32)]
      public Enums.ParsingResult ParsingStatus;

      /// <summary>
      /// Record count
      /// </summary>
      [BsonElement(Constants.DB_RecordCount)]
      [BsonRepresentation(BsonType.Int32)]

      public Int32 RecordCount;

      /// <summary>
      /// 
      /// </summary>
      [BsonElement(Constants.DB_ProcessingTimeMillisecs)]
      [BsonRepresentation(BsonType.Int32)]
      public Int32 ProcessingTimeMillisecs;
   }

   /// <summary>
   /// Db Status record
   /// </summary>
   public class ELKRunDataProcessingStatusRecord
   {
      /// <summary>
      /// Device Serial Number
      /// </summary>
      public String SerialNumber;

      /// <summary>
      /// Rundata filename without extenstion
      /// </summary>
      public String LogName;

      /// <summary>
      /// The Id of the log for each searching
      /// </summary>
      public Guid LogId; 
   }

}


