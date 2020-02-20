using System;
using System.Collections.Generic;
using System.Text;

namespace Thesis
{
    public interface IPathService
    {
        string InternalFolder { get; }
        string PublicExternalFolder { get; }
        string PrivateExternalFolder { get; }
    }
}
