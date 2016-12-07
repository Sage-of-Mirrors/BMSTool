using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src.Events
{
    class SetTempo : Event
    {
        ushort Tempo;

        public SetTempo()
        {
            Tempo = 125;
        }

        public void ReadBMS(EndianBinaryReader reader)
        {
            Tempo = reader.ReadUInt16();
        }

        public void ReadMIDI(EndianBinaryReader reader)
        {
            uint tempoVal = reader.ReadBits(24);
            Tempo = (ushort)(60000000 / tempoVal); // Tempo is stored in microseconds, so we have to convert that to Beats per Minute
        }

        public override void WriteBMS(EndianBinaryWriter writer)
        {
            writer.Write((byte)0xFD);
            writer.Write((byte)0);
            writer.Write((byte)Tempo);
        }

        public override void WriteMIDI(EndianBinaryWriter writer)
        {
            writer.Write((byte)0xFF);
            writer.Write((byte)0x51);
            writer.Write((byte)0x03);

            int convertedTempo = 60000000 / Tempo; // Convert from BPM to microseconds/quater note

            // MIDI stores the tempo in a 24 bit value. This converts it to that format
            // by breaking it up into its component bytes
            byte[] Bit24TempoVal = new byte[3];

            Bit24TempoVal[2] = (byte)(convertedTempo & 0x000000FF);
            Bit24TempoVal[1] = (byte)((convertedTempo & 0x0000FF00) >> 8);
            Bit24TempoVal[0] = (byte)((convertedTempo & 0x00FF0000) >> 16);

            writer.Write(Bit24TempoVal);
        }
    }
}
