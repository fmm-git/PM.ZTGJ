<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="number.aspx.cs" Inherits="PM.Web.UiFrame.ueditor.formdesign.number" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>数字框</title>
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
                <td>数据类型</td>
                <td>
                    <input id="orgtype" class="easyui-combobox" editable="false" panelheight="auto" panelmaxheight="120px" />
                </td>
                <td>精度</td>
                <td>
                    <input id="orgprecision" value="" class="easyui-numberbox" style="width: 40px" data-options="prompt:'auto'" disabled />
                </td>
            </tr>
            <tr>
                <td>长×高</td>
                <td>
                    <input id="orgwidth" value="100%" class="easyui-textbox" style="width: 40px" data-options="prompt:'auto'" />
                    ×
                    <input id="orgheight" value="" class="easyui-textbox" style="width: 40px" data-options="prompt:'auto'" />
                </td>
                <td>默认值</td>
                <td>
                    <input class="easyui-numberbox" id="orgvalue" style="width: 100px" data-options="prompt:'无则不填'" /></td>
            </tr>
        </table>
    </div>
    <script type="text/javascript">
        var isGrid = editor.gridTableName;
        var physicalTableName = isGrid ? editor.gridTableName : editor.tableName;
        var oNode = null;
        var thePlugins = 'number';
        window.onload = function () {
            InitTypeCombobox();
            InitFieldCombobox(physicalTableName);
            $("#orgprecision").numberbox({
                onChange: function () {
                    $('#orgvalue').numberbox({ precision: $("#orgprecision").numberbox('getText') });
                }
            });
            if (UE.plugins[thePlugins].editdom || isGrid) {
                if (isGrid) {
                    oNode = editor.hiddenObj;
                }
                else {
                    oNode = UE.plugins[thePlugins].editdom;
                }
                var gValue = oNode.getAttribute('value');
                var gName = isGrid ? oNode.getAttribute('fieldcode') : oNode.getAttribute('name')
                var gWidth = oNode.getAttribute('orgwidth');
                var gHeight = oNode.getAttribute('orgheight');
                var gType = oNode.getAttribute('orgtype');
                var gRequired = oNode.getAttribute('required');
                var gPrecision = oNode.getAttribute('precision');
                var gControlState = oNode.getAttribute('controlstate');
                var requiredDisabled = oNode.getAttribute('requireddisabled');

                gValue = gValue == null ? '' : gValue;
                gName = gName == null ? '' : gName;
                gHeight = gHeight == null ? '' : gHeight;
                $('#orgvalue').numberbox('setText', gValue);
                $('#orgname').combobox('setValue', gName);
                $('#orgrequired').prop('checked', gRequired == 'true');
                $('#orgwidth').textbox('setText', gWidth);
                $('#orgheight').textbox('setText', gHeight);
                if (gType) {
                    $('#orgtype').combobox('select', gType);
                }
                $("#orgprecision").numberbox('setText', gPrecision);
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
                onSelect: function (node) {
                    $('#orgrequired').prop('checked', node.AllowNull == '否');
                    $('#orgrequired').prop('disabled', node.AllowNull == '否');
                }
            });
        }
        //初始化类型列表
        function InitTypeCombobox() {
            $("#orgtype").combobox({
                data: [{ 'id': 'easyui-numberbox', 'text': '整数' }, { 'id': 'easyui-numberbox,decimal', 'text': '小数' }],
                textField: 'text',
                valueField: 'id',
                onSelect: function (node) {
                    if (node.id.split(',')[1] == "decimal") {
                        $("#orgprecision").numberbox('enable');
                    }
                    else {
                        $("#orgprecision").numberbox('disable');
                        $("#orgprecision").numberbox('clear');
                    }
                },
                onLoadSuccess: function (data) {
                    if (data) {
                        $(this).combobox('setValue', data[0].id);
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
            var isAdd = !oNode;
            var gValue = $('#orgvalue').numberbox('getText').replace(/\"/g, "&quot;");
            var gName = $('#orgname').combobox('getValue').replace(/\"/g, "&quot;");
            var gTitle = $('#orgname').combobox('getText').replace(/\"/g, "&quot;");
            var gWidth = $('#orgwidth').textbox('getText');
            var gHeight = $('#orgheight').textbox('getText');
            var gPrecision = $('#orgprecision').numberbox('getText');
            var gType = $('#orgtype').combobox('getValue');
            var gRequired = $('#orgrequired').prop('checked');
            var gControlState = $('#controlstate').combobox('getValue');
            var requiredDisabled = $('#orgrequired').prop('disabled');

            var gTypeArr = gType.split(',')
            var className = gTypeArr[0];
            if (isGrid) {
                oNode.setAttribute('controltype', 'numberbox');
                oNode.setAttribute('leipitype', thePlugins);
                oNode.setAttribute('value', gValue);
                oNode.setAttribute('controlstate', gControlState);
                oNode.setAttribute('requireddisabled', requiredDisabled);
                if (gPrecision) {
                    oNode.setAttribute('precision', gPrecision);
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
                if (gControlState == "1") {
                    oNode.setAttribute('readonly', "true");
                }
                else {
                    oNode.removeAttribute('readonly');
                }
                if (gType != '') {
                    oNode.setAttribute('orgtype', gType);
                }
                else {
                    oNode.removeAttribute('orgtype');
                }
            }
            else {
                if (isAdd) {
                    oNode = createElement('input', gName, gTitle);
                }
                else {
                    oNode.setAttribute('name', gName);
                    oNode.setAttribute('id', gName);
                    oNode.setAttribute('title', gTitle);
                }
                oNode.setAttribute('value', gValue);
                oNode.setAttribute('class', className);
                oNode.setAttribute('leipiPlugins', thePlugins);
                oNode.setAttribute('controlstate', gControlState);
                oNode.setAttribute('requireddisabled', requiredDisabled);
                if (gControlState == "1") {
                    oNode.setAttribute('readonly', "true");
                }
                else {
                    oNode.removeAttribute('readonly');
                }
                if (gPrecision) {
                    oNode.setAttribute('precision', gPrecision);
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
                if (gType != '') {
                    oNode.setAttribute('orgtype', gType);
                }
                else {
                    oNode.removeAttribute('orgtype');
                }
                if (isAdd) {
                    editor.execCommand('insertHtml', oNode.outerHTML);
                }
                else {
                    delete UE.plugins[thePlugins].editdom;
                }
            }
        };
    </script>
</body>
</html>
