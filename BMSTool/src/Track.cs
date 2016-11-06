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
        static byte TrackNumberSource = 0;

        // MIDI needs to know the note number in order to turn off the note.
        // So, to ensure compatibility, we're going to store and remove notes as
        // they get turned on/off
        byte[] ChannelList = Enumerable.Repeat<byte>(0xFF, 16).ToArray();
        byte TrackNumber;
        public List<Event> Events;

        public Track(EndianBinaryReader reader, FileTypes type)
        {
            Events = new List<Event>();
            TrackNumber = TrackNumberSource++;

            if (type == FileTypes.BMS)
                ReadTrackBMS(reader);
            else
                ReadTrackMIDI(reader);
        }

        private void ReadTrackBMS(EndianBinaryReader reader)
        {
            byte opCode = reader.ReadByte();
            long curPos = 0; // This will be used for 0xC4 and 0xC6 commands to run sub-functions

            while (opCode != 0xFF)
            {
                // It's a note, notes are opcodes <= 0x7F
                if (opCode <= 0x7F)
                {
                    reader.BaseStream.Position--; // Back the stream up to get the right data
                    NoteOn noteOn = new NoteOn(reader, FileTypes.BMS);
                    Events.Add(noteOn);
                    ChannelList[noteOn.Channel] = noteOn.Note; // Add the note to the note channel list
                    noteOn.Channel = TrackNumber;
                }
                // This means note off
                else if (opCode >= 0x81 && opCode <= 0x87)
                {
                    reader.BaseStream.Position--; // Back the stream up to get the right data
                    NoteOff noteOff = new NoteOff(reader, FileTypes.BMS);
                    Events.Add(noteOff);

                    // Set the note being turned off and then remove it from the channel list
                    noteOff.Note = ChannelList[noteOff.Channel];
                    ChannelList[noteOff.Channel] = 255;
                    noteOff.Channel = TrackNumber;
                }
                else
                {
                    switch (opCode)
                    {
                        case 0x80:
                        case 0x88:
                            reader.BaseStream.Position--; // We need to re-read the opcode to determine which data type to read
                            Wait wait = new Wait(reader, FileTypes.BMS);
                            Events.Add(wait);
                            break;
                        case 0x98:
                            byte secondOpcode98 = reader.ReadByte();
                            if (secondOpcode98 == 0) // Volume
                            {
                                SetVolume volume = new SetVolume(reader, FileTypes.BMS);
                                Events.Add(volume);
                            }
                            else if (secondOpcode98 == 2)
                                reader.SkipByte();
                            else if (secondOpcode98 == 4)
                                reader.SkipByte();
                            else
                                reader.SkipByte();
                            break;
                        case 0x9A: // Panning
                            byte secondOpcode9a = reader.ReadByte();
                            if (secondOpcode9a == 3)
                                reader.SkipInt16();
                            else
                                reader.SkipInt16();
                            break;
                        case 0x9C:
                            byte secondOpcode9c = reader.ReadByte();
                            if (secondOpcode9c == 0)
                                reader.SkipInt16();
                            else if (secondOpcode9c == 1)
                                reader.SkipInt16();
                            else
                                reader.SkipInt16();
                            break;
                        case 0x9E:
                            reader.SkipInt32();
                            break;
                        case 0xA0:
                            reader.SkipInt16();
                            break;
                        //case 0xA1:
                            //reader.SkipInt16();
                            //break;
                        case 0xA4: // Meaning depends on secondOpcode
                            byte secondOpcodea4 = reader.ReadByte();
                            // Set instrument source bank
                            if (secondOpcodea4 == 0x20)
                            {
                                reader.SkipByte();
                            }
                            // Set instrument index
                            else if (secondOpcodea4 == 0x21)
                            {
                                reader.SkipByte();
                            }
                            // Unknown
                            else if (secondOpcodea4 == 7)
                            {
                                reader.SkipByte();
                            }
                            else if (secondOpcodea4 == 1)
                                reader.SkipByte();
                            else
                                reader.SkipByte();
                            break;
                        case 0xA5:
                            reader.SkipInt16();
                            break;
                        case 0xA6:
                            reader.SkipInt16();
                            break;
                        case 0xA7:
                            reader.SkipInt16();
                            break;
                        case 0xA9:
                            reader.SkipInt32();
                            break;
                        case 0xAA:
                            reader.SkipInt32();
                            break;
                        case 0xAC:
                            reader.Skip(3);
                            break;
                        case 0xAD:
                            reader.Skip(3);
                            break;
                        //case 0xAF:
                        //
                        //break;
                        case 0xB1:
                            byte secondOpcodeB1 = reader.ReadByte();
                            if (secondOpcodeB1 == 0x40)
                                reader.SkipInt16();
                            else
                                reader.SkipInt32();
                            break;
                        case 0xB8:
                            reader.SkipInt16();
                            break;
                        //case 0xC0:
                            //reader.SkipInt32();
                            //break;
                        case 0xC1:
                            reader.SkipInt32();
                            break;
                        case 0xC2:
                            reader.SkipByte();
                            break;
                        case 0xC4: // Subroutine time!
                            byte secondOpcodeC4 = reader.ReadByte();
                            if (secondOpcodeC4 == 0)
                            {
                                int offset = (int)reader.ReadBits(24);
                                curPos = reader.BaseStream.Position;
                                reader.BaseStream.Seek(offset, System.IO.SeekOrigin.Begin);
                            }
                            break;
                        case 0xC5:
                            reader.SkipInt32();
                            break;
                        case 0xC6: // Return from subroutine
                            reader.BaseStream.Seek(curPos, System.IO.SeekOrigin.Begin);
                            break;
                        case 0xC7:
                            reader.SkipInt32();
                            break;
                        case 0xC8: // Jump. Used to loop. Unsupported right now
                            reader.SkipInt32();
                            break;
                        case 0xCB:
                            reader.SkipInt16();
                            break;
                        case 0xCC:
                            reader.SkipInt16();
                            break;
                        case 0xCF:
                            reader.SkipByte();
                            break;
                        case 0xD0:
                            reader.SkipInt16();
                            break;
                        case 0xD1:
                            reader.SkipInt16();
                            break;
                        case 0xD2:
                            reader.SkipInt16();
                            break;
                        case 0xD5:
                            reader.SkipInt16();
                            break;
                        case 0xD6:
                            reader.SkipByte();
                            break;
                        case 0xD8:
                            reader.Skip(3);
                            break;
                        case 0xDA:
                            reader.SkipByte();
                            break;
                        case 0xDB:
                            reader.SkipByte();
                            break;
                        case 0xDD:
                            reader.Skip(3);
                            break;
                        case 0xDF:
                            reader.SkipInt32();
                            break;
                        case 0xE0:
                            reader.SkipInt16();
                            break;
                        case 0xE2:
                            reader.SkipByte();
                            break;
                        case 0xE3:
                            reader.SkipByte();
                            break;
                        case 0xE6: // Vibrato
                            reader.SkipInt16();
                            break;
                        case 0xE7: // SynGPU
                            reader.SkipInt16();
                            break;
                        case 0xEF:
                            reader.Skip(3);
                            break;
                        case 0xF0:
                            while ((reader.ReadByte() & 0x80) == 0x80)
                            {
                                // what
                            }
                            break;
                        case 0xF1: // ?
                            reader.SkipByte();
                            break;
                        case 0xF4: // Vib pitch
                            reader.SkipByte();
                            break;
                        case 0xF9:
                            reader.SkipInt16();
                            break;
                        case 0xFD: // Time base
                                   //SetTimeBase timeBase = new SetTimeBase(reader, FileTypes.BMS);
                                   //Events.Insert(0, timeBase);
                            SetTempo tempo = new SetTempo(reader, FileTypes.BMS);
                            Events.Add(tempo);
                            break;
                        case 0xFE: // Tempo
                                   //SetTempo tempo = new SetTempo(reader, FileTypes.BMS);
                                   //Events.Add(tempo);
                            SetTimeBase timeBase = new SetTimeBase(reader, FileTypes.BMS);
                            Events.Insert(0, timeBase);
                            break;
                        default:
                            throw new FormatException("Unknown opcode " + opCode + "!");
                    }
                }

                opCode = reader.ReadByte();
            }

            CombineWaitCommands();
        }

        private void ReadTrackMIDI(EndianBinaryReader reader)
        {
            if (reader.ReadStringUntil('\0') != "MTrk")
                throw new FormatException(string.Format("Invalid track at offset 0x{0:X8}!", reader.BaseStream.Position));

            reader.BaseStream.Position--;
            int trackSize = reader.ReadInt32();

            // We need to see if this track starts out with a delta-time command, which is anything other than 0
            if (reader.PeekReadByte() != 0)
            {
                Wait initialWait = new Wait(reader, FileTypes.MIDI);
                Events.Add(initialWait);
            }
            // Skip to first command byte
            else
            {
                reader.SkipByte();
            }

            byte primaryCommand = 0;
            byte secondaryCommand = 0;
            byte lastCommand = 0;

            // 0xFF 2F 00 is the command for end of track, so we'll keep reading until we hit it
            while (secondaryCommand != 0x2F)
            {
                primaryCommand = reader.ReadByte();

                // Note on
                if ((primaryCommand & 0x90) == 0x90 && primaryCommand < 0x9F)
                {
                    reader.BaseStream.Position--;
                    NoteOn noteOn = new NoteOn(reader, FileTypes.MIDI);
                    Events.Add(noteOn);
                }
                // Note off
                else if ((primaryCommand & 0x80) == 0x80 && primaryCommand < 0x9F)
                {
                    reader.BaseStream.Position--;
                    NoteOff noteOff = new NoteOff(reader, FileTypes.MIDI);
                    Events.Add(noteOff);
                }

                switch (primaryCommand)
                {
                    // For most of these, we just read the length byte and skip it
                    case 0xC0: // Set program/instrument
                        reader.SkipByte();
                        break;
                    case 0xC1:
                        reader.SkipByte();
                        break;
                    case 0xC9:
                        reader.SkipByte();
                        break;
                    case 0xB0:
                        reader.SkipInt16();
                        break;
                    case 0xB1:
                        reader.SkipInt16();
                        break;
                    case 0xB2:
                        reader.SkipInt16();
                        break;
                    case 0xB9:
                        reader.SkipInt16();
                        break;
                    case 0xFF:
                        secondaryCommand = reader.ReadByte();
                        switch(secondaryCommand)
                        {
                            case 0x01:
                            case 0x02:
                            case 0x03:
                            case 0x04:
                            case 0x58: // This sets time signature. We'll skip it for now unless it's needed later
                            case 0x7F:
                                byte length = reader.ReadByte();
                                reader.Skip(length);
                                break;
                            case 0x2F: // End the loop
                                if (reader.BaseStream.Position + 1 < reader.BaseStream.Length)
                                    reader.SkipByte();
                                continue;
                            case 0x51:
                                reader.SkipByte();
                                SetTempo tempo = new SetTempo(reader, FileTypes.MIDI);
                                Events.Add(tempo);
                                break;

                        }
                        break;
                }

                // Now that we've gotten the event data for this pass, we read in the delta-time for the next event
                if (reader.PeekReadByte() != 0)
                {
                    Wait wait = new Wait(reader, FileTypes.MIDI);
                    if (wait.WaitTime == 0x3C)
                    {

                    }
                    Events.Add(wait);
                }
                // Skip to the command byte, there's no delta-time
                else
                {
                    reader.SkipByte();
                }

                lastCommand = primaryCommand;
            }
        }

        public void WriteTrack(EndianBinaryWriter writer, FileTypes type)
        {
            if (type == FileTypes.BMS)
                WriteTrackBMS(writer);
            else
                WriteTrackMIDI(writer);
        }

        private void WriteTrackBMS(EndianBinaryWriter writer)
        {
            long curPos = writer.BaseStream.Position;
            writer.BaseStream.Seek(((TrackNumber - 1) * 5) + 2, System.IO.SeekOrigin.Begin);

            // Offset is stored as 24 bit value, so we'll write it out like one
            writer.Write((byte)((curPos & 0xFF0000) >> 16));
            writer.Write((byte)((curPos & 0x00FF00) >> 8));
            writer.Write((byte)(curPos & 0x0000FF));

            writer.BaseStream.Seek(curPos, System.IO.SeekOrigin.Begin);

            // SyncGPU, a given at the start of every track
            writer.Write((byte)0xE7);
            writer.Write((short)0);

            // Set instrument bank
            writer.Write((byte)0xA4);
            writer.Write((byte)0x20);
            writer.Write((byte)0);

            // Set program/instrument
            writer.Write((byte)0xA4);
            writer.Write((byte)0x21);
            writer.Write((byte)0);

            // Vib depth MIDI
            writer.Write((byte)0xE6);
            writer.Write((short)0);

            // I don't know what the next 3 do, but they're in most tracks, so I'll keep them for now
            writer.Write((byte)0x98);
            writer.Write((byte)2);
            writer.Write((byte)0);

            writer.Write((byte)0x98);
            writer.Write((byte)4);
            writer.Write((byte)0);

            writer.Write((byte)0x98);
            writer.Write((byte)2);
            writer.Write((byte)0);

            foreach (Event ev in Events)
                ev.WriteBMS(writer);

            writer.Write((byte)0xFF);
        }

        private void WriteTrackMIDI(EndianBinaryWriter writer)
        {
            if (TrackNumber == 8)
            {

            }
            writer.Write("MTrk".ToCharArray()); // Track header
            writer.Write((int)0); // Track size placeholder

            long trackStart = writer.BaseStream.Position; // We'll use this to calculate track size at the end

            // Track heading, just Track<number>
            string heading = string.Format("Track{0}", TrackNumber);
            writer.Write((byte)0);
            writer.Write((byte)0xFF);
            writer.Write((byte)3);
            writer.Write((byte)heading.Length);
            writer.Write(heading.ToCharArray());

            // Program change, sets instrument to 1, Bright Accoustic Piano
            writer.Write((byte)0);
            writer.Write((byte)0xC0);
            writer.Write((byte)((TrackNumber * 3) + 1));

            // Set initial volume
            writer.Write((byte)0);
            writer.Write((byte)0xB0);
            writer.Write((byte)0x7B);
            writer.Write((byte)0x00);

            // If there's no wait command to start off with, delta-time in the track has to be set to 0
            if (Events[0].GetType() != typeof(Wait))
                writer.Write((byte)0);

            for (int i = 0; i < Events.Count; i++)
            {
                if (i == 4)
                {

                }
                Events[i].WriteMIDI(writer);

                if ((Events[i].GetType() != typeof(Wait)) && ((Events[i].GetType() != typeof(SetTimeBase)) && (i < Events.Count - 1)))
                {
                    if (Events[i + 1].GetType() != typeof(Wait))
                        writer.Write((byte)0);
                }
            }

            // End of track
            // We check if the last event is Wait. If it isn't, we need to put in a delta-time value of 0
            if (Events.Last().GetType() != typeof(Wait))
                writer.Write((byte)0);

            writer.Write((byte)0xFF);
            writer.Write((byte)0x2F);
            writer.Write((byte)0);

            int trackSize = (int)(writer.BaseStream.Position - trackStart);

            // Go to track size, write it, then return to the end of the stream
            writer.BaseStream.Position = trackStart - 4;
            writer.Write(trackSize);
            writer.BaseStream.Seek(0, System.IO.SeekOrigin.End);
        }

        private void CombineWaitCommands()
        {
            // This will make sure that there is only 1 wait command between each event, like MIDI

            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i].GetType() == typeof(Wait))
                {
                    if ((i + 1) == Events.Count)
                        break;

                    else if (Events[i + 1].GetType() == typeof(Wait))
                    {
                        Wait wait = Events[i] as Wait;
                        Wait delete = Events[i + 1] as Wait;
                        wait.WaitTime += delete.WaitTime;

                        Events.RemoveAt(i + 1);

                        i--;
                    }
                }
            }
        }
    }
}
