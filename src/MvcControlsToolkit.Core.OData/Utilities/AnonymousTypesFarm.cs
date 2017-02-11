using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.OData.Utilities
{
    public static class AnonymousTypesFarm
    {
        private static Type[] index = new Type[32];
        private static int[] totalProperties = new int[32];
        internal static int x =1;
        static AnonymousTypesFarm()
        {
            
            var o1 = new
            {
                Item1 = x,
                Item2 = x
            };
            
            var o2 = new
            {
                Item1 = 1,
                Item2 = 2,
                Item3 = 3,
                Item4 = 4
            };
            var o3 = new
            {
                Item1 = 1,
                Item2 = 2,
                Item3 = 3,
                Item4 = 4,
                Item5 = 5,
                Item6 = 6,
                Item7 = 7,
                Item8 = 8
            };
            var o4 = new
            {
                Item1 = 1,
                Item2 = 2,
                Item3 = 3,
                Item4 = 4,
                Item5 = 5,
                Item6 = 6,
                Item7 = 7,
                Item8 = 8,
                Item9 = 9,
                Item10 = 10,
                Item11 = 11,
                Item12 = 12,
                Item13 = 13,
                Item14 = 14,
                Item15 = 15,
                Item16 = 16
            };
            var o5 = new
            {
                Item1 = 1,
                Item2 = 2,
                Item3 = 3,
                Item4 = 4,
                Item5 = 5,
                Item6 = 6,
                Item7 = 7,
                Item8 = 8,
                Item9 = 9,
                Item10 = 10,
                Item11 = 11,
                Item12 = 12,
                Item13 = 13,
                Item14 = 14,
                Item15 = 15,
                Item16 = 16,
                Item17 = 17,
                Item18 = 18,
                Item19 = 19,
                Item20 = 20,
                Item21 = 21,
                Item22 = 22,
                Item23 = 23,
                Item24 = 24,
                Item25 = 25,
                Item26 = 26,
                Item27 = 27,
                Item28 = 28,
                Item29 = 29,
                Item30 = 30,
                Item31 = 31,
                Item32 = 32
            };
            var t1 = o1.GetType().GetGenericTypeDefinition();
            var t2 = o2.GetType().GetGenericTypeDefinition();
            var t3 = o3.GetType().GetGenericTypeDefinition();
            var t4 = o4.GetType().GetGenericTypeDefinition();
            var t5 = o5.GetType().GetGenericTypeDefinition();
            index[0] = t1;//1
            index[1] = t1;//2

            index[2] = t2;//3
            index[3] = t2;//4

            index[4] = t3;//5
            index[5] = t3;//6
            index[6] = t3;//7
            index[7] = t3;//8

            index[8] = t4;//9
            index[9] = t4;//10
            index[10] = t4;//11
            index[11] = t4;//12
            index[12] = t4;//13
            index[13] = t4;//14
            index[14] = t4;//15
            index[15] = t4;//16

            index[16] = t5;//17
            index[17] = t5;//18
            index[18] = t5;//19
            index[19] = t5;//20
            index[20] = t5;//21
            index[21] = t5;//22
            index[22] = t5;//23
            index[23] = t5;//24
            index[24] = t5;//25
            index[25] = t5;//26
            index[26] = t5;//27
            index[27] = t5;//28
            index[28] = t5;//29
            index[29] = t5;//30
            index[30] = t5;//31
            index[31] = t5;//32

            totalProperties[0] = 2;//1
            totalProperties[1] = 2;//2

            totalProperties[2] = 4;//3
            totalProperties[3] = 4;//4

            totalProperties[4] = 8;//5
            totalProperties[5] = 8;//6
            totalProperties[6] = 8;//7
            totalProperties[7] = 8;//8

            totalProperties[8] = 16;//9
            totalProperties[9] = 16;//10
            totalProperties[10] = 16;//11
            totalProperties[11] = 16;//12
            totalProperties[12] = 16;//13
            totalProperties[13] = 16;//14
            totalProperties[14] = 16;//15
            totalProperties[15] = 16;//16

            totalProperties[16] = 32;//17
            totalProperties[17] = 32;//18
            totalProperties[18] = 32;//19
            totalProperties[19] = 32;//20
            totalProperties[20] = 32;//21
            totalProperties[21] = 32;//22
            totalProperties[22] = 32;//23
            totalProperties[23] = 32;//24
            totalProperties[24] = 32;//25
            totalProperties[25] = 32;//26
            totalProperties[26] = 32;//27
            totalProperties[27] = 32;//28
            totalProperties[28] = 32;//29
            totalProperties[29] = 32;//30
            totalProperties[30] = 32;//31
            totalProperties[31] = 32;//32
        }
        public static PropertyInfo[]  GetProperties(Type[] args, out int n)
        {
            var t = index[args.Length-1];
            n = totalProperties[args.Length-1];
            var res = new PropertyInfo[args.Length];
            Type[] inst = args;
            if (n>args.Length)
            {
                inst = args.Concat(Enumerable.Repeat(typeof(int), n - args.Length)).ToArray();
            }
            var ft = t.MakeGenericType(inst);
            for(int i=0; i<args.Length; i++)
            {
                res[i] = ft.GetProperty("Item" + (i+1).ToString(CultureInfo.InvariantCulture), BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            }
            return res;
        }
    }
}
