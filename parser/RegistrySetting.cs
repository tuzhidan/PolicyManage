using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PolicyManage.Parser
{
    public class RegistrySetting
    {
        public string KeyPath { get; set; }

        public string Value { get; set; }

        public KeyType Type { get; set; }

        public uint Size { get; set; }

        public byte[] BinaryData { get; set; }

        public object Data { get; set; }

        public override string ToString()
        {
            return KeyPath + "\\" + Value;
        }
    }
}
