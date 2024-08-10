using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace duplexify.Application
{
    public class InvalidDirectoryConfigurationException : ApplicationException
    {
        public override string Message => "The directory configuration is invalid. WatchDirectory and OutDirectory have to point to different folders.";
    }
}
