/*
   bug
 佛   永
 主   无
 保   八
 佑   个

*/

//浏览器 大小改变的 事件
var title = "";
//处理 滚动条
function ScrollHeightFun() {
    var hh = document.documentElement.clientHeight;
    var obfm = document.getElementsByTagName("form")[0];
    if (obfm) {
        if ($(obfm).attr("id") != "form_PerformInfo") {
            $(obfm).css("height", (hh - 30));
            //$("#dlgEdit").dialog({
            //    onLoad: function () {
            //        var tables = $("#dlgEdit table.easyui-datagrid");
            //        for (var i = 0; i < tables.length; i++) {
            //            title = $("#dlgEdit").panel('options').title;
            //            $(tables[i]).datagrid({ fit: true, height: '97%' });
            //        }
            //    }
            //});
            //$("#dlgEdit").dialog("setTitle", title);
        }
    }
}
//关闭主页中当前Tab
function closeSelectedTab() {
    if (parent.$('#mainContentTabs').length == 1) {
        var tab = parent.$('#mainContentTabs').tabs('getSelected');
        if (tab) {
            var index = parent.$('#mainContentTabs').tabs('getTabIndex', tab);
            parent.$('#mainContentTabs').tabs('close', index);
        }
    }
    else {
        var _id = parent._id;
        parent.$("#" + _id).window('close');
    }

}

//写入日志
function writeLog(ActionType) {
    var ActionMenu = '';
    if (ActionType == '登陆') {
        ActionMenu = '登陆';

        $.ajax({
            type: "post",
            url: "/api/writeLog",
            data: { actionMenu: ActionMenu, actionType: ActionType },
            async: false,
            cache: false,
            dataType: 'json'
        });
    } else {
        var tab = parent.$('#mainContentTabs').tabs('getSelected');
        ActionMenu = tab.panel('options').title;
        $.post('/api/writeLog', { actionMenu: ActionMenu, actionType: ActionType });
    }

}

//更换皮肤方法
function ShowTheam(theam) {
    var link = $(document).find('link:first');
    if (theam == null) {
        link.attr('href', '~/UiFrame/EasyUi/themes/' + top.$('#cb-theme').combobox('getValue') + '/easyui.css');
        return;
    }
    link.attr('href', '~/UiFrame/EasyUi/themes/' + theam + '/easyui.css');
}

//打开高级查询对话框(queryCode：页面查询代码)
function openQueryDlg(queryCode, controlID, controlType) {
    var dlg = $("#dlg_Query");
    dlg.dialog({ href: '/page/tool/ComQuery?QueryCode=' + queryCode + '&ControlID=' + controlID + '&ControlType=' + controlType, top: 20 }, 'refresh');
    dlg.dialog('open');
}

//关闭高级查询对话框
function closeQueryDlg() {
    var dlg = $("#dlg_Query");
    dlg.dialog('close');
}

// strTitle 表格标题
// printDatagrid 要打印的datagrid
function CreateFormPage(strTitle, printDatagrid) {
    var tableString = '<table cellspacing="0" class="pb"><tr><th colspan="#colspan#"><h1 align="center">' + strTitle + '</h1></th></tr>';
    var frozenColumns = printDatagrid.datagrid("options").frozenColumns;  // 得到frozenColumns对象
    var columns = printDatagrid.datagrid("options").columns;    // 得到columns对象
    var nameList = '';
    var sumfrozenColumn = 0;//获取多少冻结列
    var sumColumn = 0;//获取多少非冻结列
    var sumrowspanIndex = 0;//记录非冻结列多少个合并行
    var sumfrozenrowspanIndex = 0;//记录冻结列多少个合并行

    // 载入title
    if (typeof columns != 'undefined' && columns != '') {
        $(columns).each(function (index) {
            tableString += '\n<tr>';
            if (typeof frozenColumns != 'undefined' && typeof frozenColumns[index] != 'undefined') {
                for (var i = 0; i < frozenColumns[index].length; ++i) {
                    if (!frozenColumns[index][i].hidden && columns[index][i].checkbox == undefined) {
                        tableString += frozenColumns[index][i].width != undefined ? '\n<th width="' + frozenColumns[index][i].width + '"' : '\n<th ';
                        if (typeof frozenColumns[index][i].rowspan != 'undefined' && frozenColumns[index][i].rowspan > 1) {
                            tableString += ' rowspan="' + frozenColumns[index][i].rowspan + '"';
                            if (index == 0) {
                                sumfrozenrowspanIndex++;
                            }
                        } else {
                            tableString += ' rowspan="' + columns.length + '"';
                        }
                        if (typeof frozenColumns[index][i].colspan != 'undefined' && frozenColumns[index][i].colspan > 1) {
                            tableString += ' colspan="' + frozenColumns[index][i].colspan + '"';
                        }
                        if (typeof frozenColumns[index][i].field != 'undefined' && frozenColumns[index][i].field != '') {
                            nameList += ',{"f":"' + frozenColumns[index][i].field + '", "a":"' + frozenColumns[index][i].align + '"}';
                        }
                        tableString += '>' + frozenColumns[0][i].title + '</th>';
                    }
                }
                //获取最大的列
                if (sumfrozenColumn < frozenColumns[index].length) {
                    sumfrozenColumn = frozenColumns[index].length;
                }
            }
            for (var i = 0; i < columns[index].length; ++i) {
                if (!columns[index][i].hidden && columns[index][i].checkbox == undefined) {
                    tableString += columns[index][i].width != undefined ? '\n<th width="' + columns[index][i].width + '"' : '\n<th ';
                    if (typeof columns[index][i].rowspan != 'undefined' && columns[index][i].rowspan > 1) {
                        tableString += ' rowspan="' + columns[index][i].rowspan + '"';
                        if (index == 0) {
                            sumrowspanIndex++;
                        }
                    }
                    if (typeof columns[index][i].colspan != 'undefined' && columns[index][i].colspan > 1) {
                        tableString += ' colspan="' + columns[index][i].colspan + '"';
                    }
                    if (typeof columns[index][i].field != 'undefined' && columns[index][i].field != '') {
                        nameList += ',{"f":"' + columns[index][i].field + '", "a":"' + columns[index][i].align + '"}';
                    }
                    tableString += '>' + columns[index][i].title + '</th>';
                }
                //获取最大的列
                if (sumColumn < columns[index].length) {
                    sumColumn = columns[index].length;
                }
            }
            tableString += '\n</tr>';
        });
    }
    tableString = tableString.replace("#colspan#", sumColumn + sumfrozenColumn + sumrowspanIndex + sumfrozenrowspanIndex);
    // 载入内容
    var rows; // 这段代码是获取当前页的所有行
    if (printDatagrid.datagrid("getData").rows.length > 0) {
        rows = printDatagrid.datagrid("getData").rows;
    }
    else {
        rows = printDatagrid.treegrid("getChildren");
    }
    var nl = eval('([' + nameList.substring(1) + '])');
    for (var i = 0; i < rows.length; ++i) {
        tableString += '\n<tr>';
        $(nl).each(function (j) {
            var e = nl[j].f.lastIndexOf('_0');

            tableString += '\n<td';
            if (nl[j].a != 'undefined' && nl[j].a != '') {
                tableString += ' style="text-align:' + nl[j].a + ';"';
            }
            tableString += '>';
            if (e + 2 == nl[j].f.length) {
                tableString += rows[i][nl[j].f.substring(0, e)];
            }
            else
                if (rows[i][nl[j].f] != null) {
                    tableString += rows[i][nl[j].f];
                }
            tableString += '</td>';
        });
        tableString += '\n</tr>';
    }
    tableString += '\n</table>';
    //window.showModalDialog("print.htm", tableString,"location:No;status:No;help:No;dialogWidth:800px;dialogHeight:600px;scroll:auto;");
    //发送到服务器处理，把打印内容保存到session
    $.post("/WebPage/FormManager/pmDatagridPrint.aspx?action=SaveHtmlSession", { tablestr: tableString }, function (data) {
        window.open('/WebPage/FormManager/pmDatagridPrint.aspx');
    });

    //OpenWindow = window.open("", "", "");



    //OpenWindow.document.write('<style type="text/css">body {background: white;margin: 0px;padding: 0px;font-size: 13px;text-align: left;}.pb {font-size: 13px;border-collapse: collapse;}.pb th {font-weight: bold;text-align: center;border: 1px solid #333333;padding: 2px;}.pb td {border: 1px solid #333333;padding: 2px;}</style>');
    //OpenWindow.document.write('<h1 align="center">' + strTitle + '</h1>')
    //OpenWindow.document.write('<center>' + tableString + '</centerti>');
    //OpenWindow.document.write('<script>window.print();</script>');

    //OpenWindow.document.write("<TITLE>例子</TITLE>") 
    //OpenWindow.document.write("<BODY BGCOLOR=#ffffff>") 
    //OpenWindow.document.write("<h1>Hello!</h1>") 
    //OpenWindow.document.write("New window opened!") 
    //OpenWindow.document.write("</BODY>") 
    //OpenWindow.document.write("</HTML>") 
    //OpenWindow.document.close();
    //location.href = "print.aspx?tablestr=" + tableString;
    //window.open('/WebPage/FormManager/pmDatagridPrint.aspx?tablestr=<h1 align="center">' + strTitle + '</h1><center>' + tableString + '</centerti>');
}

/*验证扩展*/
$.extend($.fn.validatebox.defaults.rules, {
    minLength: { // 判断最小长度
        validator: function (value, param) {
            return value.length >= param[0];
        },
        message: "最少输入 {0} 个字符。"
    },
    length: {
        validator: function (value, param) {
            var len = $.trim(value).length;
            return len >= param[0] && len <= param[1];
        },
        message: "输入内容长度必须介于{0}和{1}之间."
    },
    phone: {// 验证电话号码
        validator: function (value) {
            return /^((\+?86)|(\(\+86\)))?\d{3,4}-\d{7,8}(-\d{3,4})?$/.test(value);
        },
        message: "格式不正确,请使用下面格式:020-88888888"
    },
    mobile: {// 验证手机号码
        validator: function (value) {
            return /^((\+?86)|(\(\+86\)))?1\d{10}$/.test(value);
        },
        message: "手机号码格式不正确"
    },
    telphone: {// 验证手机和固话
        validator: function (value) {
            return /^((\+?86)|(\(\+86\)))?1\d{10}$/.test(value) || /^((\+?86)|(\(\+86\)))?\d{3,4}-\d{7,8}(-\d{3,4})?$/.test(value);
        },
        message: "电话号码格式不正确,固话格式如：028-88888888"
    },
    idcard: {// 验证身份证
        validator: function (value) {
            return validateIdCard(value);
        },
        message: "身份证号码格式不正确"
    },
    intOrFloat: {// 验证整数或小数
        validator: function (value) {
            return /^d+(.d+)?$/i.test(value);
        },
        message: "请输入数字，并确保格式正确"
    },
    currency: {// 验证货币
        validator: function (value) {
            return /^d+(.d+)?$/i.test(value);
        },
        message: "货币格式不正确"
    },
    qq: {// 验证QQ,从10000开始
        validator: function (value) {
            return /^[1-9]d{4,9}$/i.test(value);
        },
        message: "QQ号码格式不正确"
    },
    integer: {// 验证整数
        validator: function (value) {
            return /^[+]?[1-9]+d*$/i.test(value);
        },
        message: "请输入整数"
    },
    chinese: {// 验证中文
        validator: function (value) {
            return /^[u0391-uFFE5]+$/i.test(value);
        },
        message: "请输入中文"
    },
    english: {// 验证英语
        validator: function (value) {
            return /^[A-Za-z]+$/i.test(value);
        },
        message: "请输入英文"
    },
    unnormal: {// 验证是否包含空格和非法字符
        validator: function (value) {
            return /.+/i.test(value);
        },
        message: "输入值不能为空和包含其他非法字符"
    },
    username: {// 验证用户名
        validator: function (value) {
            return /^[a-zA-Z][a-zA-Z0-9_]{5,15}$/i.test(value);
        },
        message: "用户名不合法（字母开头，允许6-16字节，允许字母数字下划线）"
    },
    faxno: {// 验证传真
        validator: function (value) {
            return /^[+]{0,1}(d){1,3}[ ]?([-]?((d)|[ ]){1,12})+$/i.test(value);
            return /^(((d{2,3}))|(d{3}-))?((0d{2,3})|0d{2,3}-)?[1-9]d{6,7}(-d{1,4})?$/i.test(value);
        },
        message: "传真号码不正确"
    },
    zip: {// 验证邮政编码
        validator: function (value) {
            return /^[1-9]d{5}$/i.test(value);
        },
        message: "邮政编码格式不正确"
    },
    ip: {// 验证IP地址
        validator: function (value) {
            return /d+.d+.d+.d+/i.test(value);
        },
        message: "IP地址格式不正确"
    },
    name: {// 验证姓名，可以是中文或英文
        validator: function (value) {
            return /^[u0391-uFFE5]+$/i.test(value) | /^w+[ws]+w+$/i.test(value);
        },
        message: "请输入姓名"
    },
    carNo: {
        validator: function (value) {
            return /^[u4E00-u9FA5][da-zA-Z]{6}$/.test(value);
        },
        message: "车牌号码无效（例：粤J12350）"
    },
    carenergin: {
        validator: function (value) {
            return /^[a-zA-Z0-9]{16}$/.test(value);
        },
        message: "发动机型号无效(例：FG6H012345654584)"
    },
    email: {
        validator: function (value) {
            return /^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+(.[a-zA-Z0-9_-])+/.test(value);
        },
        message: "请输入有效的电子邮件账号(例：abc@126.com)"
    },
    msn: {
        validator: function (value) {
            return /^w+([-+.]w+)*@w+([-.]w+)*.w+([-.]w+)*$/.test(value);
        },
        message: "请输入有效的msn账号(例：abc@hotnail(msn/live).com)"
    },
    same: {
        validator: function (value, param) {
            if ($("#" + param[0]).val() != "" && value != "") {
                return $("#" + param[0]).val() == value;
            } else {
                return true;
            }
        },
        message: "两次输入的密码不一致！"
    },
    //datagrid验证行不重复
    unrepeat: {
        validator: function (value, param) {
            var gridObj = $("#" + param[0]);
            var rows = gridObj.datagrid('getRows');
            var primaryFieldList = gridObj.attr("primaryFieldList");
            var arr_primaryField = primaryFieldList.split(',');
            var gridID = gridObj.attr("id");
            var controlObj;
            var controlType;
            var fieldCode;
            var count = 0;
            var id;
            var lastRowID = gridObj.datagrid("options").LastRowID;
            for (var i = 0; i < rows.length; i++) {
                count = 0;
                for (var j = 0; j < arr_primaryField.length; j++) {
                    fieldCode = arr_primaryField[j];
                    controlObj = $("#value_" + gridID + "_" + fieldCode + "_" + lastRowID);
                    if (controlObj.attr("id")) {
                        id = lastRowID;
                        if (id != rows[i]["id"]) {
                            controlType = controlObj.attr("controlType");
                            if (rows[i][fieldCode] == controlObj[controlType]('getValue')) {
                                count++;
                            }
                        }
                    }
                }
                if (count == arr_primaryField.length) {
                    return false;
                }
            }
            return true;
        },
        message: "当前行数据已重复！"
    }
});

