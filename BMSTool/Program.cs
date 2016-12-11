using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GameFormatReader.Common;
using BMSTool.src.BMS;
using BMSTool.src.MIDI;

namespace BMSTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputPath = "";
            string outputPath = "";
            int loopCount = 3;

            
            if (args.Length != 0)
            {
              inputPath = args[0];
              if (args.Length >= 2)
                  loopCount = Convert.ToInt32(args[1]);
              if (args.Length >= 3)
                  outputPath = args[2];
            }
            else
            {
              Console.WriteLine("BMS/MIDI Converter written by Sage of Mirrors.");
              Console.WriteLine("Usage: BMSTool input_file loop_count [output_file]");
              return;
            }


            /*#region debug input
            inputPath = @"D:\SZS Tools\t_delfino.bms";
            outputPath = string.Format("D:\\{0}", Path.GetFileNameWithoutExtension(inputPath));
            #endregion*/

            if (outputPath == "")
            {
                outputPath = string.Format("{0}\\{1}", Path.GetDirectoryName(inputPath),
                                       Path.GetFileNameWithoutExtension(inputPath));
            }
            else
                outputPath = string.Format("{0}\\{1}", Path.GetDirectoryName(outputPath), Path.GetFileNameWithoutExtension(outputPath));

            using (FileStream stream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);

                // We'll check file type by looking at the first four bytes for MIDI or
                // the first byte for BMS.
                // The first four bytes are the characters "Mthd", which means this is a MIDI
                if (reader.PeekReadInt32() == 0x4D546864)
                {
                    MIDI midiFile = new MIDI();
                    midiFile.ReadMIDI(reader);

                    using (FileStream outStream = new FileStream(string.Format("{0}.bms", outputPath), FileMode.Create, FileAccess.Write))
                    {
                        EndianBinaryWriter writer = new EndianBinaryWriter(outStream, Endian.Big);
                        midiFile.WriteBMS(writer);
                    }
                }
                // The first byte is 0xC1, which means this is a BMS
                else if (reader.PeekReadByte() == 0xC1)
                {
                    BMS bmsFile = new BMS();
                    bmsFile.ReadBMS(reader, loopCount);

                    using (FileStream outStream = new FileStream(string.Format("{0}.mid", outputPath), FileMode.Create, FileAccess.Write))
                    {
                        EndianBinaryWriter writer = new EndianBinaryWriter(outStream, Endian.Big);
                        bmsFile.WriteMidi(writer);
                    }
                }
                // File type tests failed, we don't know what kind of file this is
                else
                {
                    throw new FormatException("Filetype was unrecognized!");
                }
            }
        }
    }
}
