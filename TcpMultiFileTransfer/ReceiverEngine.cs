using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpMultiFileTransfer
{
    public class ReceiverEngine
    {
        private static readonly byte[] RESUME = new byte[] { 0 };

        public static ReceiveFileInfo GetFileInfo(Socket socket)
        {
            byte[] buffer = new byte[512];
            int bytesRead = socket.Receive(buffer, SocketFlags.None);
            var fileInfo = Encoding.UTF8.GetString(buffer, 0, bytesRead).Split('|');
            socket.Send(RESUME);
            return new ReceiveFileInfo(new Receiver(socket, fileInfo[0]))
            {
                FileName = fileInfo[0],
                FileSize = Double.Parse(String.Format("{0:0.##}", long.Parse(fileInfo[1]) / 1024.0))
            };
        }
    }
}
