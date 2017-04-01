using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;

namespace SendData.src
{

    class FileStreamWrapper
    {
        
        public readonly int MAX_PACKET_SIZE = 65536;
        public List<Stream> fileList { get; private set; }
        private Object CONNECTION_OBJECT { get; set; }
        private static readonly object fsLock = new object();

        /*
         * Using OBJECT here but really the reference to the object that is actually performing
         * the sending would replace the Socket.
         */
        public FileStreamWrapper( Stream[] files, Object CONNECT_OBJECT )
        {
            if( files != null )
            {
                fileList = files.ToList();
            }
            else
            {
                fileList = new List<Stream>();
            }

            SetConnection(CONNECT_OBJECT);
        }

        public FileStreamWrapper( Stream file, Object CONNECT_OBJECT )
        {
            fileList = new List<Stream>();
            fileList.Add(file);
        }

        public FileStreamWrapper()
        {
            fileList = new List<Stream>();
        }

        public void SetConnection( Object CONNECT_OBJECT )
        {
            if(CONNECT_OBJECT != null )
            {
                // Prepare the connection object for sending the file streams
                this.CONNECTION_OBJECT = CONNECT_OBJECT;
            }
        }

        public void AddFile( Stream[] files )
        {
            foreach (Stream file in files)
            {
                fileList.Add(file);
            }
        }

        public void AddFile( Stream file )
        {
            fileList.Add(file);
        }

        private int SendChunk( byte[] buffer, int bytesREAD )
        {
            int bytesSENT = 0;
            /*
            if CONNECTION_OBJECT has connection {
                int bytesSENT = CONNECTION_OBJECT.send( buffer, bytesREAD );
                
                if( bytesSENT != valid ) {
                    throw new ChunkException();
                }

            }
             */
            return bytesSENT;
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.WorkSocket;
                int bytesRead = handler.EndReceive(ar);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                Console.WriteLine($@"Received {bytesRead}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void BeginSending( FileMode mode = FileMode.Open )
        {
            byte[] buffer = new byte[MAX_PACKET_SIZE];

            foreach ( Stream file in fileList)
            {
                int offset = 0;
                // Get the Stream's file location in your computer system
                //string filepath = Stream.filepath
                //FileStream fs = new FileStream( filepath, FileMode.Open );

                //int max = file.maxsize
                //int bytesREAD = 0;
                //int bytesSENT = 0;
                lock(fsLock)
                {
                    /*
                    // Navigate to the beginning
                    fs.Seek(0, SeekOrigin.Begin);

                    while( offset < max ) {
                        bytesREAD = fs.Read( buffer, offset, MAX_PACKET_SIZE );
                        offset += bytesREAD;
                        
                        bytesSENT = SendChunk( buffer, bytesREAD );

                        if( bytesSENT != bytesREAD ){
                            // problem has occurred throw water on the
                            // fire here.
                            break;
                        }
                    }
                    */
                }


            }
        }
    }
}
