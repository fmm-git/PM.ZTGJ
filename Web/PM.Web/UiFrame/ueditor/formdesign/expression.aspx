<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="expression.aspx.cs" Inherits="PM.Web.UiFrame.ueditor.formdesign.expression" %>

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

    </script>
    <style type="text/css">
        table td {
            padding: 5px;
        }
    </style>
</head>
<body>
    <div class="content">
        <table border="0" cellspacing="5" cellpadding="0" class="list">
            <thead>
                <tr align="center" valign="middle" id="TableTitle">
                    <th>括号</th>
                    <th>字段</th>
                    <th>括号</th>
                    <th>运算符</th>
                    <th>操作</th>
                </tr>
            </thead>
            <tbody id="TableData" count="0" lastindex="0">
            </tbody>
        </table>
    </div>
    <script type="text/javascript">
        var physicalTableName = editor.gridTableName;
        var oNode = editor.hiddenObj;
        var thePlugins = 'expression';
        var fieldList = null;
        window.onload = function () {
            arrExp = oNode.getAttribute('expjsonarray');
            GetFieldList(physicalTableName);
            if (arrExp) {
                var expJsonArray = JSON.parse(arrExp);
                for (var i = 0; i < expJsonArray.length; i++) {
                    CreateHtml(expJsonArray[i]);
                }
            }
            else {
                CreateHtml();
            }
        }
        //初始化字段列表
        function GetFieldList(physicalTableName) {
            $.ajax({
                type: 'get',
                url: '<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetTableFieldsList" })) %>',
                data: { 'PhysicalTableName': physicalTableName },
                dataType: 'json',
                async: false,
                success: function (data) {
                    fieldList = data;
                }
            });
        }

        function CreateHtml(data) {
            var tableBody = $("#TableData");
            tableBody.attr("count", eval(tableBody.attr("count")) + 1);
            var index = eval(tableBody.attr("lastIndex")) + 1;
            tableBody.attr("lastIndex", index);
            var html = '<tr id="tr_fieldList_' + index + '">';
            html += '<td><select id="leftSign_' + index + '" class="easyui-combobox" editable="false" style="width: 60px;">';
            html += '<option value="">无</option>';
            html += '<option value="(">(</option>';
            html += '<option value="((">((</option>';
            html += '<option value="(((">(((</option>';
            html += '</select></td>';

            html += '<td><input id="field_' + index + '" class="easyui-combobox" editable="false"/></td>';

            html += '<td><select id="rightSign_' + index + '" class="easyui-combobox" editable="false" style="width: 60px;">';
            html += '<option value="">无</option>';
            html += '<option value=")">)</option>';
            html += '<option value="))">))</option>';
            html += '<option value=")))">)))</option>';
            html += '</select></td>';

            html += '<td><select id="operation_' + index + '" class="easyui-combobox" editable="false" style="width: 60px;">';
            html += '<option value="">无</option>';
            html += '<option value="+">+</option>';
            html += '<option value="-">-</option>';
            html += '<option value="*">×</option>';
            html += '<option value="/">÷</option>';
            html += '</select></td>';
            html += '<td align="center">';
            html += '<a onclick="delete_field(' + index + ');" title="删除本条件" href="#">';
            html += '<img border="0" src="../../pmicons/delete.gif" /></a>';
            html += '&nbsp;&nbsp;';
            html += '<a onclick="add_field();" title="添加1个条件" href="#">';
            html += '<img border="0" src="../../pmicons/add.gif" /></a>';
            html += '</td></tr>';
            tableBody.append(html);

            $.parser.parse("#tr_fieldList_" + index);
            $("#field_" + index).combobox({
                textField: 'FieldName',
                valueField: 'FieldCode'
            });
            $("#field_" + index).combobox('loadData', fieldList);

            if (data) {
                $("#leftSign_" + index).combobox('setValue', data.leftSign);
                $("#field_" + index).combobox('setValue', data.field);
                $("#rightSign_" + index).combobox('setValue', data.rightSign);
                $("#operation_" + index).combobox('setValue', data.operation);
            }

        }
        function add_field() {
            CreateHtml();
        }
        function delete_field(index) {
            var tableBody = $("#TableData");
            var count = eval(tableBody.attr("count"));
            tableBody.attr("count", --count);
            $("#tr_fieldList_" + index).remove();
            if (count == 0) {
                CreateHtml();
            }
        }

        dialog.oncancel = function () {

        };
        dialog.onok = function () {
            var expJson = GetExpressJsonArray(); 
            oNode.setAttribute('expjsonarray', JSON.stringify(expJson));
            oNode.setAttribute('leipitype', 'expression');
        };

        function GetExpressJsonArray() {
            var tableBody = $("#TableData");
            var maxIndex = tableBody.attr("lastIndex");
            //var expStr = '';
            var jsonExp;
            var arrExp = [];
            for (var i = 1; i <= maxIndex; i++) {
                if ($("#leftSign_" + i).length != 0) {
                    jsonExp = new Object();
                    jsonExp.leftSign = $("#leftSign_" + i).combobox('getValue');
                    jsonExp.field = $("#field_" + i).combobox('getValue');
                    jsonExp.rightSign = $("#rightSign_" + i).combobox('getValue');
                    jsonExp.operation = $("#operation_" + i).combobox('getValue');
                    arrExp.push(jsonExp);
                    //expStr += jsonExp.leftSign + jsonExp.field + jsonExp.rightSign + jsonExp.operation;
                }
            }
            return arrExp;
        }
    </script>
</body>
</html>
