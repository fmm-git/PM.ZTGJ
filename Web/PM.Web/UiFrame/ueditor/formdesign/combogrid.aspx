<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="combogrid.aspx.cs" Inherits="PM.Web.UiFrame.ueditor.formdesign.combogrid" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>下拉框</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=Edge,chrome=1"/>
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
                <td>列表字段</td>
                <td>
                    <input class="easyui-combobox" id="orggridfields" editable="false" panelheight="auto" panelmaxheight="120px" data-options="prompt:'必填项',multiple:true,required:'required',missingMessage:'列表字段不能为空'" /></td>
            </tr>
            <tr>
                <td>长×高</td>
                <td colspan="3">
                    <input id="orgwidth" value="100%" class="easyui-textbox" style="width: 40px" data-options="prompt:'auto'" />
                    ×
                    <input id="orgheight" value="" class="easyui-textbox" style="width: 40px" data-options="prompt:'auto'" />
                </td>
            </tr>

        </table>
    </div>
    <script type="text/javascript">
        var physicalTableName = editor.tableName;
        var oNode = null;
        var sourceTable = null;
        var thePlugins = 'combogrid';
        window.onload = function () {
            InitFieldCombobox(physicalTableName);//初始化字段列表
            if (UE.plugins[thePlugins].editdom) {
                oNode = UE.plugins[thePlugins].editdom.children[0] ? UE.plugins[thePlugins].editdom.children[0] : UE.plugins[thePlugins].editdom;
                sourceTable = oNode.getAttribute('sourcetable');
                var gName = oNode.getAttribute('name');
                var gText = oNode.getAttribute('orgtext');
                var gWidth = oNode.getAttribute('orgwidth');
                var gGridfields = oNode.getAttribute('orggridfields');
                var gHeight = oNode.getAttribute('orgheight');
                var gRequired = oNode.getAttribute('required');
                var gControlState = oNode.getAttribute('controlstate');
                var requiredDisabled = oNode.getAttribute('requireddisabled');

                gName = gName == null ? '' : gName;
                gHeight = gHeight == null ? '' : gHeight;

                $('#orgname').combobox('setValue', gName);
                $("#orgid").textbox('setText', gName);
                if (sourceTable != '') {
                    InitSourceTableFieldCombobox(sourceTable, gText, gGridfields);//初始化or加载显示字段和列表字段
                }
                $('#orgrequired').prop('checked', gRequired == 'true');
                $('#orgwidth').textbox('setText', gWidth);
                $('#orgheight').textbox('setText', gHeight);
                $('#controlstate').combobox('setValue', gControlState);
                $('#orgrequired').prop('disabled', requiredDisabled == "true");
            }
            else {
                InitSourceTableFieldCombobox(sourceTable);
            }
        }
        //初始化字段列表
        function InitFieldCombobox(physicalTableName) {
            $("#orgname").combobox({
                url: '<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetTableFieldsList" })) %>?PhysicalTableName=' + physicalTableName,
                textField: 'FieldName',
                valueField: 'FieldCode',
                onSelect: function (row) {
                    $('#orgrequired').prop('checked', row.AllowNull == '否');
                    $('#orgrequired').prop('disabled', row.AllowNull == '否');
                    sourceTable = row.FieldCodeSourceTable;
                    InitSourceTableFieldCombobox(sourceTable);
                }
            });
        }
        //初始化or加载显示字段和列表字段
        function InitSourceTableFieldCombobox(sourceTable, textValue, gridfieldsValue) {
            $.get('<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetReadFieldList" })) %>'
                , { "PhysicalTableName": sourceTable }, function (data) {
                    $("#orgtext").combobox({
                        textField: 'FieldName',
                        valueField: 'FieldCode',
                        onLoadSuccess: function () {
                            if (textValue) {
                                $('#orgtext').combobox('setValue', textValue);
                            }
                        }
                    });
                    $("#orggridfields").combobox({
                        textField: 'FieldName',
                        valueField: 'FieldCode',
                        onLoadSuccess: function () {
                            if (gridfieldsValue) {
                                $('#orggridfields').combobox('setValues', gridfieldsValue.split(','));
                            }
                        }
                    });
                    $("#orgtext").combobox('loadData', data);
                    $("#orggridfields").combobox('loadData', data);
                }, "json");
        }
        //创建列表字段配置String
        function CreateGridFieldStr(fields, texts) {
            var optionStr = "";
            for (var i = 0; i < fields.length; i++) {
                optionStr += "{";
                optionStr += "field:'" + fields[i] + "',title:'" + texts[i] + "'";
                optionStr += "},";
            }
            if (optionStr != "") {
                optionStr = "columns:[[" + optionStr.substring(0, optionStr.length - 1) + "]]";
            }
            return optionStr;
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
            if ($('#orggridfields').combobox('getValues') == '') {
                alert('请选择列表字段');
                return false;
            }
            var isAdd = !oNode;
            var gName = $('#orgname').combobox('getValue').replace(/\"/g, "&quot;");
            var gText = $('#orgtext').combobox('getValue').replace(/\"/g, "&quot;");
            var gTitle = $('#orgname').combobox('getText').replace(/\"/g, "&quot;");
            var gWidth = $('#orgwidth').textbox('getText');
            var gHeight = $('#orgheight').textbox('getText');
            var gGridfields = $("#orggridfields").combobox('getValues');
            var gGridTexts = $("#orggridfields").combobox('getText');
            var gRequired = $('#orgrequired').prop('checked');
            var gControlState = $('#controlstate').combobox('getValue');
            var requiredDisabled = $('#orgrequired').prop('disabled');

            if (isAdd) {
                oNode = createElement('select', gName, gTitle);
            }
            else {
                oNode.setAttribute('name', gName);
                oNode.setAttribute('id', gName);
                oNode.setAttribute('title', gTitle);
            }
            oNode.setAttribute('class', 'easyui-combobox');
            oNode.setAttribute('leipiPlugins', thePlugins);
            oNode.setAttribute('controlstate', gControlState);
            oNode.setAttribute('editable', 'false');
            oNode.setAttribute('panelheight', 'auto');
            oNode.setAttribute('panelmaxheight', '120px');
            oNode.setAttribute('requireddisabled', requiredDisabled);
            if (gControlState == "1") {
                oNode.setAttribute('readonly', "true");
            }
            else {
                oNode.removeAttribute('readonly');
            }
            if (sourceTable != '') {
                oNode.setAttribute('sourcetable', sourceTable);
            }
            if (gGridfields != '') {
                var columnStr = CreateGridFieldStr(gGridfields, gGridTexts.split(','));
                oNode.setAttribute('orggridfields', gGridfields);
                oNode.setAttribute('data-options', columnStr);
            }
            else {
                oNode.removeAttribute('orggridfields');
                oNode.removeAttribute('data-options');
            }
            if (gText != '') {
                oNode.setAttribute('orgtext', gText);
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
                var html = '<span leipiplugins="combogrid" style="display:block;padding:0;width:' + gWidth + '">';
                html += oNode.outerHTML;
                html += '&nbsp;</span>';
                editor.execCommand('insertHtml', html);
            }
            else {
                delete UE.plugins[thePlugins].editdom;
            }
        };
    </script>
</body>
</html>
