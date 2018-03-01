﻿using System;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using SiteServer.CMS.Core;
using SiteServer.CMS.Model;
using SiteServer.Utils;

namespace SiteServer.BackgroundPages.Settings
{
	public class PageAdministratorAdd : BasePage
    {
        public Literal LtlPageTitle;
        public DropDownList DdlDepartmentId;
        public TextBox TbUserName;
        public TextBox TbDisplayName;
        public PlaceHolder PhPassword;
        public TextBox TbPassword;
        public DropDownList DdlAreaId;
        public TextBox TbEmail;
        public TextBox TbMobile;
        public Button BtnReturn;

        private int _departmentId;
        private int _areaId;
        private string _userName;
        private bool[] _isLastNodeArrayOfDepartment;
        private bool[] _isLastNodeArrayOfArea;

        public static string GetRedirectUrlToAdd(int departmentId)
        {
            return PageUtils.GetSettingsUrl(nameof(PageAdministratorAdd), new NameValueCollection
            {
                {"departmentID", departmentId.ToString()}
            });
        }

        public static string GetRedirectUrlToEdit(int departmentId, string userName)
        {
            return PageUtils.GetSettingsUrl(nameof(PageAdministratorAdd), new NameValueCollection
            {
                {"departmentID", departmentId.ToString()},
                {"userName", userName}
            });
        }

		public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            _departmentId = Body.GetQueryInt("departmentID");
            _areaId = Body.GetQueryInt("areaID");
            _userName = Body.GetQueryString("userName");

            if (Page.IsPostBack) return;

            VerifyAdministratorPermissions(ConfigManager.Permissions.Settings.Admin);

            LtlPageTitle.Text = string.IsNullOrEmpty(_userName) ? "添加管理员" : "编辑管理员";

            DdlDepartmentId.Items.Add(new ListItem("<无所属部门>", "0"));
            var departmentIdList = DepartmentManager.GetDepartmentIdList();
            var count = departmentIdList.Count;
            _isLastNodeArrayOfDepartment = new bool[count];
            foreach (var theDepartmentId in departmentIdList)
            {
                var departmentInfo = DepartmentManager.GetDepartmentInfo(theDepartmentId);
                var listitem = new ListItem(GetDepartment(departmentInfo.DepartmentName, departmentInfo.ParentsCount, departmentInfo.IsLastNode), theDepartmentId.ToString());
                if (_departmentId == theDepartmentId)
                {
                    listitem.Selected = true;
                }
                DdlDepartmentId.Items.Add(listitem);
            }

            DdlAreaId.Items.Add(new ListItem("<无所在区域>", "0"));
            var areaIdList = AreaManager.GetAreaIdList();
            count = areaIdList.Count;
            _isLastNodeArrayOfArea = new bool[count];
            foreach (var theAreaId in areaIdList)
            {
                var areaInfo = AreaManager.GetAreaInfo(theAreaId);
                var listitem = new ListItem(GetArea(areaInfo.AreaName, areaInfo.ParentsCount, areaInfo.IsLastNode), theAreaId.ToString());
                if (_areaId == theAreaId)
                {
                    listitem.Selected = true;
                }
                DdlAreaId.Items.Add(listitem);
            }

            if (!string.IsNullOrEmpty(_userName))
            {
                var adminInfo = DataProvider.AdministratorDao.GetByUserName(_userName);
                if (adminInfo != null)
                {
                    ControlUtils.SelectSingleItem(DdlDepartmentId, adminInfo.DepartmentId.ToString());
                    TbUserName.Text = adminInfo.UserName;
                    TbUserName.Enabled = false;
                    TbDisplayName.Text = adminInfo.DisplayName;
                    PhPassword.Visible = false;
                    ControlUtils.SelectSingleItem(DdlAreaId, adminInfo.AreaId.ToString());
                    TbEmail.Text = adminInfo.Email;
                    TbMobile.Text = adminInfo.Mobile;
                }
            }

            BtnReturn.Attributes.Add("onclick", $"location.href='{PageAdministrator.GetRedirectUrl()}';return false;");
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            if (!Page.IsPostBack || !Page.IsValid) return;

