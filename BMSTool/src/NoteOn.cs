using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src
{
    public class NoteOn : Event
    {
        public byte Note; // The actual note value
        public byte Channel; // What channel the note plays on
        public byte Velocity; // Not sure, but it can be viewed in certain situations as per-note volume

        public NoteOn(EndianBinaryReader reader, FileTypes type)
        {
            if (type == FileTypes.BMS)
            {
                // BMS stores NoteOn as <Note Number><Channel ID + 1><Velocity>
                Note = reader.ReadByte();
                Channel = (byte)(reader.ReadByte() - 1);
                Velocity = reader.ReadByte();
            }
            else
            {
                // MIDI stores NoteOn as <0x90 | Channel ID><Note Number><Velocity>
                Channel = (byte)(reader.ReadByte() & 0x0F);
                Note = (byte)(reader.ReadByte() & 0x7F);
                Velocity = (byte)(reader.ReadByte() & 0x7F);
            }
        }

        public NoteOn(byte note, byte channel, byte velocity)
        {
            Note = note;
            Channel = channel;
            Velocity = velocity;
        }

        public override void WriteBMS(EndianBinaryWriter writer)
        {
            // BMS stores NoteOn as <Note Number><Channel ID + 1><Velocity>
            writer.Write(Note);
            writer.Write((byte)(Channel + 1)); // Channel IDs start at 1 for BMS because 0x80 is the wait command, while 0x81 is the first NoteOff command
            writer.Write(Velocity);
        }

        public override void WriteMIDI(EndianBinaryWriter writer)
        {
            // MIDI stores NoteOn as <0x90 | Channel ID><Note Number><Velocity>
            writer.Write((byte)(0x90 | Channel));
            writer.Write(Note);
            writer.Write(Velocity);
        }
    }
}
