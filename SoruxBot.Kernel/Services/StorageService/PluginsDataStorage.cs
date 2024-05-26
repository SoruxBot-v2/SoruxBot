using SoruxBot.Kernel.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SoruxBot.SDK.Plugins.Service;
using SoruxBot.Kernel.Bot;

namespace SoruxBot.Kernel.Services.StorageService
{
	public class PluginsDataStorage : IPluginsDataStorage
	{
		private ILoggerService _loggerService;
		private BotContext _botContext;
		private readonly IConfiguration _configuration;
		private PluginsDataContext _context;
		private string localFileDir;
		public PluginsDataStorage(IConfiguration config)
		{
			_configuration = config;
			_context = ConstructPluginsDataContext();
			localFileDir = config.GetRequiredSection("BaseSettings")["LocalFileDir"]!;
		}
		private PluginsDataContext ConstructPluginsDataContext()
		{
			DbContextOptionsBuilder<PluginsDataContext> optionsBuilder = new();
			string dbName = _configuration.GetRequiredSection("BaseSettings")["DataBaseName"]!;
			switch (dbName.ToLower())
			{
				case "sqlite":
					optionsBuilder.UseSqlite(GetConnectionString(dbName));
					break;
				case "sqlserver":
					optionsBuilder.UseSqlServer(GetConnectionString(dbName));
					break;
				case "mysql":
					optionsBuilder.UseMySQL(GetConnectionString(dbName));
					break;
				default:
					throw new ArgumentException($"Unsupported database: {dbName}");
			}
			return new PluginsDataContext(optionsBuilder.Options);
		}
		private string GetConnectionString(string dbName)
		{
			string connectionString;
			IConfigurationSection dbSection;
			IConfigurationSection optionsSection;
			switch(dbName)
			{
				case "sqlite":
					dbSection = _configuration.GetRequiredSection("SQLiteSettings");
					connectionString = $"Data Source={dbSection.GetValue("DatabaseFile", ":memory:")};Version={dbSection.GetValue("Version", 3)};";
					optionsSection = dbSection.GetSection("Options");
					if(optionsSection.Exists())
					{
						connectionString += string.Join(";", optionsSection.GetChildren().Select(child => $"{child.Key}={child.Value}")) + ";";
					}
					break;
					// 未完成
				case "sqlserver":
					dbSection = _configuration.GetRequiredSection("SQLServerSettings");
					connectionString = $"";
					break;
					// 未完成
				case "mysql":
					dbSection = _configuration.GetRequiredSection("MySQLSettings");
					connectionString = $"";
					break;
				default:
					connectionString = string.Empty;
					break;
			}
			return connectionString;
		}
		
		public bool AddStringSettings(string pluginMark, string key, string value)
		{
			_context.Add(new PluginsData(pluginMark, key, value));
			int res = 0;
			try
			{
				res = _context.SaveChanges();
			}
			catch (DbUpdateConcurrencyException e)
			{
				// 日志写入
			}
			catch(DbUpdateException e)
			{
				// 日志写入
			}
			catch(Exception e)
			{
				// 日志写入
			}
			return res == 1;
		}
		public bool RemoveStringSettings(string pluginMark, string key)
		{
			var itemToDel = _context.Plugins.Where(e => e.PluginMark == pluginMark && e.Key == key).FirstOrDefault();
			if (itemToDel is null)
			{
				//日志写入
				return false;
			}
			_context.Remove(itemToDel);
			int res = 0;
			try
			{
				res = _context.SaveChanges();
			}
			catch (DbUpdateConcurrencyException e)
			{
				// 日志写入
			}
			catch(DbUpdateException e)
			{
				// 日志写入
			}
			catch(Exception e)
			{
				// 日志写入
			}
			return res == 1;
		}
		public string GetStringSettings(string pluginMark, string key)
		{
			var entity = _context.Plugins.Where(e => e.PluginMark == pluginMark && e.Key == key).FirstOrDefault();
			if (entity is not null)
			{
				return entity.StringValue;
			}
			return string.Empty;
		}
		public bool EditStringSettings(string pluginMark, string key, string value)
		{
			var entity = _context.Plugins.Where(e => e.PluginMark == pluginMark && e.Key == key).FirstOrDefault();
			if(entity is null)
			{
				// 日志写入
				return false;
			}
			entity.StringValue = value;
			int res = 0;
			try
			{
				res = _context.SaveChanges();
			}
			catch (DbUpdateConcurrencyException e)
			{
				// 日志写入
			}
			catch (DbUpdateException e)
			{
				// 日志写入
			}
			catch (Exception e)
			{
				// 日志写入
			}
			return res == 1;
		}
		public bool AddBinarySettings(string pluginMark, string key, byte[] value)
		{
			string dic = Path.Join(localFileDir, pluginMark);
			if(!Directory.Exists(dic)) Directory.CreateDirectory(dic);
			File.WriteAllBytes(Path.Join(dic, key + ".bin"), value);
			return true;
		}
		public bool RemoveBinarySettings(string pluginMark, string key)
		{
			string path = Path.Join(localFileDir, pluginMark, key + "bin");
			if (!Directory.Exists(path)) return false;
			File.Delete(path);
			return true;
		}
		public byte[]? GetBinarySettings(string pluginMark, string key)
		{
			string path = Path.Join(localFileDir, pluginMark, key + "bin");
			if (!Directory.Exists(path)) return null;
			return File.ReadAllBytes(path);
		}
		public bool EditBinarySettings(string pluginMark, string key, byte[] value)
		{
			string path = Path.Join(localFileDir, pluginMark, key + "bin");
			if (!Directory.Exists(path)) return false;
			File.WriteAllBytes(path, value);
			return true;
		}
	}
}
