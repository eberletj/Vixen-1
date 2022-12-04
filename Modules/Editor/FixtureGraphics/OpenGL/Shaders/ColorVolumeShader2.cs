﻿using System;

namespace VixenModules.Editor.FixtureGraphics.OpenGL.Shaders
{
	/// <summary>
	/// Shader for graphical volumes that are assigned a color.
	/// </summary>
	class ColorVolumeShader2 : ColorVolumeShader
	{
		#region Static Constructor

		/// <summary>
		/// Static Constructor
		/// </summary>
		static ColorVolumeShader2()
		{
			// Initialize the unique ID
			ShaderID = Guid.NewGuid();
		}

		#endregion
	}
}