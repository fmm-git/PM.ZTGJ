<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="label.aspx.cs" Inherits="PM.Web.UiFrame.ueditor.formdesign.label" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>多行文本框</title>
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
        function createElement(type, name) {
            var element = null;
            try {
                element = document.createElement('<' + type + '>');
                element.innerHTML = name;
            } catch (e) { }
            if (element == null) {
                element = document.createElement(type);
                element.innerHTML = name;
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
                <td>显示名称</td>
                <td>
                    <input class="easyui-textbox" id="orgname" data-options="prompt:'必填项',required:'required',missingMessage:'显示名称不能为空'" /></td>
            </tr>
        </table>
    </div>
    <script type="text/javascript">
        var thePlugins = 'label';
        var oNode;
        window.onload = function () {
            if (UE.plugins[thePlugins].editdom) {
                oNode = UE.plugins[thePlugins].editdom.children[0] ? UE.plugins[thePlugins].editdom.children[0] : UE.plugins[thePlugins].editdom;
                var gName = oNode.innerHTML;
                gName = gName == null ? '' : gName;

                $('#orgname').textbox('setText', gName);
            }
        }

        dialog.oncancel = function () {
            if (UE.plugins[thePlugins].editdom) {
                delete UE.plugins[thePlugins].editdom;
            }
        };
        dialog.onok = function () {
            if ($('#orgname').textbox('getText') == '') {
                alert('输入显示名称');
                return false;
            }
            var isAdd = !oNode;
            var gName = $('#orgname').textbox('getText').replace(/\"/g, "&quot;");

            if (isAdd) {
                oNode = createElement('label', gName);
            }
            else {
                oNode.innerHTML = gName;
            }

            oNode.setAttribute('leipiPlugins', thePlugins);
            oNode.style.width = '100%';

            if (isAdd) {
                editor.execCommand('insertHtml', oNode.outerHTML);
            }
            else {
                delete UE.plugins[thePlugins].editdom;
            }
        };
    </script>
</body>
</html>
