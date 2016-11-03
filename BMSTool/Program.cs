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

            string inputPath = @"C:\Dropbox\TWW Docs\bms\tak8_mdr.bms";

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
            while (reader.PeekReadByte() == 0xC1)
            {
                Track track = new Track(reader, FileTypes.BMS);
                tracks.Add(track);
            }
        }

        static void ReadMIDI(EndianBinaryReader reader)
        {

        }
    }
}
