<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="tab.aspx.cs" Inherits="PM.Web.UiFrame.ueditor.formdesign.tab" %>

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
        function createElement(type, title) {
            var element = null;
            try {
                element = document.createElement('<' + type + ' title="' + title + '">');
            } catch (e) { }
            if (element == null) {
                element = document.createElement(type);
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
                <td>选项卡标题</td>
                <td colspan="3">
                    <input class="easyui-textbox" id="orgtitle" data-options="prompt:'必填项',required:'required',missingMessage:'标题不能为空'" /></td>
            </tr>
        </table>
    </div>
    <script type="text/javascript">
        var oNode = null;
        var thePlugins = 'tab';
        window.onload = function () {
            if (UE.plugins[thePlugins].editdom) {
                oNode = UE.plugins[thePlugins].editdom;
                var gTitle = oNode.getAttribute('title');
                $('#orgtitle').textbox('setText', gTitle);
            }
        }


        dialog.oncancel = function () {
            if (UE.plugins[thePlugins].editdom) {
                delete UE.plugins[thePlugins].editdom;
            }
        };
        dialog.onok = function () {
            if ($('#orgtitle').combobox('getText') == '') {
                alert('请填写标题');
                return false;
            }
            var isAdd = !oNode;

            var gTitle = $('#orgtitle').textbox('getText');
            if (isAdd) {
                oNode = createElement('input', gTitle);
            }
            else {
                oNode.setAttribute('title', gTitle);

            }
            oNode.setAttribute("readonly", "true");
            oNode.setAttribute("type", "text");
            oNode.style.backgroundColor = "#A0A0A0";
            oNode.style.width = "100%";
            oNode.setAttribute("value", "{分页符:" + gTitle + "}");
            oNode.setAttribute('leipiPlugins', thePlugins);

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
