﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vixen.Module.Transform;
using Vixen.Sys;
using CommandStandard.Types;

namespace TestTransform {
	public class Module : TransformModuleDescriptorBase {
		private Guid _typeId = new Guid("{BEF25CF1-DC6E-4130-B262-6A1FF71B29DA}");

		[ModuleDataPath]
		static internal string _directory = "DimmingCurve";

		override public Type[] TypesAffected {
			get { return new Type[] { typeof(Level) }; }
		}

		override public Guid TypeId {
			get { return _typeId; }
		}

		override public Type ModuleClass {
			get { return typeof(Dimming); }
		}

		override public Type ModuleDataClass {
			get { return typeof(DimmingData); }
		}

		override public string Author {
			get { throw new NotImplementedException(); }
		}

		override public string TypeName {
			get { return "Dimming curve level transform"; }
		}

		override public string Description {
			get { throw new NotImplementedException(); }
		}

		override public string Version {
			get { throw new NotImplementedException(); }
		}

		override public void Setup(ITransform[] transforms) {
			// The intention is to allow the configuration of multiple like transforms
			// across outputs.
			using(SetupDialog setupDialog = new SetupDialog()) {
				Dimming[] dimmingTransforms = transforms.Cast<Dimming>().ToArray();
				// If all are using the same curve, set it in the dialog.
				string firstCurve = dimmingTransforms[0].DimmingCurveName;
				if(dimmingTransforms.All(x => x.DimmingCurveName.Equals(firstCurve, StringComparison.OrdinalIgnoreCase))) {
					setupDialog.DimmingCurveName = firstCurve;
				}
				//*** this doesn't account for there being different transforms in the collection
				if(setupDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
					foreach(Dimming dimmingTranform in dimmingTransforms) {
						dimmingTranform.DimmingCurveName = setupDialog.DimmingCurveName;
					}
				}
			}
		}
	}
}
