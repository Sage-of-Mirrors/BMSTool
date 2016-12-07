using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMSTool.src.MIDI
{
    enum MIDI_System_Commands
    {
        // System Common
        System_Exclusive = 0xF0,
        Undefined_1 = 0xF1,
        Song_Position_Pointer = 0xF2,
        Song_Select = 0xF3,
        Undefined_4 = 0xF4,
        Undefined_5 = 0xF5,
        Tune_Request = 0xF6,
        End_of_Exclusive = 0xF7,

        // System Real-Time
        Timing_Clock = 0xF8,
        Undefined_9 = 0xF9,
        Start = 0xFA,
        Continue = 0xFB,
        Stop = 0xFC,
        Undefined_D = 0xFD,
        Active_Sensing = 0xFE

        // 0xFF as a system real-time command means "reset."
        // However, in a MIDI file it's used to introduce
        // meta events.
        //Reset = 0xFF
    }
}
