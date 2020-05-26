using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class FlowRequest
    {
        /// <summary>
        /// 审批状态
        /// </summary>
        public int PerformState { get; set; }
    }

    public class ChildNoteModel
    {
        public int childNodeCode { get; set; }
        public bool blankNode { get; set; }
    }

    public class UserInfo
    {
        public const string ACCOUNT_ID = "PM_Account_ID";
        public const string COMPANY_CODE = "PM_Company_Code";
        public const string COMPANY_NAME = "PM_Company_Name";
        public const string CONNECTION_STRING = "PM_Account_ConnString";
        public const string DEPARTMENT_CODE = "PM_Department_Code";
        public const string DEPARTMENT_NAME = "PM_Department_Name";
        public const string EMPLOYEE_CODE = "PM_Employee_Code";
        public const string EMPLOYEE_NAME = "PM_Employee_Name";
        public const string LOGIN_TIME = "PM_Login_Time";
        public const string ORG_CODE = "PM_Org_Code";
        public const string ORG_NAME = "PM_Org_Name";
        public const string QSX_CODE = "QSX_Code";
        public const string QSX_NAME = "QSX_Name";
        public const string USER_BELONG = "User_Belong";
        public const string USER_CODE = "PM_User_Code";
        public const string USER_NAME = "PM_User_Name";
    }
}
