using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using BMSTool.src.Events;

namespace BMSTool.src.BMS
{
    class BMS
    {
        List<Track> Tracks;
        int LoopCount = 3;

        public BMS()
        {
            Tracks = new List<Track>();
        }

        public void ReadBMS(EndianBinaryReader reader, int loopCount)
        {
            LoopCount = loopCount;

            // As it turns out, BMS is actually like type 1 MIDI where there's a master track at the start that does global stuff.
            // For BMS, however, this track opens the other tracks with actual sequence data as well as sets master tempo and loop stuff.
            // So what we'll do here is open a "master track" that will parse track data like a track with normal sequence data.
            Track masterTrack = new Track();
            Tracks.Add(masterTrack);
            ReadTrackDataRecursive(reader, masterTrack);
        }

        private void ReadTrackDataRecursive(EndianBinaryReader reader, Track track, int offset = 0)
        {
            long returnOffset = reader.BaseStream.Position;
            reader.BaseStream.Seek(offset, System.IO.SeekOrigin.Begin);

            int loopCopy = LoopCount;
            int subroutineReturnOffset = 0; // Stores the return offset for a subroutine of events

            // MIDI requires the note name in order to properly terminate a note.
            // This byte[] will represent the channels that notes can be in.
            // When a NoteOn event occurs, the corresponding channel will be filled with that note.
            // When a NoteOff event occurs, the note in the corresponding channel will be recorded and the channel will be emptied.
            byte[] ChannelList = Enumerable.Repeat<byte>(0xFF, 16).ToArray();

            BMS_Command command = (BMS_Command)reader.ReadByte();

            while (command != BMS_Command.Terminate)
            {
                // Note on
                if ((byte)command <= 0x7F)
                {
                    NoteOn newNote = new NoteOn();
                    newNote.ReadBMS(reader);
                    newNote.Channel = (byte)(track.TrackNumber - 1);
                    ChannelList[newNote.Channel] = newNote.Note;
                    track.Events.Add(newNote);
                }
                // Note off
                else if ((byte)command >= 0x81 && (byte)command <= 0x87)
                {
                    NoteOff killNote = new NoteOff();
                    killNote.ReadBMS(reader);
                    killNote.Channel = (byte)(track.TrackNumber - 1);
                    killNote.Note = ChannelList[killNote.Channel];
                    ChannelList[killNote.Channel] = 0xFF;
                    track.Events.Add(killNote);
                }

                else
                {
                    switch (command)
                    {
                        case BMS_Command.Open_Track:
                            byte trackNo = reader.ReadByte();
                            int trackOffset = (int)reader.ReadBits(24);
                            Track newTrack = new Track();
                            ReadTrackDataRecursive(reader, newTrack, trackOffset);
                            Tracks.Add(newTrack);
                            break;
                        case BMS_Command.Wait_Byte:
                        case BMS_Command.Wait_Short:
                        case BMS_Command.Wait_Variable_Length:
                            Wait wait = new Wait();
                            wait.ReadBMS(reader);
                            track.Events.Add(wait);
                            break;
                        case BMS_Command.Volume_Plus:
                            byte type = reader.ReadByte();
                            if (type == 0)
                            {
                                SetVolume vol = new SetVolume();
                                vol.ReadBMS(reader, track.TrackNumber);
                                track.Events.Add(vol);
                            }
                            else
                                reader.SkipByte();
                            break;
                        case BMS_Command.Panning_Plus:
                            reader.Skip(3);
                            break;
                        case BMS_Command.NineE:
                            reader.SkipInt32();
                            break;
                        case BMS_Command.NineC:
                            reader.Skip(3);
                            break;
                        case BMS_Command.AZero:
                            byte secondOpcodeA0 = reader.ReadByte();
                            if (secondOpcodeA0 == 0xAC)
                                reader.Skip(2);
                            else
                                reader.SkipByte();
                            break;
                        case BMS_Command.Set_Instrument:
                            reader.Skip(2);
                            break;
                        case BMS_Command.ASeven:
                            reader.SkipInt16();
                            break;
                        case BMS_Command.AC:
                            reader.Skip(3);
                            break;
                        case BMS_Command.AD:
                            reader.SkipInt16();
                            break;
                        case BMS_Command.BEight:
                            reader.SkipInt16();
                            break;
                        case BMS_Command.BNine:
                            reader.Skip(3);
                            break;
                        case BMS_Command.CThree:
                            int c3JumpOffset = (int)reader.ReadBits(24);
                            subroutineReturnOffset = (int)reader.BaseStream.Position;
                            reader.BaseStream.Seek(c3JumpOffset, System.IO.SeekOrigin.Begin);
                            break;
                        case BMS_Command.Subroutine_Jump:
                            byte unknown = reader.ReadByte();
                            int jumpOffset = (int)reader.ReadBits(24);
                            subroutineReturnOffset = (int)reader.BaseStream.Position;
                            reader.BaseStream.Seek(jumpOffset, System.IO.SeekOrigin.Begin);
                            break;
                        case BMS_Command.CFive:
                        case BMS_Command.Subroutine_Return:
                            reader.BaseStream.Seek(subroutineReturnOffset, System.IO.SeekOrigin.Begin);
                            break;
                        case BMS_Command.CSeven:
                            reader.Skip(3);
                            break;
                        case BMS_Command.Loop_Jump:
                            if (loopCopy == 0)
                                reader.SkipInt32();
                            else
                            {
                                loop(reader);
                                loopCopy--;
                            }
                            break;
                        case BMS_Command.CB:
                            reader.SkipInt16();
                            break;
                        case BMS_Command.CC:
                            reader.SkipInt16();
                            break;
                        case BMS_Command.DTwo:
                            reader.SkipInt16();
                            break;
                        case BMS_Command.DSix:
                            reader.SkipByte();
                            break;
                        case BMS_Command.DEight:
                            reader.Skip(3);
                            break;
                        case BMS_Command.DD:
                            reader.Skip(3);
                            break;
                        case BMS_Command.DF:
                            reader.SkipInt32();
                            break;
                        case BMS_Command.EZero:
                            reader.SkipInt16();
                            break;
                        case BMS_Command.ETwo:
                            reader.SkipByte();
                            break;
                        case BMS_Command.EThree:
                            reader.SkipByte();
                            break;
                        case BMS_Command.Vibrato:
                            reader.Skip(2);
                            break;
                        case BMS_Command.SyncGPU:
                            reader.Skip(2);
                            break;
                        case BMS_Command.EA:
                            reader.Skip(3);
                            break;
                        case BMS_Command.EF:
                            reader.Skip(3);
                            break;
                        case BMS_Command.VibratoPitch:
                            reader.SkipByte();
                            break;
                        case BMS_Command.FNine:
                            reader.SkipInt16();
                            break;
                        case BMS_Command.Set_Time_Base:
                            SetTempo tempo = new SetTempo();
                            tempo.ReadBMS(reader);
                            track.Events.Add(tempo);
                            break;
                        case BMS_Command.Set_Tempo:
                            SetTimeBase timeBase = new SetTimeBase();
                            timeBase.ReadBMS(reader);
                            track.Events.Add(timeBase);
                            break;
                        default:
                            Console.WriteLine(string.Format("Unknown command {0:x}!", (byte)command));
                            throw new ArgumentException();
                    }
                }

                command = (BMS_Command)reader.ReadByte();

                if (command == BMS_Command.SyncGPU && track.TrackNumber == 0)
                    break;
            }

            reader.BaseStream.Seek(returnOffset, System.IO.SeekOrigin.Begin);
        }

        private void loop(EndianBinaryReader reader)
        {
            byte unknown = reader.ReadByte();
            int jumpOffset = (int)reader.ReadBits(24);
            reader.BaseStream.Seek(jumpOffset, System.IO.SeekOrigin.Begin);
        }

        public void WriteMidi(EndianBinaryWriter writer)
        {
            writer.Write("MThd".ToCharArray()); // MIDI magic
            writer.Write((int)6); // Header size, constant
            writer.Write((short)1); // This program outputs type 1 MIDIs
            writer.Write((short)Tracks.Count);
            writer.Write((short)0xF0); // Time base. Will (hopefully!) get overwritten by a SetTimeBase event in the master track

            foreach (Track track in Tracks)
                track.WriteMIDI(writer);
        }
    }
}
