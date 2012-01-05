﻿using System;
using Tempest;

namespace Alloy
{
	public sealed class Screen
	{
		public Screen (int height, int width, int order)
		{
			Height = height;
			Width = width;
			Order = order;
		}

		/// <summary>
		/// Gets the height of the screen in pixels.
		/// </summary>
		public int Height
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the width of the screen in pixels.
		/// </summary>
		public int Width
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the 0-based position of the screen, from left to right.
		/// </summary>
		public int Order
		{
			get;
			private set;
		}
	}

	internal sealed class ScreenSerializer
		: ISerializer<Screen>
	{
		public void Serialize (ISerializationContext context, IValueWriter writer, Screen element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");

			writer.WriteInt32 (element.Height);
			writer.WriteInt32 (element.Width);
			writer.WriteInt32 (element.Order);
		}

		public Screen Deserialize (ISerializationContext context, IValueReader reader)
		{
			return new Screen (reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
		}
	}
}