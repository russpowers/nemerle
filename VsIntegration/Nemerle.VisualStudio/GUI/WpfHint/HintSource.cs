using System;

namespace WpfHint
{
  internal class HintSource
  {
    private Win32.Callback _ownerWndProc;
    private Win32.Callback _rootWndProc;

    private Win32.Callback _oldRoot;
    private Win32.Callback _oldOwner;

    private IntPtr _owner;   // text editor window handle
    private IntPtr _root;    // main window handle (visual studio)

    public event Action Activate;
    public event Action MouseMove;
    public event Action MouseLeave;

    public IntPtr Owner { get { return _owner; } }

    public void SubClass(IntPtr owner)
    {
      var pt = Win32.GetCursorPos();
      var pt1 = new Win32.POINT(pt.X, pt.Y);

      _owner = owner == IntPtr.Zero ? Win32.WindowFromPoint(pt1) : owner;

      if (_owner == IntPtr.Zero)
        throw new NullReferenceException("Can't find owner");

      _root = Win32.GetAncestor(_owner, Win32.GA_ROOT);
      if (_root == IntPtr.Zero)
        throw new NullReferenceException("Can't find root");

      _ownerWndProc = WndProc;
      _oldOwner = Win32.SetWindowLong(_owner, Win32.GWL_WNDPROC, _ownerWndProc);

      if (_oldOwner == null)
        throw new InvalidOperationException("Failed subclass");

      _rootWndProc = RootWndProc;
      _oldRoot = Win32.SetWindowLong(_root, Win32.GWL_WNDPROC, _rootWndProc);

      if (_rootWndProc == null)
        throw new InvalidOperationException("Failed subclass");
    }

    public void UnSubClass()
    {
      if (_owner != IntPtr.Zero && _oldOwner != null)
        Win32.SetWindowLong(_owner, Win32.GWL_WNDPROC, _oldOwner);

      if (_root != IntPtr.Zero && _oldRoot != null)
        Win32.SetWindowLong(_root, Win32.GWL_WNDPROC, _oldRoot);
    }

    private int WndProc(IntPtr hwnd, int msg, int wParam, int lParam)
    {
      if ((msg == Win32.WM_KEYDOWN || msg == Win32.WM_MOUSEWHEEL) && Activate != null)
        Activate();

      if ((msg == Win32.WM_LBUTTONDOWN || msg == Win32.WM_RBUTTONDOWN) && Activate != null)
        Activate();

      if (msg == Win32.WM_MOUSEMOVE && MouseMove != null)
        MouseMove();

      if (msg == Win32.WM_MOUSELEAVE && MouseLeave != null)
        MouseLeave();

      return Win32.CallWindowProc(_oldOwner, hwnd, msg, wParam, lParam);
    }

    private int RootWndProc(IntPtr hwnd, int msg, int wParam, int lParam)
    {
      if ((msg == Win32.WM_ACTIVATE || msg == Win32.WM_ACTIVATEAPP) && Activate != null)
        Activate();

      return Win32.CallWindowProc(_oldRoot, hwnd, msg, wParam, lParam);
    }
  }
}