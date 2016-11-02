using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src
{
    public class Track
    {
        // MIDI needs to know the note number in order to turn off the note.
        // So, to ensure compatibility, we're going to store and remove notes as
        // they get turned on/off
        byte[] ChannelList = Enumerable.Repeat<byte>(0xFF, 16).ToArray();
        byte TrackNumber;
        List<Event> Events;

        public Track(EndianBinaryReader reader, FileTypes type)
        {
            Events = new List<Event>();

            if (type == FileTypes.BMS)
                ReadTrackBMS(reader);
            else
                ReadTrackMIDI(reader);
        }

        private void ReadTrackBMS(EndianBinaryReader reader)
        {
            reader.SkipByte(); // Skip open track opcode
            TrackNumber = reader.PeekReadByte();

            int trackOffset = reader.ReadInt32() & 0x00FFFFFF; // Get 24 bits that hold the offset
            long nextOpcode = reader.BaseStream.Position; // Save position, since we're jumping to trackOffset
            reader.BaseStream.Seek(trackOffset, System.IO.SeekOrigin.Begin); // Jump to beginning of track data

            byte opCode = reader.ReadByte();
            while (opCode != 0xFF)
            {
                // It's a note, notes are opcodes <= 0x7F
                if (opCode <= 0x7F)
                {
                    NoteOn noteOn = new NoteOn(reader, FileTypes.BMS);
                    Events.Add(noteOn);
                    ChannelList[noteOn.Channel] = noteOn.Note; // Add the note to the note channel list
                }
                opCode = reader.ReadByte();
            }

            reader.BaseStream.Position = nextOpcode; // Restore position to continue reading the file
        }

        private void ReadTrackMIDI(EndianBinaryReader reader)
        {

        }

        public void WriteTrack(EndianBinaryWriter writer)
        {

        }
    }
}
