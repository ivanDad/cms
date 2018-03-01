﻿using System;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using SiteServer.CMS.Core;
using SiteServer.CMS.Model;
using SiteServer.Utils;
using SiteServer.Utils.Enumerations;

namespace SiteServer.BackgroundPages.Settings
{
    public class PageUserAdd : BasePage
    {
        public Literal LtlPageTitle;
        public TextBox TbUserName;
        public TextBox TbDisplayName;
        public PlaceHolder PhPassword;
        public TextBox TbPassword;
        public Literal LtlPasswordTips;
        public TextBox TbEmail;
        public TextBox TbMobile;
        public Button BtnReturn;

        private int _userId;
        private string _returnUrl;

        public static string GetRedirectUrlToAdd(string returnUrl)
        {
            return PageUtils.GetSettingsUrl(nameof(PageUserAdd), new NameValueCollection
            {
                {"returnUrl", StringUtils.ValueToUrl(returnUrl) }
            });
        }

        public static string GetRedirectUrlToEdit(int userId, string returnUrl)
        {
            return PageUtils.GetSettingsUrl(nameof(PageUserAdd), new NameValueCollection
            {
                {"userID", userId.ToString() },
                {"returnUrl", StringUtils.ValueToUrl(returnUrl) }
            });
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            _userId = Body.GetQueryInt("userID");
            _returnUrl = StringUtils.ValueFromUrl(Body.GetQueryString("returnUrl"));

            if (IsPostBack) return;

            VerifyAdministratorPermissions(ConfigManager.Permissions.Settings.User);

            LtlPageTitle.Text = _userId == 0 ? "添加用户" : "编辑用户";

            if (_userId > 0)
            {
                var userInfo = DataProvider.UserDao.GetUserInfo(_userId);
                if (userInfo != null)
                {
                    TbUserName.Text = userInfo.UserName;
                    TbUserName.Enabled = false;
                    TbDisplayName.Text = userInfo.DisplayName;
                    PhPassword.Visible = false;
                    TbEmail.Text = userInfo.Email;
                    TbMobile.Text = userInfo.Mobile;
                }
            }

            if (!EUserPasswordRestrictionUtils.Equals(ConfigManager.SystemConfigInfo.UserPasswordRestriction, EUserPasswordRestriction.None))
            {
                LtlPasswordTips.Text = $"请包含{EUserPasswordRestrictionUtils.GetText(EUserPasswordRestrictionUtils.GetEnumType(ConfigManager.SystemConfigInfo.UserPasswordRestriction))}";
            }

            if (!string.IsNullOrEmpty(_returnUrl))
            {
                BtnReturn.Attributes.Add("onclick", $"window.location.href='{_returnUrl}';return false;");
            }
            else
            {
                BtnReturn.Visible = false;
            }
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            if (!Page.IsPostBack || !Page.IsValid) return;

            if (_userId == 0)
            {
                var userInfo = new UserInfo
                {
                    UserName = TbUserName.Text,
                    Password = TbPassword.Text,
                    CreateDate = DateTime.Now,
                    LastActivityDate = DateUtils.SqlMinValue,
                    IsChecked = true,
                    IsLockedOut = false,
                    DisplayName = TbDisplayName.Text,
                    Email = TbEmail.Text,
                    Mobile = TbMobile.Text
                };

                string errorMessage;
                var isCreated = DataProvider.UserDao.Insert(userInfo, userInfo.Password, string.Empty, out errorMessage);

                if (isCreated)
                {
                    Body.AddAdminLog("添加用户",
                        $"用户:{TbUserName.Text}");

                    SuccessMessage("用户添加成功，可以继续添加！");
                    AddWaitAndRedirectScript(GetRedirectUrlToAdd(_returnUrl));
                }
                else
                {
                    FailMessage($"用户添加失败：<br>{errorMessage}");
                }
            }
            else
            {
                var userInfo = DataProvider.UserDao.GetUserInfo(_userId);

                userInfo.DisplayName = TbDisplayName.Text;
                userInfo.Email = TbEmail.Text;
                userInfo.Mobile = TbMobile.Text;

                DataProvider.UserDao.Update(userInfo);

                Body.AddAdminLog("修改用户",
                    $"用户:{TbUserName.Text}");

                SuccessMessage("用户修改成功！");
                AddWaitAndRedirectScript(_returnUrl);
            }
        }
    }
}