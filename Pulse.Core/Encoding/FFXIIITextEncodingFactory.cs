using System;
using System.Collections.Generic;

namespace Pulse.Core
{
    public static class FFXIIITextEncodingFactory
    {
        public static FFXIIITextEncoding CreateEuro()
        {
            FFXIIICodePage codepage = FFXIIICodePageHelper.CreateEuro();
            return new(codepage);
        }

        public static FFXIIITextEncoding CreateCyrillic()
        {
            FFXIIICodePage codepage = FFXIIICodePageHelper.CreateCyrillic();
            return new(codepage);
        }

        public static readonly Lazy<FFXIIITextEncoding> DefaultEuroEncoding = new(CreateEuro);
    }
}