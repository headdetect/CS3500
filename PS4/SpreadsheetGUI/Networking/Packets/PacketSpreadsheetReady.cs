using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteLib.Net;

namespace SpreadsheetGUI.Networking.Packets
{
    class PacketSpreadsheetReady : Packet
    {
        public PacketSpreadsheetReady()
        {
            
        }

        public override void ReadPacket(RemoteClient c)
        {
            
        }

        public override void WritePacket(RemoteClient c)
        {
            
        }

        public override byte PacketId => 0x06;
    }
}
