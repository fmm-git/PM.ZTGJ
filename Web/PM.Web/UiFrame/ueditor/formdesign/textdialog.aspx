<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="textdialog.aspx.cs" Inherits="PM.Web.UiFrame.ueditor.formdesign.textdialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>文本弹出框</title>
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

        function createHiddenElement(name, refControlID, title) {
            var element = null;
            try {
                element = document.createElement('<input type="hidden" id="' + name + '"' + ' name="' + name + '"' + ' refControlID="' + refControlID + '"' + ' title="' + title + '">');
            } catch (e) { }
            if (element == null) {
                element = document.createElement("input");
                element.name = name;
                element.id = name;
                element.type = "hidden";
                element.setAttribute('title', title);
                element.setAttribute('refControlID', refControlID);
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
                <td>级联字段</td>
                <td>
                    <input class="easyui-combobox" id="orgchildfields" editable="false" panelheight="auto" panelmaxheight="120px" data-options="multiple:true" /></td>
            </tr>
            <tr>
                <td>长×高</td>
                <td>
                    <input id="orgwidth" value="100%" class="easyui-textbox" style="width: 40px" data-options="prompt:'auto'" />
                    ×
                    <input id="orgheight" value="" class="easyui-textbox" style="width: 40px" data-options="prompt:'auto'" />
                </td>
                <td>父级字段</td>
                <td>
                    <input class="easyui-combobox" id="orgparentfields" editable="false" panelheight="auto" panelmaxheight="120px" data-options="multiple:true" /></td>
            </tr>

        </table>
    </div>
    <script type="text/javascript">
        var isGrid = editor.gridTableName;
        var physicalTableName = isGrid ? editor.gridTableName : editor.tableName;
        var oNode = null;
        var sourceTable = null;
        var thePlugins = 'textdialog';
        window.onload = function () {
            InitFieldCombobox(physicalTableName);//初始化字段列表
            if (UE.plugins[thePlugins].editdom || isGrid) {
                if (isGrid) {
                    oNode = editor.hiddenObj;
                }
                else {
                    oNode = UE.plugins[thePlugins].editdom;
                }
                sourceTable = oNode.getAttribute('sourcetable');
                var gName = isGrid ? oNode.getAttribute('fieldcode') : oNode.getAttribute('name');
                var gText;
                if (isGrid) {
                    gText = oNode.getAttribute('textfield');
                }
                else {
                    gText = oNode.getAttribute('valuefield');
                }
                var gWidth = oNode.getAttribute('orgwidth');
                var gChildfields = oNode.getAttribute('orgchildfields');
                var gParentfields = oNode.getAttribute('orgparentfields');
                var gHeight = oNode.getAttribute('orgheight');
                var gRequired = oNode.getAttribute('required');
                var gControlState = oNode.getAttribute('controlstate');
                var requiredDisabled = oNode.getAttribute('requireddisabled');

                gName = gName == null ? '' : gName;
                gHeight = gHeight == null ? '' : gHeight;
                $('#orgname').combobox('setValue', isGrid ? gName : gText);

                if (sourceTable != '') {
                    InitSourceTableFieldCombobox(sourceTable, isGrid ? gText : gName);//初始化or加载显示字段
                }
                InitParentOrChildFieldCombobox(gParentfields, gChildfields);
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
            else {
                InitParentOrChildFieldCombobox();
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
        function InitSourceTableFieldCombobox(sourceTable, textValue) {
            $("#orgtext").combobox({
                url: '<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetReadFieldList" })) %>?PhysicalTableName=' + sourceTable,
                textField: 'FieldName',
                valueField: 'FieldCode'
            });
            if (textValue) {
                $('#orgtext').combobox('setValue', textValue);
            }
        }
        function InitParentOrChildFieldCombobox(parentValue, childValue) {
            $.get('<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetTableFieldsList" })) %>'
                , { "PhysicalTableName": physicalTableName }, function (data) {
                    $("#orgparentfields").combobox({
                        textField: 'FieldName',
                        valueField: 'FieldCode',
                        onLoadSuccess: function () {
                            if (parentValue) {
                                $('#orgparentfields').combobox('setValues', parentValue.split(','));
                            }
                        }
                    });
                    $("#orgchildfields").combobox({
                        textField: 'FieldName',
                        valueField: 'FieldCode',
                        onLoadSuccess: function () {
                            if (childValue) {
                                $('#orgchildfields').combobox('setValues', childValue.split(','));
                            }
                        }
                    });
                    $("#orgparentfields").combobox('loadData', data);
                    $("#orgchildfields").combobox('loadData', data);
                }, "json");
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
            var gChildfields = $("#orgchildfields").combobox('getValues');
            var gParentfields = $("#orgparentfields").combobox('getValues');
            var gRequired = $('#orgrequired').prop('checked');
            var gControlState = $('#controlstate').combobox('getValue');
            var requiredDisabled = $('#orgrequired').prop('disabled');
            if (isGrid) {
                oNode.setAttribute('controltype', 'textbox');
                oNode.setAttribute('leipitype', thePlugins);
                oNode.setAttribute('editable', 'false');
                oNode.setAttribute('buttonIcon', 'pmicon-find-16');
                oNode.setAttribute('controlstate', gControlState);
                oNode.setAttribute('requireddisabled', requiredDisabled);
                if (sourceTable != '') {
                    oNode.setAttribute('sourcetable', sourceTable);
                }
                if (gChildfields != '') {
                    oNode.setAttribute('orgchildfields', gChildfields);
                }
                else {
                    oNode.removeAttribute('orgchildfields');
                }
                if (gParentfields != '') {
                    oNode.setAttribute('orgparentfields', gParentfields);
                }
                else {
                    oNode.removeAttribute('orgparentfields');
                }
                if (gText != '') {
                    oNode.setAttribute('textfield', gText);
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
            }
            else {
                if (isAdd) {
                    oNode = createElement('input', gText, gTitle);
                }
                else {
                    oNode.setAttribute('name', gText);
                    oNode.setAttribute('id', gText);
                    oNode.setAttribute('title', gTitle);
                }
                oNode.setAttribute('class', 'easyui-textbox');
                oNode.setAttribute('buttonIcon', 'pmicon-find-16');
                oNode.setAttribute('leipiPlugins', thePlugins);
                oNode.setAttribute('editable', 'false');
                oNode.setAttribute('data-options', "onClickButton:function(){SetTextAndValue('" + sourceTable + "','" + gParentfields + "','" + gName + (gChildfields == "" ? gChildfields : "," + gChildfields) + "','" + gText + "');}");
                oNode.setAttribute('controlstate', gControlState);
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
                if (gChildfields != '') {
                    oNode.setAttribute('orgchildfields', gChildfields);
                }
                else {
                    oNode.removeAttribute('orgchildfields');
                }
                if (gParentfields != '') {
                    oNode.setAttribute('orgparentfields', gParentfields);
                }
                else {
                    oNode.removeAttribute('orgparentfields');
                }
                if (gName != '') {
                    oNode.setAttribute('valuefield', gName);
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
                    var hiddenInput = "";
                    if (gText != gName) {
                        hiddenInput = "<input id=\"" + gName + "\" name=\"" + gName + "\" refControlID=\"" + gText + "\" title=\"" + gTitle + "\" type=\"hidden\" />";
                    }
                    editor.execCommand('insertHtml', oNode.outerHTML + hiddenInput);
                }
                else {
                    if (UE.plugins[thePlugins].editdom.nextSibling) {
                        UE.dom.domUtils.remove(UE.plugins[thePlugins].editdom.nextSibling, false);
                    }
                    if (gText != gName) {
                        var hiddenInput = createHiddenElement(gName, gText, gTitle);
                        UE.dom.domUtils.insertAfter(UE.plugins[thePlugins].editdom, hiddenInput);
                    }
                    delete UE.plugins[thePlugins].editdom;
                }
            }
        };
    </script>
</body>
</html>
