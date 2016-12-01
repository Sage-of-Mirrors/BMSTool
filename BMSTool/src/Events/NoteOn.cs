using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src.Events
{
    class NoteOn : Event
    {
        public byte Note; // The actual note value
        public byte Channel; // What channel the note plays on
        public byte Velocity; // Not sure, but it can be viewed in certain situations as per-note volume

        public NoteOn()
        {
            Note = 0x3C; // Middle C
            Channel = 0; // Channel 0
            Velocity = 0x40; // Half volume
        }

        /// <summary>
        /// Reads a NoteOn event from the specified BMS file.
        /// </summary>
        /// <param name="reader">BMS stream to read from</param>
        public void ReadBMS(EndianBinaryReader reader)
        {
            reader.BaseStream.Position--;

            // BMS stores NoteOn as <Note Number><Channel ID + 1><Velocity>
            Note = reader.ReadByte();
            Channel = (byte)(reader.ReadByte() - 1);
            Velocity = reader.ReadByte();
        }

        /// <summary>
        /// Reads a NoteOn event from the specified MIDI file.
        /// </summary>
        /// <param name="reader">MIDI stream to read from</param>
        public void ReadMIDI(EndianBinaryReader reader)
        {
            // MIDI stores NoteOn as <0x90 | Channel ID><Note Number><Velocity>
            Channel = (byte)(reader.ReadByte() & 0x0F);
            Note = (byte)(reader.ReadByte() & 0x7F);
            Velocity = (byte)(reader.ReadByte() & 0x7F);
        }

        /// <summary>
        /// Writes a NoteOn event to the specified stream in BMS format.
        /// </summary>
        /// <param name="writer">Stream to write to</param>
        public override void WriteBMS(EndianBinaryWriter writer)
        {
            // BMS stores NoteOn as <Note Number><Channel ID + 1><Velocity>
            writer.Write(Note);
            writer.Write((byte)(Channel + 1)); // Channel IDs start at 1 for BMS because 0x80 is the wait command, while 0x81 is the first NoteOff command
            writer.Write(Velocity);
        }

        /// <summary>
        /// Writes a NoteOn event to the specified stream in MIDI format.
        /// </summary>
        /// <param name="writer">Stream to write to</param>
        public override void WriteMIDI(EndianBinaryWriter writer)
        {
            // MIDI stores NoteOn as <0x90 | Channel ID><Note Number><Velocity>
            writer.Write((byte)(0x90 | Channel));
            writer.Write(Note);
            writer.Write(Velocity);
        }
    }
}