/*
* 身份证15位编码规则：dddddd yymmdd xx p
* dddddd：6位地区编码
* yymmdd: 出生年(两位年)月日，如：910215
* xx: 顺序编码，系统产生，无法确定
* p: 性别，奇数为男，偶数为女
* 
* 身份证18位编码规则：dddddd yyyymmdd xxx y
* dddddd：6位地区编码
* yyyymmdd: 出生年(四位年)月日，如：19910215
* xxx：顺序编码，系统产生，无法确定，奇数为男，偶数为女
* y: 校验码，该位数值可通过前17位计算获得
* 
* 前17位号码加权因子为 Wi = [ 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 ]
* 验证位 Y = [ 1, 0, 10, 9, 8, 7, 6, 5, 4, 3, 2 ]
* 如果验证码恰好是10，为了保证身份证是十八位，那么第十八位将用X来代替
* 校验位计算公式：Y_P = mod( ∑(Ai×Wi),11 )
* i为身份证号码1...17 位; Y_P为校验码Y所在校验码数组位置
*/
function validateIdCard(idCard) {
    //15位和18位身份证号码的正则表达式
    var regIdCard = /^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$/;
    //如果通过该验证，说明身份证格式正确，但准确性还需计算
    if (regIdCard.test(idCard)) {
        if (idCard.length == 18) {
            var idCardWi = new Array(7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2); //将前17位加权因子保存在数组里
            var idCardY = new Array(1, 0, 10, 9, 8, 7, 6, 5, 4, 3, 2); //这是除以11后，可能产生的11位余数、验证码，也保存成数组
            var idCardWiSum = 0; //用来保存前17位各自乖以加权因子后的总和
            for (var i = 0; i < 17; i++) {
                idCardWiSum += idCard.substring(i, i + 1) * idCardWi[i];
            }
            var idCardMod = idCardWiSum % 11;//计算出校验码所在数组的位置
            var idCardLast = idCard.substring(17);//得到最后一位身份证号码
            //如果等于2，则说明校验码是10，身份证号码最后一位应该是X
            if (idCardMod == 2) {
                if (idCardLast == "X" || idCardLast == "x") {
                    return true;
                } else {
                    return false;
                }
            } else {
                //用计算出的验证码与最后一位身份证号码匹配，如果一致，说明通过，否则是无效的身份证号码
                if (idCardLast == idCardY[idCardMod]) {
                    return true;
                } else {
                    return false;
                }
            }
        }
    } else {
        return false;
    }
}

var _row;//接收选中行的信息

/**
打开文本弹出对话窗口并获取选中行的值
    参数说明：
    1.tableName:弹出窗口要加载的数据源的表名称
    2.controlName:查询条件(联动时的父级控件ID)
    3.filedValue:默认需要返回的value字段及控件ID
    4.filedText:默认需要返回的text字段及控件ID
    5.rowIndex:明细表当前的活动行索引
    6.gridID：明细表的id
    7.FunctionName:通过指定的SQL Server表值函数作为数据源（当FunctionName存在时，tableName将不再作为数据源）
    8.TextCallBack:回调函数,参数是当前选中的行
    9.maximized:为true时，表示弹出窗口以最大化方式显示
    10.fieldfrom:字段取值来源映射关系，结构为“showfield1=fromfield1,showfield2=fromfield2”，说明：showfield1表示需要值的字段或控件ID，fromfield1表示弹出窗口中实际的字段名称，即将弹出窗口中的字段fromfield1的值写回到id为showfield1的控件中
    11.singleSelect:为空或true表示单行选择，为false表示可多行选择；
    12.code:弹出窗口为自定义函数作为数据源时，分级列表的编码字段；
    13.parentcode:弹出窗口为自定义函数作为数据源时，分级列表的父级编码字段；
    说明：
        当参数2.controlName为空时，表示没有父级联动；
        当参数10.fieldfrom为空时,表示不需要进行字段映射；
**/
function SetTextAndValue(tableName, controlName, filedValue, filedText, rowIndex, gridID, FunctionName, TextCallBack, maximized, fieldfrom, singleSelect, code, parentcode) {
    OpenTextDialogAndSetValue('/WebPage/FormManager/pmFromDataGridorTreeGrid.aspx', tableName, controlName, filedValue, filedText, rowIndex, gridID, null, null, FunctionName, TextCallBack, maximized, fieldfrom, singleSelect, code, parentcode);
}

//明细表多行添加
function GridAddMultiRow(tableName, filedValue, rowIndex, gridID, primaryFieldList, gridDataJson, FunctionName, controlName, code, parentcode) {
    OpenTextDialogAndSetValue('/WebPage/FormManager/pmFromDataGridorTreeGrid_CheackBox.aspx', tableName, controlName, filedValue, "", rowIndex, gridID, primaryFieldList, gridDataJson, FunctionName, null, null, null, null, code, parentcode);
}

function OpenTextDialogAndSetValue(apiUrl, tableName, controlName, filedValue, filedText, rowIndex, gridID, primaryFieldList, gridDataJson, FunctionName, TextCallBack, maximized, fieldfrom, singleSelect, code, parentcode) {

    _row = null;
    var flag = false;
    var arr_controlName = controlName.split(',');
    var arr_controlValue = {};//存储传递过来的控件的值
    var parameterStr = "";//存储参数字符串
    var prefix = rowIndex ? "value_" + gridID + "_" : "";//前缀
    var suffix = rowIndex ? "_" + rowIndex : "";//后缀
    var controlObj;
    var pValue;//父级控件值
    if (controlName != "" && controlName != null) {
        for (var i = 0; i < arr_controlName.length; i++) {
            controlObj = $("#" + prefix + arr_controlName[i] + suffix);
            if (controlObj.length == 0) {//如果明细表中没有找到父级，则搜索主表
                controlObj = $("#" + arr_controlName[i]);
                pValue = controlObj.val();
                if (pValue == null) {
                    pValue = controlObj.combobox('getText');
                }
            }
            else {
                pValue = rowIndex ? controlObj.attr("textValue") : controlObj.val();
            }
            if (pValue == "") {
                alert(controlObj.attr("title") + "必须有值");
                flag = false;
                return flag;
            }
            else {
                arr_controlValue[i] = pValue;
                parameterStr += arr_controlName[i] + ':' + pValue + ',';
                flag = true;
            }
        }
    }
    else {
        flag = true;
    }
    if (code == undefined || code == null) { code = '' };
    if (parentcode == undefined || parentcode == null) { parentcode = '' };
    if (flag) {
        parameterStr = parameterStr.substring(0, parameterStr.length - 1);
        $("body").append('<div id="win" title="窗口"></div>');
        $('#win').window({
            width: 700,
            height: 500,
            modal: true,
            collapsible: false,
            minimizable: false,
            queryParams: { PhysicalTableName: tableName, parameterStr: escape(parameterStr), primaryFieldList: primaryFieldList, gridDataJson: JSON.stringify(gridDataJson), FunctionName: FunctionName, singleSelect: singleSelect, code: code, parentcode: parentcode },
            href: apiUrl,
            onClose: function () {
                if (_row != null) {
                    if (singleSelect == undefined || singleSelect === '' || singleSelect === 'true') {
                        singleSelect = true;
                    }
                    if (singleSelect == 'false') { singleSelect = false; }
                    //  if (_row.length) {//批量添加逻辑
                    if (singleSelect && _row.length) {//批量添加逻辑
                        GridStartBatAppendRow(gridID, _row);
                    }
                    else {//文本对话框赋值逻辑                        
                        var fieldListfrom;
                        if (fieldfrom != undefined && fieldfrom != null && fieldfrom != "") {
                            fieldListfrom = fieldfrom.split(',');
                        }
                        if (singleSelect) {//单行
                            for (var i = 0; i < filedValue.split(',').length; i++) {
                                var valueControlID = filedValue.split(',')[i];
                                var valueControlObj = $("#" + prefix + valueControlID + suffix);
                                if (valueControlObj.length != 0) {
                                    if (i == 0) {
                                        if (rowIndex) {
                                            valueControlObj.attr("textValue", _row[findFieldFrom(valueControlID, fieldListfrom)]);
                                            valueControlObj.textbox("setValue", _row[findFieldFrom(filedText, fieldListfrom)] + "");
                                        }
                                        else {
                                            if (valueControlObj.attr('id') == valueControlObj.attr('valuefield')) {
                                                valueControlObj.textbox('setValue', _row[findFieldFrom(valueControlID, fieldListfrom)]);
                                            }
                                            else {
                                                valueControlObj.val(_row[findFieldFrom(valueControlID, fieldListfrom)]);//文本弹出框保存Value隐藏域对象赋值
                                            }
                                        }
                                    } else {
                                        if (valueControlObj.attr('type') == "hidden") {
                                            valueControlObj.val(_row[findFieldFrom(valueControlID, fieldListfrom)]);
                                        }
                                        else {
                                            valueControlObj.textbox("setValue", _row[findFieldFrom(valueControlID, fieldListfrom)] + "");//级联字段文本框赋值                                    }
                                        }
                                    }
                                }
                                else {
                                    valueControlObj = $("#text_" + valueControlID + suffix);
                                    valueControlObj.prop('innerHTML', _row[findFieldFrom(valueControlID, fieldListfrom)]);
                                }
                            }
                        } else {//多行
                            var valueControlID = filedValue;
                            var valueControlObj = $("#" + prefix + valueControlID + suffix);
                            var hiddenVal = "";
                            var textValue = "";
                            var textboxdialogObj = $("#" + prefix + filedText + suffix);
                            //  textboxdialogObj.textbox("setValue", _row[findFieldFrom(filedText, fieldListfrom)] + "");
                            $.each(_row, function (m, item) {
                                if (m == 0) {
                                    hiddenVal = hiddenVal + item[findFieldFrom(valueControlID, fieldListfrom)];
                                    textValue = textValue + item[findFieldFrom(filedText, fieldListfrom)] + "";
                                } else {
                                    hiddenVal = hiddenVal + "," + item[findFieldFrom(valueControlID, fieldListfrom)];
                                    textValue = textValue + "," + item[findFieldFrom(filedText, fieldListfrom)] + "";
                                }
                            });
                            if (valueControlObj.length != 0) {
                                valueControlObj.val(hiddenVal);//隐藏域
                            }
                            textboxdialogObj.textbox("setValue", textValue);
                        }


                        //文本弹出框显示值赋值
                        if (singleSelect) {
                            var textboxdialogObj = $("#" + prefix + filedText + suffix);
                            textboxdialogObj.textbox("setValue", _row[findFieldFrom(filedText, fieldListfrom)] + "");
                        } else {
                            //选择多行

                        }

                        if (TextCallBack != undefined && TextCallBack != null & TextCallBack != '') {
                            TextCallBack(_row);
                        }
                    }
                }
                //移除掉window
                //$(".panel.window").remove();
                //$(".window-shadow").remove();
                //$(".window-mask").remove();
                $("#win").window('destroy');
            }
        });
        if (maximized == true) {
            $('#win').window({
                maximized: true
            });
        }
    }
    //查找取值控件映射的字段
    //returnToField:返回值需要写入的字段（控件ID）
    //FieldFromList:返回值的取值来源映射数组
    function findFieldFrom(returnToField, FieldFromList) {
        if (FieldFromList != undefined) {
            for (var f = 0; f < FieldFromList.length; f++) {
                if (returnToField == FieldFromList[f].split('=')[0]) {
                    return FieldFromList[f].split('=')[1];
                }
            }
        }
        return returnToField;
    }
}

function TextDialog(tableName, parentParam, FunctionName, TextCallBack, rowStyle, FieldOrder) {
    $("body").append('<div id="win" title="窗口"></div>');

    $('#win').window({
        width: 700,
        height: 500,
        modal: true,
        collapsible: false,
        minimizable: false,
        queryParams: { PhysicalTableName: tableName, parameterStr: escape(parentParam), primaryFieldList: null, gridDataJson: JSON.stringify(null), FunctionName: FunctionName, rowStyler: escape(rowStyle), FieldOrder: FieldOrder },
        href: '/WebPage/FormManager/pmFromDataGridorTreeGrid.aspx',
        onClose: function () {
            if (_row != null) {
                if (TextCallBack != undefined && TextCallBack != null & TextCallBack != '') {
                    TextCallBack(_row);
                }
            }
            //移除掉window          
            $("#win").window('destroy');
        }
    });
}
//pageUrl:要调用的选择页面
//codeID:要返回相关code值的textbox控件ID(为空表示不返回code)
//nameID:要返回相关name值的textbox控件ID(为空表示不返回name)
//callback:回调函数(参数为实际选择的控件内容)
//W:窗体显示的宽度；H：窗体显示的高度
function WindowFormTextbox(pageUrl, codeID, nameID, callback, W, H) {
    $("body").append('<div id="win" title="窗口"></div>');
    var Width = 680;
    var Height = 540;
    if (W != undefined && W != "") { Width = W };
    if (H != undefined && H != "") { Height = H };
    $('#win').dialog({
        width: Width,
        height: Height,
        modal: true,
        collapsible: false,
        minimizable: false,
        href: pageUrl,
        buttons: [{
            text: '确定',
            iconCls: 'icon-ok',
            handler: function () {
                var isOK = GetSelected(function (code, name) {
                    if (codeID) {
                        if ($('#' + codeID).attr('type') == 'hidden') {
                            $('#' + codeID).val(code);
                        } else {
                            $('#' + codeID).textbox('setValue', code);
                        }
                    }
                    if (nameID) {
                        if ($('#' + nameID).attr('type') == 'hidden') {
                            $('#' + nameID).val(name);
                        } else {
                            $('#' + nameID).textbox('setValue', name);
                        }
                    }
                }, callback);
                if (!isOK) {
                    $.messager.show({
                        title: '提示',
                        msg: '您还没有选中任何记录',
                        timeout: 5000,
                        showType: 'show',
                        style: {
                            left: '',
                            right: 0,
                            top: document.body.scrollTop + document.documentElement.scrollTop,
                            bottom: ''
                        }
                    });
                    return;
                };
                $("#win").dialog('close');
            }
        }, {
            text: '取消',
            iconCls: 'icon-cancel',
            handler: function () { $("#win").dialog('close'); }
        }],
        onClose: function () {
            //移除掉window            
            $("#win").dialog('destroy');
        }
    });
}
/*
功能：显示查看表单详细信息窗口
参数：
    pageUrl：表单URL链接地址；
    tableName：表单主表物理名称
    KeyFields:主键字段及其值,格式为:{field1:value1,field2,value2}
    W:弹出窗口的宽度，为空时使用默认值
    H：弹出窗口的高度，为空时使默认值
*/
function ShowFormDetailWindow(pageUrl, tableName, KeyFields, W, H) {
    if (KeyFields == undefined || KeyFields == null) {
        $.messager.alert('显示详情失败', '主键字段及值不能为空。');
        return;
    }
    var strFields = JSON.stringify(KeyFields);
    var id = "";
    $.post("/WebAPI/Form/FormGetIDByKeyField.ashx", { tableName: tableName, KeyFields: strFields }, function (data) {
        if (data.success) {
            id = data.message;
            if (pageUrl.substr(pageUrl.Length - 1, 1) == "/") {
                pageUrl = pageUrl + id;
            } else {
                pageUrl = pageUrl + "/" + id;
            }
            $("body").append('<div id="winFormDatil" title="查看详情"><iframe frameborder="no" id="ifrFromDetail" style="width: 100%; height: 99%;"></iframe></div>');
            var Width = 680;
            var Height = 520;
            if (W != undefined && W != "") { Width = W };
            if (H != undefined && H != "") { Height = H };
            $('#winFormDatil').window({
                width: Width,
                height: Height,
                modal: true,
                collapsible: false,
                minimizable: false,
                //href: pageUrl,        
                onClose: function () {
                    //移除掉window            
                    $("#winFormDatil").dialog('destroy');
                }
            });
            $("#ifrFromDetail").attr("src", pageUrl);

        } else {
            $.messager.alert('显示详情出错', data.message);
            return;
        }
    }, "json");
}
//--------------------------------------------------------表单设计器datagrid通用js------------------------------------------开始

