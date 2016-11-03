using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src
{
    class SetTimeBase : Event
    {
        byte TimeBase;

        public SetTimeBase(EndianBinaryReader reader, FileTypes type)
        {
            if (type == FileTypes.BMS)
            {
                TimeBase = reader.ReadByte();
            }
            else
            {
                // ?
            }
        }

        public override void WriteBMS(EndianBinaryWriter writer)
        {
            writer.Write((byte)0xFE);
            writer.Write((byte)0);
            writer.Write(TimeBase);
        }

        public override void WriteMIDI(EndianBinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
