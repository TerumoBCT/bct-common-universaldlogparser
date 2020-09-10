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
   public static class ByteArray
   {
      /// <summary>
      /// 
      /// </summary>
      static readonly Int32[] Empty = new Int32[0];

      /// <summary>
      /// 
      /// </summary>
      /// <param name="self"></param>
      /// <param name="candidate"></param>
      /// <returns></returns>
      public static Int32[] Locate(
          this Byte[] self,
          Byte[] candidate)
      {
         if (IsEmptyLocate(self, candidate))
         {
            return (ByteArray.Empty);
         }

         var list = new List<Int32>();

         for (int i = 0; i < self.Length; i++)
         {
            if (!IsMatch(self, i, candidate))
            {
               continue;
            }

            list.Add(i);
         }

         return list.Count == 0 ? Empty : list.ToArray();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="self"></param>
      /// <param name="candidate"></param>
      /// <returns></returns>
      public static Int32 LocateFirst(
          this Byte[] self,
          Byte[] candidate)
      {
         if (!IsEmptyLocate(self, candidate))
         {


            for (Int32 i = 0; i < self.Length; i++)
            {
               if (ByteArray.IsMatch(self, i, candidate))
               {
                  return (i);
               }

            }
         }

         return -1;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="array"></param>
      /// <param name="position"></param>
      /// <param name="candidate"></param>
      /// <returns></returns>
      public static Boolean IsMatch(
          Byte[] array,
          Int32 position,
          Byte[] candidate)
      {
         if (candidate.Length > ( array.Length - position ))
         {
            return ( false );
         }

         for (Int32 i = 0; i < candidate.Length; ++i)
         {
            if (array[position + i] != candidate[i])
            {
               return ( false );
            }
         }

         return ( true );
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="array"></param>
      /// <param name="candidate"></param>
      /// <returns></returns>
      public static Boolean IsEmptyLocate(
          Byte[] array,
          Byte[] candidate)
      {
         return array == null
             || candidate == null
             || array.Length == 0
             || candidate.Length == 0
             || candidate.Length > array.Length;
      }
   }
}