/*选中复选框标志，防止选中复选框后触发编辑模式*/
var checkOnClickFlag = false;

/*表单设计明细表通用加载成功事件*/
function onGridLoadSuccess(data) {
    var gridObj = $(this);
    gridObj.datagrid("options").EditorData = new Object();//创建存放单元格控件数据源数组的属性
    var autoAddRow = gridObj.attr("autoAddRow");
    var gridData = gridObj.datagrid("options").EditorData;
    var fields = gridObj.datagrid("getColumnFields");
    var hiddenObj;
    var fieldName;
    var dataURL;
    var primaryHiddenObj;
    var gridID = gridObj.attr("id");
    var gridFieldCode = gridObj.attr("fieldcode");
    var comboboxData = undefined;
    if (gridFieldCode == undefined || gridFieldCode == "") {
        $("#" + gridID + "_BatAddBtn").hide();
    }
    for (var i = 0; i < fields.length; i++) {
        hiddenObj = gridObj.find("thead [fieldcode=" + fields[i] + "]");
        if (hiddenObj.length != 0) {
            dataURL = hiddenObj.attr("dataURL");
            comboboxData = hiddenObj.attr("comboboxData") ? JSON.parse(hiddenObj.attr("comboboxData")).data : undefined;

            if (dataURL) {
                fieldName = fields[i];
                $.ajax({
                    type: "GET",
                    url: dataURL,
                    success: function (result) {
                        gridData[fieldName] = result;//将ajax获取到的值放入grid属性里
                        var a = gridData;
                    },
                    async: false,
                    dataType: "json"
                });
            } else if (comboboxData != undefined) {
                fieldName = fields[i];
                gridData[fieldName] = comboboxData;//将combobox 数据源放入grid属性里

            }

            PushExpHiddenRef(hiddenObj);//关联公式列
        }
        //获取第一个不可重复隐藏域对象
        if (!primaryHiddenObj && hiddenObj.attr("primary") == "true") {
            primaryHiddenObj = hiddenObj;
        }
    }
    PushPrimaryValidateRef(primaryHiddenObj);//关联重复行验证
    LoadOrUpdateFooterRow(gridObj);//统计脚行
    LoadCalcExpression(gridObj);//计算所有公式
    //自动添加首行
    if (autoAddRow && autoAddRow == "true") {
        var rows = data.rows;
        if (rows.length == 0) {
            GridAppend(gridID);
        }
    }

}

/* 表单设计明细表通用计算所有公式
* gridObj 明细表对象
*/
function LoadCalcExpression(gridObj) {
    var rows = gridObj.datagrid('getRows');
    var fields = gridObj.datagrid("getColumnFields");
    for (var i = 0; i < fields.length; i++) {
        hiddenObj = gridObj.find("thead [fieldcode=" + fields[i] + "]");
        if (hiddenObj.length != 0) {
            for (var j = 0; j < rows.length; j++) {
                CalcExpression(hiddenObj, rows[j]);
            }
        }
    }
}

/* 表单设计明细表通用关联重复行验证
* primaryHiddenObj 不可重复隐藏域对象
*/
function PushPrimaryValidateRef(primaryHiddenObj) {
    if (primaryHiddenObj && primaryHiddenObj.attr("primary") == "true") {
        var validTypeStr = primaryHiddenObj.attr("validType");
        var gridID = primaryHiddenObj.attr("datagridID");
        if (validTypeStr != undefined && validTypeStr.indexOf('unrepeat[') == -1) {
            primaryHiddenObj.attr("validType", validTypeStr ? validTypeStr + ",unrepeat['" + gridID + "']" : "unrepeat['" + gridID + "']");
        }
    }
}
/*
*格式化数字，千位分割符
*/
function toThousands(num) {
    var parts;
    // 判断是否为数字
    if (!isNaN(parseFloat(num)) && isFinite(num)) {
        num = Number(num);
        // 处理小数点位数
        num = (typeof ',' !== 'undefined' ? num.toFixed(2) : num).toString();
        // 分离数字的小数部分和整数部分
        parts = num.split('.');
        // 整数部分加[separator]分隔, 借用一个著名的正则表达式
        parts[0] = parts[0].toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1' + (','));

        return parts.join('.');
    }
    return num;
}

/* 表单设计明细表通用更新统计列单元格
* controlObj 编辑状态控件对象
*/
function UpdateFooterCell(controlObj) {
    if (controlObj.attr('totalabled') == 'true') {
        var totalField = controlObj.attr('totalfield');
        var datagridID = controlObj.attr('datagridID');
        var totalFieldName = controlObj.attr("fieldcode");
        var gridObj = $("#" + datagridID);
        var footerRows = gridObj.datagrid('getFooterRows');
        var sumValue = GetSumField(gridObj, totalFieldName);
        footerRows[0][totalFieldName] = sumValue;
        if (totalField && totalField != "") {
            $("#" + totalField).textbox('setValue', sumValue + "");
        }
        gridObj.datagrid('reloadFooter');
    }
}

/* 表单设计明细表通用加载或更新统计行
* gridObj 明细表对象
*/
function LoadOrUpdateFooterRow(gridObj) {
    var totalFieldList = gridObj.attr("totalFieldList");
    if (totalFieldList && totalFieldList != "") {
        var arrTotalFieldList = totalFieldList.split(",");
        var totalFieldName;
        var controlObj;
        var fristField = gridObj.datagrid("getColumnFields")[0];
        if (arrTotalFieldList.length > 0) {
            var arrFooterRow = [];
            var footerRow = {};
            footerRow[fristField] = "合计";
            for (var i = 0; i < arrTotalFieldList.length; i++) {
                totalFieldName = arrTotalFieldList[i];
                controlObj = gridObj.find("thead [fieldcode=" + totalFieldName + "]");
                var totalField = controlObj.attr('totalfield');
                var sumValue = GetSumField(gridObj, totalFieldName);
                footerRow[totalFieldName] = sumValue;
                if (totalField && totalField != "") {
                    $("#" + totalField).textbox('setValue', sumValue + "");
                }
            }
            arrFooterRow.push(footerRow);
            gridObj.datagrid('reloadFooter', arrFooterRow);
        }
    }
}

/* 表单设计明细表通用统计求和
* gridObj 明细表对象
* totalFieldName 统计列字段名称
*/
function GetSumField(gridObj, totalFieldName) {
    var rows = gridObj.datagrid('getRows');
    var sum = 0;
    for (var i = 0; i < rows.length; i++) {
        if (rows[i][totalFieldName] != "" && !isNaN(rows[i][totalFieldName])) {
            sum += eval(rows[i][totalFieldName]);
        }
    }
    return sum.toFixed(2);
}

/* 表单设计明细表通用关联公式列函数
* expHiddenObj 公式配置隐藏域对象
*/
function PushExpHiddenRef(expHiddenObj) {
    var fieldHidden;
    var gridID = expHiddenObj.attr("datagridID");
    var expJsonArrayStr = expHiddenObj.attr("expjsonarray");
    if (expJsonArrayStr && expHiddenObj.attr("disedit") == "false") {
        var expJsonArray = JSON.parse(expJsonArrayStr);
        for (var i = 0; i < expJsonArray.length; i++) {
            if (expHiddenObj.attr("refEXPFieldCode")) {
                fieldHidden = $("#value_" + gridID + "_" + expJsonArray[i].field + "_" + expHiddenObj.attr("rowID"));//获取公式中涉及字段隐藏域对象
            }
            else {
                fieldHidden = $("#" + gridID + " thead [fieldcode=" + expJsonArray[i].field + "]");//获取公式中涉及字段隐藏域对象
            }
            if (fieldHidden.length != 0) {
                //追加公式隐藏域fieldcode（多个公式已逗号分割）,相当于target的作用
                var ExpHiddenArrayStr = fieldHidden.attr("ExpHiddenFieldCodeArray") ? fieldHidden.attr("ExpHiddenFieldCodeArray") + "," : "";
                fieldHidden.attr("ExpHiddenFieldCodeArray", ExpHiddenArrayStr + expHiddenObj.attr("fieldcode"));
            }
        }
    }
}

/* 表单设计明细表通用格式化函数
* value 单元格值
* row 行数据
* index 行号
* gridID 明细表ID
*/
function GridFormatter(value, row, index, field, gridID) {
    row[field] = row[field] != undefined ? "" + row[field] : undefined;
    var hiddenObj = $("#" + gridID + " thead [fieldcode=" + field + "]");
    var leipiType = hiddenObj.attr("leipitype");
    var id = gridID + '_' + field + '_' + row.id;
    value = value != undefined ? "" + value : "";
    if (hiddenObj.attr("disedit") == "false") {
        if (leipiType == "checkbox") {//复选框显示处理(0否，1是)
            if (value == "1") {
                value = "是";
            }
            else if (value == "0") {
                value = "否";
            }
            else {
                value = "";
            }
        }
        else if (leipiType == "combotree" || leipiType == "textdialog") {
            value = row[hiddenObj.attr("textfield")] != undefined ? row[hiddenObj.attr("textfield")] : "";
        }
        else if (leipiType == "datebox") {
            value = formatDatebox2(value);
        }
        else if (leipiType == "number") {
            value = toThousands(value);
        }
        else if (leipiType == "expression") {
            value = toThousands(value);
        }
    }
    return '<div id="cell_' + id + '">' + '<span id="text_' + id + '">' + value + '</span></div>';
}

/* 表单设计明细表通用复选框格式化函数
* value 单元格值
* row 行数据
* index 行号
* gridID 明细表ID
*/
function GridCheckboxFormatter(value, row, index, gridID) {

    if (value != "合计") {
        return '<input type="checkbox"  gridID="' + gridID + '" checkBoxRowID="' + value + '" onclick="GridSetCheckOnClickFlagTrue()"/>';
    }
    return value;
}

/*设置选中复选框标志为true*/
function GridSetCheckOnClickFlagTrue() {
    checkOnClickFlag = true;
}

function GridCheckAll(rows) {
    for (var i = 0; i < rows.length; i++) {
        $("[checkBoxRowID=" + rows[i].id + "]").prop("checked", true);
    }
}

function GridUnCheckAll(rows) {
    for (var i = 0; i < rows.length; i++) {
        $("[checkBoxRowID=" + rows[i].id + "]").prop("checked", false);
    }
}

/* 表单设计明细表通用自定义格式化函数
* id 标识符(fieldcode_rowid)
* value 单元格值
*/
function GridCustomFormatter(id, value, gridID) {

    return '<div id="cell_' + gridID + "_" + id + '">' + '<span id="text_' + gridID + "_" + id + '">' + value + '</span></div>';
}

/* 表单设计明细表通用调整列宽函数
* field 字段
* width 宽度
*/
function onGridResizeColumn(field, width) {
    var gridObj = $(this);
    var row = gridObj.datagrid("getSelected");
    if (row) {
        var id = gridObj.attr("id") + "_" + field + "_" + row.id;
        var controlObj = $("#value_" + id);
        if (controlObj.length > 0) {
            var hiddenObj = gridObj.find("thead [fieldcode=" + field + "]");
            var controlType = hiddenObj.attr("controltype");
            controlObj[controlType]('resize', width - 10);
        }
    }
}

