using Vixen.Sys;

namespace VixenModules.Output.ElexolEtherIO
{
	class ElexolEtherIODataPolicyFactory : IDataPolicyFactory
	{
		public IDataPolicy CreateDataPolicy()
		{
			return new ElexolEtherIODataPolicy();
		}
	}
}