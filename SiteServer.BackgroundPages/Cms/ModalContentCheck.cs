﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI.WebControls;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.Core.Create;
using SiteServer.CMS.Model;

namespace SiteServer.BackgroundPages.Cms
{
    public class ModalContentCheck : BasePageCms
    {
        public Literal LtlTitles;
        public DropDownList DdlCheckType;
        public DropDownList DdlTranslateChannelId;
        public TextBox TbCheckReasons;

        private Dictionary<int, List<int>> _idsDictionary = new Dictionary<int, List<int>>();
        private string _returnUrl;

        public static string GetOpenWindowString(int siteId, int channelId, string returnUrl)
        {
            return LayerUtils.GetOpenScriptWithCheckBoxValue("审核内容", PageUtils.GetCmsUrl(siteId, nameof(ModalContentCheck), new NameValueCollection
            {
                {"channelId", channelId.ToString()},
                {"ReturnUrl", StringUtils.ValueToUrl(returnUrl)}
            }), "contentIdCollection", "请选择需要审核的内容！", 560, 550);
        }

        public static string GetOpenWindowStringForMultiChannels(int siteId, string returnUrl)
        {
            return LayerUtils.GetOpenScriptWithCheckBoxValue("审核内容", PageUtils.GetCmsUrl(siteId, nameof(ModalContentCheck), new NameValueCollection
            {
                {"ReturnUrl", StringUtils.ValueToUrl(returnUrl)}
            }), "IDsCollection", "请选择需要审核的内容！", 560, 550);
        }

        public static string GetOpenWindowString(int siteId, int channelId, int contentId, string returnUrl)
        {
            return LayerUtils.GetOpenScript("审核内容", PageUtils.GetCmsUrl(siteId, nameof(ModalContentCheck), new NameValueCollection
            {
                {"channelId", channelId.ToString()},
                {"contentIdCollection", contentId.ToString()},
                {"ReturnUrl", StringUtils.ValueToUrl(returnUrl)}
            }), 560, 550);
        }

