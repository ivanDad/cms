﻿using System;
using SiteServer.CMS.Core;

namespace SiteServer.BackgroundPages.Settings
{
	public class PageUtilityJsMin : BasePage
    {
        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            if (!IsPostBack)
            {
                VerifyAdministratorPermissions(ConfigManager.Permissions.Settings.Utility);
            }
        }
	}
}
