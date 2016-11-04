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
        List<Event> Events;

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
            if (reader.BaseStream.Position == 0x63)
            {

            }
            byte opCode = reader.ReadByte();
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
                        case 0xA1:
                            reader.SkipInt16();
                            break;
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
                            reader.SkipByte();
                            reader.SkipInt16();
                            break;
                        case 0xAD:
                            reader.SkipByte();
                            reader.SkipInt16();
                            break;
                        case 0xAF:

                            break;
                        case 0xC0:
                            reader.SkipInt32();
                            break;
                        case 0xC1:
                            reader.SkipInt32();
                            break;
                        case 0xC4:
                            reader.SkipInt32();
                            break;
                        case 0xC7:
                            reader.SkipInt32();
                            break;
                        case 0xC8: // Jump. Used to loop. Unsupported right now
                            reader.SkipByte();
                            reader.SkipByte();
                            reader.SkipInt16();
                            break;
                        case 0xCB:
                            reader.SkipInt16();
                            break;
                        case 0xCC:
                            reader.SkipInt16();
                            break;
                        case 0xD2:
                            reader.SkipInt16();
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
                            reader.SkipInt16();
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
                            byte secondOpcodeFE = reader.ReadByte();
                            if (secondOpcodeFE == 0)
                            {
                                SetTimeBase timeBase = new SetTimeBase(reader, FileTypes.BMS);
                                Events.Insert(0, timeBase);
                            }
                            break;
                        case 0xFE: // Tempo
                            byte secondOpcodeFD = reader.ReadByte();
                            if (secondOpcodeFD == 0)
                            {
                                SetTempo tempo = new SetTempo(reader, FileTypes.BMS);
                                Events.Add(tempo);
                            }
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
