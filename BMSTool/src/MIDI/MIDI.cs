using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameFormatReader.Common;
using BMSTool.src.Events;

namespace BMSTool.src.MIDI
{
    class MIDI
    {
        List<Track> Tracks;
        short timeBase;

        // MIDI can send a command, and then send data for that command
        // multiple times without explicitly declaring it. That's called
        // "running status." So we'll keep track of each command in case
        // running status occurs
        short runningStatusCommand;

        byte[] Channels;
        byte ChannelIndex;

        public MIDI()
        {
            Tracks = new List<Track>();
            runningStatusCommand = 0;
            Channels = Enumerable.Repeat<byte>(0xFF, 16).ToArray();
            ChannelIndex = 0;
        }

        public void ReadMIDI(EndianBinaryReader reader)
        {
            reader.SkipInt64();
            short midiType = reader.ReadInt16();

            if (midiType != 1)
                throw new FormatException("Midi wasn't type 1! This program only supports type 1 MIDIs.");

            short numTracks = reader.ReadInt16();
            timeBase = reader.ReadInt16();

            for (int i = 0; i < numTracks; i++)
            {
                Track track = new Track();
                ReadMIDITrack(reader, track);
                Tracks.Add(track);
            }
        }

        private void ReadMIDITrack(EndianBinaryReader reader, Track track)
        {
            Channels = Enumerable.Repeat<byte>(0xFF, 16).ToArray();

            string trackTag = new string(reader.ReadChars(4));
            if (trackTag != "MTrk")
                throw new FormatException(string.Format("Track tag was incorrect at {0:x}!", reader.BaseStream.Position - 4));

            int trackSize = reader.ReadInt32();
            int trackEnd = (int)reader.BaseStream.Position + trackSize;

            while (reader.BaseStream.Position < trackEnd)
            {
                GetDeltaTime(reader, track);
                ParseMIDICommand(reader, track);
            }
        }

        private void GetDeltaTime(EndianBinaryReader reader, Track track)
        {
            Wait wait = new Wait();
            wait.ReadMIDI(reader);
            if (wait.WaitTime == 0)
                return;
            else
                track.Events.Add(wait);
        }

        private void ParseMIDICommand(EndianBinaryReader reader, Track track)
        {
            byte command = reader.ReadByte();

            bool runningStatus = !((command & 0x80) == 0x80);
            
            // Running status is active. The last command that we parsed is sending new data, so we just
            // act as if we just read that command.
            if (runningStatus)
            {
                command = (byte)runningStatusCommand;
                reader.BaseStream.Position--;
            }
            else
                runningStatusCommand = command;

            // Note off
            if (command >= 0x80 && command <= 0x8F)
            {
                GetNoteOff(reader, track, (byte)(command & 0x0F));
            }
            // Note on
            else if (command >= 0x90 && command <= 0x9F)
            {
                GetNoteOn(reader, track, (byte)(command & 0x0F));
            }
            // Polyphonic key pressure
            else if (command >= 0xA0 && command <= 0xAF)
            {
                reader.SkipInt16();
            }
            // Control change
            else if (command >= 0xB0 && command <= 0xBF)
            {
                reader.SkipInt16();
            }
            // Program change
            else if (command >= 0xC0 && command <= 0xCF)
            {
                reader.SkipByte();
            }
            // Channel pressure
            else if (command >= 0xD0 && command <= 0xDF)
            {
                reader.SkipByte();
            }
            // Pitch wheel change
            else if (command >= 0xE0 && command <= 0xEF)
            {
                reader.SkipInt16();
            }
            // System messages
            else if (command >= 0xF0 && command <= 0xFE)
            {
                switch ((MIDI_System_Commands)command)
                {
                    case MIDI_System_Commands.Song_Select:
                        reader.SkipByte();
                        break;
                    case MIDI_System_Commands.Song_Position_Pointer:
                        reader.SkipInt16();
                        break;
                    case MIDI_System_Commands.System_Exclusive:
                        int length = GetVariableLength(reader);
                        reader.Skip(length);
                        break;
                    default:
                        break;
                }
            }
            // Meta events. Only some of them are relavent to BMS conversion
            else if (command == 0xFF)
            {
                MIDI_Meta_Commands metaType = (MIDI_Meta_Commands)reader.ReadByte();
                int metaLength = GetVariableLength(reader);

                switch (metaType)
                {
                    case MIDI_Meta_Commands.Set_Tempo:
                        SetTempo tempo = new SetTempo();
                        tempo.ReadMIDI(reader);
                        track.Events.Add(tempo);
                        break;
                    case MIDI_Meta_Commands.End_of_Track:
                        return;
                    case MIDI_Meta_Commands.Copyright_Notice:
                    case MIDI_Meta_Commands.Cue_Point:
                    case MIDI_Meta_Commands.Instrument_Name:
                    case MIDI_Meta_Commands.Key_Signature:
                    case MIDI_Meta_Commands.Lyric:
                    case MIDI_Meta_Commands.Marker:
                    case MIDI_Meta_Commands.MIDI_Channel_Prefix:
                    case MIDI_Meta_Commands.Sequencer_Meta_Event:
                    case MIDI_Meta_Commands.Sequence_Number:
                    case MIDI_Meta_Commands.Sequence_Track_Name:
                    case MIDI_Meta_Commands.SMTPE_Offset:
                    case MIDI_Meta_Commands.Text_Event:
                    case MIDI_Meta_Commands.Time_Signature:
                    case MIDI_Meta_Commands.Prefix_Port:
                        if (reader.BaseStream.Position + metaLength != reader.BaseStream.Length)
                            reader.Skip(metaLength);
                        else
                            reader.BaseStream.Seek(0, System.IO.SeekOrigin.End);
                        break;
                    default:
                        throw new FormatException(string.Format("Unknown meta event {0:x}!", metaType));
                }
            }
        }

