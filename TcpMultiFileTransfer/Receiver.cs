using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpMultiFileTransfer
{
    public class Receiver
    {
        private readonly Socket socket;
        private readonly string fileName;

        public Receiver(Socket socket, string fileName)
        {
            this.socket = socket;
            this.fileName = fileName;
        }

        public void LoadFile()
        {
            int bytesRead = 0;
            byte[] buffer = new byte[332800]; // 325 KB
            using (var networkStream = new NetworkStream(socket))
            using (var binaryReader = new BinaryReader(networkStream))
            using (Stream writer = File.OpenWrite("Downloads\\" + fileName))
            {
                while (networkStream.CanRead && (bytesRead = binaryReader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writer.Write(buffer, 0, bytesRead);
                    writer.Flush();
                }
            }
            Dispose();
        }

        private void Dispose()
        {
            socket.Disconnect(false);
            socket.Close();
            socket.Dispose();
        }
    }
}
