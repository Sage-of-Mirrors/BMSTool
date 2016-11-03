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
                            break;
                        case 0x9A: // Panning
                            byte secondOpcode9a = reader.ReadByte();
                            if (secondOpcode9a == 3)
                                reader.SkipInt16();
                            break;
                        case 0x9C:
                            byte secondOpcode9c = reader.ReadByte();
                            if (secondOpcode9c == 0)
                                reader.SkipInt16();
                            if (secondOpcode9c == 1)
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
                            break;
                        case 0xC8: // Jump. Used to loop. Unsupported right now
                            reader.SkipByte();
                            reader.SkipByte();
                            reader.SkipInt16();
                            break;
                        case 0xE6: // Vibrato
                            reader.SkipInt16();
                            break;
                        case 0xE7: // SynGPU
                            reader.SkipInt16();
                            break;
                        case 0xF4: // Vib pitch
                            reader.SkipByte();
                            break;
                        case 0xFD: // Tempo
                            byte secondOpcodeFD = reader.ReadByte();
                            if (secondOpcodeFD == 0)
                            {
                                SetTempo tempo = new SetTempo(reader, FileTypes.BMS);
                                Events.Add(tempo);
                            }
                            break;
                        case 0xFE: // Time base
                            byte secondOpcodeFE = reader.ReadByte();
                            if (secondOpcodeFE == 0)
                            {
                                SetTimeBase timeBase = new SetTimeBase(reader, FileTypes.BMS);
                                Events.Add(timeBase);
                            }
                            break;
                        default:
                            throw new FormatException("Unknown opcode " + opCode + "!");
                            break;
                    }
                }

                opCode = reader.ReadByte();
            }
        }

        private void ReadTrackMIDI(EndianBinaryReader reader)
        {

        }

        public void WriteTrack(EndianBinaryWriter writer)
        {

        }
    }
}
