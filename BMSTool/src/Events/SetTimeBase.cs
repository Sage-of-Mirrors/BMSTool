using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src.Events
{
    class SetTimeBase : Event
    {
        short TimeBase;

        public SetTimeBase()
        {
            TimeBase = 0xF0;
        }

        public void ReadBMS(EndianBinaryReader reader)
        {
            TimeBase = reader.ReadInt16();
        }

        public void ReadMIDI(EndianBinaryReader reader)
        {

        }

        public override void WriteBMS(EndianBinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public override void WriteMIDI(EndianBinaryWriter writer)
        {
            long curPos = writer.BaseStream.Position;
            writer.BaseStream.Seek(0x0C, System.IO.SeekOrigin.Begin);
            writer.Write(TimeBase);
            writer.BaseStream.Seek(curPos, System.IO.SeekOrigin.Begin);
        }
    }
}
