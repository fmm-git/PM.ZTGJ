<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="checkbox.aspx.cs" Inherits="PM.Web.UiFrame.ueditor.formdesign.checkbox" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>多行文本框</title>
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
            </tr>
            <tr>
                <td>控件状态</td>
                <td>
                    <select id="controlstate" class="easyui-combobox" editable="false" panelheight="auto" panelmaxheight="120px">
                        <option value="0">读写</option>
                        <option value="1">只读</option>
                        <option value="2">隐藏</option>
                    </select>
                </td>
            </tr>
            <tr>
                <td>默认值</td>
                <td>
                    <input id="orgvalue" type="checkbox" />
                </td>
            </tr>
        </table>
    </div>
    <script type="text/javascript">
        var isGrid = editor.gridTableName;
        var physicalTableName = isGrid ? editor.gridTableName : editor.tableName;
        var oNode = null;
        var thePlugins = 'checkbox';

        window.onload = function () {
            InitFieldCombobox(physicalTableName);

            if (UE.plugins[thePlugins].editdom || isGrid) {
                if (isGrid) {
                    oNode = editor.hiddenObj;
                }
                else {
                    oNode = UE.plugins[thePlugins].editdom.children[0] ? UE.plugins[thePlugins].editdom.children[0] : UE.plugins[thePlugins].editdom;
                }
                var gName = isGrid ? oNode.getAttribute('fieldcode') : oNode.getAttribute('name');
                var gValue = oNode.getAttribute('checked');
                var gControlState = oNode.getAttribute('controlstate');

                gName = gName == null ? '' : gName;
                $('#orgvalue').prop('checked', gValue);
                $('#orgname').combobox('setValue', gName);
                $('#controlstate').combobox('setValue', gControlState);

                var abledStr = isGrid ? 'disable' : 'enable';
                $('#orgname').combobox(abledStr);
            }
        }
        //初始化字段列表
        function InitFieldCombobox(physicalTableName) {
            $("#orgname").combobox({
                url: '<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetTableFieldsList" })) %>?PhysicalTableName=' + physicalTableName,
                    textField: 'FieldName',
                    valueField: 'FieldCode'
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
                var gName = $('#orgname').combobox('getValue').replace(/\"/g, "&quot;");
                var gTitle = $('#orgname').combobox('getText').replace(/\"/g, "&quot;");
                var gValue = $('#orgvalue').prop('checked');
                var gControlState = $('#controlstate').combobox('getValue');
                if (isGrid) {
                    oNode.setAttribute('controltype', 'checkbox');
                    oNode.setAttribute('leipitype', thePlugins);
                    oNode.setAttribute('controlstate', gControlState);
                    oNode.setAttribute('value', '1');
                    if (gValue) {
                        oNode.setAttribute('checked', gValue);
                    }
                    else {
                        oNode.removeAttribute('checked');
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
                        oNode = createElement('input', gName, gTitle);
                    }
                    else {
                        oNode.setAttribute('name', gName);
                        oNode.setAttribute('id', gName);
                        oNode.setAttribute('title', gTitle);
                    }
                    oNode.setAttribute('type', 'checkbox');
                    oNode.setAttribute('leipiPlugins', thePlugins);
                    oNode.setAttribute('controlstate', gControlState);
                    oNode.setAttribute('value', '1');
                    if (gControlState == "1") {
                        oNode.setAttribute('readonly', "true");
                    }
                    else {
                        oNode.removeAttribute('readonly');
                    }
                    if (gValue) {
                        oNode.setAttribute('checked', gValue);
                    }
                    else {
                        oNode.removeAttribute('checked');
                    }

                    if (isAdd) {
                        var html = '<span leipiplugins="checkbox" style="font-family:Tahoma;">';
                        html += oNode.outerHTML + $('#orgname').combobox('getText');
                        html += '</span>';
                        editor.execCommand('insertHtml', html);
                        //editor.hiddenObj.val("asd");
                        //alert(editor.hiddenObj.val());
                    }
                    else {
                        oNode.nextSibling.data = $('#orgname').combobox('getText');
                        delete UE.plugins[thePlugins].editdom;
                    }
                }
            };
    </script>
</body>
</html>
