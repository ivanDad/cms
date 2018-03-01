using System;
using SiteServer.Utils;
using SiteServer.Utils.Enumerations;

namespace SiteServer.CMS.Model
{
    public class AdministratorInfo
    {
        private string _displayName;

        public AdministratorInfo()
        {
            Id = 0;
            UserName = string.Empty;
            Password = string.Empty;
            PasswordFormat = EPasswordFormat.Encrypted;
            PasswordSalt = string.Empty;
            CreationDate = DateUtils.SqlMinValue;
            LastActivityDate = DateUtils.SqlMinValue;
            CountOfLogin = 0;
            CountOfFailedLogin = 0;
            CreatorUserName = string.Empty;
            IsLockedOut = false;
            SiteIdCollection = string.Empty;
            SiteId = 0;
            DepartmentId = 0;
            AreaId = 0;
            _displayName = string.Empty;
            Email = string.Empty;
            Mobile = string.Empty;
        }

        public AdministratorInfo(int id, string userName, string password, EPasswordFormat passwordFormat, string passwordSalt, DateTime creationDate, DateTime lastActivityDate, int countOfLogin, int countOfFailedLogin, string creatorUserName, bool isLockedOut, string siteIdCollection, int siteId, int departmentId, int areaId, string displayName, string email, string mobile)
        {
            Id = id;
            UserName = userName;
            Password = password;
            PasswordFormat = passwordFormat;
            PasswordSalt = passwordSalt;
            CreationDate = creationDate;
            LastActivityDate = lastActivityDate;
            CountOfLogin = countOfLogin;
            CountOfFailedLogin = countOfFailedLogin;
            CreatorUserName = creatorUserName;
            IsLockedOut = isLockedOut;
            SiteIdCollection = siteIdCollection;
            SiteId = siteId;
            DepartmentId = departmentId;
            AreaId = areaId;
            _displayName = displayName;
            Email = email;
            Mobile = mobile;
        }

        public int Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public EPasswordFormat PasswordFormat { get; set; }

        public string PasswordSalt { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastActivityDate { get; set; }

        public int CountOfLogin { get; set; }

        public int CountOfFailedLogin { get; set; }

        public string CreatorUserName { get; set; }

        public bool IsLockedOut { get; set; }

        public string SiteIdCollection { get; set; }

        public int SiteId { get; set; }

        public int DepartmentId { get; set; }

        public int AreaId { get; set; }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_displayName))
                {
                    _displayName = UserName;
                }
                return _displayName;
            }
            set { _displayName = value; }
        }

        public string Email { get; set; }

        public string Mobile { get; set; }
    }
}
