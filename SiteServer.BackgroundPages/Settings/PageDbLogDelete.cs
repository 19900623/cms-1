﻿using System;
using BaiRong.Core;

namespace SiteServer.BackgroundPages.Settings
{
    public class PageDbLogDelete : BasePage
    {
        protected override bool IsAccessable => true;

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            if (!IsPostBack)
            {
                VerifyAdministratorPermissions(AppManager.Permissions.Settings.Utility);
            }
        }

        public string GetLastExecuteDate()
        {
            var dt = BaiRongDataProvider.LogDao.GetLastRemoveLogDate(Body.AdminName);
            return dt == DateTime.MinValue ? "无记录" : DateUtils.GetDateAndTimeString(dt);
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            if (Page.IsPostBack && Page.IsValid)
            {
                BaiRongDataProvider.DatabaseDao.DeleteDbLog();

                Body.AddAdminLog("清空数据库日志");

                SuccessMessage("清空日志成功！");
            }
        }

    }
}
