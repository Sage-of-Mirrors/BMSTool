using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src
{
    class SetVolume : Event
    {
        byte Volume;

        public SetVolume(EndianBinaryReader reader, FileTypes type)
        {
            if (type == FileTypes.BMS)
            {
                Volume = reader.ReadByte();
            }
            else
            {
                Volume = reader.ReadByte();
            }
        }

        public override void WriteBMS(EndianBinaryWriter writer)
        {
            writer.Write((byte)0x98);
            writer.Write((byte)0);
            writer.Write(Volume);
        }

        public override void WriteMIDI(EndianBinaryWriter writer)
        {
            writer.Write((byte)0xB0);
            writer.Write((byte)0x27);
            writer.Write(Volume);
        }
    }
}
