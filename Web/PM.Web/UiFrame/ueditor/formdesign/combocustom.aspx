<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="combocustom.aspx.cs" Inherits="PM.pgzjWeb.UiFrame.ueditor.formdesign.combocustom" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>下拉字典</title>
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

                <td>字典名称</td>
                <td>
                    <input class="easyui-combotree" id="orgdictionarytable" editable="false" panelheight="auto" panelmaxheight="120px" data-options="prompt:'必填项',required:'required',missingMessage:'显示名称不能为空'" />
                </td>
                <td>长×高</td>
                <td>
                    <input id="orgwidth" value="100%" class="easyui-textbox" style="width: 40px" data-options="prompt:'auto'" />
                    ×
                    <input id="orgheight" value="" class="easyui-textbox" style="width: 40px" data-options="prompt:'auto'" />
                </td>
            </tr>


        </table>
    </div>
    <script type="text/javascript">
        var isGrid = editor.gridTableName;
        var physicalTableName = isGrid ? editor.gridTableName : editor.tableName;
        var oNode = null;
        var thePlugins = 'combocustom';
        window.onload = function () {
            InitFieldCombobox(physicalTableName);//初始化字段列表
            if (UE.plugins[thePlugins].editdom || isGrid) {
                if (isGrid) {
                    oNode = editor.hiddenObj;
                }
                else {
                    oNode = UE.plugins[thePlugins].editdom.children[0] ? UE.plugins[thePlugins].editdom.children[0] : UE.plugins[thePlugins].editdom;
                }
                var gName = isGrid ? oNode.getAttribute('fieldcode') : oNode.getAttribute('name')
                var gDictionarytable = oNode.getAttribute('orgdictionarytable');
                var gWidth = oNode.getAttribute('orgwidth');
                var gHeight = oNode.getAttribute('orgheight');
                var gRequired = oNode.getAttribute('required');
                var gControlState = oNode.getAttribute('controlstate');
                var requiredDisabled = oNode.getAttribute('requireddisabled');

                gName = gName == null ? '' : gName;
                gHeight = gHeight == null ? '' : gHeight;

                $('#orgname').combobox('setValue', gName);

                InitDictionaryTableFieldCombobox(gDictionarytable);

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
                InitDictionaryTableFieldCombobox();//初始化字典列表
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
        //初始化or加载显示字段
        function InitDictionaryTableFieldCombobox(textValue) {
            $("#orgdictionarytable").combotree({
                url: '<%= ResolveUrl(GetRouteUrl("TreeListByDoubleAllCode", new { FunCode = "GetDictionaryTypeTree" })) %>',
                textField: 'text',
                valueField: 'id',
                onLoadSuccess: function () {
                    if (textValue) {
                        $('#orgdictionarytable').combotree('setValue', textValue);
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
            if ($('#orgdictionarytable').combobox('getValue') == '') {
                alert('请选择字典');
                return false;
            }
            var isAdd = !oNode;
            var gName = $('#orgname').combobox('getValue').replace(/\"/g, "&quot;");
            var gTitle = $('#orgname').combobox('getText').replace(/\"/g, "&quot;");
            var gDictionarytable = $('#orgdictionarytable').combotree('getValue').replace(/\"/g, "&quot;");
            var gWidth = $('#orgwidth').textbox('getText');
            var gHeight = $('#orgheight').textbox('getText');
            var gRequired = $('#orgrequired').prop('checked');
            var gControlState = $('#controlstate').combobox('getValue');
            var requiredDisabled = $('#orgrequired').prop('disabled');
            if (isGrid) {
                oNode.setAttribute('controltype', 'combobox');
                oNode.setAttribute('leipitype', thePlugins);
                oNode.setAttribute('valueField', 'DictionaryText');
                oNode.setAttribute('textField', 'DictionaryText');
                oNode.setAttribute('editable', 'false');
                oNode.setAttribute('panelheight', 'auto');
                oNode.setAttribute('panelmaxheight', '120px');
                oNode.setAttribute('controlstate', gControlState);
                oNode.setAttribute('requireddisabled', requiredDisabled);
                if (gDictionarytable != '') {
                    oNode.setAttribute('orgdictionarytable', gDictionarytable);
                    oNode.setAttribute('dataURL', "/api/form/DataDictionary/" + gDictionarytable);
                }
                else {
                    oNode.removeAttribute('orgdictionarytable');
                    oNode.removeAttribute('url');
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
                    oNode = createElement('select', gName, gTitle);
                }
                else {
                    oNode.setAttribute('name', gName);
                    oNode.setAttribute('id', gName);
                    oNode.setAttribute('title', gTitle);
                }

                oNode.setAttribute('valueField', 'DictionaryText');
                oNode.setAttribute('textField', 'DictionaryText');
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
                if (gDictionarytable != '') {
                    oNode.setAttribute('orgdictionarytable', gDictionarytable);
                    oNode.setAttribute('data-options', "onLoadSuccess: function (data) {if (data) { $(this).combobox('select', data[0].DictionaryText); }},url:'/api/form/DataDictionary/" + gDictionarytable + "'");
                }
                else {
                    oNode.removeAttribute('orgdictionarytable');
                    oNode.removeAttribute('data-options');
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
                    var html = '<span leipiplugins="combocustom" style="display:block;padding:0;width:' + gWidth + '">';
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
