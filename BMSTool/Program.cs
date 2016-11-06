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
        static void Main(string[] args)
        {
            string inputPath = "";
            string outputPath = "";

            /*
            if (args.Length != 0)
            {
                inputPath = args[0];
                if (args.Length >= 2)
                   outputPath = args[1];
            }
            else
            {
                Console.WriteLine("BMS/MIDI Converter written by Sage of Mirrors.");
                Console.WriteLine("Usage: BMSTool input_file [output_file]");
                return;
            }*/

            inputPath = @"D:\SZS Tools\bms\kaminoto.bms";
            outputPath = string.Format("D:\\{0}", Path.GetFileNameWithoutExtension(inputPath));

            if (outputPath == "")
                outputPath = string.Format("{0}\\{1}", Path.GetDirectoryName(inputPath), 
                                                       Path.GetFileNameWithoutExtension(inputPath));

            using (FileStream stream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);

                // Get the file type. We'll check if the first four bytes are 0x4D546864 ("Mthd", the start of a MIDI file)
                // or if the first byte is 0xC1, the command to open a track in BMS
                if (reader.PeekReadInt32() == 0x4D546864)
                {
                    MIDI inputMidi = new MIDI(reader);
                    inputMidi.WriteBMS(outputPath);
                }
                else if (reader.PeekReadByte() == 0xC1)
                {
                    BMS inputBMS = new BMS(reader);
                    inputBMS.WriteMIDIFile(outputPath);
                }
                else
                {
                    throw new FormatException("Filetype was unrecognized!");
                }
            }
        }
    }
}
