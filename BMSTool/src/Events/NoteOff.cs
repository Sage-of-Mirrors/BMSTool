using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src.Events
{
    class NoteOff : Event
    {
        public byte Note; // The actual note value
        public byte Channel; // What channel the note plays on
        public byte Velocity; // Not sure, but it can be viewed in certain situations as per-note volume

        public NoteOff()
        {
            Note = 0x3C; // Middle C
            Channel = 0; // Channel 0
            Velocity = 0x40; // Half volume
        }

        /// <summary>
        /// Reads a NoteOff event from the specified BMS file.
        /// </summary>
        /// <param name="reader">BMS stream to read from</param>
        public void ReadBMS(EndianBinaryReader reader)
        {
            reader.BaseStream.Position--;

            // BMS stores NoteOff as <0x80 | (Channel ID + 1)>
            Channel = (byte)((reader.ReadByte() & 0x0F) - 1);
        }

        /// <summary>
        /// Reads a NoteOff event from the specified MIDI file.
        /// </summary>
        /// <param name="reader">MIDI stream to read from</param>
        public void ReadMIDI(EndianBinaryReader reader)
        {
            // MIDI stores NoteOff as <0x80 | Channel ID><Note Number><Velocity>
            Channel = (byte)(reader.ReadByte() & 0x0F);
            Note = (byte)(reader.ReadByte() & 0x7F);
            Velocity = (byte)(reader.ReadByte() & 0x7F);
        }

        /// <summary>
        /// Writes a NoteOff event to the specified stream in BMS format.
        /// </summary>
        /// <param name="writer">Stream to write to</param>
        public override void WriteBMS(EndianBinaryWriter writer)
        {
            // MIDI stores NoteOff as <0x80 | Channel ID><Note Number><Velocity>
            // MIDI needs to know the note number to turn off the note, unlike BMS, so Note will be set from outside the class
            writer.Write((byte)(0x80 | Channel));
            writer.Write(Note);
            writer.Write(Velocity);
        }

        /// <summary>
        /// Writes a NoteOff event to the specified stream in MIDI format.
        /// </summary>
        /// <param name="writer">Stream to write to</param>
        public override void WriteMIDI(EndianBinaryWriter writer)
        {
            // BMS stores NoteOff as <0x80 | (Channel ID + 1)>
            byte noteOff = (byte)(0x80 | (Channel + 1));
            writer.Write(noteOff);
        }
    }
}
