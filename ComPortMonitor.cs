using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;

namespace COM_INFO
{
    internal sealed class ComPortMonitor : IDisposable
    {
        private readonly StringComparer portComparer = StringComparer.OrdinalIgnoreCase;
        private List<string> currentPorts = new List<string>();
        private bool disposed;

        public event EventHandler<PortChangeEventArgs> PortsChanged;

        public IReadOnlyList<string> CurrentPorts
        {
            get { return currentPorts.AsReadOnly(); }
        }

        public void Initialize()
        {
            ThrowIfDisposed();
            currentPorts = ReadAvailablePorts();
        }

        public void CheckPorts()
        {
            ThrowIfDisposed();

            List<string> detectedPorts = ReadAvailablePorts();

            List<string> addedPorts = detectedPorts.Except(currentPorts, portComparer).ToList();
            List<string> removedPorts = currentPorts.Except(detectedPorts, portComparer).ToList();

            if (addedPorts.Count == 0 && removedPorts.Count == 0)
            {
                return;
            }

            currentPorts = detectedPorts;
            EventHandler<PortChangeEventArgs> portsChanged = PortsChanged;

            if (portsChanged != null)
            {
                portsChanged(this, new PortChangeEventArgs(currentPorts.AsReadOnly(), addedPorts.AsReadOnly(), removedPorts.AsReadOnly()));
            }
        }

        public void Dispose()
        {
            disposed = true;
            PortsChanged = null;
        }

        private List<string> ReadAvailablePorts()
        {
            try
            {
                return SerialPort.GetPortNames()
                    .Where(port => !string.IsNullOrWhiteSpace(port))
                    .Distinct(portComparer)
                    .OrderBy(port => port, portComparer)
                    .ToList();
            }
            catch (Exception exception)
            {
                Trace.TraceError("Error checking COM ports: {0}", exception);
                return new List<string>(currentPorts);
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("ComPortMonitor");
            }
        }
    }
}