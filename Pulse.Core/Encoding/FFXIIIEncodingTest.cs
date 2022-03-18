using System;
using System.Collections.Generic;

namespace Pulse.Core
{
    public sealed class FFXIIIEncodingTest
    {
        public static void Test()
        {
            HashSet<int> values = new HashSet<int>();
            HashSet<int> indices = new HashSet<int>();
            foreach (var h in new[] {0x81, 0x85})
            {
                for (int l = 0; l < 256; l++)
                {
                    if (l == 0x80)
                        continue;
                    
                    int index = FFXIIIEncodingMap.ValueToIndex(h,l);
                    if (!indices.Add(index))
                        throw new NotImplementedException();

                    //int hight;
                    //int low;
                    //FFXIIITextEncoder.IndexToValue(index, out hight, out low);
                    //int value = (hight << 8) | low;
                    //
                    //if (h != hight || l != low)
                    //    throw new NotImplementedException();
                    //
                    //if (!values.Add(value))
                    //    throw new NotImplementedException();
                }
            }
        }
    }
}