using Bct.Common.LogParsing.Sinks;
using BCT.Common.Logging.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bct.Common.LogParsing.Utils
{
   /// <summary>
   /// 
   /// </summary>
   public static class PrintfPort
   {
      #region Public Methods
      #region IsNumericType
      /// <summary>
      /// Determines whether the specified value is of numeric type.
      /// </summary>
      /// <param name="o">The object to check.</param>
      /// <returns>
      /// <c>true</c> if o is a numeric type; otherwise, <c>false</c>.
      /// </returns>
      public static bool IsNumericType(object o)
      {
         return ( o is byte ||
             o is sbyte ||
             o is short ||
             o is ushort ||
             o is int ||
             o is uint ||
             o is long ||
             o is ulong ||
             o is float ||
             o is double ||
             o is decimal );
      }
      #endregion
      #region IsPositive
      /// <summary>
      /// Determines whether the specified value is positive.
      /// </summary>
      /// <param name="value">The value.</param>
      /// <param name="zeroIsPositive">if set to <c>true</c> treats 0 as positive.</param>
      /// <returns>
      /// 	<c>true</c> if the specified value is positive; otherwise, <c>false</c>.
      /// </returns>
      public static bool IsPositive(
         object value,
         bool zeroIsPositive)
      {
         return (Type.GetTypeCode(value.GetType())) switch
         {
            TypeCode.SByte => (zeroIsPositive ? (sbyte)value >= 0 : (sbyte)value > 0),
            TypeCode.Int16 => (zeroIsPositive ? (short)value >= 0 : (short)value > 0),
            TypeCode.Int32 => (zeroIsPositive ? (int)value >= 0 : (int)value > 0),
            TypeCode.Int64 => (zeroIsPositive ? (long)value >= 0 : (long)value > 0),
            TypeCode.Single => (zeroIsPositive ? (float)value >= 0 : (float)value > 0),
            TypeCode.Double => (zeroIsPositive ? (double)value >= 0 : (double)value > 0),
            TypeCode.Decimal => (zeroIsPositive ? (decimal)value >= 0 : (decimal)value > 0),
            TypeCode.Byte => (zeroIsPositive ? true : (byte)value > 0),
            TypeCode.UInt16 => (zeroIsPositive ? true : (ushort)value > 0),
            TypeCode.UInt32 => (zeroIsPositive ? true : (uint)value > 0),
            TypeCode.UInt64 => (zeroIsPositive ? true : (ulong)value > 0),
            TypeCode.Char => (zeroIsPositive ? true : (char)value != '\0'),
            _ => false,
         };
      }
      #endregion
      #region ToUnsigned
      /// <summary>
      /// Converts the specified values boxed type to its correpsonding unsigned
      /// type.
      /// </summary>
      /// <param name="value">The value.</param>
      /// <returns>A boxed numeric object whos type is unsigned.</returns>
      public static object ToUnsigned(
         object value)
      {
         return (Type.GetTypeCode(value.GetType())) switch
         {
            TypeCode.SByte => (byte)((sbyte)value),
            TypeCode.Int16 => (ushort)((short)value),
            TypeCode.Int32 => (uint)((int)value),
            TypeCode.Int64 => (ulong)((long)value),
            TypeCode.Byte => value,
            TypeCode.UInt16 => value,
            TypeCode.UInt32 => value,
            TypeCode.UInt64 => value,
            TypeCode.Single => (UInt32)((float)value),
            TypeCode.Double => (ulong)((double)value),
            TypeCode.Decimal => (ulong)((decimal)value),
            _ => null,
         };
      }
      #endregion
      #region ToInteger
      /// <summary>
      /// Converts the specified values boxed type to its correpsonding integer
      /// type.
      /// </summary>
      /// <param name="value">The value.</param>
      /// /// <param name="bRound">The value.</param>
      /// <returns>A boxed numeric object whos type is an integer type.</returns>
      public static object ToInteger(
          object value,
          bool bRound)
      {
         return (Type.GetTypeCode(value.GetType())) switch
         {
            TypeCode.SByte => value,
            TypeCode.Int16 => value,
            TypeCode.Int32 => value,
            TypeCode.Int64 => value,
            TypeCode.Byte => value,
            TypeCode.UInt16 => value,
            TypeCode.UInt32 => value,
            TypeCode.UInt64 => value,
            TypeCode.Single => (bRound ? (int)Math.Round((float)value) : (int)((float)value)),
            TypeCode.Double => (bRound ? (long)Math.Round((double)value) : (long)((double)value)),
            TypeCode.Decimal => (bRound ? Math.Round((decimal)value) : (decimal)value),
            _ => null,
         };
      }
      #endregion
      #region UnboxToLong
      /// <summary>
      /// 
      /// </summary>
      /// <param name="value"></param>
      /// <param name="round"></param>
      /// <returns></returns>    
      public static long UnboxToLong(
         object value,
         bool round)
      {
         return (Type.GetTypeCode(value.GetType())) switch
         {
            TypeCode.SByte => (long)((sbyte)value),
            TypeCode.Int16 => (long)((short)value),
            TypeCode.Int32 => (long)((int)value),
            TypeCode.Int64 => (long)value,
            TypeCode.Byte => (long)((byte)value),
            TypeCode.UInt16 => (long)((ushort)value),
            TypeCode.UInt32 => (long)((uint)value),
            TypeCode.UInt64 => (long)((ulong)value),
            TypeCode.Single => (round ? (long)Math.Round((float)value) : (long)((float)value)),
            TypeCode.Double => (round ? (long)Math.Round((double)value) : (long)((double)value)),
            TypeCode.Decimal => (round ? (long)Math.Round((decimal)value) : (long)((decimal)value)),
            _ => 0,
         };
      }
      #endregion
      #region ReplaceMetaChars
      /// <summary>
      /// Replaces the string representations of meta chars with their corresponding
      /// character values.
      /// </summary>
      /// <param name="input">The input.</param>
      /// <returns>A string with all string meta chars are replaced</returns>
      public static string ReplaceMetaChars(string input)
      {
         return Regex.Replace(input, @"(\\)(\d{3}|[^\d])?", new MatchEvaluator(ReplaceMetaCharsMatch));
      }
      private static string ReplaceMetaCharsMatch(Match m)
      {
         // convert octal quotes (like \040)
         if (m.Groups[2].Length == 3)
         {
            return Convert.ToChar(Convert.ToByte(m.Groups[2].Value, 8)).ToString();
         }
         else
         {
            // convert all other special meta characters
            //TODO: \xhhh hex and possible dec !!
            return m.Groups[2].Value switch
            {
               // null
               "0" => "\0",
               // alert (beep)
               "a" => "\a",
               // BS
               "b" => "\b",
               // FF
               "f" => "\f",
               // vertical tab
               "v" => "\v",
               // CR
               "r" => "\r",
               // LF
               "n" => "\n",
               // Tab
               "t" => "\t",
               _ => m.Groups[2].Value,// if neither an octal quote nor a special meta character
                                      // so just remove the backslash
            };
         }
      }
      #endregion

      #region sprintf
      /// <summary>
      /// We made it static to improve performance
      ///"%[parameter][flags][width][.precision][length]type"
      /// </summary>
      private static Regex ms_sPrintfExp = new Regex(@"\%(\d*\$)?([\'\#\-\+ ]*)(\d*)(?:\.(\d+))?([hl])?([dioxXucsfeEgGpn%])");


      //private static Dictionary<String, Object> ms_formats = new Dictionary<string, object>();

      /// <summary>
      /// TODO_PERFORMANCE: This method gets called a lot, make sure it is as optimized as possible for performance
      /// </summary>
      /// <param name="inputFormat"></param>
      /// <param name="inputParams"></param>
      /// <returns></returns>
      public static Object Sprintf(
          String inputFormat,
          params Object[] inputParams)
      {
         // TODO_CLG: here we want the return ValueTask to be an object type based on the format of inputFormat

         #region Variables
         //lock (ms_formats)
         //{
         //   if (false == ms_formats.ContainsKey(inputFormat))
         //   {
         //      ms_formats.TryAdd(inputFormat, null);
         //   }
         //}

         StringBuilder formatBuilder = new StringBuilder();

         Match m = null;
         String w = String.Empty;
         Int32 defaultParamIx = 0;
         Int32 paramIx;
         Object o = null;

         Boolean flagLeft2Right = false;
         Boolean flagAlternate = false;
         Boolean flagPositiveSign = false;
         Boolean flagPositiveSpace = false;
         Boolean flagZeroPadding = false;
         Boolean flagGroupThousands = false;

         Int32 fieldLength = 0;
         Int32 fieldPrecision = 0;
         char shortLongIndicator = '\0';
         char formatSpecifier = '\0';
         char paddingCharacter = ' ';
         #endregion


         // find all format parameters in format string
         formatBuilder.Append(inputFormat);
         m = ms_sPrintfExp.Match(inputFormat);
         while (m.Success)
         {
            #region parameter index
            paramIx = defaultParamIx;
            if (m.Groups[1] != null && m.Groups[1].Value.Length > 0)
            {
               string val = m.Groups[1].Value.Substring(0, m.Groups[1].Value.Length - 1);
               paramIx = Convert.ToInt32(val) - 1;
            };
            #endregion

            #region format flags
            // extract format flags
            flagAlternate = false;
            flagLeft2Right = false;
            flagPositiveSign = false;
            flagPositiveSpace = false;
            flagZeroPadding = false;
            flagGroupThousands = false;
            if (m.Groups[2] != null && m.Groups[2].Value.Length > 0)
            {
               string flags = m.Groups[2].Value;

               flagAlternate = ( flags.IndexOf('#') >= 0 );
               flagLeft2Right = ( flags.IndexOf('-') >= 0 );
               flagPositiveSign = ( flags.IndexOf('+') >= 0 );
               flagPositiveSpace = ( flags.IndexOf(' ') >= 0 );
               flagGroupThousands = ( flags.IndexOf('\'') >= 0 );

               // positive + indicator overrides a
               // positive space character
               if (flagPositiveSign && flagPositiveSpace)
               {
                  flagPositiveSpace = false;
               }
            }
            #endregion

            #region field length
            // extract field length and 
            // pading character
            paddingCharacter = ' ';
            fieldLength = int.MinValue;
            if (m.Groups[3] != null && m.Groups[3].Value.Length > 0)
            {
               fieldLength = Convert.ToInt32(m.Groups[3].Value);
               flagZeroPadding = ( m.Groups[3].Value[0] == '0' );
            }
            #endregion

            if (flagZeroPadding)
            {
               paddingCharacter = '0';
            }

            // left2right allignment overrides zero padding
            if (flagLeft2Right && flagZeroPadding)
            {
               flagZeroPadding = false;
               paddingCharacter = ' ';
            }

            #region field precision
            // extract field precision
            fieldPrecision = int.MinValue;
            if (m.Groups[4] != null && m.Groups[4].Value.Length > 0)
            {
               fieldPrecision = Convert.ToInt32(m.Groups[4].Value);
            }
            #endregion

            #region short / long indicator
            // extract short / long indicator
            shortLongIndicator = Char.MinValue;
            if (m.Groups[5] != null && m.Groups[5].Value.Length > 0)
            {
               shortLongIndicator = m.Groups[5].Value[0];
            }
            #endregion

            #region format specifier
            // extract format
            formatSpecifier = Char.MinValue;
            if (m.Groups[6] != null && m.Groups[6].Value.Length > 0)
            {
               formatSpecifier = m.Groups[6].Value[0];
            }
            #endregion

            // default precision is 6 digits if none is specified except
            if (fieldPrecision == int.MinValue &&
                formatSpecifier != 's' &&
                formatSpecifier != 'c' &&
                Char.ToUpper(formatSpecifier) != 'X' &&
                formatSpecifier != 'o')
            {
               fieldPrecision = 6;
            }

            #region get next value parameter
            // get next value parameter and convert value parameter depending on short / long indicator
            if (inputParams == null || paramIx >= inputParams.Length)
            {
               o = null;
            }
            else
            {
               o = inputParams[paramIx];
               if (o is Byte[] inputData)
               {
                  o = inputData.Length switch
                  {
                     // Byte
                     1 => (UInt16)inputData[0],
                     // UInt16
                     2 => BitConverter.ToUInt16(inputData, 0),
                     // UInt32
                     4 => BitConverter.ToUInt32(inputData, 0),
                     // UInt64
                     8 => BitConverter.ToUInt64(inputData, 0),
                     _ => Encoding.ASCII.GetString(inputData),
                  };
               }

               if (shortLongIndicator == 'h')
               {
                  if (o is int)
                  {
                     o = (short)( (int)o );
                  }
                  else if (o is long)
                  {
                     o = (short)( (long)o );
                  }
                  else if (o is uint)
                  {
                     o = (ushort)( (uint)o );
                  }
                  else if (o is ulong)
                  {
                     o = (ushort)( (ulong)o );
                  }
                  else if (o is Int64)
                  {
                     o = (ushort)( (Int64)o );
                  }
                  else if (o is UInt64)
                  {
                     o = (ushort)( (UInt64)o );
                  }
                  else
                  {
                     throw new NotImplementedException();
                  }
               }
               else if (shortLongIndicator == 'l')
               {
                  if (o is short)
                  {
                     o = (long)( (short)o );
                  }
                  else if (o is int)
                  {
                     o = (long)( (int)o );
                  }
                  else if (o is ushort)
                  {
                     o = (ulong)( (ushort)o );
                  }
                  else if (o is uint)
                  {
                     o = (ulong)( (uint)o );
                  }
                  else if (o is Int64)
                  {
                     o = (Int64)o;
                  }
                  else if (o is UInt64)
                  {
                     o = (UInt64)o;
                  }
                  else
                  {
                     throw new NotImplementedException();
                  }
               }
            }
            #endregion

            // convert value parameters to a string depending on the formatSpecifier
            w = String.Empty;
            switch (formatSpecifier)
            {
               #region % - character
               case '%':   // % character
                  w = "%";
                  break;
               #endregion
               #region d - integer
               case 'd':   // integer
                  w = FormatNumber(( flagGroupThousands ? "n" : "d" ), flagAlternate,
                                  fieldLength, int.MinValue, flagLeft2Right,
                                  flagPositiveSign, flagPositiveSpace,
                                  paddingCharacter, o);
                  defaultParamIx++;
                  break;
               #endregion
               #region i - integer
               case 'i':   // integer
                  goto case 'd';
               #endregion
               #region o - octal integer
               case 'o':   // octal integer - no leading zero
                  w = FormatOct("o", flagAlternate,
                                  fieldLength, int.MinValue, flagLeft2Right,
                                  paddingCharacter, o);
                  defaultParamIx++;
                  break;
               #endregion
               #region x - hex integer
               case 'x':   // hex integer - no leading zero
                  w = FormatHex("x", flagAlternate,
                                  fieldLength, fieldPrecision, flagLeft2Right,
                                  paddingCharacter, o);
                  defaultParamIx++;
                  break;
               #endregion
               #region X - hex integer
               case 'X':   // same as x but with capital hex characters
                  w = FormatHex("X", flagAlternate,
                                  fieldLength, fieldPrecision, flagLeft2Right,
                                  paddingCharacter, o);
                  defaultParamIx++;
                  break;
               #endregion
               #region u - unsigned integer
               case 'u':   // unsigned integer
                  w = FormatNumber(( flagGroupThousands ? "n" : "d" ), flagAlternate,
                                  fieldLength, int.MinValue, flagLeft2Right,
                                  false, false,
                                  paddingCharacter, ToUnsigned(o));
                  defaultParamIx++;
                  break;
               #endregion
               #region c - character
               case 'c':   // character
                  if (IsNumericType(o))
                  {
                     w = Convert.ToChar(o).ToString();
                  }
                  else if (o is char)
                  {
                     w = ( (char)o ).ToString();
                  }
                  else if (o is string && ( (string)o ).Length > 0)
                  {
                     w = ( (string)o )[0].ToString();
                  }
                  defaultParamIx++;
                  break;
               #endregion
               #region s - string
               case 's':   // string
                  string t = "{0" + ( fieldLength != int.MinValue ? "," + ( flagLeft2Right ? "-" : String.Empty ) + fieldLength.ToString() : String.Empty ) + ":s}";
                  w = o != null ? o.ToString() : String.Empty;
                  if (fieldPrecision >= 0)
                  {
                     w = w.Substring(0, fieldPrecision);
                  }

                  if (fieldLength != int.MinValue)
                  {
                     if (flagLeft2Right)
                     {
                        w = w.PadRight(fieldLength, paddingCharacter);
                     }
                     else
                     {
                        w = w.PadLeft(fieldLength, paddingCharacter);
                     }
                  }
                  defaultParamIx++;
                  break;
               #endregion
               #region f - double number
               case 'f':   // double
                  w = FormatNumber(( flagGroupThousands ? "n" : "f" ), flagAlternate,
                                  fieldLength, fieldPrecision, flagLeft2Right,
                                  flagPositiveSign, flagPositiveSpace,
                                  paddingCharacter, o);
                  defaultParamIx++;
                  break;
               #endregion
               #region e - exponent number
               case 'e':   // double / exponent
                  w = FormatNumber("e", flagAlternate,
                                  fieldLength, fieldPrecision, flagLeft2Right,
                                  flagPositiveSign, flagPositiveSpace,
                                  paddingCharacter, o);
                  defaultParamIx++;
                  break;
               #endregion
               #region E - exponent number
               case 'E':   // double / exponent
                  w = FormatNumber("E", flagAlternate,
                                  fieldLength, fieldPrecision, flagLeft2Right,
                                  flagPositiveSign, flagPositiveSpace,
                                  paddingCharacter, o);
                  defaultParamIx++;
                  break;
               #endregion
               #region g - general number
               case 'g':   // double / exponent
                  w = FormatNumber("g", flagAlternate,
                                  fieldLength, fieldPrecision, flagLeft2Right,
                                  flagPositiveSign, flagPositiveSpace,
                                  paddingCharacter, o);
                  defaultParamIx++;
                  break;
               #endregion
               #region G - general number
               case 'G':   // double / exponent
                  w = FormatNumber("G", flagAlternate,
                                  fieldLength, fieldPrecision, flagLeft2Right,
                                  flagPositiveSign, flagPositiveSpace,
                                  paddingCharacter, o);
                  defaultParamIx++;
                  break;
               #endregion
               #region p - pointer
               case 'p':   // pointer
                  if (o is IntPtr)
                  {
                     w = "0x" + ( (IntPtr)o ).ToString("x");
                  }
                  defaultParamIx++;
                  break;
               #endregion
               #region n - number of processed chars so far
               case 'n':   // number of characters so far
                  w = FormatNumber("d", flagAlternate,
                                  fieldLength, int.MinValue, flagLeft2Right,
                                  flagPositiveSign, flagPositiveSpace,
                                  paddingCharacter, m.Index);
                  break;
               #endregion
               default:
                  w = String.Empty;
                  defaultParamIx++;
                  break;
            }

            // replace format parameter with parameter value
            // and start searching for the next format parameter
            // AFTER the position of the current inserted value
            // to prohibit recursive matches if the value also
            // includes a format specifier
            formatBuilder.Remove(m.Index, m.Length);
            formatBuilder.Insert(m.Index, w);
            m = ms_sPrintfExp.Match(formatBuilder.ToString(), m.Index + w.Length);
         }

         String retValAsString = formatBuilder.ToString();
         switch (inputFormat)
         {
            // TODO_CLG: Validate each case
            case "%x":
               return ( Int64.Parse(retValAsString, NumberStyles.HexNumber, CultureInfo.InvariantCulture) );

            case "%ld":
               return ( Convert.ToInt64(retValAsString, CultureInfo.InvariantCulture) );

            case "%ul":
               retValAsString = retValAsString.Trim(new Char[] { 'l', });
               return ( Convert.ToUInt64(retValAsString, CultureInfo.InvariantCulture) );

            case "%u":
               return ( Convert.ToUInt32(retValAsString, CultureInfo.InvariantCulture) );

            case "%0.5f":
            case "%.3f":
            case "%f":
               return ( Convert.ToSingle(retValAsString, CultureInfo.InvariantCulture) );

            case "%d":
               // TODO_CLG this code has issues since we do not know right here if this is a signed or unsigned int and conversion might fail for -1 0xFFFFFFFF
               //if("4294967295" == retValAsString)
               //{
               //    return ( -1 );
               //}
               //return ( Convert.ToInt32(retValAsString, CultureInfo.InvariantCulture) );
               try
               {
                  return (Convert.ToUInt32(retValAsString, CultureInfo.InvariantCulture));
               }
               catch( Exception ex)
               {
                  BCTLoggerService.GetLogger<DlogInputReaderSink>()
                        .WithError(String.Format(CultureInfo.InvariantCulture, "Exception '{0}' parsing value : {1} to UInt32",
                                                 ex.Message,
                                                 retValAsString))
                        .Log();

                  // TODO_CLG: This is not right, we have a formatting problem, maybe we have to "SKIP" this ?
                  return (null);
               }
            case "%s":
               return retValAsString;

            default:
               // TODO_CLG:
               return retValAsString;
         }

      }
      #endregion
      #endregion

      #region Private Methods
      #region FormatOCT
      private static string FormatOct(
          string nativeFormat,
          bool alternate,
          int fieldLength,
          int fieldPrecision,
          bool left2Right,
          char padding,
          object value)
      {
         string w = String.Empty;
         string lengthFormat = "{0" + ( fieldLength != int.MinValue ?
                                         "," + ( left2Right ?
                                                 "-" :
                                                 String.Empty ) + fieldLength.ToString() :
                                         String.Empty ) + "}";

         if (IsNumericType(value))
         {
            w = Convert.ToString(UnboxToLong(value, true), 8);

            if (left2Right || padding == ' ')
            {
               if (alternate && w != "0")
               {
                  w = "0" + w;
               }
               w = String.Format(lengthFormat, w);
            }
            else
            {
               if (fieldLength != int.MinValue)
               {
                  w = w.PadLeft(fieldLength - ( alternate && w != "0" ? 1 : 0 ), padding);
               }
               if (alternate && w != "0")
               {
                  w = "0" + w;
               }
            }
         }

         return w;
      }
      #endregion
      #region FormatHEX
      private static string FormatHex(
          string nativeFormat,
          bool alternate,
          int fieldLength,
          int fieldPrecision,
          bool left2Right,
          char padding,
          object value)
      {
         string w = String.Empty;
         string lengthFormat = "{0" + ( fieldLength != int.MinValue ?
                                         "," + ( left2Right ?
                                                 "-" :
                                                 String.Empty ) + fieldLength.ToString() :
                                         String.Empty ) + "}";
         string numberFormat = "{0:" + nativeFormat + ( fieldPrecision != int.MinValue ?
                                         fieldPrecision.ToString() :
                                         String.Empty ) + "}";

         if (IsNumericType(value))
         {
            w = String.Format(numberFormat, value);

            if (left2Right || padding == ' ')
            {
               if (alternate)
               {
                  w = ( nativeFormat == "x" ? "0x" : "0X" ) + w;
               }
               w = String.Format(lengthFormat, w);
            }
            else
            {
               if (fieldLength != int.MinValue)
               {
                  w = w.PadLeft(fieldLength - ( alternate ? 2 : 0 ), padding);
               }
               if (alternate)
               {
                  w = ( nativeFormat == "x" ? "0x" : "0X" ) + w;
               }
            }
         }

         return w;
      }
      #endregion
      #region FormatNumber
      private static string FormatNumber(
         string nativeFormat,
         bool alternate,
         int fieldLength, int fieldPrecision,
         bool left2Right,
         bool positiveSign,
         bool positiveSpace,
         char padding,
         object value)
      {
         string w = String.Empty;
         string lengthFormat = "{0" + ( fieldLength != int.MinValue ?
                                         "," + ( left2Right ?
                                                 "-" :
                                                 String.Empty ) + fieldLength.ToString() :
                                         String.Empty ) + "}";
         string numberFormat = "{0:" + nativeFormat + ( fieldPrecision != int.MinValue ?
                                         fieldPrecision.ToString() :
                                         "0" ) + "}";

         if (IsNumericType(value))
         {
            w = String.Format(numberFormat, value);

            if (left2Right || padding == ' ')
            {
               if (IsPositive(value, true))
               {
                  w = ( positiveSign ?
                      "+" : ( positiveSpace ? " " : String.Empty ) ) + w;
               }
               w = String.Format(lengthFormat, w);
            }
            else
            {
               if (w.StartsWith("-"))
               {
                  w = w.Substring(1);
               }
               if (fieldLength > 0 &&
                   fieldLength != int.MinValue)
               {
                  w = w.PadLeft(fieldLength - 1, padding);
               }

               if (IsPositive(value, true))
               {
                  w = ( positiveSign ?
                          "+" : ( positiveSpace ?
                                  " " : ( fieldLength != int.MinValue ?
                                          padding.ToString() : String.Empty ) ) ) + w;
               }
               else
               {
                  w = "-" + w;
               }
            }
         }

         return w;
      }
      #endregion
      #endregion
   }
}
