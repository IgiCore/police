using JetBrains.Annotations;
using NFive.SDK.Core.Diagnostics;
using NFive.SDK.Server.Communications;
using NFive.SDK.Server.Controllers;
using NFive.SDK.Server.Rcon;
using IgiCore.Police.Shared;

namespace IgiCore.Police.Server
{
	[PublicAPI]
	public class PoliceController : ConfigurableController<Configuration>
	{
		public PoliceController(ILogger logger, Configuration configuration, ICommunicationManager comms, IRconManager rcon) : base(logger, configuration)
		{
			// Send configuration when requested
			comms.Event(PoliceEvents.Configuration).FromClients().OnRequest(e => e.Reply(this.Configuration));
		}
	}
}
