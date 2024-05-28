using SoruxBot.Kernel.Bot;
using SoruxBot.Kernel.Interface;
using SoruxBot.SDK.Plugins.Service;
using System.Data;
using System.Text;


namespace SoruxBot.Kernel.Services.StorageService
{
	public class PluginsLocalStorage : IPluginsStorage
	{
		private readonly ILoggerService _loggerService;
		private BotContext _botContext;
		private static readonly DataSet DataSet = new DataSet();
		private DataTable _pluginsInformationTable;
		private readonly Dictionary<string, object> _pluginsInstanceMap = new();
		
		public PluginsLocalStorage(BotContext botContext, ILoggerService loggerService)
		{
			this._botContext = botContext;
			this._loggerService = loggerService;
			//初始化数据库
			InitDataSet();
		}
		private void InitDataSet()
		{
			_pluginsInformationTable = new DataTable("pluginsInformation");
			DataColumn dataColumn;

			#region 插件信息生成

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "name";
			dataColumn.DataType = typeof(string);
			dataColumn.Unique = true;
			dataColumn.AutoIncrement = false;
			_pluginsInformationTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "author";
			dataColumn.DataType = typeof(string);
			dataColumn.AutoIncrement = false;
			_pluginsInformationTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "filename";
			dataColumn.DataType = typeof(string);
			dataColumn.AutoIncrement = false;
			_pluginsInformationTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "version";
			dataColumn.DataType = typeof(string);
			dataColumn.AutoIncrement = false;
			_pluginsInformationTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "description";
			dataColumn.DataType = typeof(string);
			dataColumn.AutoIncrement = false;
			_pluginsInformationTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "privilege";
			dataColumn.DataType = typeof(int);
			dataColumn.AutoIncrement = false;
			_pluginsInformationTable.Columns.Add(dataColumn);

			dataColumn = new DataColumn();
			dataColumn.ColumnName = "isUpperWhenPrivilege";
			dataColumn.DataType = typeof(bool);
			dataColumn.AutoIncrement = false;
			_pluginsInformationTable.Columns.Add(dataColumn);

			#endregion

			DataSet.Tables.Add(_pluginsInformationTable);
			_pluginsInformationTable = DataSet.Tables["pluginsInformation"]!;
		}
		
		public bool AddPlugin(string name, string author, string filename, string version, string description, int privilege, bool isUpperWhenPrivilege)
		{
			DataRow dataRow = _pluginsInformationTable.NewRow();
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
				_pluginsInformationTable.Rows.Add(dataRow);
				_pluginsInformationTable.AcceptChanges();
			}
			catch (Exception e)
			{
				_loggerService.Error(e, 
					Constant.NameValue.KernelPluginServiceLocalStorageLogName, 
					"Cannot load plugin:" + name, e.Message);
				return false;
			}

			_loggerService.Info(Constant.NameValue.KernelPluginServiceLocalStorageLogName
				, "Plugin:" + name + " is loaded exactly.");
			
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
			DataSet.Tables.Add(dataTable);
			return true;
		}
		public void RemovePlugin(string name)
		{
			DataRow dataRow = _pluginsInformationTable.AsEnumerable().First(p => ((string)p["name"]) == name);
			try
			{
				_pluginsInformationTable.Rows.Remove(dataRow);
				_pluginsInformationTable.AcceptChanges();
				DataSet.Tables.Remove(name);
			}
			catch (Exception e)
			{
				_loggerService.Error(e, Constant.NameValue.KernelPluginServiceLocalStorageLogName, "Cannot remove plugin:" + name, e.Message);
				return;
			}
			_loggerService.Info(Constant.NameValue.KernelPluginServiceLocalStorageLogName, $"Plugin {name} is removed successfully.");
		}
		public void RemoveAllPlugins()
		{
			_pluginsInformationTable.Clear();
			DataSet.Clear();
		}
		public string? GetAuthor(string name)
		{
			return (string?)_pluginsInformationTable.AsEnumerable().First(p => ((string)p["name"]) == name)["author"];
		}
		public string? GetFileName(string name)
		{
			return (string?)_pluginsInformationTable.AsEnumerable().First(p => ((string)p["name"]) == name)["filename"];
		}
		public string? GetVersion(string name)
		{
			return (string?)_pluginsInformationTable.AsEnumerable().First(p => ((string)p["name"]) == name)["version"];
		}
		public string? GetDescription(string name)
		{
			return (string?)_pluginsInformationTable.AsEnumerable().First(p => ((string)p["name"]) == name)["description"];
		}
		public int? GetPrivilege(string name)
		{
			return (int?)_pluginsInformationTable.AsEnumerable().First(p => ((string)p["name"]) == name)["privilege"];
		}
		public IEnumerable<string>? GetPluginByPrivilege(int privilege)
		{
			return _pluginsInformationTable.AsEnumerable().Where(p => (int)p["privilege"] == privilege).Select(p => (string)p["name"]);
		}
		public bool IsExists(string name)
		{
			return _pluginsInformationTable.AsEnumerable().Any(p => ((string)p["name"]) == name);
		}
		public bool SetPluginInformation(string name, string key, string value)
		{
			if (DataSet.Tables[name] is null)
			{
				_loggerService.Warn("PluginsLocalStorage", "Unexpected Error in the system. Error call for " +
														   name + " in the pluginsStorageDataSet.");
				return false;
			}
			var row = DataSet.Tables[name]!.AsEnumerable().FirstOrDefault(p => (string)p["key"] == key);
			// 添加这个row 
			if(row is null)
			{
				row = DataSet.Tables[name]!.NewRow();
				row["key"] = key;
				row["value"] = value;
			}
			else
			{
				row["value"] = value;
			}
			DataSet.Tables[name]!.AcceptChanges();
			return true;
		}
		public string? GetPluginInformation(string name, string key)
		{
			if (DataSet.Tables[name] is null)
			{
				_loggerService.Warn("PluginsLocalStorage", "Unexpected Error in the system. Error call for " +
														   name + " in the pluginsStorageDataSet.");
				return string.Empty;
			}
			return (string?)DataSet.Tables[name]!.AsEnumerable().FirstOrDefault(p => (string)p["key"] == key)?["value"];
		}
		public bool TryGetPluginInformation(string name, string key, out string? value)
		{
			value = null;
			if (DataSet.Tables[name] is null)
			{
				_loggerService.Warn("PluginsLocalStorage", "Unexpected Error in the system. Error call for " +
														   name + " in the pluginsStorageDataSet.");
				return false;
			}
			var row = DataSet.Tables[name]!.AsEnumerable().FirstOrDefault(p => (string)p["key"] == key);
			if (row is null)
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
			var list = _pluginsInformationTable.AsEnumerable().OrderBy(p => 2 * (int)p["privilege"] + ((bool)p["isUpperWhenPrivilege"] ? 1 : 0)).ToList();
			var ret = new List<string>();
			foreach (var plugin in list)
			{
				ret.Add((string)plugin["name"]);
			}
			return ret;
		}
	}
}