/* 表单设计明细表通用单击行事件
* index 行号
* row 行数据
*/
function onGridClickRow(index, row) {
    if (!checkOnClickFlag) {//没有点击复选框则开启编辑模式
        var gridObj = $(this);
        var gridID = gridObj.attr("id");
        var lastRowID = gridObj.datagrid("options").LastRowID;
        var lastRowIndex = gridObj.datagrid("options").LastRowIndex;
        if (lastRowID != row.id) {
            var fieldList = gridObj.datagrid("getColumnFields");
            var datas = gridObj.datagrid("options").EditorData;
            var isPass = ValidateLastRow(gridID, fieldList, lastRowID);//先验证上一行
            if (isPass) {//通过验证
                gridObj.datagrid("options").LastRowID = row.id;
                gridObj.datagrid("options").LastRowIndex = index;
                EndLastRowCellEdit(gridID, fieldList, lastRowID);//结束上一行的编辑
                //取消选中上一行
                if (lastRowIndex) {
                    gridObj.datagrid("unselectRow", lastRowIndex);
                }
                //开始当前行编辑
                for (var i = 0; i < fieldList.length; i++) {
                    var field = fieldList[i];
                    var id = gridID + "_" + field + "_" + row.id;
                    var div_Cell = $("#cell_" + id);
                    var spanObj = $("#text_" + id);
                    var controlObj = $("#value_" + id);
                    var hiddenObj = gridObj.find("thead [fieldcode=" + field + "]").clone();
                    var controlType = hiddenObj.attr("controltype");
                    var leipiType = hiddenObj.attr("leipitype");

                    if (hiddenObj.length != 0 && hiddenObj.attr("disedit") == "false" && leipiType != "expression") {
                        spanObj.hide();//隐藏当前单元格显示值
                        var fieldOption = gridObj.datagrid("getColumnOption", field);
                        if (div_Cell.attr("isParse")) {//解析过则直接赋值并显示
                            //调整控件宽度适应列宽
                            controlObj[controlType]('resize', fieldOption.width - 10);
                            $("#span_" + id).show();//显示控件
                        }
                        else {
                            //转换头部隐藏域为对应EasyUI控件
                            hiddenObj = ChangeToEasyUIControl(hiddenObj, fieldOption, row.id);
                            //多加一层span方便隐藏和显示
                            div_Cell.append('<span id="span_' + id + '">' + hiddenObj.prop("outerHTML") + "</span>");
                            //解析单元格easyui控件
                            $.parser.parse("#cell_" + id);
                            //$("#span_" + id).hide();
                            div_Cell.attr("isParse", "1");//解析标志
                            //重新找解析后元素
                            controlObj = $("#value_" + id);
                            //初始化并加载EasyUI控件
                            InitEasyUIControl(controlObj, leipiType, controlType, datas ? datas[field] : null, spanObj, row);
                            //调整宽度
                            //gridObj.datagrid("autoSizeColumn", field);
                        }
                    }
                }
            }
        }
    }
    checkOnClickFlag = false;//关闭点击复选框标志
}

/* 结束上一行的编辑
* field 字段名
* lastRowID 上一行行ID
*/
function EndLastRowCellEdit(gridID, fieldList, lastRowID) {
    if (lastRowID) {
        for (var i = 0; i < fieldList.length; i++) {
            var field = fieldList[i];
            var lastID = gridID + "_" + field + "_" + lastRowID;
            $("#text_" + lastID).show();
            $("#span_" + lastID).hide();
        }
    }
}

/* 验证上一行
* fieldList 字段列表
* lastRowID 上一行行ID
*/
function ValidateLastRow(gridID, fieldList, lastRowID) {
    var isPass = true;
    if (lastRowID) {
        for (var i = 0; i < fieldList.length; i++) {
            var field = fieldList[i];
            var lastID = gridID + "_" + field + "_" + lastRowID;
            var hiddenObj = $("#" + gridID + " thead [fieldcode=" + field + "]");
            var controlType = hiddenObj.attr("controltype");
            var controlObj = $("#value_" + lastID);

            if (controlObj.length != 0 && controlType && controlType != "checkbox") {//复选框不是easyui控件，不支持验证方法                
                if (!controlObj[controlType]('isValid')) {
                    isPass = false;
                    break;
                }
            }
        }
    }
    return isPass;
}

/* 修改标签为easyui控件
* hiddenObj 隐藏域控件对象
* field 字段对象
* rowID 行ID
*/
function ChangeToEasyUIControl(hiddenObj, fieldOption, rowID) {
    var gridID = hiddenObj.attr("datagridID");
    var id = gridID + "_" + fieldOption.field + "_" + rowID;
    var type = hiddenObj.attr("controltype");
    var leipiType = hiddenObj.attr("leipitype");

    switch (leipiType) {
        case "checkbox":
            hiddenObj.attr("type", type);
            break;
        case "textdialog":
            var sourceTable = hiddenObj.attr('sourcetable');
            var gName = hiddenObj.attr('fieldcode');
            var gText = hiddenObj.attr('textfield');
            var gChildfields = hiddenObj.attr('orgchildfields');
            var gParentfields = hiddenObj.attr('orgparentfields');
            var gCodeFields = hiddenObj.attr('codefield');//分级编码字段
            var gParentCodeFields = hiddenObj.attr('parentcodefield');//分级的父级编码字段
            var gFunctionName = hiddenObj.attr('FunctionName');
            gFunctionName = gFunctionName ? gFunctionName : "";
            gChildfields = gChildfields ? gChildfields : "";
            gParentfields = gParentfields ? gParentfields : "";
            gCodeFields = gCodeFields || "";
            gParentCodeFields = gParentCodeFields || "";

            var fieldfrom = hiddenObj.attr('fieldfrom');//字段来源对应关系
            hiddenObj.attr("data-options", "onClickButton:function(){SetTextAndValue('" + sourceTable + "','" + gParentfields + "','"
                + gName + (gChildfields == "" ? gChildfields : "," + gChildfields) + "','" + gText + "','" + rowID + "','" + gridID + "','"
                + gFunctionName + "','','','" + fieldfrom + "','','" + gCodeFields + "','" + gParentCodeFields + "');}");
            hiddenObj.removeAttr("type");
            hiddenObj.attr("class", "easyui-" + type);
            break;
        case "customexpression":
            //自定义公式---------------------------------------------------------no
            var physicalTableName = hiddenObj.attr('physicalTableName');
            var gName = hiddenObj.attr('fieldcode');
            var expID = gridID + "_" + fieldOption.field + "_EXP_" + rowID;
            hiddenObj.attr("data-options", "onClickButton:function(){OpenCustomExpressionConfig('" + physicalTableName + "','" + gName + "_EXP','" + rowID + "','" + gridID + "');}");
            hiddenObj.removeAttr("type");
            hiddenObj.attr("class", "easyui-" + type);
            $("#cell_" + id).append('<input type="hidden" disedit="false" id="' + expID + '"' + ' name="' + expID + '"' + ' fieldcode="' + gName + '_EXP" rowID="' + rowID + '" datagridID="' + gridID + '" refEXPFieldCode="' + gName + '">');
            break;
        default:
            hiddenObj.removeAttr("type");
            hiddenObj.attr("class", "easyui-" + type);
            break;
    }

    hiddenObj.attr("id", "value_" + id);
    //调整控件宽度适应列宽
    hiddenObj.attr("style", "width:" + (fieldOption.width - 10) + "px");
    return hiddenObj;
}

/* 自定义公式配置
* physicalTableName 公式字段表名
* expFieldCode 自定义公式字段代码
* rowID 行ID
* gridID 明细表ID
*/
function OpenCustomExpressionConfig(physicalTableName, expFieldCode, rowID, gridID) {
    $("body").append('<div id="winExp" title="公式配置"></div>');
    //$("body").append('<div id="winExp_btnList" style="text-align: center; margin: 10px;"><a href="#" class="easyui-linkbutton" data-options="iconCls:\'icon-save\'" onclick="SaveExp()">确定</a>&nbsp;&nbsp;<a href="#" class="easyui-linkbutton" data-options="iconCls:\'icon-cancel\'" onclick="CancelExp()">取消</a></div>');
    $('#winExp').dialog({
        width: 550,
        height: 240,
        modal: true,
        collapsible: false,
        minimizable: false,
        buttons: [{
            text: '保存',
            iconCls: 'icon-save',
            handler: function () { SaveExp(); }
        }, {
            text: '关闭',
            iconCls: 'icon-cancel',
            handler: function () { CancelExp(); }
        }],
        queryParams: { physicalTableName: physicalTableName, expFieldCode: expFieldCode, rowID: rowID, gridID: gridID },
        href: '/WebPage/FormManager/ExpressionDialog.aspx',
        onClose: function () {
            $("#winExp").window('destroy');
        }
    });
}


/* 初始化并加载combobox控件
* controlObj 控件对象
* comboData 下拉框数据源
* spanObj 显示标签对象
* row 行数据对象
*/
function InitCombobox(controlObj, comboData, spanObj, row) {
    controlObj.combobox('loadData', comboData);
    controlObj.combobox({
        onSelect: function (record) {
            var obj = $(this);
            var value = obj.combobox('getText');
            var fieldName = obj.attr("fieldcode");
            var textID = obj.attr("id").replace('value', 'text');
            $("#" + textID).prop('innerHTML', value);
            row[fieldName] = value;
            CalcExpression(obj, row);
            UpdateFooterCell(obj);
        }
    });
    //赋值控件
    controlObj.combobox('setValue', spanObj.prop('innerHTML'));
}

/* 初始化并加载combotree控件
* controlObj 控件对象
* comboData 下拉框数据源
* spanObj 显示标签对象
* row 行数据对象
*/
function InitCombotree(controlObj, comboData, spanObj, row) {
    controlObj.combotree('loadData', comboData);
    var tree = controlObj.combotree('tree');
    tree.attr("fieldcode", controlObj.attr("fieldcode"));
    tree.attr("textID", controlObj.attr("id").replace('value', 'text'))
    controlObj.combotree({
        onSelect: function (node) {
            var obj = $(this);
            var fieldName = obj.attr("fieldcode");
            var textName = obj.attr("textfield");
            var textID = obj.attr("textID");
            $("#" + textID).prop('innerHTML', node.text);
            row[fieldName] = node.id;
            row[textName] = node.text;
            CalcExpression(obj, row);//计算公式列
            UpdateFooterCell(obj);//更新统计列单元格
        }
    });
    //赋值控件
    controlObj.combotree('setText', spanObj.prop('innerHTML'));
}

/* 初始化并加载Checkbox控件
* controlObj 控件对象
* spanObj 显示标签对象
* row 行数据对象
*/
function InitCheckbox(controlObj, spanObj, row) {
    controlObj.on("click", function () {
        var obj = $(this);
        var fieldName = obj.attr("fieldcode");
        var textID = this.id.replace('value', 'text');
        $("#" + textID).prop('innerHTML', obj.prop("checked") ? "是" : "否");
        row[fieldName] = obj.prop("checked") ? 1 : 0;//复选框处理(0否，1是)
        CalcExpression(obj, row);//计算公式列
        UpdateFooterCell(obj);//更新统计列单元格
    });
    //赋值控件
    controlObj.prop("checked", spanObj.prop('innerHTML') == "是");
}

/* 初始化并加载TextDialog控件
* controlObj 控件对象
* spanObj 显示标签对象
* row 行数据对象
*/
function InitTextDialog(controlObj, spanObj, row) {
    controlObj.textbox({
        onChange: function (newValue, oldValue) {
            var obj = $(this);
            var fieldName = obj.attr("fieldcode");
            var textName = obj.attr("textfield");
            var value = obj.attr("textValue");
            var textID = this.id.replace('value', 'text');
            $("#" + textID).prop('innerHTML', newValue);//text
            row[fieldName] = value;
            row[textName] = newValue;
            CalcExpression(obj, row);//计算公式列
            UpdateFooterCell(obj);//更新统计列单元格
        }
    });
    //赋值控件
    controlObj.attr("textValue", row[controlObj.attr("fieldcode")]);
    controlObj.textbox('setValue', spanObj.prop('innerHTML'));
}

/* 初始化并加载基础控件
* controlObj 控件对象
* controlType 控件类型
* spanObj 显示标签对象
* row 行数据对象
*/
function InitBasicControl(controlObj, controlType, spanObj, row) {
    controlObj[controlType]({
        onChange: function (newValue, oldValue) {
            var obj = $(this);
            var fieldName = obj.attr("fieldcode");
            var textID = this.id.replace('value', 'text');
            if (obj.attr('controlType') == "numberbox") {
                $("#" + textID).prop('innerHTML', toThousands(newValue));
            }
            else {
                $("#" + textID).prop('innerHTML', newValue);
            }
            row[fieldName] = newValue;
            CalcExpression(obj, row);//计算公式列
            UpdateFooterCell(obj);//更新统计列单元格
        }
    });

    //赋值控件
    if (controlType == 'numberbox') {
        controlObj[controlType]('setValue',spanObj.length<=0? '': spanObj.prop('innerHTML').replace(',', ''));
    }
    else {
        controlObj[controlType]('setValue', spanObj.prop('innerHTML'));
    }
}

/* 初始化并加载EasyUI控件
* controlObj 控件对象
* leipiType 雷劈控件类型
* controlType 控件类型
* comboData 下拉框数据源
* spanObj 显示标签对象
* row 行数据对象
*/
function InitEasyUIControl(controlObj, leipiType, controlType, comboData, spanObj, row) {
    switch (leipiType) {
        case "combocustom": InitCombobox(controlObj, comboData, spanObj, row); break;
        case "combotree": InitCombotree(controlObj, comboData, spanObj, row); break;
        case "checkbox": InitCheckbox(controlObj, spanObj, row); break;
        case "textdialog": InitTextDialog(controlObj, spanObj, row); break;
        default: InitBasicControl(controlObj, controlType, spanObj, row); break;
    }
}

/* 计算公式列的值
* hiddenObj 当前onchange触发控件对象
* row 行数据对象
*/
function CalcExpression(hiddenObj, row) {
    //是否有公式隐藏域ID，有则代表该列为公式参与列
    if (hiddenObj.attr("ExpHiddenFieldCodeArray")) {
        var expHiddenArrayIDs = hiddenObj.attr("ExpHiddenFieldCodeArray").split(",");//取出该列涉及的公式隐藏域配置对象（为了拿到字段和公式表达式）
        var gridID = hiddenObj.attr("datagridID");
        var rowID = row.id;
        var expHiddenObj;
        var expFieldCode;
        var expJsonArrayStr;
        var expJsonArray
        var refEXPFieldCode;
        var value;

        for (var i = 0; i < expHiddenArrayIDs.length; i++) {
            expHiddenObj = $("#" + gridID + " thead [fieldcode=" + expHiddenArrayIDs[i] + "]");
            //没有获取到thead中公式隐藏域，说明是自定义公式，获取当前row公式隐藏域
            if (expHiddenObj.length == 0) {
                expHiddenObj = $("#" + gridID + "_" + expHiddenArrayIDs[i] + "_" + row.id);
            }
            //自定义公式隐藏域，公式列值存放字段
            refEXPFieldCode = expHiddenObj.attr("refEXPFieldCode");
            expFieldCode = expHiddenObj.attr("fieldcode");
            expJsonArrayStr = expHiddenObj.attr("expjsonarray");
            var result = GetExpCalcResult(gridID, expJsonArrayStr, row);
            if (refEXPFieldCode) {
                $("#text_" + gridID + "_" + refEXPFieldCode + "_" + rowID).prop("innerHTML", result);//自定义公式列显示值
                $("#value_" + gridID + "_" + refEXPFieldCode + "_" + rowID).numberbox('setValue', result);//自定义公式列值
            }
            else {
                $("#text_" + gridID + "_" + expFieldCode + "_" + rowID).prop("innerHTML", toThousands(result));//公式列显示值
            }
            //公式计算后结果如果是另一公式的涉及字段，则计算另一公式结果
            CalcExpression(expHiddenObj, row);

            row[expFieldCode] = "" + result;//写入公式列数据源

            UpdateFooterCell(expHiddenObj);//更新公式列统计
        }
    }
}

