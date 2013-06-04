﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Radial.Tools.NhDbFirst.Kernel
{
    interface IOutput
    {
        void WriteCode(TextWriter writer);


        void WriteXml(TextWriter writer);

    }
}