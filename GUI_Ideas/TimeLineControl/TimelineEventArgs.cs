﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Timeline
{
	public class ElementEventArgs : EventArgs
	{
		public ElementEventArgs(TimelineElement te)
		{
			Element = te;
		}

		public TimelineElement Element { get; private set; }
	}


	public class MultiElementEventArgs : EventArgs
	{
		public List<TimelineElement> Elements { get; set; }
	}


	public class RowHeightChangedEventArgs : EventArgs
	{
		public RowHeightChangedEventArgs(int heightChange)
		{
			HeightChange = heightChange;
		}

		public int HeightChange { get; private set; }
	}


	public class TimeSpanEventArgs : EventArgs
	{
		public TimeSpanEventArgs(TimeSpan t)
		{
			Time = t;
		}

		public TimeSpan Time { get; private set; }
	}


	public class ModifierKeysEventArgs : EventArgs
	{
		public ModifierKeysEventArgs(Keys k)
		{
			ModifierKeys = k;
		}

		public Keys ModifierKeys { get; private set; }
	}


}