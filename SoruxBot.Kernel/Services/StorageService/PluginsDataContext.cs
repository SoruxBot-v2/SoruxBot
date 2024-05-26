using SoruxBot.SDK.Plugins.Service;
using Microsoft.EntityFrameworkCore;

namespace SoruxBot.Kernel.Services.StorageService
{
	internal class PluginsDataContext : DbContext
	{
		public PluginsDataContext(DbContextOptions<PluginsDataContext> options) : base(options) { }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<PluginsData>(entity =>
			{
				entity.ToTable("PluginsData");
				entity.HasKey(e => new { e.PluginMark, e.Key });
				entity.Property(e => e.PluginMark).IsRequired();
				entity.Property(e => e.Key).IsRequired();
				entity.Property(e => e.StringValue).IsRequired();
			});
			

		}
	}
}