            if (string.IsNullOrEmpty(_userName))
            {
                var adminInfo = new AdministratorInfo
                {
                    UserName = TbUserName.Text.Trim(),
                    Password = TbPassword.Text,
                    CreatorUserName = Body.AdminName,
                    DisplayName = TbDisplayName.Text,
                    Email = TbEmail.Text,
                    Mobile = TbMobile.Text,
                    DepartmentId = TranslateUtils.ToInt(DdlDepartmentId.SelectedValue),
                    AreaId = TranslateUtils.ToInt(DdlAreaId.SelectedValue)
                };

                if (!string.IsNullOrEmpty(DataProvider.AdministratorDao.GetUserNameByEmail(TbEmail.Text)))
                {
                    FailMessage("管理员添加失败，邮箱地址已存在");
                    return;
                }

                if (!string.IsNullOrEmpty(DataProvider.AdministratorDao.GetUserNameByMobile(TbMobile.Text)))
                {
                    FailMessage("管理员添加失败，手机号码已存在");
                    return;
                }

                string errorMessage;
                if (!AdminManager.CreateAdministrator(adminInfo, out errorMessage))
                {
                    FailMessage($"管理员添加失败：{errorMessage}");
                    return;   
                }

                Body.AddAdminLog("添加管理员", $"管理员:{TbUserName.Text.Trim()}");
                SuccessMessage("管理员添加成功！");
                AddWaitAndRedirectScript(PageAdministrator.GetRedirectUrl());
            }
            else
            {
                var adminInfo = DataProvider.AdministratorDao.GetByUserName(_userName);

                if (adminInfo.Email != TbEmail.Text && !string.IsNullOrEmpty(DataProvider.AdministratorDao.GetUserNameByEmail(TbEmail.Text)))
                {
                    FailMessage("管理员设置失败，邮箱地址已存在");
                    return;
                }

                if (adminInfo.Mobile != TbMobile.Text && !string.IsNullOrEmpty(DataProvider.AdministratorDao.GetUserNameByMobile(adminInfo.Mobile)))
                {
                    FailMessage("管理员设置失败，手机号码已存在");
                    return;
                }

                adminInfo.DisplayName = TbDisplayName.Text;
                adminInfo.Email = TbEmail.Text;
                adminInfo.Mobile = TbMobile.Text;
                adminInfo.DepartmentId = TranslateUtils.ToInt(DdlDepartmentId.SelectedValue);
                adminInfo.AreaId = TranslateUtils.ToInt(DdlAreaId.SelectedValue);

                DataProvider.AdministratorDao.Update(adminInfo);

                Body.AddAdminLog("修改管理员属性", $"管理员:{TbUserName.Text.Trim()}");
                SuccessMessage("管理员设置成功！");
                AddWaitAndRedirectScript(PageAdministrator.GetRedirectUrl());
            }
        }

        private string GetDepartment(string departmentName, int parentsCount, bool isLastNode)
        {
            var str = "";
            if (isLastNode == false)
            {
                _isLastNodeArrayOfDepartment[parentsCount] = false;
            }
            else
            {
                _isLastNodeArrayOfDepartment[parentsCount] = true;
            }
            for (var i = 0; i < parentsCount; i++)
            {
                str = string.Concat(str, _isLastNodeArrayOfDepartment[i] ? "　" : "│");
            }
            str = string.Concat(str, isLastNode ? "└" : "├");
            str = string.Concat(str, departmentName);
            return str;
        }

        private string GetArea(string areaName, int parentsCount, bool isLastNode)
        {
            var str = "";
            if (isLastNode == false)
            {
                _isLastNodeArrayOfArea[parentsCount] = false;
            }
            else
            {
                _isLastNodeArrayOfArea[parentsCount] = true;
            }
            for (var i = 0; i < parentsCount; i++)
            {
                str = string.Concat(str, _isLastNodeArrayOfArea[i] ? "　" : "│");
            }
            str = string.Concat(str, isLastNode ? "└" : "├");
            str = string.Concat(str, areaName);
            return str;
        }
    }
}
