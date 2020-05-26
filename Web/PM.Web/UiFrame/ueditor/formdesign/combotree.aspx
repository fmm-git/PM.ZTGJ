<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="combotree.aspx.cs" Inherits="PM.Web.UiFrame.ueditor.formdesign.combotree" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>下拉框</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=Edge,chrome=1" />
    <meta name="generator" content="www.leipi.org" />
    <link href="../../EasyUi/themes/default/easyui.css" rel="stylesheet" />
    <link href="../../EasyUi/themes/icon.css" rel="stylesheet" />
    <script src="../../EasyUi/jquery.min.js"></script>
    <script src="../../EasyUi/jquery.easyui.min.js"></script>
    <script src="../../EasyUi/easyui-lang-zh_CN.js"></script>
    <script type="text/javascript" src="../dialogs/internal.js"></script>
    <script type="text/javascript">

        function createElement(type, name, title) {
            var element = null;
            try {
                element = document.createElement('<' + type + ' id="' + name + '"' + ' name="' + name + '"' + ' title="' + title + '">');
            } catch (e) { }
            if (element == null) {
                element = document.createElement(type);
                element.name = name;
                element.id = name;
                element.setAttribute('title', title);
            }
            return element;
        }
    </script>
    <style type="text/css">
        table td {
            padding: 5px;
        }
    </style>
