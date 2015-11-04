using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteLib.Net;

namespace SpreadsheetGUI.Networking.Packets
{
    class PacketSelectionChanged : Packet
    {
        /// <summary>
        /// Gets the cell that has changed
        /// </summary>
        public string Cell { get; private set; }

        public override void ReadPacket(RemoteClient c)
        {
            Cell = c.ReadString();
        }

        public override void WritePacket(RemoteClient c)
        {
            c.WriteString(Cell);
        }

        public PacketSelectionChanged(string cell)
        {
            Cell = cell;
        }

        public PacketSelectionChanged()
        {
        }


        public override byte PacketId => 0x05;
    }
}
