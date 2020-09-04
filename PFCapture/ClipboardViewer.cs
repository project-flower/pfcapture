﻿using System;
using System.Windows.Forms;
using Win32Api;

namespace PFCapture
{
    public class ClipboardEventArgs : EventArgs
    {
    }

    public delegate void ClipboardEventHandler(object sender, ClipboardEventArgs e);

    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class ClipboardViewer : NativeWindow
    {
        #region Private Fields

        private bool messageReceived;
        private IntPtr nextHandle;

        #endregion

        #region Public Properties

        public event ClipboardEventHandler DrawClipboard = delegate { };

        #endregion

        #region Public Methods

        public ClipboardViewer(Form form)
        {
            messageReceived = false;
            form.HandleCreated += new EventHandler(formHandleCreated);
            form.HandleDestroyed += new EventHandler(formHandleDestroyed);
        }

        #endregion

        #region Protected Methods

        protected override void WndProc(ref Message m)
        {
            switch ((WM)(m.Msg))
            {
                case WM.DRAWCLIPBOARD:
                    if (!messageReceived)
                    {
                        // SetClipboardViewer 直後のメッセージは、クリップボードの更新ではないので無視
                        messageReceived = true;
                        break;
                    }

                    DrawClipboard(this, new ClipboardEventArgs());

                    if (nextHandle != IntPtr.Zero)
                    {
                        User32.PostMessage(nextHandle, m.Msg, m.WParam, m.LParam);
                    }

                    break;
            }

            base.WndProc(ref m);
        }

        #endregion

        #region Private Methods

        private void formHandleCreated(object sender, EventArgs e)
        {
            AssignHandle((sender as Form).Handle);
            messageReceived = false;
            nextHandle = User32.SetClipboardViewer(Handle);
        }

        private void formHandleDestroyed(object sender, EventArgs e)
        {
            User32.ChangeClipboardChain(Handle, nextHandle);
            ReleaseHandle();
        }

        #endregion
    }
}
