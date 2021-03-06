﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Radial.UnitTest
{
    [TestFixture]
    public class ToolkitsTest
    {
        [Test]
        public void Ping()
        {
            ((List<System.Net.NetworkInformation.PingReply>)Toolkits.GetPingReplies("www.baidu.com")).ForEach(o =>
            {
                if (o.Status == System.Net.NetworkInformation.IPStatus.Success)
                    Console.WriteLine("Reply from {0} Bytes={1} Time={2}ms TTL={3}", o.Address, o.Buffer.Length, o.RoundtripTime, o.Options.Ttl);
                else
                    Console.WriteLine(o.Status);
            });
        }

        [Test]
        public void GetGeoInfo()
        {
            for (int i = 0; i < 10; i++)
            {
                var geo = Toolkits.GetGeoInfo("5" + i + ".246.87.15" + i);
                if (geo != null)
                    Console.WriteLine("{0}, {1}, {2}", geo.Country, geo.Division, geo.City);
            }
        }
    }
}
