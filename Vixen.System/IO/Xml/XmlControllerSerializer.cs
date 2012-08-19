﻿using System;
using System.Linq;
using System.Xml.Linq;
using Vixen.Factory;
using Vixen.Sys;
using Vixen.Sys.Output;

namespace Vixen.IO.Xml {
	class XmlControllerSerializer : IXmlSerializer<IOutputDevice> {
		private const string ELEMENT_CONTROLLER = "Controller";
		private const string ELEMENT_OUTPUTS = "Outputs";
		private const string ELEMENT_OUTPUT = "Output";
		private const string ATTR_NAME = "name";
		private const string ATTR_HARDWARE_ID = "hardwareId";
		private const string ATTR_ID = "id";

		public XElement WriteObject(IOutputDevice value) {
			OutputController controller = (OutputController)value;

			XElement element = new XElement(ELEMENT_CONTROLLER,
				new XAttribute(ATTR_NAME, controller.Name),
				new XAttribute(ATTR_HARDWARE_ID, controller.ModuleId),
				new XAttribute(ATTR_ID, controller.Id),
				_WriteOutputs(controller));

			return element;
		}

		public IOutputDevice ReadObject(XElement element) {
			string name = XmlHelper.GetAttribute(element, ATTR_NAME);
			if(name == null) return null;

			Guid? moduleId = XmlHelper.GetGuidAttribute(element, ATTR_HARDWARE_ID);
			if(moduleId == null) return null;

			Guid? id = XmlHelper.GetGuidAttribute(element, ATTR_ID);
			if(id == null) return null;

			ControllerFactory controllerFactory = new ControllerFactory();
			OutputController controller = (OutputController)controllerFactory.CreateDevice(id.Value, moduleId.Value, name);

			_ReadOutputs(controller, element);

			return controller;
		}

		private XElement _WriteOutputs(OutputController controller) {
			return new XElement(ELEMENT_OUTPUTS,
					controller.Outputs.Select((x, index) =>
						new XElement(ELEMENT_OUTPUT,
							new XAttribute(ATTR_NAME, x.Name),
							new XAttribute(ATTR_ID, x.Id))));
		}

		private void _ReadOutputs(OutputController controller, XElement element) {
			XElement outputsElement = element.Element(ELEMENT_OUTPUTS);
			if(outputsElement != null) {
				foreach(XElement outputElement in outputsElement.Elements(ELEMENT_OUTPUT)) {
					Guid? id = XmlHelper.GetGuidAttribute(outputElement, ATTR_ID);
					string name = XmlHelper.GetAttribute(outputElement, ATTR_NAME) ?? "Unnamed output";

					CommandOutputFactory outputFactory = new CommandOutputFactory();
					CommandOutput output = (CommandOutput)outputFactory.CreateOutput(id.GetValueOrDefault(), name);

					controller.AddOutput(output);
				}
			}
		}
	}
}