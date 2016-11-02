using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src
{
    public class NoteOff : Event
    {
        public byte Note;
        public byte Channel;
        public byte Velocity;

        public NoteOff(EndianBinaryReader reader, FileTypes type)
        {
            if (type == FileTypes.BMS)
            {
                // BMS stores NoteOff as <0x80 | (Channel ID + 1)>
                Channel = (byte)((reader.ReadByte() & 0x0F) - 1);
            }
            else
            {
                // MIDI stores NoteOff as <0x80 | Channel ID><Note Number><Velocity>
                Channel = (byte)(reader.ReadByte() & 0x0F);
                Note = (byte)(reader.ReadByte() & 0x7F);
                Velocity = (byte)(reader.ReadByte() & 0x7F);
            }
        }

        public override void WriteBMS(EndianBinaryWriter writer)
        {
            // BMS stores NoteOff as <0x80 | (Channel ID + 1)>
            byte noteOff = (byte)(0x80 | (Channel + 1));
            writer.Write(noteOff);
        }

        public override void WriteMIDI(EndianBinaryWriter writer)
        {
            // MIDI stores NoteOff as <0x80 | Channel ID><Note Number><Velocity>
            // MIDI needs to know the note number to turn off the note, unlike BMS, so Note will be set from outside the class
            writer.Write((byte)(0x80 | Channel));
            writer.Write(Note);
            writer.Write(Velocity);
        }
    }
}
