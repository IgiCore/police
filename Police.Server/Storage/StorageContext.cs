using System.Data.Entity;
using NFive.SDK.Core.Models.Player;
using NFive.SDK.Server.Storage;

namespace IgiCore.Police.Server.Storage
{
	public class StorageContext : EFContext<StorageContext>
	{
		public DbSet<User> Users { get; set; }
	}
}