        private void GetNoteOff(EndianBinaryReader reader, Track track, byte channel)
        {
            NoteOff off = new NoteOff();
            off.ReadMIDI(reader, channel);

            int notesChannel = 0;
            while (Channels[notesChannel] != off.Note)
                notesChannel++;
            Channels[notesChannel] = 0xFF;
            off.Channel = (byte)notesChannel;

            track.Events.Add(off);
        }

        private void GetNoteOn(EndianBinaryReader reader, Track track, byte channel)
        {
            NoteOn on = new NoteOn();
            on.ReadMIDI(reader, channel);

            if (on.Velocity == 0)
            {
                NoteOff newOff = new NoteOff();
                newOff.Note = on.Note;

                int notesChannel = 0;
                while (Channels[notesChannel] != newOff.Note)
                    notesChannel++;
                Channels[notesChannel] = 0xFF;
                newOff.Channel = (byte)notesChannel;

                track.Events.Add(newOff);
            }
            else
            {
                int freeChannel = 0;
                for (int i = 0; i < 16; i++)
                {
                    if (Channels[i] == 0xFF)
                    {
                        freeChannel = i;
                        break;
                    }
                }

                Channels[freeChannel] = on.Note;
                on.Channel = (byte)freeChannel;
                track.Events.Add(on);
            }
        }

        public void WriteBMS(EndianBinaryWriter writer)
        {
            Track masterTrack = Tracks[0];
            Tracks.RemoveAt(0);

            WriteMasterTrack(writer, masterTrack);

            foreach (Track track in Tracks)
                WriteTrack(writer, track);
        }

        private void WriteMasterTrack(EndianBinaryWriter writer, Track master)
        {
            foreach (Track track in Tracks)
            {
                writer.Write((byte)0xC1);
                writer.Write((byte)(track.TrackNumber - 1));
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);
            }

            writer.Write((byte)0xFE);
            writer.Write(timeBase);

            foreach (Event ev in master.Events)
                ev.WriteBMS(writer);

            long pos = writer.BaseStream.Position;
            Wait wait = new Wait();
            wait.WaitTime = 0xFFFF;
            wait.WriteBMS(writer);

            // Hack for keeping the song going indefinitely
            writer.Write((byte)0xC8);
            writer.Write((int)pos);

            writer.Write((byte)0xFF);
        }

        private void WriteTrack(EndianBinaryWriter writer, Track track)
        {
            long curPos = writer.BaseStream.Position;
            writer.Seek((track.TrackNumber - 1) * 5 + 2, System.IO.SeekOrigin.Begin);

            writer.Write((byte)((curPos & 0xFF0000) >> 16));
            writer.Write((byte)((curPos & 0x00FF00) >> 8));
            writer.Write((byte)((curPos & 0x0000FF) >> 0));

            writer.BaseStream.Seek(curPos, System.IO.SeekOrigin.Begin);

            // SyncGPU
            writer.Write((byte)0xE7);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);

            // Set instrument bank
            writer.Write((byte)0xA4);
            writer.Write((byte)0x20);
            writer.Write((byte)0x00);

            // Set instrument sample
            writer.Write((byte)0xA4);
            writer.Write((byte)0x21);
            writer.Write((byte)(track.TrackNumber + 2));
            //writer.Write((byte)0x00);

            // Set volume
            writer.Write((byte)0x98);
            writer.Write((byte)0);
            writer.Write((byte)0x40);

            foreach (Event ev in track.Events)
                ev.WriteBMS(writer);

            writer.Write((byte)0xFF);
        }

        private int GetVariableLength(EndianBinaryReader reader)
        {
            uint length = 0;
            byte[] inputArray = new byte[4];
            int inputIndex = 1;
            byte testByte = reader.ReadByte();
            inputArray[0] = testByte;

            // Read in the data
            while (testByte >= 0x80)
            {
                testByte = reader.ReadByte();
                inputArray[inputIndex] = testByte;
                inputIndex++;
            }

            // Get the actual value
            for (int i = 0; i < inputIndex; i++)
            {
                length = length << 7;
                length |= (uint)(inputArray[i] & 0x7F);
            }

            return (int)length;
        }
    }
}