/* 获取公式列计算后的值
* expJsonArrayStr 公式json数组字符串
* row 行数据对象
*/
function GetExpCalcResult(gridID, expJsonArrayStr, row) {
    var expStr = '';
    var value;
    var expJsonArray = JSON.parse(expJsonArrayStr);

    //将公式字段替换为实际值，构造四则运算表达式
    for (var j = 0; j < expJsonArray.length; j++) {
        var expJson = expJsonArray[j];
        var kidExpHidden = $("#" + gridID + " thead [fieldcode=" + expJson.field + "]");
        if (kidExpHidden.attr('leipitype') == 'expression') {//公式涉及字段包含子公式，则递归计算结果
            value = GetExpCalcResult(gridID, kidExpHidden.attr('expjsonarray'), row);
        }
        else {
            value = row[expJson.field] == "" ? 0 : row[expJson.field];
        }
        expStr += expJson.leftSign + value + expJson.rightSign + expJson.operation
    }
    var result = CalcOperation(expStr);//计算公式表达式的值
    return result.toFixed(2);
}
/*
* 计算四则运算字符串的值
* 四则运算字符串
*/
function CalcOperation(operationStr) {
    try {
        return eval(operationStr);
    } catch (e) {
        return "";
    }
}

/* 表单设计明细表通用追加行
* gridID 控件对象
*/
function GridAppend(gridID) {
    var gridObj = $('#' + gridID);
    var rows = gridObj.datagrid('getRows');
    var newID = rows.length > 0 ? rows[rows.length - 1].id + 1 : 1;
    var fieldList = gridObj.datagrid('getColumnFields');
    var hiddenObj;
    var row = new Object();
    row.id = newID;
    var value;
    for (var i = 0; i < fieldList.length; i++) {
        if (fieldList[i] != "id") {
            hiddenObj = gridObj.find("thead [fieldcode=" + fieldList[i] + "]");
            //定义了默认值则使用默认值，反之则为空
            if (hiddenObj.attr("leipitype") == "checkbox") {
                value = hiddenObj.prop("checked");
                value = value ? "1" : "0";
            }
            else if (hiddenObj.attr("expjsonarray")) {
                continue;
            }
            else {
                value = hiddenObj.attr("value");
                value = value == undefined ? "" : value;
            }
            row[fieldList[i]] = value;
            CalcExpression(hiddenObj, row);//计算公式列
        }
    }
    gridObj.datagrid('appendRow', row);
    LoadOrUpdateFooterRow(gridObj);//更新统计行(考虑到有默认值的情况)
}

/* 表单设计明细表通用批量追加行
* gridID 控件对象
*/
function GridBatAppend(gridID) {

    var gridObj = $('#' + gridID);
    var rows = gridObj.datagrid('getRows');
    var primaryFieldList = gridObj.attr('primaryFieldList');
    var sourceTable = gridObj.attr('sourcetable');
    var gName = gridObj.attr('fieldcode');
    var gorgparentfields = gridObj.attr('orgparentfields');
    var gChildfields = gridObj.attr('orgchildfields');
    GridAddMultiRow(sourceTable, gName + (gChildfields == "" ? gChildfields : "," + gChildfields), null, gridID, primaryFieldList, rows, null, gorgparentfields);
}

/* 表单设计明细表通用追加行
* gridID 控件对象
*/
function GridStartBatAppendRow(gridID, selectedRows) {

    var gridObj = $('#' + gridID);
    var rows = gridObj.datagrid('getRows');
    var fieldList = gridObj.datagrid('getColumnFields');
    var hiddenObj;
    var value;
    var fieldcode;
    var selectValue;
    for (var j = 0; j < selectedRows.length; j++) {
        var newID = rows.length > 0 ? rows[rows.length - 1].id + 1 : 1;
        var row = new Object();
        for (var i = 0; i < fieldList.length; i++) {
            fieldcode = fieldList[i];
            selectValue = selectedRows[j][fieldcode];
            if (selectValue) {
                value = selectValue;
            }
            else {
                hiddenObj = gridObj.find("thead [fieldcode=" + fieldcode + "]");
                //定义了默认值则使用默认值，反之则为空
                if (hiddenObj.attr("leipitype") == "checkbox") {
                    value = hiddenObj.prop("checked");
                    value = value ? "1" : "0";
                }
                else {
                    value = hiddenObj.attr("value");
                    value = value == "" ? "" : value;
                }
            }
            row[fieldcode] = value;
        }
        row.id = newID;
        gridObj.datagrid('appendRow', row);
    }
    LoadOrUpdateFooterRow(gridObj);//更新统计行(考虑到有默认值的情况)
}

/* 表单设计明细表通用删除行
* gridID 控件对象
*/
function GridRemoveit(gridID) {
    var gridObj = $('#' + gridID);
    var lastRowID = gridObj.datagrid("options").LastRowID;
    var rowID;

    var checkedObjs = $("[type=checkbox][gridID=" + gridID + "]:checked");
    for (var i = 0; i < checkedObjs.length; i++) {
        rowID = $(checkedObjs[i]).attr("checkBoxRowID");
        var rowIndex = gridObj.datagrid('getRowIndex', rowID);
        gridObj.datagrid('deleteRow', rowIndex);
        if (lastRowID == rowID) {//如果当前选中行就是上一行，则清除缓存属性
            gridObj.datagrid("options").LastRowID = null;
            gridObj.datagrid("options").LastRowIndex = null;
        }
    }
    LoadOrUpdateFooterRow(gridObj);//更新统计行
}

/* 表单设计明细表通用提交行
* gridID 控件对象
*/
function GridAccept(gridID) {
    //var gridObj = $('#' + gridID);
    //var fieldList = gridObj.datagrid('getColumnFields');
    //var lastRowID = gridObj.datagrid("options").LastRowID;
    //var isPass = ValidateLastRow(fieldList, lastRowID);//验证通过后接受提交
    //if (isPass) {
    //    EndLastRowCellEdit(fieldList, lastRowID);
    //    gridObj.datagrid("options").LastRowID = null;
    //    gridObj.datagrid("options").LastRowIndex = null;
    //}
}

/* 表单设计明细表通用行规则验证
* gridID 控件对象
*/
function GridValidRows(gridID) {
    var gridObj = $('#' + gridID);
    var rows = gridObj.datagrid('getRows');
    var row;
    var jsonValid;
    var jsonRule;
    var fieldValue;
    var compareValue;
    var validStr = "";
    var rowIndex;
    var isPass = true;
    var validJsonArray = gridObj.attr("validjsonarray");
    if (validJsonArray && validJsonArray != "") {
        validJsonArray = JSON.parse(validJsonArray);
        for (var i = 0; i < rows.length; i++) {
            row = rows[i];
            for (var j = 0; j < validJsonArray.length; j++) {
                validStr = ""
                jsonValid = validJsonArray[j];
                for (var k = 0; k < jsonValid.length; k++) {
                    jsonRule = jsonValid[k];

                    fieldValue = row[jsonRule.field];
                    compareValue = jsonRule.compareField == "" ? jsonRule.compareValue : row[jsonRule.compareField];
                    validStr += jsonRule.leftSign + fieldValue + jsonRule.operation + compareValue + jsonRule.rightSign + jsonRule.connectSign;
                }

                var result = eval(validStr);
                if (!result) {
                    isPass = false;
                    rowIndex = gridObj.datagrid('getRowIndex', row.id);
                    $.messager.alert("", "第" + (rowIndex + 1) + "行," + jsonValid[0].msg);
                    return isPass;
                }
            }
        }
    }
    return isPass;
}

/* 表单设计明细表通用获取统计列合计值函数
* gridID 控件对象
* fieldName 统计字段
*/
function GridGetSumFieldValue(gridID, fieldName) {
    var gridObj = $('#' + gridID);
    var footerRows = gridObj.datagrid('getFooterRows');
    if (footerRows[fieldName]) {
        return footerRows[fieldName];
    }
    else {
        return -1;
    }
}

//将附件列表FileIDList写入隐藏域
function SetAttachmentFileIDListToHidden(fileIDList, fieldcode) {
    $("#" + fieldcode).attr("value", fileIDList);
}

//上传
function CommonUpload(maximized) {
    $("#uploadDlg").dialog({
        href: "/WebPage/FileCenter/FileUpLoad.aspx?settoken=0&fieldName=uploadDlg&attachmentName=attachmentList",
        title: "上传附件",
        width: 640,
        height: 460
    }).dialog('open');

    if (maximized == true) {
        $("#uploadDlg").dialog({
            maximized: true
        })
    }
}

function Download() {

}

//下载
function CommonDownload(fileID) {
    var rows = $("#attachmentList").datagrid('getChecked');//获取选中的所有行
    if (rows.length > 0) {
        $.each(rows, function (i, item) {
            window.open('/api/FileCenter/FileDownload?FileID=' + item.FileID);
        });
    } else {
        $.messager.alert("提示", "未选择下载附件", "info");
    }
}

//删除文件
function CommonDeleteFile() {
    var rows = $("#attachmentList").datagrid('getChecked');//获取选中的所有行
    if (rows.length > 0) {
        $.each(rows, function (i, item) {
            //删除存档文件
            $.ajax({
                url: '/api/FileCenter/FileDelete',
                type: 'post',
                data: { FileID: item.FileID },
                success: function (result) {
                    var index = $("#attachmentList").datagrid('getRowIndex', item);
                    $("#attachmentList").datagrid('deleteRow', index);
                    result = eval('(' + result + ')');
                    if (!result.success) {
                        $.messager.alert('提示', '删除附件文件 ' + item.FileName + ' 失败；错误提示：' + result.message + '', 'info');
                    }
                }
            });
        });
    } else {
        $.messager.alert("提示", "未选择下载附件", "info");
    }
}

//刷新附件列表状态
function CommonUpdateUploadState() {
    var gridObj = $("#attachmentList");
    var rows = gridObj.datagrid('getRows');
    var fieldcode = gridObj.attr("fieldcode");
    var fileIDList = "";
    var splitSign = "";
    for (var i = 0; i < rows.length; i++) {
        fileIDList += splitSign + rows[i].FileID;
        splitSign = ",";
    }
    //if (fieldcode != "" && gridObj.datagrid("options").url == null) {
    //    gridObj.datagrid({ url: '/api/common/gridlist/GetAttachmentListByFileIDList', queryParams: { FileIDList: fileIDList } })
    //} else {
    //    gridObj.datagrid('reload', { FileID: fileIDList });
    //}
    //gridObj.datagrid('reload', { FileIDList: fileIDList });
    SetAttachmentFileIDListToHidden(fileIDList, fieldcode);
    //弹窗刷新，避免和明细弹窗冲突
    $(this).dialog('clear');
}

//返回适合读的文件大小
function HumanReadableFilesize(size) {
    var units = new Array("B", "KB", "MB", "GB", "TB", "PB");
    var mod = 1024;
    var i = 0;
    var fileSize = "0 KB";
    try {
        while (size >= mod) {
            size /= mod;
            i++;
        }
        fileSize = Math.round(size * 10) / 10 + units[i];
    } catch (e) {

        fileSize = "0 KB";
    }
    return fileSize;
}

//附件列表下载链接格式化器
function CommonAttachmentDownloadFormatter(row) {
    if (row.StorageOver == 0) {
        //  return "<a href=\"javascript:void(0);\" onclick=\"CommonDownload('" + row.FileID + "');\">下载</a>";
        return "<a href=\"javascript:void(0);\" onclick=\"window.open('/api/FileCenter/FileDownload?FileID=" + row.FileID + "');\">下载</a>";
    }
    else {
        return "";
    }
}