        public static string GetRedirectUrl(int siteId, int channelId, int contentId, string returnUrl)
        {
            return PageUtils.GetCmsUrl(siteId, nameof(ModalContentCheck), new NameValueCollection
            {
                {"channelId", channelId.ToString()},
                {"ReturnUrl", StringUtils.ValueToUrl(returnUrl)},
                {"contentIdCollection", contentId.ToString()}
            });
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            PageUtils.CheckRequestParameter("siteId", "ReturnUrl");
            _returnUrl = StringUtils.ValueFromUrl(Body.GetQueryString("ReturnUrl"));

            _idsDictionary = ContentUtility.GetIDsDictionary(Request.QueryString);

            if (!IsPostBack)
            {
                var titles = new StringBuilder();
                foreach (var channelId in _idsDictionary.Keys)
                {
                    var tableName = ChannelManager.GetTableName(SiteInfo, channelId);
                    var contentIdList = _idsDictionary[channelId];
                    foreach (var contentId in contentIdList)
                    {
                        var title = DataProvider.ContentDao.GetValue(tableName, contentId, ContentAttribute.Title);
                        titles.Append(title + "<br />");
                    }
                }

                if (!string.IsNullOrEmpty(LtlTitles.Text))
                {
                    titles.Length -= 6;
                }
                LtlTitles.Text = titles.ToString();

                var checkedLevel = 5;
                var isChecked = true;

                foreach (var channelId in _idsDictionary.Keys)
                {
                    int checkedLevelByChannelId;
                    var isCheckedByChannelId = CheckManager.GetUserCheckLevel(Body.AdminName, SiteInfo, channelId, out checkedLevelByChannelId);
                    if (checkedLevel > checkedLevelByChannelId)
                    {
                        checkedLevel = checkedLevelByChannelId;
                    }
                    if (!isCheckedByChannelId)
                    {
                        isChecked = false;
                    }
                }

                CheckManager.LoadContentLevelToCheck(DdlCheckType, SiteInfo, isChecked, checkedLevel);

                var listItem = new ListItem("<保持原栏目不变>", "0");
                DdlTranslateChannelId.Items.Add(listItem);

                ChannelManager.AddListItemsForAddContent(DdlTranslateChannelId.Items, SiteInfo, true, Body.AdminName);
            }
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            var checkedLevel = TranslateUtils.ToIntWithNagetive(DdlCheckType.SelectedValue);

            var isChecked = checkedLevel >= SiteInfo.Additional.CheckContentLevel;

            var contentInfoListToCheck = new List<ContentInfo>();
            var idsDictionaryToCheck = new Dictionary<int, List<int>>();
            foreach (var channelId in _idsDictionary.Keys)
            {
                var tableName = ChannelManager.GetTableName(SiteInfo, channelId);
                var contentIdList = _idsDictionary[channelId];
                var contentIdListToCheck = new List<int>();

                int checkedLevelOfUser;
                var isCheckedOfUser = CheckManager.GetUserCheckLevel(Body.AdminName, SiteInfo, channelId, out checkedLevelOfUser);

                foreach (var contentId in contentIdList)
                {
                    var contentInfo = DataProvider.ContentDao.GetContentInfo(tableName, contentId);
                    if (contentInfo != null)
                    {
                        if (CheckManager.IsCheckable(SiteInfo, contentInfo.ChannelId, contentInfo.IsChecked, contentInfo.CheckedLevel, isCheckedOfUser, checkedLevelOfUser))
                        {
                            contentInfoListToCheck.Add(contentInfo);
                            contentIdListToCheck.Add(contentId);
                        }

                        DataProvider.ContentDao.Update(tableName, SiteInfo, contentInfo);

                        if (contentInfo.IsChecked)
                        {
                            CreateManager.CreateContentAndTrigger(SiteId, contentInfo.ChannelId, contentId);
                        }

                    }
                }
                if (contentIdListToCheck.Count > 0)
                {
                    idsDictionaryToCheck[channelId] = contentIdListToCheck;
                }
            }

            if (contentInfoListToCheck.Count == 0)
            {
                LayerUtils.CloseWithoutRefresh(Page, "alert('您的审核权限不足，无法审核所选内容！');");
            }
            else
            {
                try
                {
                    var translateChannelId = TranslateUtils.ToInt(DdlTranslateChannelId.SelectedValue);

                    foreach (var channelId in idsDictionaryToCheck.Keys)
                    {
                        var tableName = ChannelManager.GetTableName(SiteInfo, channelId);
                        var contentIdList = idsDictionaryToCheck[channelId];
                        DataProvider.ContentDao.UpdateIsChecked(tableName, SiteId, channelId, contentIdList, translateChannelId, true, Body.AdminName, isChecked, checkedLevel, TbCheckReasons.Text);

                        DataProvider.ChannelDao.UpdateContentNum(SiteInfo, channelId, true);
                    }

                    if (translateChannelId > 0)
                    {
                        DataProvider.ChannelDao.UpdateContentNum(SiteInfo, translateChannelId, true);
                    }

                    Body.AddSiteLog(SiteId, SiteId, 0, "设置内容状态为" + DdlCheckType.SelectedItem.Text, TbCheckReasons.Text);

                    if (isChecked)
                    {
                        foreach (var channelId in idsDictionaryToCheck.Keys)
                        {
                            var contentIdList = _idsDictionary[channelId];
                            if (contentIdList != null)
                            {
                                foreach (var contentId in contentIdList)
                                {
                                    CreateManager.CreateContent(SiteId, channelId, contentId);
                                }
                            }
                        }
                    }

                    LayerUtils.CloseAndRedirect(Page, _returnUrl);
                }
                catch (Exception ex)
                {
                    FailMessage(ex, "操作失败！");
                }
            }
        }
    }
}
