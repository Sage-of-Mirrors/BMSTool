using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src
{
    public class Wait : Event
    {
        int WaitTime;

        public Wait(EndianBinaryReader reader, FileTypes type)
        {
            if (type == FileTypes.BMS)
            {
                int opCode = reader.ReadByte();

                // This one uses a byte to store time to wait
                if (opCode == 0x80)
                    WaitTime = (int)reader.ReadByte();
                // This one uses a short to store time to wait
                else if (opCode == 0x88)
                    WaitTime = (int)reader.ReadInt16();
            }
            else
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
                    WaitTime |= inputArray[i] & 0x7F;
                }
            }
        }

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

        public override void WriteMIDI(EndianBinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
