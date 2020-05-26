<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="validrulelist.aspx.cs" Inherits="PM.Web.UiFrame.ueditor.formdesign.validrulelist" %>

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
        <table>
            <tr>
                <td>
                    <input id="hidden_ValidRule" type="hidden" />
                    <a id="btn_AddValidRule" class="easyui-linkbutton" onclick="add_rule()" href="javascript:void(0);">添加验证规则</a>
                </td>
            </tr>
        </table>
        <table id="tableRule" style="margin-left: 5px;">
            <thead>
            </thead>
            <tbody id="tableRuleData" count="0" lastindex="0">
            </tbody>
        </table>
    </div>
    <script type="text/javascript">
        var physicalTableName = editor.gridTableName;
        var oNode = editor.hiddenObj;
        var thePlugins = 'validrulelist';
        var fieldList = null;
        window.onload = function () {
            arrValid = oNode.getAttribute('validjsonarray');
            if (arrValid) {
                
                var validJsonArray = JSON.parse(arrValid);
                for (var i = 0; i < validJsonArray.length; i++) {
                    CreateHtml(validJsonArray[i]);
                }
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
            var thead = $("#tableRule thead");
            
            if (thead.attr("load") != "true") {
                thead.html('<tr><th style="width: 150px;">提示信息</th><th style="width: 45px;">配置</th><th align="left" style="width: 300px;">验证规则</th><th style="width: 60px;">操作</th></tr>');
                thead.attr("load", "true");
            }
            var tableBody = $("#tableRuleData");
            tableBody.attr("count", eval(tableBody.attr("count")) + 1);
            var index = eval(tableBody.attr("lastIndex")) + 1;
            tableBody.attr("lastIndex", index);
            var html = '<tr id="tr_ruleList_' + index + '">';
            html += '<td><input id="msg_' + index + '" class="easyui-textbox" style="width: 150px;"/></td>';
            html += '<td><a id="hidden_validruleconfig_' + index + '" type="hidden" />';
            html += '<a id="btn_config_' + index + '" class="easyui-linkbutton" href="javascript:void(0);">配置</a></td>';

            html += '<td><span id="rule_' + index + '">未配置</span></td>';

            html += '<td align="center">';
            html += '<a onclick="delete_rule(' + index + ');" title="删除本条件" href="#">';
            html += '<img border="0" src="../../pmicons/delete.gif" /></a>';
            html += '</td></tr>';
            tableBody.append(html);

            $.parser.parse("#tr_ruleList_" + index);
            BindConfigBtn(index);
            if (data) {
                $("#msg_" + index).textbox('setValue', data[0].msg);
                $("#hidden_validruleconfig_" + index).attr('validjsonarray', JSON.stringify(data));
                var validStr="";
                var jsonValid;
                for (var i = 0; i < data.length; i++) {
                    jsonValid=data[i];
                    validStr += jsonValid.leftSign + jsonValid.fieldText + jsonValid.operation + jsonValid.compareText + jsonValid.rightSign + jsonValid.connectSign;
                }
                $("#rule_" + index).prop("innerHTML", validStr);
            }

        }

        function BindConfigBtn(index) {
            $("#btn_config_" + index).linkbutton({
                onClick: function () {
                    editor.validruleconfigHiddenObj = document.getElementById("hidden_validruleconfig_" + index);
                    editor.spanruleObj = document.getElementById("rule_" + index);
                    OpenControlDlg("validruleconfig");
                }
            });
        }
        //打开控件属性页面
        function OpenControlDlg(type) {
            editor.execCommand(type);
        }

        function add_rule() {
            CreateHtml();
        }
        function delete_rule(index) {
            var tableBody = $("#tableRuleData");
            var count = eval(tableBody.attr("count"));
            tableBody.attr("count", --count);
            $("#tr_ruleList_" + index).remove();
        }

        dialog.oncancel = function () {

        };
        dialog.onok = function () {
            var validJsonArray = new Array();
            var tableBody = $("#tableRuleData");
            var count = eval(tableBody.attr("count"));
            var validJson;
            for (var i = 1; i <= count; i++) {
                validJson = $("#hidden_validruleconfig_" + i).attr("validjsonarray");

                validJson = JSON.parse(validJson);
                if (validJson) {
                    validJson[0].msg = $("#msg_" + i).textbox("getText");
                    validJsonArray.push(validJson);
                }
            } 
            oNode.setAttribute('validjsonarray', JSON.stringify(validJsonArray));
            oNode.setAttribute('leipitype', 'validrulelist');
        };

    </script>
</body>
</html>
