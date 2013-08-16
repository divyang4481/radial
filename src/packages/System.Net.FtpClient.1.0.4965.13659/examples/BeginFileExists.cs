﻿using System;
using System.Net;
using System.Net.FtpClient;
using System.Threading;

namespace Examples {
    public static class BeginFileExistsExample {
        static ManualResetEvent m_reset = new ManualResetEvent(false);

        public static void BeginFileExists() {
            using (FtpClient conn = new FtpClient()) {
                m_reset.Reset();
                
                conn.Host = "localhost";
                conn.Credentials = new NetworkCredential("ftptest", "ftptest");
                conn.Connect();
                conn.BeginFileExists("foobar", new AsyncCallback(BeginFileExistsCallback), conn);

                m_reset.WaitOne();
                conn.Disconnect();
            }
        }

        static void BeginFileExistsCallback(IAsyncResult ar) {
            FtpClient conn = ar.AsyncState as FtpClient;

            try {
                if (conn == null)
                    throw new InvalidOperationException("The FtpControlConnection object is null!");

                Console.WriteLine("File exists: {0}", conn.EndFileExists(ar));
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            finally {
                m_reset.Set();
            }
        }
    }
}
