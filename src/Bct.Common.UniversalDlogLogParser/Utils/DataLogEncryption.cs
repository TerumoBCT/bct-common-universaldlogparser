using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bct.Common.LogParsing.Utils
{
   /// <summary>
   /// 
   /// </summary>
   public sealed class DataLogEncryption
   {
      private static Byte ms_encryptor = 0xA5;

      /// <summary>
      /// TODO_PERFORMANCE: This method gets called a lot, make sure it is as optimized as possible for performance
      /// </summary>
      /// <param name="input"></param>
      /// <param name="count"></param>
      /// <returns></returns>
      public static Byte[] DefaultDecryptFunc(
          Byte[] input,
          Int32 count)
      {
         Byte[] output = null;
         if (input != null
             && input.Length > 0
             && count > 0)
         {
            Int32 countToDecrypt = ( count > input.Length ? input.Length : count );
            output = new Byte[countToDecrypt];
            for (Int32 i = 0; i < countToDecrypt; ++i)
            {
               output[i] = (Byte)( input[i] ^ ms_encryptor );
            }
         }

         return ( output );
      }
   }
}
