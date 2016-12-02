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
        
        public byte TrackNumber;
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
            int trackSize;
            long trackSizePos;

            CombineWaitCommands();

            writer.Write("MTrk".ToCharArray()); // Track magic
            trackSizePos = writer.BaseStream.Position;
            writer.Write((int)0); // Placeholder for track size

            // Track heading, just Track<number>
            string heading = string.Format("Track{0}", TrackNumber);
            writer.Write((byte)0);
            writer.Write((byte)0xFF);
            writer.Write((byte)3);
            writer.Write((byte)heading.Length);
            writer.Write(heading.ToCharArray());

            if (TrackNumber != 0)
            {
                // Program change, sets instrument to 1, Bright Accoustic Piano
                writer.Write((byte)0);
                writer.Write((byte)0xC0);
                //writer.Write((byte)(((TrackNumber - 1) * 3) + 1));
                writer.Write((byte)((TrackNumber + 4) * 3));
            }

            // Set initial volume
            writer.Write((byte)0);
            writer.Write((byte)0xB0);
            writer.Write((byte)0x7B);
            writer.Write((byte)0x00);

            if (Events[0].GetType() != typeof(Wait))
                writer.Write((byte)0);

            for (int i = 0; i < Events.Count; i++)
            {
                Events[i].WriteMIDI(writer);

                if ((Events[i].GetType() != typeof(Wait)) && ((Events[i].GetType() != typeof(SetTimeBase)) && (i < Events.Count - 1)))
                {
                    if (Events[i + 1].GetType() != typeof(Wait))
                        writer.Write((byte)0);
                }
            }

            // End of track
            // We check if the last event is Wait. If it isn't, we need to put in a delta-time value of 0
            if (Events.Last().GetType() != typeof(Wait))
                writer.Write((byte)0);

            // "Track end" event
            writer.Write((ushort)0xFF2F);
            writer.Write((byte)0);

            trackSize = (int)(writer.BaseStream.Position - trackSizePos) - 4;
            writer.BaseStream.Seek(trackSizePos, System.IO.SeekOrigin.Begin);
            writer.Write(trackSize);

            writer.BaseStream.Seek(0, System.IO.SeekOrigin.End);
        }

        private void CombineWaitCommands()
        {
            // This will make sure that there is only 1 wait command between each event, like MIDI

            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i].GetType() == typeof(Wait))
                {
                    if ((i + 1) == Events.Count)
                        break;

                    else if (Events[i + 1].GetType() == typeof(Wait))
                    {
                        Wait wait = Events[i] as Wait;
                        Wait delete = Events[i + 1] as Wait;
                        wait.WaitTime += delete.WaitTime;

                        Events.RemoveAt(i + 1);

                        i--;
                    }
                }
            }
        }
    }
}
