using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SendData.src
{

    class FileStreamWrapper
    {
        public const int MAX_PACKET_SIZE = 65536;
        public List<Stream> fileList { get; private set; }

        public FileStreamWrapper( Stream[] files )
        {
            if( files != null )
            {
                fileList = files.ToList();
            }
            else
            {
                fileList = new List<Stream>();
            }
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

        public void addFile( Stream file )
        {
            fileList.Add(file);
        }
    }
}
