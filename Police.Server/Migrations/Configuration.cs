using JetBrains.Annotations;
using NFive.SDK.Server.Migrations;
using IgiCore.Police.Server.Storage;

namespace IgiCore.Police.Server.Migrations
{
	[UsedImplicitly]
	public sealed class Configuration : MigrationConfiguration<StorageContext> { }
}
