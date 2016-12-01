using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src.Events
{
    class Wait : Event
    {
        public uint WaitTime;

        public Wait()
        {
            WaitTime = 0;
        }

        /// <summary>
        /// Reads a Wait command from a BMS file.
        /// </summary>
        /// <param name="reader">BMS stream to read from</param>
        public void ReadBMS(EndianBinaryReader reader)
        {
            reader.BaseStream.Position--;
            byte opCode = reader.ReadByte();

            // This one uses a byte to store time to wait
            if (opCode == 0x80)
                WaitTime = (uint)reader.ReadByte();
            // This one uses a short to store time to wait
            else if (opCode == 0x88)
                WaitTime = (uint)reader.ReadUInt16();
        }

        /// <summary>
        /// Reads a Delta Time (Wait) command from a MIDI file.
        /// </summary>
        /// <param name="reader">MIDI stream to read from</param>
        public void ReadMIDI(EndianBinaryReader reader)
        {
            // So, MIDI stores delta times in "variable length" format, which means that data is stored weirdly.
            // Each byte in the delta time has only 7 bits of data. The most significant bit, bit 7, is used to
            // signal whether there are more bytes in the delta time to read.
            // So, what we do is grab all of the delta time bytes by checking if they've got the MSB set.
            // Once we get one that doesn't have it set, we have to convert the array into a readable value.
            // To do this, we shift the output left 7 bits, then or a byte anded by 0x7F in.
            // What this looks like is
            // output = output << 7;
            // output |= inputArray[i] & 0x7F;
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
                WaitTime = WaitTime << 7;
                WaitTime |= (uint)(inputArray[i] & 0x7F);
            }
        }

        /// <summary>
        /// Writes a Wait command to the specified stream in BMS format.
        /// </summary>
        /// <param name="writer">Stream to write to</param>
        public override void WriteBMS(EndianBinaryWriter writer)
        {
            // Basically, if it's above what a byte can store, we'll use 0x88
            if (WaitTime > 0xFF)
            {
                writer.Write((byte)0x88);
                writer.Write((short)WaitTime);
            }
            // Otherwise we store it in 0x80
            else
            {
                writer.Write((byte)0x80);
                writer.Write((byte)WaitTime);
            }
        }

        /// <summary>
        /// Writes a Delta Time (Wait) command to the specified stream in MIDI format.
        /// </summary>
        /// <param name="writer">Stream to write to</param>
        public override void WriteMIDI(EndianBinaryWriter writer)
        {
            long WaitCopy = WaitTime;
            long buffer = WaitCopy & 0x7F;

            int iterations;

            // for variable length quantities, determine the length of the output.
            // there can be a maximum of 4 bytes output.
            if (WaitCopy < 0x80)
            {
                iterations = 1;
            }
            else if (WaitCopy < 0x4000)
            {
                iterations = 2;
            }
            else if (WaitCopy < 0x200000)
            {
                iterations = 3;
            }
            else
            {
                iterations = 4;
            }

            while ((WaitCopy >>= 7) != 0)
            {
                buffer <<= 8;
                //buffer |= 0x80;
                buffer |= ((WaitCopy & 0x7F) | 0x80);
            }

            for (int i = 0; i < iterations; i++)
            {
                writer.Write((byte)(buffer & 0xFF));
                buffer = buffer >> 8;
            }

            /*
            while (true)
            {
                writer.Write((byte)(buffer & 0xFF));
                if ((buffer & 0x80) == 0x80)
                    buffer >>= 8;
                else
                    break;
            }*/
        }
    }
}
