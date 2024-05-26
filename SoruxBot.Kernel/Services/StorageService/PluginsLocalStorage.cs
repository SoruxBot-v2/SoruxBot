using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Plugins.Service;
using System.Data;
using System.Text;


namespace SoruxBot.Kernel.Services.StorageService
{
	public class PluginsLocalStorage : IPluginsStorage
	{
		private ILoggerService _loggerService;
		private BotContext _botContext;
		private static DataSet _dataSet = new DataSet();
		private int lastPrivilege = 0;
		private DataTable pluginsInformationTable;
		private Dictionary<string, object> _pluginsInstanceMap = new();
		public PluginsLocalStorage(BotContext botContext, ILoggerService loggerService)
		{
			this._botContext = botContext;
			this._loggerService = loggerService;
			_loggerService.Info("PluginsLocalStorage", "Built-in Plugins Local Storage: version:1.0.0");
			//初始化数据库
			InitDataSet();
		}
		private void InitDataSet()
		{
			pluginsInformationTable = new DataTable("pluginsInformation");
			DataColumn dataColumn;

			#region 插件信息生成

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "name";
			dataColumn.DataType = typeof(string);
			dataColumn.Unique = true;
			dataColumn.AutoIncrement = false;
			pluginsInformationTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "author";
			dataColumn.DataType = typeof(string);
			dataColumn.AutoIncrement = false;
			pluginsInformationTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "filename";
			dataColumn.DataType = typeof(string);
			dataColumn.AutoIncrement = false;
			pluginsInformationTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "version";
			dataColumn.DataType = typeof(string);
			dataColumn.AutoIncrement = false;
			pluginsInformationTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "description";
			dataColumn.DataType = typeof(string);
			dataColumn.AutoIncrement = false;
			pluginsInformationTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "privilege";
			dataColumn.DataType = typeof(int);
			dataColumn.AutoIncrement = false;
			pluginsInformationTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "isUpperWhenPrivilege";
			dataColumn.DataType = typeof(bool);
			dataColumn.AutoIncrement = false;
			pluginsInformationTable.Columns.Add(dataColumn);

			#endregion

			_dataSet.Tables.Add(pluginsInformationTable);
			pluginsInformationTable = _dataSet.Tables["pluginsInformation"]!;
		}
		public bool AddPlugin(string name, string author, string filename, string version, string description, int privilege, bool isUpperWhenPrivilege)
		{
			DataRow dataRow = pluginsInformationTable.NewRow();
			dataRow["name"] = name;
			dataRow["author"] = author;
			dataRow["filename"] = filename;
			dataRow["version"] = version;
			dataRow["description"] = description;
			dataRow["privilege"] = privilege;
			dataRow["isUpperWhenPrivilege"] = isUpperWhenPrivilege;

			// 检查插件名重复
			try
			{
				pluginsInformationTable.Rows.Add(dataRow);
				pluginsInformationTable.AcceptChanges();
			}
			catch (Exception e)
			{
				_loggerService.Error(e, "PluginsLocalStorage", "Cannot load plugin:" + name, e.Message);
				return false;
			}

			_loggerService.Info("PluginsLocalStorage", "Plugin:" + name + " is loaded exactly.");
			_loggerService.Info(name,
				"Loading from framework. Author:" + author + " ,version:" + version + ", with privilege:" + privilege);
			_loggerService.Info(name, "Description:" + description);

			//插件内部信息表的生成
			DataTable dataTable = new DataTable(name);
			DataColumn dataColumn;

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "key";
			dataTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "value";
			dataTable.Columns.Add(dataColumn);
			_dataSet.Tables.Add(dataTable);
			return true;
		}
		public void RemovePlugin(string name)
		{
			DataRow dataRow = pluginsInformationTable.AsEnumerable().First(p => ((string)p["name"]) == name);
			try
			{
				pluginsInformationTable.Rows.Remove(dataRow);
				pluginsInformationTable.AcceptChanges();
				_dataSet.Tables.Remove(name);
			}
			catch (Exception e)
			{
				_loggerService.Error(e, "PluginsLocalStorage", "Cannot remove plugin:" + name, e.Message);
				return;
			}
			_loggerService.Info("PluginsLocalStorage", $"Plugin {name} is removed successfully.");
		}
		public void RemoveAllPlugins()
		{
			pluginsInformationTable.Clear();
			_dataSet.Clear();
		}
		public string? GetAuthor(string name)
		{
			return (string?)pluginsInformationTable.AsEnumerable().First(p => ((string)p["name"]) == name)["author"];
		}
		public string? GetFileName(string name)
		{
			return (string?)pluginsInformationTable.AsEnumerable().First(p => ((string)p["name"]) == name)["filename"];
		}
		public string? GetVersion(string name)
		{
			return (string?)pluginsInformationTable.AsEnumerable().First(p => ((string)p["name"]) == name)["version"];
		}
		public string? GetDescription(string name)
		{
			return (string?)pluginsInformationTable.AsEnumerable().First(p => ((string)p["name"]) == name)["description"];
		}
		public int? GetPrivilege(string name)
		{
			return (int?)pluginsInformationTable.AsEnumerable().First(p => ((string)p["name"]) == name)["privilege"];
		}
		public IEnumerable<string>? GetPluginByPrivilege(int privilege)
		{
			return pluginsInformationTable.AsEnumerable().Where(p => (int)p["privilege"] == privilege).Select(p => (string)p["name"]);
		}
		public bool IsExists(string name)
		{
			return pluginsInformationTable.AsEnumerable().Any(p => ((string)p["name"]) == name);
		}
		public bool SetPluginInformation(string name, string key, string value)
		{
			if (_dataSet.Tables[name] is null)
			{
				_loggerService.Warn("PluginsLocalStorage", "Unexpected Error in the system. Error call for " +
														   name + " in the pluginsStorageDataSet.");
				return false;
			}
			var row = _dataSet.Tables[name]!.AsEnumerable().First(p => (string)p["key"] == key);
			if(row == null)
			{
				row = _dataSet.Tables[name]!.NewRow();
				row["key"] = key;
				row["value"] = value;
				_dataSet.Tables[name]!.Rows.Add(row);
			}
			else
			{
				row["value"] = value;
			}
			_dataSet.Tables[name]!.AcceptChanges();
			return true;
		}
		public string? GetPluginInformation(string name, string key)
		{
			if (_dataSet.Tables[name] is null)
			{
				_loggerService.Warn("PluginsLocalStorage", "Unexpected Error in the system. Error call for " +
														   name + " in the pluginsStorageDataSet.");
				return string.Empty;
			}
			return (string?)_dataSet.Tables[name]!.AsEnumerable().First(p => (string)p["key"] == key)["value"];
		}
		public bool TryGetPluginInformation(string name, string key, out string? value)
		{
			value = null;
			if (_dataSet.Tables[name] is null)
			{
				_loggerService.Warn("PluginsLocalStorage", "Unexpected Error in the system. Error call for " +
														   name + " in the pluginsStorageDataSet.");
				return false;
			}
			var row = _dataSet.Tables[name]!.AsEnumerable().First(p => (string)p["key"] == key);
			if(row == null)
			{
				return false;
			}
			value = (string)row["value"];
			return true;
		}
		public bool SetPluginInstance(string name, object instance)
		{
			return _pluginsInstanceMap.TryAdd(name, instance);
		}
		public object GetPluginInstance(string name)
		{
			return _pluginsInstanceMap[name];
		}
		public bool TryGetPluginInstance(string name, out object instance)
		{
			return _pluginsInstanceMap.TryGetValue(name, out instance!);
		}
		public List<string> GetPluginsOrderedByPrivilegeAsc()
		{
			var list = pluginsInformationTable.AsEnumerable().OrderBy(p => 2 * (int)p["privilege"] + ((bool)p["isUpperWhenPrivilege"] ? 1 : 0)).ToList();
			var ret = new List<string>();
			foreach (var plugin in list)
			{
				ret.Add((string)plugin["name"]);
			}
			return ret;
		}
	}
}
