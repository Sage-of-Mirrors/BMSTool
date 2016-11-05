using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using BMSTool.src;

namespace BMSTool
{
    class Program
    {
        // A list of all the tracks we've read
        static List<Track> tracks = new List<Track>();

        static FileTypes InputType;
        static FileTypes OutputType;

        static string FileName;

        static void Main(string[] args)
        {
            /*
            if (args.Length != 0)
                string inputPath = args[0];
            else
            {
                Console.WriteLine("No file specified!");
                return;
            }*/

            string inputPath = @"D:\SZS Tools\FDS_BIOS_-_Startup.mid";

            FileName = Path.GetFileNameWithoutExtension(inputPath);

            using (FileStream stream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);

                // Get the file type. We'll check if the first four bytes are 0x4D546864 ("Mthd", the start of a MIDI file)
                // or if the first byte is 0xC1, the command to open a track in BMS
                if (reader.PeekReadInt32() == 0x4D546864)
                {
                    InputType = FileTypes.MIDI;
                    OutputType = FileTypes.BMS;
                }
                else if (reader.PeekReadByte() == 0xC1)
                {
                    InputType = FileTypes.BMS;
                    OutputType = FileTypes.MIDI;
                }
                else
                {
                    throw new FormatException("Filetype was unrecognized!");
                }

                if (InputType == FileTypes.BMS)
                    ReadBMS(reader);
                else
                    ReadMIDI(reader);
            }
        }

        static void ReadBMS(EndianBinaryReader reader)
        {
            // Get the tracks that are defined at the start of the file
            while (reader.PeekReadByte() == 0xC1)
            {
                reader.SkipInt16(); // Skip open track opcode and track number

                int trackOffset =(int)reader.ReadBits(24); //& 0x00FFFFFF; // Get 24 bits that hold the offset
                long nextOpcode = reader.BaseStream.Position; // Save position, since we're jumping to trackOffset
                reader.BaseStream.Seek(trackOffset, System.IO.SeekOrigin.Begin); // Jump to beginning of track data

                Track track = new Track(reader, FileTypes.BMS);
                tracks.Add(track);

                reader.BaseStream.Position = nextOpcode; // Restore position to continue reading the file
            }

            // MIDI format 1, which this program will output, uses the first track in the file to set global things
            // like tempo and volume. We'll create a track out of the BMS's header here and insert it at the top
            // of the track list.

            Track headerTrack = new Track(reader, FileTypes.BMS);
            tracks.Insert(0, headerTrack);

            WriteMidi();
        }

        static void ReadMIDI(EndianBinaryReader reader)
        {
            reader.SkipInt64(); // Skips "MThd" magic and size, which is always 6

            // There's type 0, type 1, and type 2
            // Type 1 is the easiest to turn into BMS.
            // Type 0 puts individual notes into the different channels. Slightly harder but still possible.
            // Type 2 is meant for tracks that are to be played invidivdually. Not supported for BMS output.
            short midiType = reader.ReadInt16();

            if (midiType == 0)
            {
                //ReadType0Midi(reader);
            }
            else if (midiType == 1)
            {
                ReadType1Midi(reader);
            }
            else if (midiType == 2)
            {
                throw new FormatException("MIDI type 2 is unsupported!");
            }
            else
            {
                throw new FormatException("MIDI type unrecognized!");
            }
        }

        static void ReadType1Midi(EndianBinaryReader reader)
        {
            short numTracks = reader.ReadInt16();
            short timeBase = reader.ReadInt16();

            for (int i = 0; i < numTracks; i++)
            {
                Track track = new Track(reader, FileTypes.MIDI);
                tracks.Add(track);
            }

            WriteBMSType1();
        }

        static void WriteMidi()
        {
            using (FileStream stream = new FileStream(string.Format("D:\\{0}.mid", FileName), FileMode.Create, FileAccess.Write))
            {
                EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Big);
                writer.Write("MThd".ToCharArray()); // Header magic
                writer.Write((int)6); // Header size, always 6
                writer.Write((short)1); // Format, this program outputs format 1
                writer.Write((short)tracks.Count); // Number of tracks
                writer.Write((short)0xF0); // This is for timing and divisions. It's overwritten (hopefully) by the SetTimeBase event.

                foreach (Track track in tracks)
                    track.WriteTrack(writer, FileTypes.MIDI);
            }
        }

        static void WriteBMSType1()
        {
            using (FileStream stream = new FileStream(string.Format("D:\\{0}.bms", FileName), FileMode.Create, FileAccess.Write))
            {
                EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Big);

                // - 1 because MIDI type 1's first track is global info like tempo
                for (int i = 0; i < tracks.Count - 1; i++)
                {
                    writer.Write((byte)0xC1); // Open track command
                    writer.Write((byte)i); // Track number
                    writer.Write((byte)0);
                    writer.Write((byte)0); // Placeholder for track offset. It's only 24 bits, hence the 3 bytes
                    writer.Write((byte)0);
                }

                tracks.RemoveAt(0);

                foreach (Track track in tracks)
                    track.WriteTrack(writer, FileTypes.BMS);
            }
        }
    }
}
