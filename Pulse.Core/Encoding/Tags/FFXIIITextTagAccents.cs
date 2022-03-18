namespace Pulse.Core
{
    /// <summary>
    /// Use to add language accented letters.
    /// </summary>
    /// <example>
    /// Ztr 0x85A0 == {Accents AAcute} Strings
    /// Strings {Accents AAcute} ==  Ztr 0x85A0
    /// </example>
    public enum FFXIIITextTagAccents : byte
        {
            ACrase      = 0x9F, //À
            AAcute      = 0xA0, //Á
            ACircumflex = 0xA1, //Â
            ATil        = 0xA2, //Ã
            ADiaeresis  = 0xA3, //Ä
            Cedilha     = 0xA6, //Ç
            ECrase      = 0xA7, //È
            EAcute      = 0xA8, //É
            ECircumflex = 0xA9, //Ê
            ICrase      = 0xAB, //Ì
            IAcute      = 0xAC, //Í
            OCrase      = 0xB1, //Ò
            OAcute      = 0xB2, //Ó
            OCircumflex = 0xB3, //Ô
            OTilde      = 0xB4, //Õ
            //Multiplication = 0xB6, //avoid confuse
            UCrase      = 0xB8, //Ù
            UAcute      = 0xB9  //Ú
    }
}