//--------------------------------------------------------明细datagrid附件上传通用js------------------------------------------开始
/* 表单设计明细表通用格式化*/
function formatOper(val, row, index) {
    if (row.id != "合计") {
        if (val == "" || val == null) {
            return '<a href="#" onclick="detailAttachment(&quot;' + val + '&quot;,' + JSON.stringify(row).replace(/"/g, '&quot;') + ',' + index + ');">无附件</a>';
        }
        else {
            return '<a href="#" onclick="detailAttachment(&quot;' + val + '&quot;,' + JSON.stringify(row).replace(/"/g, '&quot;') + ',' + index + ');">附件</a>';
        }
    }
    else {
        return '';
    }
}

//明细附件点击事件
function detailAttachment(val, row, index) {
    var gridObj = $("#detailAttachmentList");
    var fieldcode = gridObj.attr("fieldcode");
    //是否手动修改后的数据
    var manualchanges = gridObj.attr('manualchanges');
    var detailtableId = gridObj.attr("fieldtable");

    $("#" + detailtableId).datagrid("clearSelections");

    if (manualchanges === 'true') {
        var _grid = $('#' + detailtableId);//得到明细对象
        //验证是否填写完毕
        var isValid = _grid.datagrid('validateRow', index);
        if (!isValid) {
            $.messager.alert("提示", "必填字段是否填写完毕！", "info");
            return;
        }
    }
    $('#detailIndex').val(index);
    // 打开面板且刷新其内容。
    $('#detailAttachmentListDiv').panel('open').panel('refresh');
    var detailAttachmentList = $('#detailAttachmentList');
    var view = "";
    //正常页面操作
    if ($("#dlgEdit").length > 0) {
        var viewList = $("#dlgEdit").panel('options').href.split('/'),
            view = viewList[viewList.length - 2];
    }
        //流程引用页面操作
    else if ($("#Flowtabs").length > 0) {
        var folwViewList = $("#Flowtabs").tabs('getTab', 0).panel('options').href.split('/'),
            view = folwViewList[folwViewList.length - 2];
    }
    if (detailAttachmentList.length > 0) {
        var attachmentFieldCode = detailAttachmentList.attr('fieldcode');
        var attachmentFileIDList = row[attachmentFieldCode];
        $.post('/api/common/gridlist/GetAttachmentListByFileIDList', { FileIDList: attachmentFileIDList }, function (result) {
            if (result.length > 0) {
                detailAttachmentList.datagrid('loadData', result);
            }
            else {
                detailAttachmentList.datagrid('loadData', []);
            }
        }, 'json');
    }
    if (view == 'view') {
        detailAttachmentList.datagrid({ toolbar: '' });
    }
    else {
        detailAttachmentList.datagrid({
            toolbar: [
                { text: '上传', iconCls: 'pmicon-moveup-16', handler: detailCommonUpload }, '-',
                { text: '批量下载', iconCls: 'pmicon-movedown-16', handler: detailCommonDownload }, '-',
                { text: '删除', iconCls: 'pmicon-delete-GrayScaled-16', handler: detailCommonDeleteFile }
            ]
        });
    }
}

//明细附件上传
function detailCommonUpload(maximized) {
    $("#detailuploadDlg").dialog({
        href: "/WebPage/FileCenter/FileUpLoad.aspx?settoken=0&fieldName=detailuploadDlg&attachmentName=detailAttachmentList",
        title: "上传明细附件",
        width: 640,
        height: 460
    }).dialog('open');

    if (maximized == true) {
        $("#detailuploadDlg").dialog({
            maximized: true
        })
    }
}

//明细下载
function detailCommonDownload() {
    var rows = $("#detailAttachmentList").datagrid('getChecked');//获取选中的所有行
    if (rows.length > 0) {
        $.each(rows, function (i, item) {
            window.open('/api/FileCenter/FileDownload?FileID=' + item.FileID);
        });
    } else {
        $.messager.alert("提示", "未选择下载附件", "info");
    }
}

//明细删除文件
function detailCommonDeleteFile() {
    var rows = $("#detailAttachmentList").datagrid('getChecked');//获取选中的所有行
    if (rows.length > 0) {
        $.each(rows, function (i, item) {
            //删除存档文件
            $.ajax({
                url: '/api/FileCenter/FileDelete',
                type: 'post',
                data: { FileID: item.FileID },
                async: false,//改为同步
                success: function (result) {
                    result = eval('(' + result + ')');
                    if (!result.success) {
                        $.messager.alert('提示', '删除附件文件 ' + item.FileName + ' 失败；错误提示：' + result.message + '', 'info');
                    }
                    else {
                        $("#detailAttachmentList").datagrid('deleteRow', $("#detailAttachmentList").datagrid('getRowIndex', item));
                    }
                }
            });
        });
        //刷新状态
        detailCommonUpdateUploadState(rows);
    } else {
        $.messager.alert("提示", "未选择下载附件", "info");
    }
}

//刷新明细附件状态
function detailCommonUpdateUploadState(deleteRows) {
    var gridObj = $("#detailAttachmentList");
    var rows = gridObj.datagrid('getRows');
    var fieldcode = gridObj.attr("fieldcode");
    //是否手动修改后的数据
    var manualchanges = gridObj.attr('manualchanges');
    var detailtableId = gridObj.attr("fieldtable");
    var fileIDList = "";
    var splitSign = "";
    for (var i = 0; i < rows.length; i++) {
        if (deleteRows != undefined) {
            var isExists = false;
            for (var j = 0; j < deleteRows.length; j++) {
                if (rows[i].FileID === deleteRows[j].FileID) {
                    isExists = true;
                    break;
                }
            }
            if (!isExists) {
                fileIDList += splitSign + rows[i].FileID;
                splitSign = ",";
            }
        }
        else {
            fileIDList += splitSign + rows[i].FileID;
            splitSign = ",";
        }
    }
    var dataIndex = parseInt($('#detailIndex').val());
    //明细附件赋值
    var _grid = $("#" + detailtableId);
    if (!manualchanges || manualchanges == 'false') {
        ///自动生成

        //先选中行
        var _gridSelected = _grid.datagrid('selectRow', dataIndex);
        //获得选中行数据
        var row = _grid.datagrid('getSelected');
        if (_gridSelected && row) {
            row[fieldcode] = fileIDList;
            _grid.datagrid('updateRow', { index: dataIndex, row: row });
            //开启行编辑---自动生成明细
            _grid.datagrid("options").LastRowID = null;
            _grid.datagrid("options").LastRowIndex = null;
        }
    }
    else {
        ///手动修改后的

        //验证是否填写完毕
        var isValid = _grid.datagrid('validateRow', dataIndex);
        if (!isValid) {
            $.messager.alert("提示", "必填字段是否填写完毕！", "info");
            return;
        }
        //先结束行编辑，才能拿到原来行数据
        _grid.datagrid('endEdit', dataIndex);
        //获得原行数据
        var row = _grid.datagrid('getSelected');
        if (row) {
            row[fieldcode] = fileIDList;
            //更新附件获得新数据
            _grid.datagrid('updateRow', { index: dataIndex, row: row });
            //开启行编辑---手动修改后的明细
            if (typeof (editIndex) !== "undefined") {
                editIndex = undefined;
            }
        }
    }

    //弹窗刷新，避免和主表弹窗冲突
    $("#detailuploadDlg").dialog('clear');
}
//--------------------------------------------------------明细datagrid附件上传通用js------------------------------------------结束



//--------------------------------------------------------表单设计器datagrid通用js------------------------------------------结束

//隐藏grid列方法
//隐藏grid列
var cmenu;
function createColumnMenu(gridId) {
    cmenu = $('<div/>').appendTo('body');
    cmenu.menu({
        onClick: function (item) {
            if (item.iconCls == 'icon-ok') {
                $('#' + gridId).datagrid('hideColumn', item.name);
                cmenu.menu('setIcon', {
                    target: item.target,
                    iconCls: 'icon-empty'
                });
            } else {
                $('#' + gridId).datagrid('showColumn', item.name);
                cmenu.menu('setIcon', {
                    target: item.target,
                    iconCls: 'icon-ok'
                });
            }
        }
    });
    var fields = $('#' + gridId).datagrid('getColumnFields');
    for (var i = 0; i < fields.length; i++) {
        var field = fields[i];
        var col = $('#' + gridId).datagrid('getColumnOption', field);
        if (field != "" && field != "id") {
            if (col.hidden) {
                cmenu.menu('appendItem', {
                    text: col.title,
                    name: field,
                    iconCls: 'icon-empty'
                });
            } else {
                cmenu.menu('appendItem', {
                    text: col.title,
                    name: field,
                    iconCls: 'icon-ok'
                });
            }
        }
    }
}

//金额数字转大写
function ConvertAmount(numberObj, capitalInput) {
    var isMinus = false;
    var num = $(numberObj).numberbox('getValue');
    if (num.indexOf("-") !=-1) {
        num = num.replace("-", "");
        isMinus = true;
    }
    var numberValue = new String(Math.round(num * 100)); // 数字金额

    var chineseValue = ""; // 转换后的汉字金额
    var String1 = "零壹贰叁肆伍陆柒捌玖"; // 汉字数字
    var String2 = "万仟佰拾亿仟佰拾万仟佰拾元角分"; // 对应单位
    var len = numberValue.length; // numberValue 的字符串长度
    var Ch1; // 数字的汉语读法
    var Ch2; // 数字位的汉字读法
    var nZero = 0; // 用来计算连续的零值的个数
    var String3; // 指定位置的数值
    if (len > 15) {
        alert("超出计算范围");
        return "";
    }
    if (numberValue == 0) {
        chineseValue = "零元整";
        return chineseValue;
    }
    String2 = String2.substr(String2.length - len, len); // 取出对应位数的STRING2的值
    for (var i = 0; i < len; i++) {
        String3 = parseInt(numberValue.substr(i, 1), 10); // 取出需转换的某一位的值
        if (i != (len - 3) && i != (len - 7) && i != (len - 11) && i != (len - 15)) {
            if (String3 == 0) {
                Ch1 = "";
                Ch2 = "";
                nZero = nZero + 1;
            }
            else if (String3 != 0 && nZero != 0) {
                Ch1 = "零" + String1.substr(String3, 1);
                Ch2 = String2.substr(i, 1);
                nZero = 0;
            }
            else {
                Ch1 = String1.substr(String3, 1);
                Ch2 = String2.substr(i, 1);
                nZero = 0;
            }
        }
        else { // 该位是万亿，亿，万，元位等关键位
            if (String3 != 0 && nZero != 0) {
                Ch1 = "零" + String1.substr(String3, 1);
                Ch2 = String2.substr(i, 1);
                nZero = 0;
            }
            else if (String3 != 0 && nZero == 0) {
                Ch1 = String1.substr(String3, 1);
                Ch2 = String2.substr(i, 1);
                nZero = 0;
            }
            else if (String3 == 0 && nZero >= 3) {
                Ch1 = "";
                Ch2 = "";
                nZero = nZero + 1;
            }
            else {
                Ch1 = "";
                Ch2 = String2.substr(i, 1);
                nZero = nZero + 1;
            }
            if (i == (len - 11) || i == (len - 3)) { // 如果该位是亿位或元位，则必须写上
                Ch2 = String2.substr(i, 1);
            }
        }
        chineseValue = chineseValue + Ch1 + Ch2;
    }
    if (String3 == 0) { // 最后一位（分）为0时，加上“整”
        chineseValue = chineseValue + "整";
    }
    if (isMinus) {
        chineseValue = "负" + chineseValue;
    }

    $('#' + capitalInput).textbox('setValue', chineseValue);
    //  return chineseValue;
}
//直接从管理界面发起审批流程
function SendProcessFlow(FormCode, id, Grid_ID) {
    $.post('/api/common/gridlist/GetPerformFlowList', { FormCode: FormCode, id: id }, function (result) {//获取表单发起流程列表
        $("#PerformFlowWind").dialog({
            width: 600,
            height: 460,
            modal: true,
            maximizable: false
        });
        if (result.length > 0) {//存在发起流程
            $("#PerformFlowWind").dialog('open').dialog('setTitle', '查看已发出的审批流程');
            $("#PerformFlowWind").dialog('refresh', '/WebPage/OA/PerformFlowList.aspx?FormCode=' + FormCode + '&ID=' + id + '&Grid_ID=' + Grid_ID);
        } else {//不存在发起流程
            //初次发起
            $("#PerformFlowWind").dialog('open').dialog('setTitle', '选择要发起的审批流程');
            // $("#PerformFlowWind").dialog('refresh', '/WebPage/OA/ChoosePerformFlow.aspx?DialogID=PerformFlowWind&FormCode=' + FormCode + '&ID=' + id);
            $("#PerformFlowWind").dialog('refresh', '/WebPage/OA/ChoosePerformFlow.aspx?FormCode=' + FormCode + '&ID=' + id + '&Grid_ID=' + Grid_ID);
        }

    }, 'json');
}

//--------修改
//直接从管理界面发起审批流程
function SendProcessFlow1(FormCode, id, Grid_ID, RegProjcetCode) {
    $.post('/api/common/gridlist/GetPerformFlowList1', { FormCode: FormCode, RegProjcetCode: RegProjcetCode }, function (result) {//获取表单发起流程列表
        $("#PerformFlowWind").dialog({
            width: 600,
            height: 460,
            modal: true,
            maximizable: false
        });
        if (result.length > 0) {//存在发起流程
            $("#PerformFlowWind").dialog('open').dialog('setTitle', '查看已发出的审批流程');
            $("#PerformFlowWind").dialog('refresh', '/WebPage/OA/PerformFlowList.aspx?FormCode=' + FormCode + '&ID=' + id + '&Grid_ID=' + Grid_ID);
        } else {//不存在发起流程
            //初次发起
            $("#PerformFlowWind").dialog('open').dialog('setTitle', '选择要发起的审批流程');
            // $("#PerformFlowWind").dialog('refresh', '/WebPage/OA/ChoosePerformFlow.aspx?DialogID=PerformFlowWind&FormCode=' + FormCode + '&ID=' + id);
            $("#PerformFlowWind").dialog('refresh', '/WebPage/OA/ChoosePerformFlow.aspx?FormCode=' + FormCode + '&ID=' + id + '&Grid_ID=' + Grid_ID);
        }

    }, 'json');
}
//--------修改

//通过弹出的编辑窗口发起审批流程
function ApprovalProcessFlow(FormCode, id) {
    var btn = $("#btnPerformFlow");
    btn.show();
    btn.linkbutton('enable');
    btn.unbind().bind('click', function () {
        $("#dlgEdit").dialog('close');

        $.post('/api/common/gridlist/GetPerformFlowList', { FormCode: FormCode, id: id }, function (result) {//获取表单发起流程列表
            $("#PerformFlowWind").dialog({
                width: 600,
                height: 460,
                modal: true,
                maximizable: false
            });
            if (result.length > 0) {//存在发起流程
                $("#PerformFlowWind").dialog('open').dialog('setTitle', '查看已发出的审批流程');
                $("#PerformFlowWind").dialog('refresh', '/WebPage/OA/PerformFlowList.aspx?FormCode=' + FormCode + '&ID=' + id);

            } else {//不存在发起流程
                //初次发起
                $("#PerformFlowWind").dialog('open').dialog('setTitle', '选择要发起的审批流程');
                // $("#PerformFlowWind").dialog('refresh', '/WebPage/OA/ChoosePerformFlow.aspx?DialogID=PerformFlowWind&FormCode=' + FormCode + '&ID=' + id);
                $("#PerformFlowWind").dialog('refresh', '/WebPage/OA/ChoosePerformFlow.aspx?FormCode=' + FormCode + '&ID=' + id);
            }

        }, 'json');
    });
}


//合并单元格 需要传入需要合并的列名
function MergedCell(fieldName) {
    var rows = $("#first_region").datagrid("getRows");
    var jsonstr = "";//存储合并的json
    var index = 0, rowspan = 0;//存储合并的起始位置和合并的单元格个数
    for (var i = 1; i < rows.length; i++) {
        if (rows[i - 1].QSXmc == rows[i].QSXmc) {
            rowspan++;
        } else {
            rowspan++;
            jsonstr += "{index: " + index + ",rowspan: " + rowspan + "},";
            rowspan = 0;
            index = i;
        }
    }
    jsonstr = jsonstr.substring(0, jsonstr.length - 1);
    var json = eval("[" + jsonstr + "]");
    for (var n = 0; n < json.length; n++) {
        $("#first_region").datagrid('mergeCells', {
            index: json[n].index,
            field: fieldName,
            rowspan: json[n].rowspan
        });
    }
}

//金额格式化
function AmountFormat(value, row, index) {
    f = value < 0 ? '-' : '';
    value = parseFloat((Math.abs(value) + '').replace(/[^\d\.-]/g, '')).toFixed(2) + '';
    var l = value.split('.')[0].split('').reverse(), r = value.split('.')[1]; t = '';
    for (i = 0; i < l.length; i++) {
        t += l[i] + ((i + 1) % 3 == 0 && (i + 1) != l.length ? ',' : '');
    }
    return f + t.split('').reverse().join('') + '.' + r.substring(0, 2);
}
//百分比格式化
function PercentFormat(value) {
    return (parseFloat(value) * 100).toString().substring(0, s.indexOf(".") + 3) + "%";
}

/*                                                           datagrid转化为树                                                                            */
/*获取datagrid转化为树的json字符串  
//[{ "C1": [{ "pcode": "", "code": "0_0,1_0,2_0" }, { "pcode": "", "code": "0_1,0_2" }], "C2": [{ "pcode": "", "code": "0_0,1_0,2_0" }, { "pcode": "", "code": "0_1,0_2" }] }]
*/
function gridTotree(DatagridObj) {
    var frozenColumns = DatagridObj.datagrid("options").frozenColumns;  // 得到frozenColumns对象
    var columns = DatagridObj.datagrid("options").columns;    // 得到columns对象
    var columnsTreeStr = "";//存储非冻结列的树字符串
    var thisTreeStr = ""//存储当前行的tree字符串
    var arrCode = Array_2(columns.length, DatagridObj.datagrid('getColumnFields').length);//二维数组存储已生成的code
    var parentJson = new Array();
    if (typeof columns != 'undefined' && columns != '') {
        columnsTreeStr = "[{";
        $(columns).each(function (index) {
            thisTreeStr = '"C' + index + '":[';
            //处理冻结列
            if (typeof frozenColumns != 'undefined' && typeof frozenColumns[index] != 'undefined') {

            }
            //遍历非冻结列
            for (var c = 0; c < columns[index].length; ++c) {
                if (columns[index][c].field != "id" && typeof columns[index][c].title != "undefined" && columns[index][c].title != "") {
                    var code = getCode(columns[index][c], index, c, arrCode);//自己的编码
                    var pcode = getPcode(columns[index][c], index, c, code, parentJson);//父级编码
                    thisTreeStr += '{"pcode":"' + pcode + '","code":"' + code + '","text":"' + columns[index][c].title + '","fildid":"' + (columns[index][c].field != undefined ? columns[index][c].field : "") + '","rowspan":"' + (columns[index][c].rowspan != undefined ? columns[index][c].rowspan : "") + '","colspan":"' + (columns[index][c].colspan != undefined ? columns[index][c].colspan : "") + '","align":"' + (columns[index][c].align != undefined ? columns[index][c].align : "") + '","width":"' + (columns[index][c].width != undefined ? columns[index][c].width : "") + '","hidden":"' + (columns[index][c].hidden != undefined ? columns[index][c].hidden : "") + '"},';
                }
            }
            columnsTreeStr += thisTreeStr.substring(0, thisTreeStr.length - 1) + "],";
            parentJson[index] = eval("[{" + thisTreeStr.substring(0, thisTreeStr.length - 1) + "]}]");
        })
        columnsTreeStr = columnsTreeStr.substring(0, columnsTreeStr.length - 1) + "}]";
    }
    return columnsTreeStr;
}
/*获取code   
* @columnsObj 单元格对象
* @rowIndex 第几行
* @columnIndex 第几列
* @arrCode 已存在的code数组
*/
function getCode(columnsObj, rowIndex, columnIndex, arrCode) {
    var Code = JudgeCoordinateExist(rowIndex, columnIndex, arrCode);//获取当前的起始code
    var CodeSplit = Code.split('_');
    //如果同时有合并列和行改变处理方式
    if (typeof columnsObj.rowspan != 'undefined' && typeof columnsObj.rowspan != '' && typeof columnsObj.colspan != 'undefined' && typeof columnsObj.colspan != '') {
        for (var m = 0; m < columnsObj.rowspan; m++) {
            for (var x = 0; x < columnsObj.colspan; x++) {
                if (m == 0) {
                    if (x != 0) {
                        Code += ',' + (parseInt(CodeSplit[0]) + m) + '_' + (parseInt(CodeSplit[1]) + x);
                        arrCode[(parseInt(CodeSplit[0]) + m)][(parseInt(CodeSplit[1]) + x)] = (parseInt(CodeSplit[0]) + m) + '_' + (parseInt(CodeSplit[1]) + x);//生成的编码存到二维数组
                    }
                } else {
                    Code += ',' + (parseInt(CodeSplit[0]) + m) + '_' + (parseInt(CodeSplit[1]) + x);
                    arrCode[(parseInt(CodeSplit[0]) + m)][(parseInt(CodeSplit[1]) + x)] = (parseInt(CodeSplit[0]) + m) + '_' + (parseInt(CodeSplit[1]) + x);//生成的编码存到二维数组
                }
            }
        }
    }
    else {
        //如果有合并 行 则生成对应的code
        if (typeof columnsObj.rowspan != 'undefined' && typeof columnsObj.rowspan != '') {
            for (var i = 1; i < columnsObj.rowspan; i++) {
                Code += ',' + (parseInt(CodeSplit[0]) + i) + '_' + CodeSplit[1];
                arrCode[rowIndex + i][CodeSplit[1]] = (parseInt(CodeSplit[0]) + i) + '_' + CodeSplit[1];//生成的编码存到二维数组
            }
        }
        //如果有合并 列 则生成对应的code
        if (typeof columnsObj.colspan != 'undefined' && typeof columnsObj.colspan != '') {
            for (var n = 1; n < columnsObj.colspan; n++) {
                Code += ',' + CodeSplit[0] + '_' + (parseInt(CodeSplit[1]) + n);
                arrCode[rowIndex][(parseInt(CodeSplit[1]) + n)] = CodeSplit[0] + '_' + (parseInt(CodeSplit[1]) + n);//生成的编码存到二维数组
            }
        }
    }
    return Code;
}
/*获取pcode   
* @columnsObj 单元格对象
* @rowIndex 第几行
* @columnIndex 第几列
* @code 当前的code
* @parentJson 父级Json字符串
*/
function getPcode(columnsObj, rowIndex, columnIndex, code, parentJson) {
    var Pcode = "";
    var Index = 1;
    if (rowIndex > 0) {
        code = parseInt(code.split(',')[0].split('_')[0] - 1) + "_" + code.split(',')[0].split('_')[1];//当前元素的起始位置
        var _parentJson = parentJson[rowIndex - Index];
        //遍历父级
        while (Pcode == "") {
            for (var i = 0; i < _parentJson[0]['C' + (rowIndex - Index)].length; i++) {
                var _Pcode = _parentJson[0]['C' + (rowIndex - Index)][i].code.split(',');
                for (var n = 0; n < _Pcode.length; n++) {
                    if (_Pcode[n] == code) {
                        Pcode = _parentJson[0]['C' + (rowIndex - Index)][i].code;
                        break;
                    }
                }
                if (Pcode != "") {
                    break;
                }
            }
            if (Pcode == "") {
                Index++;
                _parentJson = parentJson[rowIndex - Index];
            }
        }
    }
    return Pcode;
}
/*判断当前的位置是否已存在 并找出当前行的空位
* @rowIndex 第几行
* @columnIndex 第几列
*/
function JudgeCoordinateExist(rowIndex, columnIndex, arrCode) {
    var Code = rowIndex + '_' + columnIndex;
    var flag = true;
    var isExist = false;
    var index = 0;
    //循环遍历空位置
    while (flag) {
        //当前位置不存在元素
        if (arrCode[rowIndex][columnIndex + index] == "") {
            arrCode[rowIndex][columnIndex + index] = Code;//生成的编码存到二维数组
            flag = false;
        }
        else {
            index++;
            Code = rowIndex + '_' + (columnIndex + index);
        }
    }
    return Code;
}
//生成二维数组
function Array_2(nRow, nColumn) {
    var array1 = new Array(); //定义一维数组
    for (i = 0; i < nRow; i++) {
        //将每一个子元素又定义为数组
        array1[i] = new Array();
        //----------------------------------------
        for (n = 0; n < nColumn; n++) {
            array1[i][n] = ''; //此时aa[i][n]可以看作是一个二级数组
        }
        //--------------------------------------
    }
    return array1;
}

/*转化为树json
@jsonObj 传入需要转化的json数据
*/
function ConversionTreeJson(jsonObj, code, index) {
    var treeJson = "";
    if (index < getJsonObjLength(jsonObj[0])) {
        var arr = jsonObj[0]["C" + index];
        if (code == "") {
            for (var n = 0; n < arr.length; n++) {
                treeJson += '{"id":"' + arr[n].code + '","text":"' + arr[n].text + '","state":"open","checked":' + (arr[n].hidden == "true" ? "false" : "true") + ',"attributes":{"fildid":"' + arr[n].fildid + '","pcode":"' + arr[n].pcode + '","align":"' + arr[n].align + '","colspan":"' + arr[n].colspan + '","rowspan":"' + arr[n].rowspan + '","width":"' + arr[n].width + '"},"children":[' + ConversionTreeJson(jsonObj, arr[n].code, index + 1) + ']},';
            }
            treeJson = treeJson.substring(0, treeJson.length - 1);
        } else {
            for (var x = index ; x < getJsonObjLength(jsonObj[0]) ; x++) {
                arr = jsonObj[0]["C" + x];
                for (var m = 0; m < arr.length; m++) {
                    if (arr[m].pcode == code) {
                        treeJson += '{"id":"' + arr[m].code + '","text":"' + arr[m].text + '","state":"open","checked":' + (arr[m].hidden == "true" ? "false" : "true") + ',"attributes":{"fildid":"' + arr[m].fildid + '","pcode":"' + arr[m].pcode + '","align":"' + arr[m].align + '","colspan":"' + arr[m].colspan + '","rowspan":"' + arr[m].rowspan + '","width":"' + arr[m].width + '"},"children":[' + ConversionTreeJson(jsonObj, arr[m].code, index + 1) + ']},';
                    }
                }
            }
            if (treeJson != "") {
                treeJson = treeJson.substring(0, treeJson.length - 1);
            }
        }
    }
    return treeJson;
}

/*                                                           datagrid转化为树                                                                            */

/*                                                           把datagrid的列转成菜单面板                                                                  */
/*
*@gridId datagrid的ID
*@gridJson 生成的datagrid树json
*/
var DgHeadCmenu;
function createDgHeadMenu(gridId, gridJson) {
    DgHeadCmenu = $('<div/>').appendTo('body');
    DgHeadCmenu.menu({
        onClick: function (item) {
            if (item.iconCls == 'icon-ok') {
                alert("成功ok");
                //$('#' + gridId).datagrid('hideColumn', item.name);
                //DgHeadCmenu.menu('setIcon', {
                //    target: item.target,
                //    iconCls: 'icon-empty'
                //});
            } else {
                alert("成功empty");
                //$('#' + gridId).datagrid('showColumn', item.name);
                //DgHeadCmenu.menu('setIcon', {
                //    target: item.target,
                //    iconCls: 'icon-ok'
                //});
            }
        }
    });
    //根据datagrid生成的树json，生成所需要的菜单面板
    for (var i = 0; i < getJsonObjLength(gridJson[0]) ; i++) {
        var arr = gridJson[0]["C" + i];
        for (var n = 0; n < arr.length; n++) {
            if (arr[n].fildid != "id") {
                if (arr[n].pcode != "") {
                    var item = DgHeadCmenu.menu('findItem', getParentName(gridJson, arr[n].pcode, i));
                    DgHeadCmenu.menu('appendItem', {
                        id: arr[n].code,//当前单元格的code    
                        text: arr[n].text,//当前单元格的显示内容
                        fildid: arr[n].fildid,//当前单元格的fildid
                        align: arr[n].align,//当前单元格的对齐方式
                        colspan: arr[n].colspan,//当前单元格的合并列
                        pcode: arr[n].pcode,//当前单元格的父code   
                        rowspan: arr[n].rowspan,//当前单元格的合并行
                        width: arr[n].width,//当前单元格的宽度
                        iconCls: 'icon-ok',
                        parent: item.target
                    });
                } else {
                    DgHeadCmenu.menu('appendItem', {
                        id: arr[n].code,//当前单元格的code    
                        text: arr[n].text,//当前单元格的显示内容
                        fildid: arr[n].fildid,//当前单元格的fildid
                        align: arr[n].align,//当前单元格的对齐方式
                        colspan: arr[n].colspan,//当前单元格的合并列
                        pcode: arr[n].pcode,//当前单元格的父code   
                        rowspan: arr[n].rowspan,//当前单元格的合并行
                        width: arr[n].width,//当前单元格的宽度
                        iconCls: 'icon-ok'
                    });
                }
            }
        }
    }
}
//获取json对象的长度
function getJsonObjLength(jsonObj) {
    var Length = 0;
    for (var item in jsonObj) {
        Length++;
    }
    return Length;
}
/*获取父级的名字
*@gridJson datagrid的json数据
*@code 父级code
*@index 当前行数
*/
function getParentName(gridJson, code, index) {
    var PName = "";
    while (PName == "") {
        //从他的上一行开始遍历这个json对象
        index = index - 1;
        for (var i = index; i >= 0; i--) {
            var arr = gridJson[0]["C" + i];
            for (var n = 0; n < arr.length; n++) {
                //如果当前的code和传入的code一样，则找到他的父级名字
                if (arr[n].code == code) {
                    PName = arr[n].text;
                    break;
                }
            }
            if (PName != "") {
                break;
            }
        }
    }
    return PName;
}
/*                                                           把datagrid的列转成菜单面板                                                                  */
//隐藏列
var arrDataGrid = new Array();//存储表头的数组
function HiddenColumns(dgId, treeId, windowId, columnsLength) {
    var data = $("#" + dgId).datagrid("getData");
    var _opts = $("#" + dgId).datagrid("options");
    var url = _opts.url;
    arrDataGrid.length = 0;//重置数组
    //初始化存储表头的数组,初始化长度并且为他初始化成数组对象
    for (var i = 0; i < columnsLength; i++) {
        arrDataGrid[i] = new Array();
    }
    var node = $("#" + treeId).tree("getRoots");
    $("#" + windowId).dialog("close");
    RecursiveDatagridJson(node);

    var opts = {};
    opts.url = "";
    opts.columns = arrDataGrid;
    opts.data = data;
    $("#" + dgId).datagrid(opts);//重构表格布局

    //$("#" + dgId).datagrid({
    //    url: "",
    //    columns: arrDataGrid,
    //    data: data
    //});
    if (_opts.remoteFilter) {
        $("#" + dgId).datagrid('enableFilter', [{}]);
    }
    _opts.url = url;//恢复请求数据地址
}
//递归生成datagrid的json
function RecursiveDatagridJson(nodeObj) {
    for (var i = 0; i < nodeObj.length; i++) {
        //判断树节点前面的标识图片 只要节点为选中或者子节点有选中的节点
        if (nodeObj[i].target.childNodes[nodeObj[i].target.childNodes.length - 2].className.split(' ')[1] != "tree-checkbox0") {
            //arrDataGrid[nodeObj[i].id.substring(0, 1)] += "{field:'" + nodeObj[i].attributes.fildid + "',title:'" + nodeObj[i].text + "',width:'" + nodeObj[i].attributes.width + "',rowspan:'" + nodeObj[i].attributes.rowspan + "',colspan:'" + nodeObj[i].attributes.colspan + "',align:'" + nodeObj[i].attributes.align + "'},";
            arrDataGrid[nodeObj[i].id.substring(0, 1)].push(eval("[{" + "title:'" + nodeObj[i].text + "'" + (nodeObj[i].attributes.fildid == "" ? "" : ",field:'" + nodeObj[i].attributes.fildid + "'") + (nodeObj[i].attributes.width == "" ? "" : ",width:'" + nodeObj[i].attributes.width + "'") + (nodeObj[i].attributes.rowspan == "" ? "" : ",rowspan:" + nodeObj[i].attributes.rowspan) + (nodeObj[i].attributes.colspan == "" ? "" : ",colspan:" + nodeObj[i].attributes.colspan) + (nodeObj[i].attributes.align == "" ? "" : ",align:'" + nodeObj[i].attributes.align + "'") + "}]")[0]);
            if (nodeObj[i].children.length > 0) {
                RecursiveDatagridJson(nodeObj[i].children);
            }
        }
    }
}
/*改变节点自身信息，并为他所有的父级改变合并的列数
*@treeObj 树对象
*@node 树节点对象
*@mark true增加合并列，false减少合并列
*/
function setColspan(treeObj, node, mark, childrenColspan) {
    //var pnode = treeObj.tree('find', node.attributes.pcode);
    if (childrenColspan == null) {
        //处理自己节点的信息
        if (node.attributes.colspan != "") {
            childrenColspan = parseInt(node.attributes.colspan);
            if (mark) {
                node.attributes.colspan = node.children.length.toString();
            } else {
                node.attributes.colspan = "0";
            }
        }
        else {
            childrenColspan = 1;
        }
    } else {
        //处理父级节点
        //增加合并列
        if (mark) {
            node.attributes.colspan = (parseInt(node.attributes.colspan) + childrenColspan).toString();
        }
            //减少合并列
        else {
            node.attributes.colspan = (parseInt(node.attributes.colspan) - childrenColspan).toString();
        }
    }
    if (node.attributes.pcode != "") {
        setColspan(treeObj, treeObj.tree('find', node.attributes.pcode), mark, childrenColspan);
    }
}

//为页面添加dialog和tree组件 
/*
*@e 当前右键点击的元素对象
*@gridId 需要隐藏列的datagrid  ID
*/
function setWindowComponent(e, gridId) {
    gridId = gridId.id;
    var _wid = "setWindowComponent_treeWindow_" + gridId;
    var _tid = "setWindowComponent_treeDataGred_" + gridId;

    if ($("#" + _wid).length == 0) {
        var htmlStr = "<div id=\"" + _wid + "\" style=\"width:300px;height:320px;\"><ul id=\"" + _tid + "\"></ul></div>";
        $("body").append(htmlStr);

        if ($("#" + _tid)[0].className == "") {
            //禁用当前页面鼠标右键事件
            $(document).bind("contextmenu", function (e) {
                return false;
            });
            e.preventDefault();
            var MenuJson = gridTotree($("#" + gridId));//获取datagrid的树形数据
            $("#" + _tid).tree({
                data: eval("[" + ConversionTreeJson(eval(MenuJson), "", 0) + "]"),//ConversionTreeJson是把对象json转化为树json数据    ConversionTreeJson(eval(MenuJson), "", 0)
                checkbox: true,
                //lines: true,
                dnd: true,
                onBeforeDrop: function (target, source, point) {
                    var flag = true;
                    if (point == 'append' || $("#" + _tid).tree("getNode", target).attributes.pcode != source.attributes.pcode) {
                        flag = false;
                    }
                    return flag;
                },
                onCheck: function (node, checked) {
                    //每次点击复选框都为他的所有父级改变合并列的数值
                    setColspan($("#" + _tid), node, checked, null);
                }
            });
            $("#" + _wid).dialog({
                closed: true,
                modal: true,
                minimizable: false,
                maximizable: false,
                resizable: false,
                collapsible: false,
                resizable: true,
                title: "设置列显示与隐藏",
                buttons: [{ text: '确定', iconCls: 'icon-ok', handler: function () { HiddenColumns(gridId, _tid, _wid, $('#' + gridId).datagrid('options').columns.length); } }, { text: '取消', iconCls: 'icon-cancel', handler: function () { $('#' + _wid).dialog('close'); } }]
            });
        }
    }
    $("#" + _wid).dialog("open").dialog("move", { left: e.pageX, top: e.pageY });

}

function autoSizeColumn($grid) {
    if ($grid.datagrid('options').autoColumnWidth == undefined) {
        return false;
    }
    if ($grid.datagrid('options').autoColumnWidth == false) {//当data-options中设置autoColumnWidth:true时才处理自动列宽
        return false;
    }
    var cols = $grid.datagrid('options').columns[0];    // 得到columns对象       
    var rows = $grid.datagrid('getRows')
    if (rows.length > 0) {
        for (var i = 0; i < cols.length; i++) {
            if (!cols[i].hidden) {
                $grid.datagrid('autoSizeColumn', cols[i].field);
            }
        }
    }
}

//覆盖datagrid的右键表头事件
$.extend($.fn.datagrid.defaults, {
    onHeaderContextMenu: function (e, field) {
        setWindowComponent(e, this);//打开勾选隐藏列窗口，并生成隐藏列的树
    }
});
//覆盖datagrid的加载成功事件
$.extend($.fn.datagrid.defaults, {
    onLoadSuccess: function (data) {
        autoSizeColumn($(this));  //设置所有列自适应宽度
    }
});

/*导入EXCEL
*FromCode：表单代码
*ActionCode：操作代码
*Params:传入参数
*CallBack：回调函数
*SaveToDB：true表示直接将数据保存到数据库，False表示直接将数据返回前台，为空表示True
*/
function UploadExcel(FormCode, ActionCode, Params, CallBack, SaveToDB) {
    if (SaveToDB == undefined || SaveToDB == null) SaveToDB = true;
    $("body").append('<div id="winExcel" title="Excel数据导入向导"></div>');
    $('#winExcel').window({
        width: 800,
        height: 560,
        modal: true,
        collapsible: false,
        minimizable: false,
        maximizable: false,
        resizable: false,
        queryParams: Params,
        href: '/WebPage/FileCenter/ExcelUpload.aspx?formcode=' + FormCode + "&actioncode=" + ActionCode + "&SaveToDB=" + SaveToDB,
        onClose: function () {
            try {
                if (CallBack != undefined && CallBack != '') {
                    CallBack(excelData);
                }
            } catch (e) {

            }
            $("#winExcel").window('destroy');//移除掉原来的window             
        }
    });
}

// 对Date的扩展，将 Date 转化为指定格式的String 
// 月(M)、日(d)、小时(h)、分(m)、秒(s)、季度(q) 可以用 1-2 个占位符， 
// 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字) 
// 例子： 
// (new Date()).Format("yyyy-MM-dd hh:mm:ss.S") ==> 2006-07-02 08:09:04.423 
// (new Date()).Format("yyyy-M-d h:m:s.S")      ==> 2006-7-2 8:9:4.18 
Date.prototype.Format = function (fmt) { //author: meizz 
    var o = {
        "M+": this.getMonth() + 1,                 //月份 
        "d+": this.getDate(),                    //日 
        "h+": this.getHours(),                   //小时 
        "m+": this.getMinutes(),                 //分 
        "s+": this.getSeconds(),                 //秒 
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
        "S": this.getMilliseconds()             //毫秒 
    };
    if (/(y+)/.test(fmt))
        fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt))
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
}



