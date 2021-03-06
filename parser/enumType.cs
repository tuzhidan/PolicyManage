using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolicyManage.Parser
{
    public enum PolType
    {
        Computer,
        User
    }
    public enum KeyType : uint
    {
        REG_NONE = 0,
        REG_SZ                 = 1,       /* string type (ASCII) */
        REG_EXPAND_SZ          = 2,       /* string, includes %ENVVAR% (expanded by caller) (ASCII) */
        REG_BINARY             = 3,      /* binary format, callerspecific */
        REG_DWORD              = 4,       /* DWORD in little endian format */
        REG_DWORD_BIG_ENDIAN   = 5,       /* DWORD in big endian format  */
        REG_LINK               = 6,       /* symbolic link (UNICODE) */
        REG_MULTI_SZ           = 7,       /* multiple strings, delimited by \0, terminated by \0\0 (ASCII) */
        REG_RESOURCE_LIST      = 8,
        REG_FULL_RESOURCE_DESCRIPTOR = 9,
        REG_RESOURCE_REQUIREMENTS_LIST = 10,
        REG_QWORD              = 11 
    }
}
