using System;

namespace SiteServer.CMS.Model
{
	public class ConfigInfo
	{
	    public ConfigInfo()
	    {
	        Id = 0;
            IsInitialized = false;
            DatabaseVersion = string.Empty;
            UpdateDate = DateTime.Now;
            SystemConfig = string.Empty;
		}

        public ConfigInfo(bool isInitialized, string databaseVersion, DateTime updateDate, string systemConfig) 
		{
            IsInitialized = isInitialized;
            DatabaseVersion = databaseVersion;
            UpdateDate = updateDate;
            SystemConfig = systemConfig;
		}

        public int Id { get; set; }

        public bool IsInitialized { get; set; }

	    public string DatabaseVersion { get; set; }

	    public DateTime UpdateDate { get; set; }

	    public string SystemConfig { get; set; }

	    private SystemConfigInfo _systemConfigInfo;
	    public SystemConfigInfo SystemConfigInfo => _systemConfigInfo ?? (_systemConfigInfo = new SystemConfigInfo(SystemConfig));
	}
}