//function ApprovalProcessFlow(FormCode, id) {
//    var btn = $("#btnPerformFlow");
//    btn.show();
//    btn.linkbutton('enable');
//    btn.unbind().bind('click', function () {
//        $("#dlgEdit").dialog('close');

//        $.post('/api/common/gridlist/GetPerformFlowList', { FormCode: FormCode, id: id }, function (result) {//获取表单发起流程列表
//            if (result.length > 0) {//存在发起流程
//                $("#PerformFlowWind").dialog('open').dialog('setTitle', '查看已发出流程');
//                $("#PerformFlowWind").dialog('refresh', '/WebPage/OA/PerformFlowList.aspx?FormCode=' + FormCode + '&ID=' + id);

//            } else {//不存在发起流程
//                //初次发起
//                $("#PerformFlowWind").dialog('open').dialog('setTitle', '选择发起流程');
//                // $("#PerformFlowWind").dialog('refresh', '/WebPage/OA/ChoosePerformFlow.aspx?DialogID=PerformFlowWind&FormCode=' + FormCode + '&ID=' + id);
//                $("#PerformFlowWind").dialog('refresh', '/WebPage/OA/ChoosePerformFlow.aspx?FormCode=' + FormCode + '&ID=' + id);
//            }
//        }, 'json');
//    });
//}

