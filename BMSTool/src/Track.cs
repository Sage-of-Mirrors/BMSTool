using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using BMSTool.src.Events;
using BMSTool.src.BMS;

namespace BMSTool.src
{
    class Track
    {
        static byte TrackIDSource = 0;

        // MIDI needs to know the note number in order to turn off the note.
        // So, to ensure compatibility, we're going to store and remove notes as
        // they get turned on/off
        
        byte TrackNumber;
        public List<Event> Events;

        public Track()
        {
            TrackNumber = TrackIDSource++;
            Events = new List<Event>();
        }

        public void WriteBMS(EndianBinaryWriter writer)
        {
            // Sync the GPU
            writer.Write((byte)BMS_Command.SyncGPU);
            writer.Write((byte)0x00);
            writer.Write((byte)0x00);

            // Set instrument bank
            writer.Write((byte)BMS_Command.Set_Instrument);
            writer.Write((byte)0x20);
            writer.Write((byte)0x00);

            // Set instrument sample
            writer.Write((byte)BMS_Command.Set_Instrument);
            writer.Write((byte)0x21);
            writer.Write((byte)0x00);

            // Needs some more data before the actual
            // events

            foreach (Event ev in Events)
                ev.WriteBMS(writer);

            writer.Write((byte)0xFF);
        }

        public void WriteMIDI(EndianBinaryWriter writer)
        {

        }
    }
}
