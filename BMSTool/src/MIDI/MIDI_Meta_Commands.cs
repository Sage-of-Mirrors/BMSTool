using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMSTool.src.MIDI
{
    enum MIDI_Meta_Commands
    {
        Sequence_Number = 0,
        Text_Event = 1,
        Copyright_Notice = 2,
        Sequence_Track_Name = 3,
        Instrument_Name = 4,
        Lyric = 5,
        Marker = 6,
        Cue_Point = 7,
        MIDI_Channel_Prefix = 0x20,
        End_of_Track = 0x2F,
        Set_Tempo = 0x51,
        SMTPE_Offset = 0x54,
        Time_Signature = 0x58,
        Key_Signature = 0x59,
        Sequencer_Meta_Event = 0x7F
    }
}
