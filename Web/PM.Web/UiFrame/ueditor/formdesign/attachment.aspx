<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="attachment.aspx.cs" Inherits="PM.Web.UiFrame.ueditor.formdesign.attachment" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>附件列表</title>
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
        function createElement(type, name) {
            var element = null;
            try {
                element = document.createElement('<' + type + ' id="' + name + '">');
            } catch (e) { }
            if (element == null) {
                element = document.createElement(type);
                element.id = name;
            }
            return element;
        }

        function createHiddenElement(name) {
            var element = null;
            try {
                element = document.createElement('<input type="hidden" id="' + name + '"' + ' name="' + name + '">');
            } catch (e) { }
            if (element == null) {
                element = document.createElement("input");
                element.name = name;
                element.id = name;
                element.type = "hidden";
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
        </table>
    </div>
    <script type="text/javascript">
        var physicalTableName = editor.tableName;
        var oNode = null;
        var thePlugins = 'attachment';
        window.onload = function () {

            InitFieldCombobox(physicalTableName);

            if (UE.plugins[thePlugins].editdom) {
                oNode = UE.plugins[thePlugins].editdom;
                var fieldcode = oNode.getAttribute('fieldcode');
                $('#orgname').combobox('setValue', fieldcode);
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
            var html = "";
            html += "<thead>";
            html += "<tr>";
            html += "<th data-options=\"field:'ck',checkbox:true\"></th>";
            html += "<th data-options=\"field:'FileID'\" hidden=\"hidden\">附件ID</th>";
            html += "<th data-options=\"field:'FileName',width:200,align:'left'\">附件名称</th>";
            html += "<th data-options=\"field:'FileSize',width:100,formatter:function(value){";
            html += "return HumanReadableFilesize(value);";
            html += "}\">附件大小</th>";
            html += "<th data-options=\"field:'UserCode',width:100\">上传人</th>";
            html += "<th data-options=\"field:'LastTime',width:100\">上传日期</th>";
            html += "<th data-options=\"field:'StorageOver',width:100,formatter:function(value){";
            html += "return value==0?'完成':'未完成';";
            html += "}\">上传状态</th>";
            html += "<th data-options=\"field:'download',width:100,formatter:function(value,row){";
            html += "return CommonAttachmentDownloadFormatter(row);";
            html += "}\">操作</th>";
            html += "</tr>";
            html += "</thead>";

            var isAdd = !oNode;
            var fieldcode = $('#orgname').combobox('getValue');
            if (isAdd) {
                oNode = createElement('table', "attachmentList");
                oNode.setAttribute('name', "attachmentList");
                oNode.setAttribute('class', "easyui-datagrid");
                oNode.setAttribute('controlstate', "0");
                oNode.setAttribute('data-options', "border:false,toolbar:[{text:'上传',iconCls:'pmicon-moveup-16',handler:CommonUpload},'-',{text:'批量下载',iconCls:'pmicon-movedown-16',handler:CommonDownload},'-',{text:'删除',iconCls:'pmicon-delete-GrayScaled-16',handler:CommonDeleteFile}]");
                oNode.innerHTML = html;
            }
            oNode.setAttribute('fieldcode', fieldcode);

            if (isAdd) {
                oNode.setAttribute('leipiPlugins', thePlugins);
                editor.execCommand('insertHtml', oNode.outerHTML + "<div id=\"uploadDlg\" data-options=\"onClose:CommonUpdateUploadState\"></div><input id=\"" + fieldcode + "\" name=\"" + fieldcode + "\" type=\"hidden\"/>");
            }
            else {
                if (UE.plugins[thePlugins].editdom.nextSibling) {
                    UE.dom.domUtils.remove(UE.plugins[thePlugins].editdom.nextSibling.nextSibling, false);
                }
                var hiddenInput = createHiddenElement(fieldcode);
                UE.dom.domUtils.insertAfter(UE.plugins[thePlugins].editdom.nextSibling, hiddenInput);
                delete UE.plugins[thePlugins].editdom;
            }
        };
    </script>
</body>
</html>
