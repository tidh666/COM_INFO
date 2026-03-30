using System;
using System.Collections.Generic;

namespace COM_INFO
{
    internal sealed class PortChangeEventArgs : EventArgs
    {
        public PortChangeEventArgs(IReadOnlyList<string> allPorts, IReadOnlyList<string> addedPorts, IReadOnlyList<string> removedPorts)
        {
            AllPorts = allPorts;
            AddedPorts = addedPorts;
            RemovedPorts = removedPorts;
        }

        public IReadOnlyList<string> AllPorts { get; private set; }

        public IReadOnlyList<string> AddedPorts { get; private set; }

        public IReadOnlyList<string> RemovedPorts { get; private set; }
    }
}