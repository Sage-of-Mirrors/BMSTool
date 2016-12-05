using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src.Events
{
    class SetVolume : Event
    {
        byte Channel;
        byte Volume;

        public SetVolume()
        {
            Channel = 0;
            Volume = 0x7F;
        }

        public void ReadBMS(EndianBinaryReader reader, byte channel)
        {
            Channel = channel;
            Volume = reader.ReadByte();
        }

        public override void WriteBMS(EndianBinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public override void WriteMIDI(EndianBinaryWriter writer)
        {
            writer.Write((byte)(0xB0 | Channel));
            writer.Write((byte)0x27);
            writer.Write(Volume);
        }
    }
}
