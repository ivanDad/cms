﻿using System;
using System.Collections.Specialized;
using SiteServer.CMS.Controllers.Sys.Packaging;
using SiteServer.CMS.Core;
using SiteServer.CMS.Plugin;
using SiteServer.Utils;

namespace SiteServer.BackgroundPages.Plugins
{
    public class PageInstall : BasePage
    {
        public static string GetRedirectUrl(bool isUpdate, string packageId)
        {
            return PageUtils.GetPluginsUrl(nameof(PageInstall), new NameValueCollection
            {
                { "type", isUpdate ? "update" : "install" },
                { "packageId", packageId }
            });
        }

        public string Type => Body.GetQueryString("type") == "update" ? "升级" : "安装";

        public string AdminUrl => PageUtils.GetAdminDirectoryUrl(string.Empty);

        public string PackagesIdAndVersionList => TranslateUtils.JsonSerialize(PluginManager.PackagesIdAndVersionList);

        public string PackageId { get; set; }

        public string DownloadApiUrl => ApiRouteDownload.GetUrl(PageUtility.InnerApiUrl);

        public string UpdateApiUrl => ApiRouteUpdate.GetUrl(PageUtility.InnerApiUrl);

        public string ClearCacheApiUrl => ApiRouteClearCache.GetUrl(PageUtility.InnerApiUrl);

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            PackageId = Body.GetQueryString("packageId");

            if (IsPostBack) return;

            VerifyAdministratorPermissions(ConfigManager.Permissions.Plugins.Add, ConfigManager.Permissions.Plugins.Management);
        }
    }
}
