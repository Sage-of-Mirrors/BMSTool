using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;

namespace BMSTool.src.Events
{
    public abstract class Event
    {
        public abstract void WriteMIDI(EndianBinaryWriter writer);
        public abstract void WriteBMS(EndianBinaryWriter writer);
    }
}
