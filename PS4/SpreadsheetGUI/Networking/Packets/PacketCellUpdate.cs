using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteLib.Net;

namespace SpreadsheetGUI.Networking.Packets
{
    class PacketCellUpdate : Packet
    {
        /// <summary>
        /// Gets the associated cell from this packet.
        /// </summary>
        public string Cell { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Content { get; private set; }

        public PacketCellUpdate(string cell, string content)
        {
            Cell = cell;
            Content = content;
        }

        public PacketCellUpdate()
        {
            
        }

        public override void ReadPacket(RemoteClient c)
        {
            Cell = c.ReadString();
            Content = c.ReadString();
        }

        public override void WritePacket(RemoteClient c)
        {
            c.WriteString(Cell);
            c.WriteString(Content);
        }

        public override byte PacketId => 0x04;
    }
}
