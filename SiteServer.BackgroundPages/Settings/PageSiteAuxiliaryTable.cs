﻿using System;
using System.Web.UI.WebControls;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.Model;

namespace SiteServer.BackgroundPages.Settings
{
	public class PageSiteAuxiliaryTable : BasePageCms
    {
		public Repeater RptContents;
        public Button BtnAdd;

        public static string GetRedirectUrl()
	    {
	        return PageUtils.GetSettingsUrl(nameof(PageSiteAuxiliaryTable), null);
	    }

		public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

			if (Body.IsQueryExists("Delete"))
			{
                var enName = Body.GetQueryString("ENName");//辅助表
                var enNameArchive = enName + "_Archive";//辅助表归档
			
				try
				{
                    DataProvider.TableDao.DeleteCollectionTableInfoAndDbTable(enName);//删除辅助表
                    DataProvider.TableDao.DeleteCollectionTableInfoAndDbTable(enNameArchive);//删除辅助表归档

                    Body.AddAdminLog("删除辅助表", $"辅助表:{enName}");

					SuccessDeleteMessage();
				}
				catch(Exception ex)
				{
                    FailDeleteMessage(ex);
				}
			}

            if (IsPostBack) return;

            VerifyAdministratorPermissions(ConfigManager.Permissions.Settings.Site);

            RptContents.DataSource = DataProvider.TableDao.GetTableCollectionInfoList();
            RptContents.ItemDataBound += RptContents_ItemDataBound;
            RptContents.DataBind();

            BtnAdd.OnClientClick = ModalAuxiliaryTableAdd.GetOpenWindowString();
        }

        private void RptContents_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var collectionInfo = (TableInfo)e.Item.DataItem;
            var tableName = collectionInfo.TableName;
            //var isHighlight = !collectionInfo.IsCreatedInDb || collectionInfo.IsChangedAfterCreatedInDb;
            var isTableUsed = DataProvider.SiteDao.IsTableUsed(tableName);

            //if (isHighlight) e.Item.Attributes.Add("style", "color: red");

            var ltlTableName = (Literal)e.Item.FindControl("ltlTableName");
            var ltlDisplayName = (Literal)e.Item.FindControl("ltlDisplayName");
            var ltlIsUsed = (Literal)e.Item.FindControl("ltlIsUsed");
            var ltlIsCreatedInDb = (Literal)e.Item.FindControl("ltlIsCreatedInDB");
            var ltlIsChangedAfterCreatedInDb = (Literal)e.Item.FindControl("ltlIsChangedAfterCreatedInDb");
            var ltlMetadataEdit = (Literal)e.Item.FindControl("ltlMetadataEdit");
            var ltlStyleEdit = (Literal)e.Item.FindControl("ltlStyleEdit");
            var ltlEdit = (Literal)e.Item.FindControl("ltlEdit");
            var ltlDelete = (Literal)e.Item.FindControl("ltlDelete");

            ltlTableName.Text = tableName;
            ltlDisplayName.Text = collectionInfo.DisplayName;
            ltlIsUsed.Text = StringUtils.GetBoolText(isTableUsed);
            ltlIsCreatedInDb.Text = StringUtils.GetBoolText(collectionInfo.IsCreatedInDb);
            ltlIsChangedAfterCreatedInDb.Text = collectionInfo.IsCreatedInDb == false
                ? "----"
                : StringUtils.GetBoolText(collectionInfo.IsChangedAfterCreatedInDb);

            ltlMetadataEdit.Text =
                $@"<a href=""{PageSiteTableMetadata.GetRedirectUrl(tableName)}"">管理真实字段</a>";

            ltlStyleEdit.Text = $@"<a href=""{PageSiteTableStyle.GetRedirectUrl(tableName)}"">管理虚拟字段</a>";

            ltlEdit.Text = $@"<a href=""javascript:;"" onclick=""{ModalAuxiliaryTableAdd.GetOpenWindowString(tableName)}"">编辑</a>";

            if (!isTableUsed)
            {
                var script = AlertUtils.Warning("删除辅助表", $"此操作将删除辅助表“{tableName}”，如果辅助表已在数据库中建立，将同时删除建立的辅助表，确认吗？", "取 消",
                    "确认删除", $"location.href = '{GetRedirectUrl()}?Delete=True&ENName={tableName}';");
                ltlDelete.Text =
                $@"<a href=""javascript:;"" onClick=""{script}"">删除</a>";
            }
        }
	}
}
