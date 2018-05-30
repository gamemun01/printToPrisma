using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Renci.SshNet;
using System.IO;

namespace printToPrisma
{
    class sftp
    {
        public static bool UploadSFTPFile(string host, string username, string password, string sourcefile, string destinationpath, int port)
        {
            using (SftpClient client = new SftpClient(host, port, username, password))
            {
                try
                {
                    client.Connect();
                    client.ChangeDirectory(destinationpath);
                    using (FileStream fs = new FileStream(sourcefile, FileMode.Open))
                    {
                        client.BufferSize = 4 * 1024;
                        client.UploadFile(fs, Path.GetFileName(sourcefile));
                    }
                    return true;
                }
                catch (Exception)
                {

                    return false;
                } 
            }
        }
    }
}
