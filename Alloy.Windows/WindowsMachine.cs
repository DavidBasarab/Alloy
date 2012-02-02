﻿using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Kennedy.ManagedHooks;

namespace Alloy.Windows
{
	public class WindowsMachine
		: IMachine
	{
		public WindowsMachine()
		{
			this.virtualScreen = new Screen (Interop.GetSystemMetrics (SystemMetric.VirtualScreenHeight), Interop.GetSystemMetrics (SystemMetric.VirtualScreenWidth));
			this.virtualX = Math.Abs (Interop.GetSystemMetrics (SystemMetric.XVirtualScreen));
			this.virtualY = Math.Abs (Interop.GetSystemMetrics (SystemMetric.YVirtualScreen));

			this.keyboard.KeyboardEvent += OnKeyboardEvent;
			this.mouse.MouseEvent += OnMouseEvent;
		}

		public event EventHandler ScreenChanged;
		public event EventHandler<MouseEventArgs> MouseEvent;
		public event EventHandler<KeyboardEventArgs> KeyboardEvent;

		public Screen Screen
		{
			get { return this.virtualScreen; }
		}

		public void StartListening()
		{
			this.mouse.InstallHook();
			this.keyboard.InstallHook();
			//this.proc = KeyboardHook;
			//int result = Interop.SetWindowsHookEx (Interop.WH_KEYBOARD, this.proc, (IntPtr) 0, AppDomain.GetCurrentThreadId());
		}

		public void StopListening()
		{
			this.mouse.UninstallHook();
			this.keyboard.UninstallHook();
			//Interop.UnhookWindowsHookEx (Interop.WH_KEYBOARD);
		}

		public void SetCursorVisibility (bool visible)
		{
			throw new NotImplementedException();
		}

		public void InvokeMouseEvent (MouseEvent ev)
		{
			throw new NotImplementedException();
		}

		public void InvokeKeyboardEvent (KeyboardEvent ev)
		{
			INPUT input = TranslateKeyEvent (ev);
			//Interop.SendInput (1, new[] { input }, );
		}

		private Screen virtualScreen;

		private int virtualX;
		private int virtualY;

		private readonly KeyboardHook keyboard = new KeyboardHook();
		private readonly MouseHook mouse = new MouseHook();

		private void OnMouseEvent (MouseEvents mEvent, Point point)
		{
			var ev = MouseEvent;
			if (ev == null)
				return;

			MouseEventType type;
			switch (mEvent)
			{
				case MouseEvents.LeftButtonDown:
					type = MouseEventType.LeftDown;
					break;

				case MouseEvents.LeftButtonUp:
					type = MouseEventType.LeftUp;
					break;

				case MouseEvents.RightButtonDown:
					type = MouseEventType.RightDown;
					break;

				case MouseEvents.RightButtonUp:
					type = MouseEventType.RightUp;
					break;

				case MouseEvents.Move:
					type = MouseEventType.Move;
					break;

				default:
					return;
			}

			ev (this, new MouseEventArgs (new MouseEvent (type, GetNormalizedPosition (point))));
		}

		private Position GetNormalizedPosition (Point point)
		{
			ushort x = (ushort)(point.X + this.virtualX);
			ushort y = (ushort)(point.Y + this.virtualY);
			return new Position (x, y);
		}

		private void OnKeyboardEvent (KeyboardEvents kEvent, Keys key)
		{
			var ev = KeyboardEvent;
			if (ev == null)
				return;

			KeyboardEventType type;
			switch (kEvent)
			{
				case KeyboardEvents.KeyDown:
				case KeyboardEvents.SystemKeyDown:
					type = KeyboardEventType.Down;
					break;

				case KeyboardEvents.KeyUp:
				case KeyboardEvents.SystemKeyUp:
					type = KeyboardEventType.Up;
					break;

				default:
					return;
			}

			var kev = new KeyboardEvent (type, KeyModifiers.None, GetPIKey (key), 1);
			ev (this, new KeyboardEventArgs (kev));
		}

		private HookProc proc;
		private int KeyboardHook (int nCode, int wParam, int lParam)
		{
			if (nCode < 0)
				return nCode;

			bool stop = false;

			var kev = KeyboardEvent;
			if (kev != null)
			{
				var args = new KeyboardEventArgs (TranslateKeyEvent (nCode, wParam, lParam));
				kev (this, args);
				stop = args.Handled;
			}

			return (stop) ? -1 : Interop.CallNextHookEx (Interop.WH_KEYBOARD, nCode, wParam, lParam);
		}

		//private KeyModifiers GetModifiers (int lParam)
		//{
			
		//}

		private KeyCode GetPIKey (Keys keys)
		{
			switch (keys)
			{
				case Keys.A:
					return KeyCode.A;
				case Keys.B:
					return KeyCode.B;
				case Keys.C:
					return KeyCode.C;
				case Keys.D:
					return KeyCode.D;
				case Keys.E:
					return KeyCode.E;
				case Keys.F:
					return KeyCode.F;
				case Keys.LControlKey:
					return KeyCode.LeftControl;
				case Keys.RControlKey:
					return KeyCode.RightControl;

				default:
					return KeyCode.None;
			}
		}

		private Keys GetWKey (KeyCode code)
		{
			return (Keys)((ushort)code);
		}

		private KeyboardEvent TranslateKeyEvent (int nCode, int wParam, int lParam)
		{
			KeyCode code = GetPIKey ((Keys)wParam);
			return new KeyboardEvent (KeyboardEventType.Up, KeyModifiers.None, code, 1);
		}

		private INPUT TranslateKeyEvent (KeyboardEvent kevent)
		{
			Keys k = GetWKey (kevent.Code);

			return new INPUT
			{
				type = 1,
				u = new InputUnion
				{
					ki = new KEYBDINPUT
					{
						wVk = (ushort)k,
						//todo
					}
				}
			};
		}
	}
}