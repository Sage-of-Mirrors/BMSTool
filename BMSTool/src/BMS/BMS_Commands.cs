using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMSTool.src.BMS
{
    enum BMS_Command
    {
        Wait_Byte = 0x80,
        Wait_Short = 0x88,

        Volume_Plus = 0x98, // Volume plus other functions depending on second byte
        Panning_Plus = 0x9A, // Panning plus other functions depending on second byte

        NineC = 0x9C, // Unknown
        NineE = 0x9E, // Unknown
        AZero = 0xA0, // Unknown
        AOne = 0xA1, // Unknown

        Set_Instrument = 0xA4, // Sets instrument bank or sample based on second byte

        AFive = 0xA5, // Unknown
        ASix = 0xA6, // Unknown
        ASeven = 0xA7, // Unknown
        ANine = 0xA9, // Unknown
        AA = 0xAA, // Unknown
        AC = 0xAC, // Unknown
        AD = 0xAD, // Unknown
        AF = 0xAF, // Unknown
        BOne = 0xB1, // Unknown
        BEight = 0xB8, // Unknown
        BNine = 0xB9,
        CZero = 0xC0, // Unknown
        
        Open_Track = 0xC1,

        CTwo = 0xC2, // Unknown
        CThree = 0xC3,

        Subroutine_Jump = 0xC4, // Jumps to a block of event data

        CFive = 0xC5,

        Subroutine_Return = 0xC6,

        CSeven = 0xC7, // Unknown

        Loop_Jump = 0xC8, // Jumps to the specified offset to loop the sequence

        CB = 0xCB, // Unknown
        CC = 0xCC, // Unknown
        CF = 0xCF, // Unknown
        DZero = 0xD0, // Unknown
        DOne = 0xD1, // Unknown
        DTwo = 0xD2, // Unknown
        DFive = 0xD5, // Unknown
        DSix = 0xD6, // Unknown
        DEight = 0xD8, // Unknown
        DA = 0xDA, // Unknown
        DB = 0xDB, // Unknown
        DD = 0xDD, // Unknown
        DF = 0xDF, // Unknown
        EZero = 0xE0, // Unknown
        ETwo = 0xE2, // Unknown
        EThree = 0xE3, // Unknown
        Vibrato = 0xE6, // Unknown
        SyncGPU = 0xE7, // Unknown
        EA = 0xEA,
        EB = 0xEB,
        EF = 0xEF, // Unknown

        Wait_Variable_Length = 0xF0, // Seems to be an implementation of MIDI's variable length delta time. Used in TP but not earlier games

        FOne = 0xF1, // Unknown
        FThree = 0xF3,
        FSix = 0xF6,

        VibratoPitch = 0xF4,

        FNine = 0xF9, // Unknown

        Set_Time_Base = 0xFD,
        Set_Tempo = 0xFE,
        Terminate = 0xFF
    }
}
