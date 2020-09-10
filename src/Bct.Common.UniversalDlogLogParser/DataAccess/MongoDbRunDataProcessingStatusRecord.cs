using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bct.Common.LogParsing.Records;

namespace Bct.Common.LogParsing.DataAccess
{
   /// <summary>
   /// Db Status record
   /// </summary>
   [BsonIgnoreExtraElements]
   internal class MongoDbRunDataProcessingStatusRecord
   {
      /// <summary>
      /// Database Id
      /// </summary>
      [BsonIgnore]
      internal ObjectId _id { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [BsonElement(Constants.DB_RecordGuid)]
      [BsonRepresentation(BsonType.String)]
      internal Guid LogId { get; set; }

      /// <summary>
      /// String representation of the time this record was created
      /// </summary>
      [BsonElement(Constants.DB_UTCTimeStamp)]
      [BsonRepresentation(BsonType.DateTime)]
      internal DateTime UTCTimeStamp
      {
         set;
         get;
      }

      /// <summary>
      /// Device Serial Number
      /// </summary>
      [BsonElement(Constants.DB_SerialNumber)]
      [BsonRepresentation(BsonType.String)]
      internal String SerialNumber;

      /// <summary>
      /// Rundata filename without extenstion
      /// </summary>
      [BsonElement(Constants.DB_LogName)]
      [BsonRepresentation(BsonType.String)]
      internal String LogName;

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
      internal Enums.ParsingResult ParsingStatus;

      /// <summary>
      /// Record count
      /// </summary>
      [BsonElement(Constants.DB_RecordCount)]
      [BsonRepresentation(BsonType.Int32)]

      internal Int32 RecordCount;

      /// <summary>
      /// 
      /// </summary>
      [BsonElement(Constants.DB_ProcessingTimeMillisecs)]
      [BsonRepresentation(BsonType.Int32)]
      internal Int32 ProcessingTimeMillisecs;
   }
}


