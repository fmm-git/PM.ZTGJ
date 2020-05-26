<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="datagrid.aspx.cs" Inherits="PM.Web.UiFrame.ueditor.formdesign.datagrid" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>明细表</title>
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
                element = document.createElement('<' + type + ' id="' + name + '"' + ' name="' + name + '">');
            } catch (e) { }
            if (element == null) {
                element = document.createElement(type);
                element.name = name;
                element.id = name;
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
    <div>
        <table>
            <tr>
                <td><b>来源表</b></td>
                <td>
                    <input class="easyui-combobox" id="orgname" editable="false" panelheight="auto" panelmaxheight="300px" data-options="prompt:'必填项',required:'required',missingMessage:'控件名称不能为空'" />
                </td>
                <td><b>宽度</b></td>
                <td>
                    <input id="orgwidth" value="100%" class="easyui-textbox" style="width: 60px" data-options="prompt:'auto'" />
                </td>
                <td><b>高度</b></td>
                <td>
                    <input id="orgheight" class="easyui-textbox" style="width: 60px" data-options="prompt:'auto'" />
                </td>
                <td>
                    <input id="hidden_BatAdd" type="hidden" sourcetable="" fieldcode="" textfield="" orgchildfields="" orgparentfields="" />
                    <a id="btn_BatAddConfig" class="easyui-linkbutton" href="javascript:void(0);">启用添多行</a>
                </td>
                <td>
                    <a id="btn_ClearConfig" class="easyui-linkbutton" href="javascript:void(0);">禁用添多行</a>
                </td>
                <td>
                    <input id="hidden_ValidRule" type="hidden" />
                    <a id="btn_ValidRuleConfig" class="easyui-linkbutton" href="javascript:void(0);">验证规则配置</a>
                </td>
            </tr>
        </table>
        <table id="tableField" style="margin-left: 5px;">
            <thead>
            </thead>
            <tbody>
            </tbody>
        </table>
    </div>
    <script type="text/javascript">
        var mainTableName = editor.tableName;
        var oNode = null;
        var thePlugins = 'datagrid';
        var fieldCount = 0;
        var loadNodeSuccess;
        var oldGridTableName;
        var totalFieldList;
        window.onload = function () {
            $("#btn_BatAddConfig").linkbutton('disable');
            $("#btn_ClearConfig").linkbutton('disable');
            InitTableCombobox();
            BindBatAddConfig();
            BindValidRuleConfig();
            InitTotalFieldCombobox(mainTableName);
            if (UE.plugins[thePlugins].editdom) {
                $("#btn_BatAddConfig").linkbutton('enable');
                $("#btn_ClearConfig").linkbutton('enable');
                oNode = UE.plugins[thePlugins].editdom;
                loadNodeSuccess = true;
                var gName = oNode.getAttribute('id');
                var gWidth = oNode.getAttribute('width');
                var gHeight = oNode.getAttribute('height');
                gName = gName == null ? '' : gName;
                gHeight = gHeight == null ? '' : gHeight;
                var hidden_BatAdd = $("#hidden_BatAdd");
                if (oNode.getAttribute("fieldcode") != '') {//加载添多行配置
                    hidden_BatAdd.attr('sourcetable', oNode.getAttribute("sourcetable"));
                    hidden_BatAdd.attr('fieldcode', oNode.getAttribute("fieldcode"));
                    hidden_BatAdd.attr('textfield', oNode.getAttribute("textfield"));
                    hidden_BatAdd.attr('orgchildfields', oNode.getAttribute("orgchildfields"));
                    hidden_BatAdd.attr('orgparentfields', oNode.getAttribute("orgparentfields"));
                }
                var hidden_ValidRule = $("#hidden_ValidRule");
                if (oNode.getAttribute('validjsonarray') && oNode.getAttribute('validjsonarray') != "") {
                    hidden_ValidRule.attr("validjsonarray", oNode.getAttribute('validjsonarray'));
                }
                $('#orgname').combobox('setValue', gName);
                $('#orgwidth').textbox('setText', gWidth);
                $('#orgheight').textbox('setText', gHeight);
                editor.gridTableName = gName;
                oldGridTableName = gName;
                InitFieldCombobox(gName);
            }
        }

        //绑定批量添加配置
        function BindBatAddConfig() {
            $("#btn_BatAddConfig").linkbutton({
                onClick: function () {
                    editor.hiddenObj = document.getElementById("hidden_BatAdd");
                    OpenControlDlg("bataddconfig");
                }
            });
            $("#btn_ClearConfig").linkbutton({
                onClick: function () {
                    var hidden_BatAdd = $("#hidden_BatAdd");
                    hidden_BatAdd.attr('sourcetable', '');
                    hidden_BatAdd.attr('fieldcode', '');
                    hidden_BatAdd.attr('textfield', '');
                    hidden_BatAdd.attr('orgchildfields', '');
                    hidden_BatAdd.attr('orgparentfields', '');
                }
            });
        }

        //绑定验证规则配置
        function BindValidRuleConfig() {
            $("#btn_ValidRuleConfig").linkbutton({
                onClick: function () {
                    editor.hiddenObj = document.getElementById("hidden_ValidRule");
                    OpenControlDlg("validrulelist");
                }
            });
        }

        //初始化业务表列表
        function InitTableCombobox() {
            $("#orgname").combobox({
                url: '<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetAllTableList" })) %>',
                textField: 'BusinessTableName',
                valueField: 'PhysicalTableName',
                onSelect: function (node) {
                    $("#btn_BatAddConfig").linkbutton('enable');
                    $("#btn_ClearConfig").linkbutton('enable');
                    editor.gridTableName = node.PhysicalTableName;
                    InitFieldCombobox(node.PhysicalTableName);
                }
            });
        }

        //初始化字段列表
        function InitFieldCombobox(physicalTableName) {
            $.ajax({
                url: '<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetGridTableFieldList" })) %>',
                data: { "PhysicalTableName": physicalTableName },
                success: function (data) {
                    CreateListFieldElements(data);
                },
                dataType: "json",
                async: false
            });
        }

        //初始化统计字段列表
        function InitTotalFieldCombobox(physicalTableName) {
            $.ajax({
                url: '<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "GetTableFieldsList" })) %>',
                data: { "PhysicalTableName": physicalTableName },
                success: function (data) {
                    totalFieldList = data;
                    totalFieldList.unshift({ FieldCode: '', FieldName: "无" });
                },
                dataType: "json",
                async: false
            });
        }

        //初始化格式化器列表
        function InitFormatterCombobox(fieldCount) {
            //valueField:'FormatterJs',
            //textField:'FormatterName',
            $.ajax({
                url: '<%= ResolveUrl(GetRouteUrl("GridListApi", new { FunCode = "getFormatterContent" })) %>',
                success: function (data) {
                    BindFormatterCombobox(data, fieldCount);
                },
                dataType: "json",
                async: false
            });
        }

        //绑定格式化器下拉框
        function BindFormatterCombobox(data, fieldCount) {
            data.unshift({ FormatterJs: '', FormatterName: "无" });
            var formatterComboboxObj;
            for (var i = 0; i < fieldCount; i++) {
                formatterComboboxObj = $("#formatter_" + i);
                formatterComboboxObj.combobox({
                    valueField: 'FormatterJs',
                    textField: 'FormatterName'
                });
                formatterComboboxObj.combobox('loadData', data);
            }
        }

        //创建字段html
        function CreateListFieldElements(data) {
            var html = "";
            fieldCount = data.length;
            for (var i = 0; i < data.length; i++) {
                html += "<tr>";
                html += "<td>";
                html += '<a  href="javascript:void(0);" onclick="MoveUp(' + i + ')">上移</a>&nbsp;<a  href="javascript:void(0);" onclick="MoveDown(' + i + ')">下移</a>';
                html += "</td>";
                html += "<td>";
                html += i + 1;
                html += "</td>";
                html += "<td>";
                html += '<input id="head_' + i + '" class="easyui-combobox" editable="false" panelheight="auto" panelmaxheight="200px" style="width: 100px" textField="FieldName" valueField= "FieldCode" />';
                html += "</td>";
                html += "<td>";
                html += '<input id="width_' + i + '" value="150px" class="easyui-textbox" style="width: 60px" />';
                html += "</td>";
                html += "<td>";
                html += '<select id="align_' + i + '" class="easyui-combobox" editable="false" panelheight="auto" panelmaxheight="120px">';
                html += '<option value="left">左对齐</option>';
                html += '<option value="center">居中</option>';
                html += '<option value="right">右对齐</option>';
                html += '</select>';
                html += "</td>";
                html += '<td style="text-align:center;width:50px">';
                html += '<input id="check_' + i + '" type="checkbox" />';
                html += "</td>";
                html += "<td>";
                html += '<input id="type_' + i + '" class="easyui-combobox" editable="false" panelheight="auto" panelmaxheight="200px" />';
                html += "</td>";
                html += "<td>";
                html += '<a id="button_' + i + '" class="easyui-linkbutton"  href="javascript:void(0);">配置</a>';
                html += '<input id="hidden_' + i + '" type="hidden" disedit="false" controlstate="0" />';
                html += "</td>";
                html += '<td style="text-align:center;width:50px">';
                html += '<input id="total_' + i + '" type="checkbox" />';
                html += "</td>";
                html += '<td>';
                html += '<input id="totalField_' + i + '" class="easyui-combobox" editable="false" panelheight="auto" panelmaxheight="200px" style="width: 100px" textField="FieldName" valueField= "FieldCode" />';
                html += "</td>";
                html += '<td style="text-align:center;width:50px">';
                html += '<input id="primary_' + i + '" type="checkbox" />';
                html += "</td>";
                html += "<td>";
                html += '<input id="formatter_' + i + '" class="easyui-combobox" editable="false" panelheight="auto" panelmaxheight="200px"  style="width: 120px" />';
                html += "</td>";
                html += "</tr>";
            }
            $("#tableField thead").html("<tr><th>操作</th><th>序号</th><th>表头</th><th>宽度</th><th>对齐方式</th><th>禁止编辑</th><th>类型</th><th></th><th>启用合计</th><th>主表合计字段</th><th>验证重复</th><th>格式化器</th></tr>");
            $("#tableField tbody").html(html);
            $.parser.parse("#tableField tbody");
            data.unshift({ FieldCode: '', FieldName: "无" });
            for (var i = 0; i < data.length; i++) {
                $("#head_" + i).combobox('loadData', data);
                $("#totalField_" + i).combobox('loadData', totalFieldList);
                InitTypeCombobox(i);
                BindConfigBtnOnClick(i);
                BindCheckboxOnChange(i);
            }
            InitFormatterCombobox(fieldCount);
            //修改时加载
            LoadDataGridConfig();
        }

        //修改时，加载明细表配置
        function LoadDataGridConfig() {
            if (loadNodeSuccess) {//修改时加载
                var kid = oNode.children[0].children[0].children;
                var length = parseInt(kid.length / 2);
                for (var i = 0; i < length; i++) {
                    var oHidden = kid[i];
                    $("#head_" + i).combobox('setValue', oHidden.getAttribute("fieldcode") + "," + oHidden.getAttribute("sourcetable"));
                    $("#totalField_" + i).combobox('setValue', oHidden.getAttribute("totalfield") ? oHidden.getAttribute("totalfield") : "");

                    $("#align_" + i).combobox('setValue', oHidden.getAttribute("aligntype"));
                    $("#width_" + i).textbox('setText', oHidden.getAttribute("fieldWidth"));
                    $("#check_" + i).prop("checked", oHidden.getAttribute("disedit") == "true");
                    $("#total_" + i).prop("checked", oHidden.getAttribute("totalabled") == "true");
                    $("#primary_" + i).prop("checked", oHidden.getAttribute("primary") == "true");
                    $("#formatter_" + i).combobox("setValue", oHidden.getAttribute("fomatterJs") ? oHidden.getAttribute("fomatterJs") : "");
                    if (oHidden.getAttribute("disedit") == "true") {
                        $("#type_" + i).combobox("disable");
                        $("#button_" + i).linkbutton("disable");
                        $("#total_" + i).unbind().click(function () {
                            return false;
                        });
                        $("#primary_" + i).unbind().click(function () {
                            return false;
                        });
                    }
                    $("#type_" + i).combobox('setValue', oHidden.getAttribute("leipitype"));
                    oHidden.id = "hidden_" + i;
                    $("#hidden_" + i).prop('outerHTML', oHidden.outerHTML);
                }
                loadNodeSuccess = false;
            }
        }

        //绑定禁止编辑
        function BindCheckboxOnChange(i) {
            $("#check_" + i).on('change', function () {
                DisOrEnEdit($(this), i);
            });
        }

        //启用或禁用编辑
        function DisOrEnEdit(cbx, i) {
            var isChecked = cbx.prop("checked");
            var isDisable = isChecked ? 'disable' : 'enable';
            $("#type_" + i).combobox(isDisable);
            $("#totalField_" + i).combobox(isDisable);
            $("#formatter_" + i).combobox(isDisable);
            $("#button_" + i).linkbutton(isDisable);
            $("#total_" + i).prop("checked", false);
            $("#primary_" + i).prop("checked", false);
            if (isChecked) {
                $("#total_" + i).unbind().click(function () {
                    return false;
                });
                $("#primary_" + i).unbind().click(function () {
                    return false;
                });
            } else {
                $("#total_" + i).unbind().click(function () {
                    return true;
                });
                $("#primary_" + i).unbind().click(function () {
                    return true;
                });
            }
        }

        //绑定配置按钮
        function BindConfigBtnOnClick(i) {
            $("#button_" + i).linkbutton({
                onClick: function () {
                    var fieldInfo = $("#head_" + i).combobox('getValue');
                    var arr = fieldInfo.split(',');
                    var fieldcode = arr[0];
                    var sourcetable = arr[1];
                    var type = $("#type_" + i).combobox('getValue');
                    if (fieldcode) {
                        editor.hiddenObj = document.getElementById("hidden_" + i);
                        editor.hiddenObj.setAttribute("fieldcode", fieldcode);
                        editor.hiddenObj.setAttribute("sourcetable", sourcetable);
                        OpenControlDlg(type);
                    }
                    else {
                        alert("请先选择表头字段");
                    }
                }
            });
        }

        function ChangeValue(i, isUp) {
            var lastIndex = isUp ? i - 1 : i + 1;
            var tempValue;
            var lastObj;
            var curObj;
            if (lastIndex < 0 || lastIndex > fieldCount) {
                return;
            }
            lastObj = $("#head_" + lastIndex);
            curObj = $("#head_" + i);
            tempValue = lastObj.combobox('getValue');
            lastObj.combobox('setValue', curObj.combobox('getValue'));
            curObj.combobox('setValue', tempValue);

            lastObj = $("#width_" + lastIndex);
            curObj = $("#width_" + i);
            tempValue = lastObj.textbox('getValue');
            lastObj.textbox('setValue', curObj.textbox('getValue'));
            curObj.textbox('setValue', tempValue);

            lastObj = $("#align_" + lastIndex);
            curObj = $("#align_" + i);
            tempValue = lastObj.combobox('getValue');
            lastObj.combobox('setValue', curObj.combobox('getValue'));
            curObj.combobox('setValue', tempValue);

            lastObj = $("#check_" + lastIndex);
            curObj = $("#check_" + i);
            tempValue = lastObj.prop('checked');
            lastObj.prop('checked', curObj.prop('checked'));
            curObj.prop('checked', tempValue);
            DisOrEnEdit(lastObj, lastIndex);
            DisOrEnEdit(curObj, i);

            lastObj = $("#type_" + lastIndex);
            curObj = $("#type_" + i);
            tempValue = lastObj.combobox('getValue');
            lastObj.combobox('setValue', curObj.combobox('getValue'));
            curObj.combobox('setValue', tempValue);

            lastObj = $("#hidden_" + lastIndex);
            curObj = $("#hidden_" + i);
            tempValue = lastObj.clone();
            ChangeHidden(curObj, lastObj);
            ChangeHidden(tempValue, curObj);
            lastObj.attr("id", "hidden_" + lastIndex);
            curObj.attr("id", "hidden_" + i);

            lastObj = $("#total_" + lastIndex);
            curObj = $("#total_" + i);
            tempValue = lastObj.prop('checked');
            lastObj.prop('checked', curObj.prop('checked'));
            curObj.prop('checked', tempValue);

            lastObj = $("#totalField_" + lastIndex);
            curObj = $("#totalField_" + i);
            tempValue = lastObj.combobox('getValue');
            lastObj.combobox('setValue', curObj.combobox('getValue'));
            curObj.combobox('setValue', tempValue);

            lastObj = $("#primary_" + lastIndex);
            curObj = $("#primary_" + i);
            tempValue = lastObj.prop('checked');
            lastObj.prop('checked', curObj.prop('checked'));
            curObj.prop('checked', tempValue);

            lastObj = $("#formatter_" + lastIndex);
            curObj = $("#formatter_" + i);
            tempValue = lastObj.combobox('getValue');
            lastObj.combobox('setValue', curObj.combobox('getValue'));
            curObj.combobox('setValue', tempValue);
        }

        //替换隐藏域配置
        function ChangeHidden(oldObj, newObj) {
            var attrs = newObj.get(0).attributes;
            var length = attrs.length;
            for (var i = length - 1; i >= 0; i--) {
                newObj.removeAttr(attrs[i].name);
            }
            attrs = oldObj.get(0).attributes;
            length = attrs.length;
            for (var i = 0; i < length; i++) {
                newObj.attr(attrs[i].name, attrs[i].value);
            }
        }

        function MoveUp(i) {
            ChangeValue(i, true);
        }

        function MoveDown(i) {
            ChangeValue(i, false);
        }

        //打开控件属性页面
        function OpenControlDlg(type) {
            editor.execCommand(type);
        }
        //初始化类型列表
        function InitTypeCombobox(i) {
            $("#type_" + i).combobox({
                data: [
                { 'id': 'text', 'text': '文本框' },
                { 'id': 'number', 'text': '数字框' },
                { 'id': 'datebox', 'text': '日期框' },
                { 'id': 'textdialog', 'text': '文本弹出框' },
                //{ 'id': 'combobox', 'text': '下拉框' },
                { 'id': 'combotree', 'text': '下拉树' },
                { 'id': 'combocustom', 'text': '下拉字典' },
                //{ 'id': 'combogrid', 'text': '下拉表格' },
                { 'id': 'checkbox', 'text': '复选框' },
                { 'id': 'expression', 'text': '公式' }],
                textField: 'text',
                valueField: 'id',
                onLoadSuccess: function (data) {
                    if (data) {
                        $(this).combobox('setValue', data[0].id);
                    }
                },
                onSelect: function () {
                    RemoveHiddenAttr(i);
                }
            });
        }

        function RemoveHiddenAttr(i) {
            $("#hidden_" + i).removeAttr("controltype editable expjsonarray onClickButton orgchildfields orgparentfields orgtext required missingMessage prompt valuefield panelheight panelmaxheight requireddisabled url textfield value validType orgtype orgdictionarytable");
        }

        dialog.oncancel = function () {
            editor.gridTableName = null;
            if (UE.plugins[thePlugins].editdom) {
                delete UE.plugins[thePlugins].editdom;
            }
        };

        function InsertDataGridHtml() {
            var isAdd = !oNode;
            var gName = $('#orgname').combobox('getValue').replace(/\"/g, "&quot;");
            var gWidth = $('#orgwidth').textbox('getText');
            var gHeight = $('#orgheight').textbox('getText');
            if (isAdd) {
                oNode = createElement('table', gName);
            }
            else {
                oNode.setAttribute('id', gName);
            }
            oNode.setAttribute('name', gName);
            oNode.setAttribute('class', 'easyui-datagrid');
            oNode.setAttribute('leipiPlugins', thePlugins);
            oNode.setAttribute('controlstate', "0");
            oNode.setAttribute('fieldcount', fieldCount);
            oNode.setAttribute('toolbar', "#tb_" + gName);
            oNode.setAttribute('url', "/api/form/viewdetail/" + editor.formCode + "/" + gName + "/{RouteID}");
            oNode.setAttribute('method', "get");
            oNode.setAttribute('width', gWidth);
            if (gHeight) {
                oNode.setAttribute('height', gHeight);
            }
            oNode.setAttribute('data-options', "striped:'true',rownumbers:'true',onCheckAll:GridCheckAll,onUncheckAll:GridUnCheckAll,idField:'id',showFooter:'true',onLoadSuccess:onGridLoadSuccess,onClickRow:onGridClickRow,onResizeColumn:onGridResizeColumn");

            var hidden_BatAdd = $("#hidden_BatAdd");
            oNode.setAttribute('sourcetable', hidden_BatAdd.attr("sourcetable"));
            oNode.setAttribute('fieldcode', hidden_BatAdd.attr("fieldcode"));
            oNode.setAttribute('textfield', hidden_BatAdd.attr("textfield"));
            oNode.setAttribute('orgchildfields', hidden_BatAdd.attr("orgchildfields"));
            oNode.setAttribute('orgparentfields', hidden_BatAdd.attr("orgparentfields"));

            var hidden_ValidRule = $("#hidden_ValidRule");
            if (hidden_ValidRule.attr("validjsonarray") && hidden_ValidRule.attr("validjsonarray") != "") {
                oNode.setAttribute('validjsonarray', hidden_ValidRule.attr("validjsonarray"));
            }

            var htmlJson = CreateDataGridTheadHtmlJson(gName, fieldCount);

            oNode.setAttribute('fieldList', htmlJson.fieldList);
            oNode.setAttribute('totalFieldList', htmlJson.totalFieldList);
            oNode.setAttribute('primaryFieldList', htmlJson.primaryFieldList);

            oNode.innerHTML = htmlJson.html;
            editor.gridTableName = null;
            if (isAdd) {
                editor.execCommand('insertHtml', oNode.outerHTML);
            }
            else {
                delete UE.plugins[thePlugins].editdom;
            }
        }

        //创建明细表表头HtmlJson
        function CreateDataGridTheadHtmlJson(gName, fieldCount) {
            var html = '';//明细表Html
            var thead = '';//表头Html
            var hiddenHtml = '';//隐藏域累加Html
            var fieldInfo;//字段信息（字段代码,来源表名）
            var fieldText;//字段显示名称
            var sourceTable;//来源表名
            var fieldCode;//字段代码
            var fieldList = '';//字段列表
            var fieldSplitStr = '';//字段列表分隔符
            var totalFieldList = '';//统计字段列表
            var totalFieldSplitStr = '';//统计字段列表分隔符
            var primaryFieldList = '';//验证重复字段列表
            var primaryFieldSplitStr = '';//验证重复字段列表分隔符
            var editorNode;//字段配置隐藏域
            var align;//对齐方式
            var fieldWidth;//表头宽度
            var isDisedit;//是否不能编辑
            var fomatterJs;//格式化器js
            var totalField;
            html += "<thead><tr>";
            for (var i = 0; i < fieldCount; i++) {
                fieldInfo = $('#head_' + i).combobox('getValue').split(',');
                totalField = $('#totalField_' + i).combobox('getValue');
                align = $('#align_' + i).combobox('getValue');
                fieldWidth = $('#width_' + i).textbox('getText');
                editorNode = $('#hidden_' + i);
                isDisedit = $('#check_' + i).prop('checked');
                fomatterJs = $('#formatter_' + i).combobox('getValue');
                fomatterJs = fomatterJs == "" ? "function (value, row, index) { return GridFormatter(value, row, index,this.field,'" + gName + "');}" : "function (value, row, index) {" + fomatterJs + " var id = this.field + '_' + row.id; return GridCustomFormatter(id, reValue,'" + gName + "');}";
                if (fieldInfo != '') {
                    if (isDisedit) {
                        RemoveHiddenAttr(i);
                    }
                    fieldText = $('#head_' + i).combobox('getText');
                    fieldCode = fieldInfo[0];
                    sourceTable = fieldInfo[1];
                    fieldList += fieldSplitStr + fieldCode;
                    fieldSplitStr = ',';
                    if (fieldWidth != "") {
                        editorNode.attr('fieldWidth', fieldWidth);
                    }
                    if (totalField != "") {
                        editorNode.attr('totalfield', totalField);
                    }
                    if (align != "") {
                        editorNode.attr('aligntype', align);
                    }
                    else {
                        editorNode.removeAttr('aligntype');
                    }
                    editorNode.attr('datagridID', gName);//保存明细表ID
                    //是否统计字段
                    editorNode.attr('totalabled', $('#total_' + i).prop('checked'));
                    if ($('#total_' + i).prop('checked')) {
                        totalFieldList += totalFieldSplitStr + fieldCode;
                        totalFieldSplitStr = ',';
                    }
                    //是否不重复字段
                    editorNode.attr('primary', $('#primary_' + i).prop('checked'));
                    if ($('#primary_' + i).prop('checked')) {
                        primaryFieldList += primaryFieldSplitStr + fieldCode;
                        primaryFieldSplitStr = ',';
                    }
                    //是否选择了自定义格式化器
                    if (fomatterJs) {
                        editorNode.attr("fomatterJs", $('#formatter_' + i).combobox('getValue'));
                    }
                    editorNode.attr("disedit", isDisedit);
                    //
                    thead += "<th data-options=\"field:'" + fieldCode + "',align:'" + align + "' ,formatter:" + fomatterJs + "\" " + (fieldWidth == "" ? "" : 'width="' + fieldWidth + '"') + ">" + fieldText + "</th>";
                    hiddenHtml += editorNode.prop('outerHTML');
                }
            }
            thead = thead == "" ? "" : "<th data-options=\"field:'id',formatter:function (value, row, index) {return GridCheckboxFormatter(value, row, index,'" + gName + "');}\"><input type=\"checkbox\"></th>" + thead;
            html += hiddenHtml + thead + "</tr></thead>";
            return {
                html: html,
                fieldList: fieldList,
                totalFieldList: totalFieldList,
                primaryFieldList: primaryFieldList
            };
        }

        //保存明细表到数据库
        function SaveGridContorls() {
            var controlsData = GetControlData();
            var repickMsg = HasRepicField(controlsData);
            if (repickMsg) {
                $.messager.alert("", repickMsg);
                return false;
            }
            else {
                $.ajax({
                    type: "post",
                    url: '<%=ResolveUrl(Page.GetRouteUrl("ClearAndBatInsert", new { FunCode = "AddFormControl" }))%>',
                    data: {
                        "FormCode": editor.formCode,
                        "PhysicalTableName": editor.gridTableName,
                        "DelTableName": oldGridTableName ? oldGridTableName : editor.gridTableName,
                        "DelFunCode": "DeleteFormControls",
                        "OtherItemCUDFunCode": "InsertSubTable",
                        "data": JSON.stringify(controlsData)
                    },
                    success: function (data) {
                        if (data.success == 'true') {

                        }
                    },
                    dataType: "json",
                    async: false
                });
                return true;
            }

        }
        //转换控件json数组
        function GetControlData() {
            var arrControls = [];
            var control = new Object();
            var hiddenObj;
            var fieldInfo;
            for (var i = 0; i < fieldCount; i++) {
                hiddenObj = $("#hidden_" + i);
                fieldInfo = $('#head_' + i).combobox('getValue').split(',');
                if (fieldInfo != '') {
                    control = new Object();
                    control.FormCode = editor.formCode;
                    control.PhysicalTableName = editor.gridTableName;
                    control.ControlType = hiddenObj.attr("controltype");
                    control.ControlID = hiddenObj.attr("fieldcode");
                    control.FieldCode = hiddenObj.attr("fieldcode");
                    control.FieldCodeSourceTable = hiddenObj.attr("sourcetable") ? hiddenObj.attr("sourcetable") : "";
                    control.ControlState = hiddenObj.attr("controlstate");
                    control.valueField = hiddenObj.attr("valuefield") ? hiddenObj.attr("valuefield") : "";
                    control.textField = hiddenObj.attr("textfield") ? hiddenObj.attr("textfield") : "";
                    control.FieldList = hiddenObj.attr("fieldlist") ? hiddenObj.attr("fieldlist") : "";
                    control.JsText = hiddenObj.attr("jstext") ? hiddenObj.attr("jstext") : "";
                    arrControls.push(control);
                }
            }
            return arrControls
        }

        //判断是否有重复字段
        function HasRepicField(controls) {
            var obj = new Object();
            for (var i = 0; i < controls.length; i++) {
                if (obj[controls[i].FieldCode]) {
                    return "明细表中有重复表头：" + controls[i].FieldCode;
                }
                else {
                    obj[controls[i].FieldCode] = 1;
                }
            }
            return null;
        }

        dialog.onok = function () {
            if ($('#orgname').combobox('getValue') == '') {
                alert('请选择字段');
                return false;
            }
            if (SaveGridContorls()) {
                InsertDataGridHtml();
            }
            else {
                return false;
            }
        };
    </script>
</body>
</html>