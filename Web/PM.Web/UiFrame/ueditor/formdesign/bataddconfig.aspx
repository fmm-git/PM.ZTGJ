<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="bataddconfig.aspx.cs" Inherits="PM.Web.UiFrame.ueditor.formdesign.bataddconfig" %>

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
                <td>级联字段</td>
                <td>
                    <input class="easyui-combobox" id="orgchildfields" editable="false" panelheight="auto" panelmaxheight="120px" data-options="multiple:true" /></td>
            </tr>
        </table>
    </div>
    <script type="text/javascript">
        var physicalTableName = editor.gridTableName;
        var oNode = null;
        var sourceTable = null;
        var thePlugins = 'bataddconfig';
        window.onload = function () {
            InitFieldCombobox(physicalTableName);//初始化字段列表
            if (editor.hiddenObj) {
                oNode = editor.hiddenObj;
                sourceTable = oNode.getAttribute('sourcetable');
                var gName = oNode.getAttribute('fieldcode');
                var gChildfields = oNode.getAttribute('orgchildfields');

                gName = gName == null ? '' : gName;

                $('#orgname').combobox('setValue', gName);

                InitParentOrChildFieldCombobox( gChildfields);

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
                    sourceTable = row.FieldCodeSourceTable;
                    InitSourceTableFieldCombobox(sourceTable);
                }
            });
        }

        function InitParentOrChildFieldCombobox(childValue) {
            $.get('<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetTableFieldsList" })) %>'
                , { "PhysicalTableName": physicalTableName }, function (data) {
                    $("#orgchildfields").combobox({
                        textField: 'FieldName',
                        valueField: 'FieldCode',
                        onLoadSuccess: function () {
                            if (childValue) {
                                $('#orgchildfields').combobox('setValues', childValue.split(','));
                            }
                        }
                    });                   
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
            var gName = $('#orgname').combobox('getValue').replace(/\"/g, "&quot;");
            var gChildfields = $("#orgchildfields").combobox('getValues');
           
            oNode.setAttribute('fieldcode', gName);
            oNode.setAttribute('leipitype', thePlugins);
            oNode.setAttribute('sourcetable', sourceTable != '' ? sourceTable : '');
            oNode.setAttribute('orgchildfields', gChildfields != '' ? gChildfields : '');

        };
    </script>
</body>
</html>
