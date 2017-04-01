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
            fileList = new List<Stream>();
            foreach (Stream file in files)
            {
                fileList.Add(file);
            }
        }

        public FileStreamWrapper( Stream file )
        {
            fileList = new List<Stream>();
            fileList.Add(file);
        }

        public FileStreamWrapper()
        {
            fileList = new List<Stream>();
        }

        public void addFile( Stream[] files )
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
