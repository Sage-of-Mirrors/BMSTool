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

        public MIDI()
        {
            Tracks = new List<Track>();
            runningStatusCommand = 0;
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
                GetNoteOff(reader, track, (byte)(command | 0x0F));
            }
            // Note on
            else if (command >= 0x90 && command <= 0x9F)
            {
                GetNoteOn(reader, track, (byte)(command | 0x0F));
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
                        break;
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
                        reader.Skip(metaLength);
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
            track.Events.Add(off);
        }

        private void GetNoteOn(EndianBinaryReader reader, Track track, byte channel)
        {
            NoteOn on = new NoteOn();
            on.ReadMIDI(reader, channel);
            track.Events.Add(on);
        }

        public void WriteBMS(EndianBinaryWriter writer)
        {

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
