﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using BaiRong.Core;
using BaiRong.Core.Model;
using BaiRong.Core.Model.Enumerations;
using BaiRong.Core.Table;
using SiteServer.Plugin.Models;

namespace SiteServer.BackgroundPages.Cms
{
	public class ModalTableStyleValidateAdd : BasePageCms
    {
        public DropDownList DdlIsValidate;
        public PlaceHolder PhValidate;
        public DropDownList DdlIsRequired;
        public PlaceHolder PhNum;
        public TextBox TbMinNum;
        public TextBox TbMaxNum;
        public DropDownList DdlValidateType;
        public PlaceHolder PhRegExp;
        public TextBox TbRegExp;
        public TextBox TbErrorMessage;

        private int _tableStyleId;
        private List<int> _relatedIdentities;
        private string _tableName;
        private string _attributeName;
        private string _redirectUrl;
        private TableStyleInfo _styleInfo;

        public static string GetOpenWindowString(int tableStyleId, List<int> relatedIdentities, string tableName, string attributeName, string redirectUrl)
        {
            return LayerUtils.GetOpenScript("设置表单验证", PageUtils.GetCmsUrl(nameof(ModalTableStyleValidateAdd), new NameValueCollection
            {
                {"TableStyleID", tableStyleId.ToString()},
                {"RelatedIdentities", TranslateUtils.ObjectCollectionToString(relatedIdentities)},
                {"TableName", tableName},
                {"AttributeName", attributeName},
                {"RedirectUrl", StringUtils.ValueToUrl(redirectUrl)}
            }));
        }

		public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            _tableStyleId = Body.GetQueryInt("TableStyleID");
            _relatedIdentities = TranslateUtils.StringCollectionToIntList(Body.GetQueryString("RelatedIdentities"));
            if (_relatedIdentities.Count == 0)
            {
                _relatedIdentities.Add(0);
            }
            _tableName = Body.GetQueryString("TableName");
            _attributeName = Body.GetQueryString("AttributeName");
            _redirectUrl = StringUtils.ValueFromUrl(Body.GetQueryString("RedirectUrl"));

            _styleInfo = _tableStyleId != 0
                ? BaiRongDataProvider.TableStyleDao.GetTableStyleInfo(_tableStyleId)
                : TableStyleManager.GetTableStyleInfo(_tableName, _attributeName, _relatedIdentities);

            if (IsPostBack) return;

            DdlIsValidate.Items[0].Value = true.ToString();
            DdlIsValidate.Items[1].Value = false.ToString();

            ControlUtils.SelectSingleItem(DdlIsValidate, _styleInfo.Additional.IsValidate.ToString());

            DdlIsRequired.Items[0].Value = true.ToString();
            DdlIsRequired.Items[1].Value = false.ToString();

            ControlUtils.SelectSingleItem(DdlIsRequired, _styleInfo.Additional.IsRequired.ToString());

            PhNum.Visible = InputTypeUtils.EqualsAny(_styleInfo.InputType, InputType.Text, InputType.TextArea);

            TbMinNum.Text = _styleInfo.Additional.MinNum.ToString();
            TbMaxNum.Text = _styleInfo.Additional.MaxNum.ToString();

            ValidateTypeUtils.AddListItems(DdlValidateType);
            ControlUtils.SelectSingleItem(DdlValidateType, ValidateTypeUtils.GetValue(_styleInfo.Additional.ValidateType));

            TbRegExp.Text = _styleInfo.Additional.RegExp;
            TbErrorMessage.Text = _styleInfo.Additional.ErrorMessage;

            DdlValidate_SelectedIndexChanged(null, EventArgs.Empty);
        }

        public void DdlValidate_SelectedIndexChanged(object sender, EventArgs e)
        {
            PhValidate.Visible = !EBooleanUtils.Equals(EBoolean.False, DdlIsValidate.SelectedValue);
            var type = ValidateTypeUtils.GetEnumType(DdlValidateType.SelectedValue);
            PhRegExp.Visible = type == ValidateType.RegExp;
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            var isChanged = InsertOrUpdateTableStyleInfo();

            if (isChanged)
            {
                LayerUtils.CloseAndRedirect(Page, _redirectUrl);
            }
		}

        private bool InsertOrUpdateTableStyleInfo()
        {
            var isChanged = false;

            _styleInfo.Additional.IsValidate = TranslateUtils.ToBool(DdlIsValidate.SelectedValue);
            _styleInfo.Additional.IsRequired = TranslateUtils.ToBool(DdlIsRequired.SelectedValue);
            _styleInfo.Additional.MinNum = TranslateUtils.ToInt(TbMinNum.Text);
            _styleInfo.Additional.MaxNum = TranslateUtils.ToInt(TbMaxNum.Text);
            _styleInfo.Additional.ValidateType = ValidateTypeUtils.GetEnumType(DdlValidateType.SelectedValue);
            _styleInfo.Additional.RegExp = TbRegExp.Text.Trim('/');
            _styleInfo.Additional.ErrorMessage = TbErrorMessage.Text;

            try
            {
                if (_tableStyleId == 0)//数据库中没有此项的表样式，但是有父项的表样式
                {
                    var relatedIdentity = _relatedIdentities[0];
                    _styleInfo.RelatedIdentity = relatedIdentity;
                    _styleInfo.TableStyleId = TableStyleManager.Insert(_styleInfo);
                }

                if (_styleInfo.TableStyleId > 0)
                {
                    TableStyleManager.Update(_styleInfo);
                    Body.AddSiteLog(PublishmentSystemId, "修改表单验证", $"字段:{_styleInfo.AttributeName}");
                }
                else
                {
                    TableStyleManager.Insert(_styleInfo);
                    Body.AddSiteLog(PublishmentSystemId, "新增表单验证", $"字段:{_styleInfo.AttributeName}");
                }
                isChanged = true;
            }
            catch (Exception ex)
            {
                FailMessage(ex, "设置表单验证失败：" + ex.Message);
            }
            return isChanged;
        }
	}
}
