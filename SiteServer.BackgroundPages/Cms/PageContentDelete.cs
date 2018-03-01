﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI.WebControls;
using SiteServer.Utils;
using SiteServer.BackgroundPages.Core;
using SiteServer.CMS.Core;
using SiteServer.CMS.Core.Create;
using SiteServer.CMS.Model;

namespace SiteServer.BackgroundPages.Cms
{
    public class PageContentDelete : BasePageCms
    {
        public Literal LtlContents;
        public PlaceHolder PhRetain;
        public RadioButtonList RblRetainFiles;

        private Dictionary<int, List<int>> _idsDictionary = new Dictionary<int, List<int>>();
        private bool _isDeleteFromTrash;
        private string _returnUrl;

        public static string GetRedirectClickStringForMultiChannels(int siteId, bool isDeleteFromTrash,
            string returnUrl)
        {
            return PageUtils.GetRedirectStringWithCheckBoxValue(PageUtils.GetCmsUrl(siteId, nameof(PageContentDelete),
                new NameValueCollection
                {
                    {"IsDeleteFromTrash", isDeleteFromTrash.ToString()},
                    {"ReturnUrl", StringUtils.ValueToUrl(returnUrl)}
                }), "IDsCollection", "IDsCollection", "请选择需要删除的内容！");
        }

        public static string GetRedirectClickStringForSingleChannel(int siteId, int channelId,
            bool isDeleteFromTrash, string returnUrl)
        {
            return PageUtils.GetRedirectStringWithCheckBoxValue(PageUtils.GetCmsUrl(siteId, nameof(PageContentDelete),
                new NameValueCollection
                {
                    {"channelId", channelId.ToString()},
                    {"IsDeleteFromTrash", isDeleteFromTrash.ToString()},
                    {"ReturnUrl", StringUtils.ValueToUrl(returnUrl)}
                }), "contentIdCollection", "contentIdCollection", "请选择需要删除的内容！");
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            PageUtils.CheckRequestParameter("siteId", "ReturnUrl");
            _returnUrl = StringUtils.ValueFromUrl(Body.GetQueryString("ReturnUrl"));
            _isDeleteFromTrash = Body.GetQueryBool("IsDeleteFromTrash");
            _idsDictionary = ContentUtility.GetIDsDictionary(Request.QueryString);

            //if (this.channelId > 0)
            //{
            //    this.nodeInfo = NodeManager.GetChannelInfo(base.SiteId, this.channelId);
            //}
            //else
            //{
            //    this.nodeInfo = NodeManager.GetChannelInfo(base.SiteId, -this.channelId);
            //}
            //if (this.nodeInfo != null)
            //{
            //    this.tableStyle = NodeManager.GetTableStyle(base.SiteInfo, nodeInfo);
            //    this.tableName = NodeManager.GetTableName(base.SiteInfo, nodeInfo);
            //}

            //if (this.contentID == 0)
            //{
            //    if (!base.HasChannelPermissions(Math.Abs(this.channelId), AppManager.CMS.Permission.Channel.ContentDelete))
            //    {
            //        PageUtils.RedirectToErrorPage("您没有删除此栏目内容的权限！");
            //        return;
            //    }
            //}
            //else
            //{
            //    ContentInfo contentInfo = DataProvider.ContentDAO.GetContentInfo(this.tableStyle, this.tableName, this.contentID);

            //    if (contentInfo == null || !string.Equals(Body.AdminName, contentInfo.AddUserName))
            //    {
            //        if (!base.HasChannelPermissions(Math.Abs(this.channelId), AppManager.CMS.Permission.Channel.ContentDelete))
            //        {
            //            PageUtils.RedirectToErrorPage("您没有删除此栏目内容的权限！");
            //            return;
            //        }
            //    }
            //}

            if (IsPostBack) return;

            var builder = new StringBuilder();
            foreach (var channelId in _idsDictionary.Keys)
            {
                var tableName = ChannelManager.GetTableName(SiteInfo, channelId);
                var contentIdList = _idsDictionary[channelId];
                foreach (var contentId in contentIdList)
                {
                    var contentInfo = DataProvider.ContentDao.GetContentInfo(tableName, contentId);
                    if (contentInfo != null)
                    {
                        builder.Append(
                            $@"{WebUtils.GetContentTitle(SiteInfo, contentInfo, _returnUrl)}<br />");
                    }
                }
            }
            LtlContents.Text = builder.ToString();

            if (!_isDeleteFromTrash)
            {
                PhRetain.Visible = true;
                InfoMessage("此操作将把所选内容放入回收站，确定吗？");
            }
            else
            {
                PhRetain.Visible = false;
                InfoMessage("此操作将从回收站中彻底删除所选内容，确定吗？");
            }
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            if (!Page.IsPostBack || !Page.IsValid) return;

            try
            {
                foreach (var channelId in _idsDictionary.Keys)
                {
                    var tableName = ChannelManager.GetTableName(SiteInfo, channelId);
                    var contentIdList = _idsDictionary[channelId];

                    if (!_isDeleteFromTrash)
                    {
                        if (bool.Parse(RblRetainFiles.SelectedValue) == false)
                        {
                            DirectoryUtility.DeleteContents(SiteInfo, channelId, contentIdList);
                            SuccessMessage("成功删除内容以及生成页面！");
                        }
                        else
                        {
                            SuccessMessage("成功删除内容，生成页面未被删除！");
                        }

                        if (contentIdList.Count == 1)
                        {
                            var contentId = contentIdList[0];
                            var contentTitle = DataProvider.ContentDao.GetValue(tableName, contentId, ContentAttribute.Title);
                            Body.AddSiteLog(SiteId, channelId, contentId, "删除内容",
                                $"栏目:{ChannelManager.GetChannelNameNavigation(SiteId, channelId)},内容标题:{contentTitle}");
                        }
                        else
                        {
                            Body.AddSiteLog(SiteId, "批量删除内容",
                                $"栏目:{ChannelManager.GetChannelNameNavigation(SiteId, channelId)},内容条数:{contentIdList.Count}");
                        }

                        DataProvider.ContentDao.TrashContents(SiteId, tableName, contentIdList);

                        //引用内容，需要删除
                        var tableList = DataProvider.TableDao.GetTableCollectionInfoListCreatedInDb();
                        foreach (var table in tableList)
                        {
                            var targetContentIdList = DataProvider.ContentDao.GetReferenceIdList(table.TableName, contentIdList);
                            if (targetContentIdList.Count > 0)
                            {
                                var targetContentInfo = DataProvider.ContentDao.GetContentInfo(table.TableName, TranslateUtils.ToInt(targetContentIdList[0].ToString()));
                                DataProvider.ContentDao.DeleteContents(targetContentInfo.SiteId, table.TableName, targetContentIdList, targetContentInfo.ChannelId);
                            }
                        }

                        CreateManager.CreateContentTrigger(SiteId, channelId);
                    }
                    else
                    {
                        SuccessMessage("成功从回收站清空内容！");
                        DataProvider.ContentDao.DeleteContents(SiteId, tableName, contentIdList, channelId);

                        Body.AddSiteLog(SiteId, "从回收站清空内容", $"内容条数:{contentIdList.Count}");
                    }
                }


                AddWaitAndRedirectScript(_returnUrl);
            }
            catch (Exception ex)
            {
                LogUtils.AddSystemErrorLog(ex);
                FailMessage(ex, "删除内容失败！");
            }
        }

        public void Return_OnClick(object sender, EventArgs e)
        {
            PageUtils.Redirect(_returnUrl);
        }

    }
}
