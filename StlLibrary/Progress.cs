using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace StlLibrary
{
    public class Progress : IDisposable
    {
        private ulong count = 0;
        private ulong current = 0;
        public bool Cancel { get; set; } = false;

        public ulong Count => count;
        public ulong Current => current;

        public void SetCount(ulong count)
        {
            this.count = count;
            if (this.ProgressChanged == null) return;
            if (count == 0) this.ProgressChanged(this, new ProgressChangedEventArgs(0));
            this.ProgressChanged(this, new ProgressChangedEventArgs((double)current/count));
        }
        public void SetCurrent(ulong current)
        {
            this.current = current;
            if (this.ProgressChanged == null) return;
            if (count == 0) this.ProgressChanged(this, new ProgressChangedEventArgs(0));
            this.ProgressChanged(this, new ProgressChangedEventArgs((double)current / count));
        }

        public void Dispose()
        {
            this.ProgressChanged = null;
        }

        public event ProgressChangedEventHandler ProgressChanged;
    }

    public class ProgressChangedEventArgs : EventArgs
    {
        public ProgressChangedEventArgs(double progress) { this.Progress = progress; }
        public double Progress { get; private set; }
    }

    public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);
}
