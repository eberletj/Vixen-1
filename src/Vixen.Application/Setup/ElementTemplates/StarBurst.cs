﻿using Common.Controls.Theme;
using Common.Resources.Properties;
using NLog;
using Vixen.Rule;
using Vixen.Services;
using Vixen.Sys;
using Vixen.Utility;

namespace VixenApplication.Setup.ElementTemplates
{
	public partial class Starburst : ElementTemplateBase, IElementTemplate
	{
		private static readonly Logger Logging = LogManager.GetCurrentClassLogger();

		private string _treeName;
		private int _stringCount;
		private bool _pixelTree;
		private int _pixelsPerString;

		public Starburst()
		{
			InitializeComponent();
			Icon = Resources.Icon_Vixen3;
			ThemeUpdateControls.UpdateControls(this);

			_treeName = "Starburst";
			_stringCount = 8;
			_pixelTree = false;
			_pixelsPerString = 50;
		}

		public string TemplateName
		{
			get { return "Starburst"; }
		}

		public bool SetupTemplate(IEnumerable<ElementNode>? selectedNodes = null)
		{
			DialogResult result = ShowDialog();

			if (result == DialogResult.OK)
				return true;

			return false;
		}

		public async Task<IEnumerable<ElementNode>> GenerateElements(IEnumerable<ElementNode>? selectedNodes = null)
		{
			List<ElementNode> result = new List<ElementNode>();

			if (_treeName.Length == 0)
			{
				Logging.Error("starburst is null");
				return await Task.FromResult(result);
			}

			if (_stringCount < 0)
			{
				Logging.Error("negative count");
				return await Task.FromResult(result);
			}

			if (_pixelTree && _pixelsPerString < 0)
			{
				Logging.Error("negative pixelsperstring");
				return await Task.FromResult(result);
			}

			//Optimize the name check for performance. We know we are going to create a bunch of them and we can handle it ourselves more efficiently
			HashSet<string> elementNames = new HashSet<string>(VixenSystem.Nodes.Select(x => x.Name));

			ElementNode head = ElementNodeService.Instance.CreateSingle(null, NamingUtilities.Uniquify(elementNames, _treeName), true, false);
			result.Add(head);

			for (int i = 0; i < _stringCount; i++)
			{
				string stringname = head.Name + " " + textBoxSpokePrefix.Text + (i + 1);
				ElementNode stringnode = ElementNodeService.Instance.CreateSingle(head, NamingUtilities.Uniquify(elementNames, stringname), true, false);
				result.Add(stringnode);

				if (_pixelTree)
				{
					for (int j = 0; j < _pixelsPerString; j++)
					{
						string pixelname = stringnode.Name + " " + textBoxPixelPrefix.Text + (j + 1);

						ElementNode pixelnode = ElementNodeService.Instance.CreateSingle(stringnode, NamingUtilities.Uniquify(elementNames, pixelname), true, false);
						result.Add(pixelnode);
					}
				}
			}

			return await Task.FromResult(result);
		}

		private void checkBoxPixelTree_CheckedChanged(object sender, EventArgs e)
		{
			numericUpDownPixelsPerString.Enabled = checkBoxPixelTree.Checked;
			textBoxPixelPrefix.Enabled = checkBoxPixelTree.Checked;
		}

		private void Megatree_Load(object sender, EventArgs e)
		{
			textBoxTreeName.Text = _treeName;
			numericUpDownStrings.Value = _stringCount;
			checkBoxPixelTree.Checked = _pixelTree;
			numericUpDownPixelsPerString.Value = _pixelsPerString;
		}

		private void Megatree_FormClosed(object sender, FormClosedEventArgs e)
		{
			_treeName = textBoxTreeName.Text;
			_stringCount = Decimal.ToInt32(numericUpDownStrings.Value);
			_pixelTree = checkBoxPixelTree.Checked;
			_pixelsPerString = Decimal.ToInt32(numericUpDownPixelsPerString.Value);
		}

		private void buttonBackground_MouseHover(object sender, EventArgs e)
		{
			var btn = (Button)sender;
			btn.BackgroundImage = Resources.ButtonBackgroundImageHover;
		}

		private void buttonBackground_MouseLeave(object sender, EventArgs e)
		{
			var btn = (Button)sender;
			btn.BackgroundImage = Resources.ButtonBackgroundImage;

		}
	}
}