</head>
<body>
    <div class="content">
        <table>
            <tr>
                <td>字段名称</td>
                <td>
                    <input class="easyui-combobox" id="orgname" editable="false" panelheight="auto" panelmaxheight="120px" data-options="prompt:'必填项',required:'required',missingMessage:'控件名称不能为空'" /></td>
                <td>
                    <span style="float: left">必填项</span>
                    <input id="orgrequired" type="checkbox" style="float: left" />
                </td>
                <td>
                    <select id="controlstate" class="easyui-combobox" editable="false" panelheight="auto" panelmaxheight="120px">
                        <option value="0">读写</option>
                        <option value="1">只读</option>
                        <option value="2">隐藏</option>
                    </select>
                </td>
            </tr>
            <tr>
                <td>显示名称</td>
                <td>
                    <input class="easyui-combobox" id="orgtext" editable="false" panelheight="auto" panelmaxheight="120px" data-options="prompt:'必填项',required:'required',missingMessage:'显示名称不能为空'" />
                </td>
                <td>父级字段</td>
                <td>
                    <input class="easyui-combobox" id="orgparentfields" editable="false" panelheight="auto" panelmaxheight="120px" data-options="multiple:true" /></td>
            </tr>
            <tr>
                <td>长×高</td>
                <td colspan="3">
                    <input id="orgwidth" value="100%" class="easyui-textbox" style="width: 60px" data-options="prompt:'auto'" />
                    ×
                    <input id="orgheight" value="" class="easyui-textbox" style="width: 60px" data-options="prompt:'auto'" />
                </td>
            </tr>

        </table>
    </div>
    <script type="text/javascript">
        var isGrid = editor.gridTableName;
        var physicalTableName = isGrid ? editor.gridTableName : editor.tableName;
        var oNode = null;
        var sourceTable;
        var formCode = editor.formCode;
        var thePlugins = 'combotree';
        window.onload = function () {
            InitFieldCombobox(physicalTableName);//初始化字段列表
            if (UE.plugins[thePlugins].editdom || isGrid) {
                if (isGrid) {
                    oNode = editor.hiddenObj;
                }
                else {
                    oNode = UE.plugins[thePlugins].editdom.children[0] ? UE.plugins[thePlugins].editdom.children[0] : UE.plugins[thePlugins].editdom;
                }
                sourceTable = oNode.getAttribute('sourcetable');
                var gName = isGrid ? oNode.getAttribute('fieldcode') : oNode.getAttribute('name')
                var gText = oNode.getAttribute('textfield');
                var gWidth = oNode.getAttribute('orgwidth');
                var gHeight = oNode.getAttribute('orgheight');
                var gRequired = oNode.getAttribute('required');
                var gControlState = oNode.getAttribute('controlstate');
                var gParentfields = oNode.getAttribute('orgparentfields');
                var requiredDisabled = oNode.getAttribute('requireddisabled');

                gName = gName == null ? '' : gName;
                gHeight = gHeight == null ? '' : gHeight;

                InitSourceTableFieldCombobox(sourceTable, gText);//初始化字段列表
                if (gParentfields) {
                    $('#orgparentfields').combobox('setValues', gParentfields.split(','));
                }
                $('#orgname').combobox('select', gName);
                $('#orgrequired').prop('checked', gRequired == 'true');
                $('#orgwidth').textbox('setText', gWidth);
                $('#orgheight').textbox('setText', gHeight);
                $('#controlstate').combobox('setValue', gControlState);
                $('#orgrequired').prop('disabled', requiredDisabled == "true");

                var abledStr = isGrid ? 'disable' : 'enable';
                $('#orgwidth').textbox(abledStr);
                $('#orgheight').textbox(abledStr);
                $('#orgname').combobox(abledStr);
            }
        }
        //初始化字段列表
        function InitFieldCombobox(physicalTableName) {
            $("#orgname").combobox({
                url: '<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetTableFieldsList" })) %>?PhysicalTableName=' + physicalTableName,
                textField: 'FieldName',
                valueField: 'FieldCode',
                onSelect: function (row) {
                    if (row) {
                        $('#orgrequired').prop('checked', row.AllowNull == '否');
                        $('#orgrequired').prop('disabled', row.AllowNull == '否');
                        sourceTable = row.FieldCodeSourceTable;
                        InitSourceTableFieldCombobox(sourceTable);
                    }
                }
            });
            $("#orgparentfields").combobox({
                url: '<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetTableFieldsList" })) %>?PhysicalTableName=' + physicalTableName,
                textField: 'FieldName',
                valueField: 'FieldCode'
            });
        }

        //初始化or加载显示字段
        function InitSourceTableFieldCombobox(sourceTable, textValue) {
            $("#orgtext").combobox({
                url: '<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetReadFieldList" })) %>?PhysicalTableName=' + sourceTable,
                textField: 'FieldName',
                valueField: 'FieldCode',
                onLoadSuccess: function () {
                    if (textValue) {
                        $('#orgtext').combobox('setValue', textValue);
                    }
                }
            });
        }

        dialog.oncancel = function () {
            if (UE.plugins[thePlugins].editdom) {
                delete UE.plugins[thePlugins].editdom;
            }
        };
        dialog.onok = function () {
            if ($('#orgname').combobox('getValue') == '') {
                alert('请选择字段');
                return false;
            }
            if ($('#orgtext').combobox('getValue') == '') {
                alert('请选择显示字段');
                return false;
            }
            var isAdd = !oNode;
            var gName = $('#orgname').combobox('getValue').replace(/\"/g, "&quot;");
            var gText = $('#orgtext').combobox('getValue').replace(/\"/g, "&quot;");
            var gTitle = $('#orgname').combobox('getText').replace(/\"/g, "&quot;");
            var gWidth = $('#orgwidth').textbox('getText');
            var gHeight = $('#orgheight').textbox('getText');
            var gRequired = $('#orgrequired').prop('checked');
            var gControlState = $('#controlstate').combobox('getValue');
            var gParentfields = $("#orgparentfields").combobox('getValues');
            var requiredDisabled = $('#orgrequired').prop('disabled');
            if (isGrid) {
                oNode.setAttribute('controltype', 'combotree');
                oNode.setAttribute('leipitype', thePlugins);
                oNode.setAttribute('valuefield', gName);
                oNode.setAttribute('editable', 'false');
                oNode.setAttribute('panelheight', 'auto');
                oNode.setAttribute('panelmaxheight', '120px');
                oNode.setAttribute('controlstate', gControlState);
                oNode.setAttribute('requireddisabled', requiredDisabled);
                oNode.setAttribute('dataURL', "/api/form/ComboTree/" + formCode + "/" + gName + "?parentField=" + gParentfields + "&id={RouteID}");
                if (sourceTable != '') {
                    oNode.setAttribute('sourcetable', sourceTable);
                }
                if (gText != '') {
                    oNode.setAttribute('textfield', gText);
                }
                else {
                    oNode.removeAttribute('textfield');
                }
                if (gParentfields != '') {
                    oNode.setAttribute('orgparentfields', gParentfields);
                }
                else {
                    oNode.removeAttribute('orgparentfields');
                }
                if (gControlState == "1") {
                    oNode.setAttribute('readonly', "true");
                }
                else {
                    oNode.removeAttribute('readonly');
                }
                if (gRequired) {
                    oNode.setAttribute('required', gRequired);
                    oNode.setAttribute('missingMessage', '必填');
                    oNode.setAttribute('prompt', '必填项');
                }
                else {
                    oNode.removeAttribute('required');
                    oNode.removeAttribute('missingMessage');
                    oNode.removeAttribute('prompt');
                }
            }
            else {
                if (isAdd) {
                    oNode = createElement('select', gName, gTitle);
                }
                else {
                    oNode.setAttribute('name', gName);
                    oNode.setAttribute('id', gName);
                    oNode.setAttribute('title', gTitle);
                }
                oNode.setAttribute('valuefield', gName);
                oNode.setAttribute('class', 'easyui-combotree');
                oNode.setAttribute('leipiPlugins', thePlugins);
                oNode.setAttribute('controlstate', gControlState);
                oNode.setAttribute('editable', 'false');
                oNode.setAttribute('panelheight', 'auto');
                oNode.setAttribute('panelmaxheight', '120px');
                oNode.setAttribute('requireddisabled', requiredDisabled);
                oNode.setAttribute('data-options', "url:'/api/form/ComboTree/" + formCode + "/" + gName + "?parentField=" + gParentfields + "&id={RouteID}'");
                if (sourceTable != '') {
                    oNode.setAttribute('sourcetable', sourceTable);
                }
                if (gControlState == "1") {
                    oNode.setAttribute('readonly', "true");
                }
                else {
                    oNode.removeAttribute('readonly');
                }
                if (gText != '') {
                    oNode.setAttribute('textfield', gText);
                }
                else {
                    oNode.removeAttribute('textfield');
                }
                if (gParentfields != '') {
                    oNode.setAttribute('orgparentfields', gParentfields);
                }
                else {
                    oNode.removeAttribute('orgparentfields');
                }
                if (gRequired) {
                    oNode.setAttribute('required', gRequired);
                    oNode.setAttribute('missingMessage', '必填');
                    oNode.setAttribute('prompt', '必填项');
                }
                else {
                    oNode.removeAttribute('required');
                    oNode.removeAttribute('missingMessage');
                    oNode.removeAttribute('prompt');
                }
                if (gWidth != '') {
                    oNode.style.width = gWidth;
                    oNode.setAttribute('orgwidth', gWidth);
                }
                else {
                    oNode.removeAttribute('orgwidth');
                }
                if (gHeight != '') {
                    oNode.style.height = gHeight;
                    oNode.setAttribute('orgheight', gHeight);
                }
                else {
                    oNode.removeAttribute('orgheight');
                }
                if (isAdd) {
                    var html = '<span leipiplugins="combotree" style="display:block;padding:0;width:' + gWidth + '">';
                    html += oNode.outerHTML;
                    html += '&nbsp;</span>';
                    editor.execCommand('insertHtml', html);
                }
                else {
                    delete UE.plugins[thePlugins].editdom;
                }
            }
        };
    </script>
</body>
</html>