//显示自由选人节点的执行人选择窗口
//FlowCode:流程代码；FlowPerformID:流程执行流水号；FlowNodeCode：当前执行节点代码；CallBack：回调函数
function ShowFreeNodeUser(FlowCode, FlowPerformID, FlowNodeCode, CallBack, W, H) {
    $.post('/Flow/GetNoUserFreeChildNode', { FlowCode: FlowCode, FlowPerformID: FlowPerformID, FlowNodeCode: FlowNodeCode }, function (data) {
        if (data.success == undefined && data.length > 0) { //获取到未设置执行人的节点
            $("body").append('<div id="winFreeNodeUser" title="节点执行人设置"></div>');
            $('#winFreeNodeUser').dialog({
                width: 360,
                height: 260,
                modal: true,
                collapsible: false,
                minimizable: false,
                href: '/Flow/FreeChildNodeUser',
                queryParams: { nodelist: JSON.stringify(data), W: W, H: H },
                buttons: [{
                    text: '确定',
                    iconCls: 'icon-ok',
                    handler: function () {
                        var strJson = GetFreeNodeUser(CallBack);

                        if (strJson == '') {
                            return;
                        };
                        CallBack(FlowCode, FlowPerformID, FlowNodeCode, strJson);
                        $("#winFreeNodeUser").dialog('close');
                    }
                }, {
                    text: '取消',
                    iconCls: 'icon-cancel',
                    handler: function () { $("#winFreeNodeUser").dialog('close'); }
                }],
                onClose: function () {
                    //移除掉window            
                    $("#winFreeNodeUser").dialog('destroy');
                    try {
                        $("#winFreeNodeUser").remove();
                    } catch (e) { }
                }
            });
        } else {
            //没有获取到未设置执行人的节点
            CallBack(FlowCode, FlowPerformID, FlowNodeCode);
        }
    }, 'json');
}
//显示一个最顶层的窗口
//参数：
//src:要显示的内容页地址；params:传入的参数；title:窗口的标题；max:为true表示以最大化方式显示；width:窗口的默认宽度；height窗口的高度;CallBack:回调函数
function ShowTopWindow(src, params, title, max, width, height, CallBack) {
    var win = window.top.$('<div id="topwin"></div>').appendTo(window.top.document.body);
    if (max != true) {
        max = false;
    }
    if (width == undefined || width == null) {
        width = 640;
    }
    if (height == undefined || height == null) {
        height = 480;
    }
    win.window({
        width: width,
        height: height,
        title: title,
        maximized: max,
        modal: true,
        collapsible: false,
        minimizable: false,
        queryParams: params,
        href: src,
        onClose: function () {
            try {
                if (CallBack != undefined && CallBack != '') {
                    CallBack(params);
                }
            } catch (e) { }
            window.top.$("#topwin").window('destroy');
        }
    });
}

//------时间转换
function Format() {
    var o = {
        "M+": this.getMonth() + 1, // month  
        "d+": this.getDate(), // day  
        "h+": this.getHours(), // hour  
        "m+": this.getMinutes(), // minute  
        "s+": this.getSeconds(), // second  
        "q+": Math.floor((this.getMonth() + 3) / 3), // quarter  
        "S": this.getMilliseconds()
        // millisecond  
    }
    if (/(y+)/.test(format))
        format = format.replace(RegExp.$1, (this.getFullYear() + "")
            .substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(format))
            format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
    return format;
}
function formatDatebox1(value) {
    if (value == null || value == '') {
        return '';
    }
    var dt;
    if (value instanceof Date) {
        dt = value;
    } else {
        dt = new Date(value);
    }
    return dt.Format("yyyy-MM-dd hh:mm:ss"); //扩展的Date的format方法(上述插件实现)  
}
function formatDatebox2(value) {
    if (value == null || value == '') {
        return '';
    }
    var dt;
    if (value instanceof Date) {
        dt = value;
    } else {
        dt = new Date(value);
    }
    return dt.Format("yyyy-MM-dd"); //扩展的Date的format方法(上述插件实现)  
}
//------时间转换

//消息显示
function ShowMessage(strmessage) {
    // 消息将显示在顶部中间
    $.messager.show({
        timeout: 5000,
        title: '提示',
        msg: strmessage,
        showType: 'show',
        style: {
            right: '',
            top: document.body.scrollTop + document.documentElement.scrollTop,
            bottom: ''
        }
    });
}

/** 多行文本框换行时自适应高度
*   @val :文本内容
*   @objId :控件id
*/
function MultiTextBoxAotuHeight(val, objId) {
    debugger;
    var s = $("#" + objId).css("font-size");
    var defRows = parseFloat($("#" + objId).css("height")) / parseFloat(s);
    var strArr = val.split('\n');
    var rows = strArr.length;
    var _width = parseFloat($("#" + objId).css("width"));
    var len = rows * parseFloat(s);
    if (parseFloat(rows) > parseFloat(defRows)) {
        $("#" + objId).css("height", (len + 20).toString() + "px");
        $("#" + objId).next().css("height", (len + 20).toString() + "px");
        $("#" + objId).next().find("textarea").css("height", (len + 20).toString() + "px");
    } else {
        $("#" + objId).next().css("height", $("#" + objId).css("height"));
        $("#" + objId).next().find("textarea").css("height", $("#" + objId).css("height"));
    }
}