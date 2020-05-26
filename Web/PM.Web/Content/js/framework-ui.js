$(function () {
    document.body.className = localStorage.getItem('config-skin');
    $("[data-toggle='tooltip']").tooltip();
})
$.reload = function () {
    if (myBrowser() == "FF") {
        $("#SearchType option:first").prop("selected", 'selected');
        var consh = $(".SearchContent");
        $.each(consh, function (i, item) {
            $(item).addClass("hidSearchContent");
            $(item).val("");
        });

    }
    window.location.href=window.location.href.split('?')[0];
    return false;
}
$.loading = function (bool, text) {
    var $loadingpage = top.$("#loadingPage");
    var $loadingtext = $loadingpage.find('.loading-content');
    if (bool) {
        $loadingpage.show();
    } else {
        if ($loadingtext.attr('istableloading') == undefined) {
            $loadingpage.hide();
        }
    }
    if (!!text) {
        $loadingtext.html(text);
    } else {
        $loadingtext.html("数据加载中，请稍后…");
    }
    $loadingtext.css("left", (top.$('body').width() - $loadingtext.width()) / 2 - 50);
    $loadingtext.css("top", (top.$('body').height() - $loadingtext.height()) / 2);
}
$.request = function (name) {
    var search = location.search.slice(1);
    var arr = search.split("&");
    for (var i = 0; i < arr.length; i++) {
        var ar = arr[i].split("=");
        if (ar[0] == name) {
            if (decodeURI(ar[1]) == 'undefined') {
                return "";
            } else {
                return decodeURI(ar[1]);
            }
        }
    }
    return "";
}

//去左右空格;
function trim(s) {
    return s.toString().replace(/(^\s*)|(\s*$)/g, "");
}

/**
 * 设置光标到文本末
 * @param {} obj 
 * @returns {} 
 */
function setFocus(obj) {
    var len = obj.value.length;
    if (document.selection) {
        var sel = obj.createTextRange();
        sel.moveStart('character', len);
        sel.collapse();
        sel.select();
    } else if (typeof obj.selectionStart == 'number' && typeof obj.selectionEnd == 'number') {
        obj.selectionStart = obj.selectionEnd = len;
    }
    if (obj.localName != "select") {
        obj.select();
    }
}

/*
验证是否为空
*/
function IsNullOrEmpty(str) {
    var isOK = false;
    if (str == undefined) {
        isOK = true;
    } else {
        str = trim(str);
    }
    if (str == undefined || str == "" || str == 'null') {
        isOK = true;
    }
    return isOK;
}

$.currentWindow = function () {
    var iframeId = top.$(".NFine_iframe:visible").attr("id");
    return top.frames[iframeId];
}
$.browser = function () {
    var userAgent = navigator.userAgent;
    var isOpera = userAgent.indexOf("Opera") > -1;
    if (isOpera) {
        return "Opera"
    };
    if (userAgent.indexOf("Firefox") > -1) {
        return "FF";
    }
    if (userAgent.indexOf("Chrome") > -1) {
        if (window.navigator.webkitPersistentStorage.toString().indexOf('DeprecatedStorageQuota') > -1) {
            return "Chrome";
        } else {
            return "360";
        }
    }
    if (userAgent.indexOf("Safari") > -1) {
        return "Safari";
    }
    if (userAgent.indexOf("compatible") > -1 && userAgent.indexOf("MSIE") > -1 && !isOpera) {
        return "IE";
    };
}
$.download = function (url, data, method) {
    if (url && data) {
        data = typeof data == 'string' ? data : jQuery.param(data);
        var inputs = '';
        $.each(data.split('&'), function () {
            var pair = this.split('=');
            inputs += '<input type="hidden" name="' + pair[0] + '" value="' + pair[1] + '" />';
        });
        $('<form action="' + url + '" method="' + (method || 'post') + '">' + inputs + '</form>').appendTo('body').submit().remove();
    };
};
$.modalOpen = function (options) {
    var defaults = {
        id: null,
        title: '系统窗口',
        width: "100px",
        height: "100px",
        url: '',
        shade: 0.3,
        btn: ['确认', '关闭'],
        btnclass: ['btn btn-primary', 'btn btn-danger'],
        callBack: null,
        callBack2: null,
        cancelBack: null,
    };
    var options = $.extend(defaults, options);
    var _width = top.$(window).width() > parseInt(options.width.replace('px', '')) ? options.width : top.$(window).width() + 'px';
    var _height = top.$(window).height() > parseInt(options.height.replace('px', '')) ? options.height : top.$(window).height() + 'px';
    top.layer.open({
        id: options.id,
        type: 2,
        shade: options.shade,
        title: options.title,
        fix: false,
        area: [_width, _height],
        content: options.url,
        btn: options.btn,
        btnclass: options.btnclass,
        maxmin: true,
        yes: function () {
            options.callBack(options.id)
        }
        , btn2: function (index, layero) {
            if (options.callBack2 != null) {
                options.callBack2(options.id)
                return false;
            } else {
                return true;
            }
        }
        , cancel: function () {
            if (options.cancelBack != null) {
                //刷新首页审批消息数量
                window.parent.getcount();
                //刷新父页面
                location.reload();
                return true;
            } else {
                return true;
            }
        }
        //maxmin: true
    });
}
$.modalConfirm = function (content, callBack) {
    top.layer.confirm(content, {
        icon: "fa-exclamation-circle",
        title: "系统提示",
        btn: ['确认', '取消'],
        btnclass: ['btn btn-primary', 'btn btn-danger'],
    }, function () {
        callBack(true);
    }, function () {
        callBack(false)
    });
}
$.modalAlert = function (content, type) {
    var icon = "";
    if (type == 'success') {
        icon = "fa-check-circle";
    }
    if (type == 'error') {
        icon = "fa-times-circle";
    }
    if (type == 'warning') {
        icon = "fa-exclamation-circle";
    }
    top.layer.alert(content, {
        icon: icon,
        title: "系统提示",
        btn: ['确认'],
        btnclass: ['btn btn-primary'],
    });
}
$.modalMsg = function (content, type) {
    if (type != undefined) {
        var icon = "";
        if (type == 'success') {
            icon = "fa-check-circle";
        }
        if (type == 'error') {
            icon = "fa-times-circle";
        }
        if (type == 'warning') {
            icon = "fa-exclamation-circle";
        }
        top.layer.msg(content, { icon: icon, time: 4000, shift: 5 });
        top.$(".layui-layer-msg").find('i.' + icon).parents('.layui-layer-msg').addClass('layui-layer-msg-' + type);
    } else {
        top.layer.msg(content);
    }
}
$.modalClose = function () {
    var index = top.layer.getFrameIndex(window.name); //先得到当前iframe层的索引
    var $IsdialogClose = top.$("#layui-layer" + index).find('.layui-layer-btn').find("#IsdialogClose");
    var IsClose = $IsdialogClose.is(":checked");
    if ($IsdialogClose.length == 0) {
        IsClose = true;
    }
    if (IsClose) {
        top.layer.close(index);
    } else {
        location.reload();
    }
}
$.submitForm = function (options) {
    var defaults = {
        url: "",
        param: [],
        loading: "正在提交数据...",
        success: null,
        close: true
    };
    var options = $.extend(defaults, options);
    $.loading(true, options.loading);
    window.setTimeout(function () {
        if ($('[name=__RequestVerificationToken]').length > 0) {
            options.param["__RequestVerificationToken"] = $('[name=__RequestVerificationToken]').val();
        }
        $.ajax({
            url: options.url,
            data: options.param,
            type: "post",
            dataType: "json",
            success: function (data) {
                if (data.state == "success") {
                    options.success(data);
                    $.modalMsg(data.message, data.state);
                    if (options.close == true) {
                        $.modalClose();
                    }
                } else {
                    $.modalAlert(data.message, data.state);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $.loading(false);
                $.modalMsg(errorThrown, "error");
            },
            beforeSend: function () {
                $.loading(true, options.loading);
            },
            complete: function () {
                $.loading(false);
            }
        });
    }, 500);
}
$.deleteForm = function (options) {
    var defaults = {
        prompt: "注：您确定要删除该项数据吗？",
        url: "",
        param: [],
        loading: "正在删除数据...",
        success: null,
        close: true
    };
    var options = $.extend(defaults, options);
    if ($('[name=__RequestVerificationToken]').length > 0) {
        options.param["__RequestVerificationToken"] = $('[name=__RequestVerificationToken]').val();
    }
    $.modalConfirm(options.prompt, function (r) {
        if (r) {
            $.loading(true, options.loading);
            window.setTimeout(function () {
                $.ajax({
                    url: options.url,
                    data: options.param,
                    type: "post",
                    dataType: "json",
                    success: function (data) {
                        if (data.state == "success") {
                            options.success(data);
                            $.modalMsg(data.message, data.state);
                        } else {
                            $.modalAlert(data.message, data.state);
                        }
                    },
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        $.loading(false);
                        $.modalMsg(errorThrown, "error");
                    },
                    beforeSend: function () {
                        $.loading(true, options.loading);
                    },
                    complete: function () {
                        $.loading(false);
                    }
                });
            }, 500);
        }
    });

}
$.jsonWhere = function (data, action) {
    if (action == null) return;
    var reval = new Array();
    $(data).each(function (i, v) {
        if (action(v)) {
            reval.push(v);
        }
    })
    return reval;
}
$.fn.jqGridRowValue = function () {
    var $grid = $(this);
    var selectedRowIds = $grid.jqGrid("getGridParam", "selarrrow");
    if (selectedRowIds != "") {
        var json = [];
        var len = selectedRowIds.length;
        for (var i = 0; i < len ; i++) {
            var rowData = $grid.jqGrid('getRowData', selectedRowIds[i]);
            json.push(rowData);
        }
        return json;
    } else {
        return $grid.jqGrid('getRowData', $grid.jqGrid('getGridParam', 'selrow'));
    }
}
$.fn.formValid = function () {
    return $(this).valid({
        errorPlacement: function (error, element) {
            element.parents('.formValue').addClass('has-error');
            element.parents('.has-error').find('i.error').remove();
            element.parents('.has-error').append('<i class="form-control-feedback fa fa-exclamation-circle error" data-placement="left" data-toggle="tooltip" title="' + error + '"></i>');
            $("[data-toggle='tooltip']").tooltip();
            if (element.parents('.input-group').hasClass('input-group')) {
                element.parents('.has-error').find('i.error').css('right', '33px')
            }
        },
        success: function (element) {
            element.parents('.has-error').find('i.error').remove();
            element.parent().removeClass('has-error');
        }
    });
}
$.fn.formSerialize = function (formdate) {
    var element = $(this);
    if (!!formdate) {
        for (var key in formdate) {
            var $id = element.find('#' + key);
            var value = $.trim(formdate[key]).replace(/&nbsp;/g, '');
            var type = $id.attr('type');
            if ($id.hasClass("select2-hidden-accessible")) {
                type = "select";
            }
            switch (type) {
                case "checkbox":
                    if (value == "true" || value == "True") {
                        $id.attr("checked", 'checked');
                    } else {
                        $id.removeAttr("checked");
                    }
                    break;
                case "select":
                    $id.val(value).trigger("change");
                    break;
                default:
                    $id.val(value);
                    break;
            }
        };
        if (element.selector == "#formd")//查看页面禁用元素
        {
            element.find('.form-control,select,input').attr('disabled', 'disabled');
            element.find('div.ckbox label').attr('for', '');
        }

        return false;
    }
    var postdata = {};
    element.find('input,select,textarea').each(function (r) {
        var $this = $(this);
        var id = $this.attr('id');
        var type = $this.attr('type');
        if (!type) {
            type = $this[0].type
        }
        switch (type) {
            case "checkbox":
                postdata[id] = $this.is(":checked");
                break;
            case "textarea":
                postdata[id] = $this.val();
                break;
            default:
                //var value = ($this.val() == "" || $this.val() == null) ? "&nbsp;" : $this.val();
                //if (!$.request("keyValue")) {
                //    value = value.replace(/&nbsp;/g, '');
                //}
                var value = ($this.val() == "" || $this.val() == null) ? "" : $this.val();
                //postdata[id] = value.replace(/[ ]/g, "");//去除字符串所有空格
                postdata[id] = $.trim(value);//去除字符串前后空格
                break;
        }
    });
    //if ($('[name=__RequestVerificationToken]').length > 0) {
    //    postdata["__RequestVerificationToken"] = $('[name=__RequestVerificationToken]').val();
    //}
    if (top.clients.projectId != "" && top.clients.projectId != null && top.clients.projectId != undefined) {
        postdata["ProjectId"] = top.clients.projectId;
    }
    return postdata;
};
$.fn.bindSelect = function (options) {
    var defaults = {
        id: "id",
        text: "text",
        search: false,
        url: "",
        param: [],
        attr: "",
        change: null
    };
    var options = $.extend(defaults, options);
    var $element = $(this);
    if (options.url != "") {
        $.ajax({
            url: options.url,
            data: options.param,
            dataType: "json",
            async: false,
            success: function (data) {
                $.each(data, function (i) {
                    if (options.attr) {
                        var str = "<option " + options.attr + "=" + data[i][options.attr] + "></option>"
                        $element.append($(str).val(data[i][options.id]).html(data[i][options.text]));
                    }
                    else {
                        $element.append($("<option></option>").val(data[i][options.id]).html(data[i][options.text]));
                    }
                });
                if (typeof ($element.select2) == 'function') {
                    $element.select2({
                        minimumResultsForSearch: options.search == true ? 0 : -1
                    });
                }
                $element.on("change", function (e) {
                    if (options.change != null) {
                        options.change(data[$(this).find("option:selected").index()]);
                    }
                    $("#select2-" + $element.attr('id') + "-container").html($(this).find("option:selected").text().replace(/　　/g, ''));
                });
            }
        });
    } else {
        $element.select2({
            minimumResultsForSearch: -1
        });
    }
}
$.fn.authorizeButton = function () {
    var moduleId = top.$(".NFine_iframe:visible").attr("id").substr(6);
    var dataJson = top.clients.authorizeButton[moduleId];
    var $element = $(this);
    $element.find('a[authorize=yes]').attr('authorize', 'no');
    if (dataJson != undefined) {
        $.each(dataJson, function (i) {
            $element.find("#" + dataJson[i].F_EnCode).attr('authorize', 'yes');
        });
    }
    $element.find("[authorize=no]").parents('li').prev('.split').remove();
    $element.find("[authorize=no]").parents('li').remove();
    $element.find('[authorize=no]').remove();
}
$.fn.dataGrid = function (options) {
    var defaults = {
        datatype: "json",
        autowidth: true,
        rownumbers: true,
        shrinkToFit: false,
        gridview: true
    };
    var options = $.extend(defaults, options);
    var $element = $(this);
    options["onSelectRow"] = function (rowid) {
        var length = 0;
        var selectedRowIds = $(this).jqGrid("getGridParam", "selarrrow");//多选
        if (selectedRowIds != "") {
            length = selectedRowIds.length;
        }
        else {
            var selectedRowId = $(this).jqGrid("getGridParam", "selrow");//单选
            if (selectedRowId) {
                length = 1;
            }
        }

        var $operate = $(".operate");
        if (length > 0) {
            $operate.animate({ "left": 0 }, 200);
        } else {
            $operate.animate({ "left": '-100.1%' }, 200);
        }
        $operate.find('.close').click(function () {
            $operate.animate({ "left": '-100.1%' }, 200);
        })
    };
    $element.jqGrid(options);
};
$.LodeMenuBtn = function (strUri) {
    $.ajax({
        url: "/Common/LoadPermissions?strUri=" + strUri,
        type: "get",
        async: false,
        success: function (data) {
            $("#toolbar").append(data);
        }
    });
}
$.fn.GetSearchCondition = function (formdate) {
    var element = $(this);
    var postdata = {};
    element.find('input,select,textarea').each(function (r) {
        var $this = $(this);
        var id = $this.attr('id');
        var type = $this.attr('type');
        if (!type) {
            type = $this[0].type
        }
        switch (type) {
            case "checkbox":
                postdata[id] = $this.is(":checked");
                break;
            case "textarea":
                postdata[id] = $this.val();
                break;
            default:
                var value = ($this.val() == "" || $this.val() == null) ? "" : $this.val();
                postdata[id] = $.trim(value);//去除字符串前后空格
                break;
        }
    });
    return postdata;
};
/**
打开文本弹出对话窗口并获取选中行的值
    参数说明：
    1.tableName:弹出窗口要加载的表头名称的表
    2.GetDataUrl:弹出窗口数据源地址
    3.GridType:加载表格类型(Grid/TreeGrid)
    4.SortField:数据排序字段
    5.Key:可以为空（用于treegrid分级）
    6.BackFiles：需要回显的字段字符串(如果当前字段与表中字段不一致时用(页面字段名称=弹框中字段名称))
    7.width:弹出窗口的宽度
    8.height:弹出窗口的高度
    9.FunBack:回调函数
    10.multiselect:是否多选
    11.queryCriteria :placeholder
    12.isPage:是否分页 (true/false)
**/
//数据弹框
function selectClick(tableName, GetDataUrl, GridType, SortField, Key, BackFiles, width, height, FunBack, multiselect, queryCriteria, isPage) {
    $.modalOpen({
        id: "SelectForm",
        title: "窗口",
        url: "/Common/SelectForm?tableName=" + tableName + "&GetDataUrl=" + GetDataUrl + "&GridType=" + GridType + "&SortField=" + SortField + "&Key=" + Key + "&multiselect=" + multiselect + "&queryCriteria=" + queryCriteria + "&isPage=" + isPage,
        width: width,
        height: height,
        callBack: function (iframeId) {
            var BackData = top.frames[iframeId].BackData(multiselect);
            if (BackData == "" || BackData == null || BackData == undefined) {
                return false;
            }
            var a = BackFiles.split(",");
            for (var key in BackData[0]) {
                var $id = $('#' + key);
                if ($id.length > 0) {
                    $id.val(BackData[0][key]);
                }
                else {
                    for (var i = 0; i < a.length; i++) {
                        var b = a[i].split("=");
                        if (b.length == 2) {
                            $("#" + b[0] + "").val(BackData[0][b[1]]);
                        } else {
                            $("#" + a[i] + "").val(BackData[0][a[i]]);
                        }
                    }
                }
            }

            if (FunBack != "" && FunBack != null && FunBack != undefined) {
                //回调函数
                FunBack(BackData);
            }
            var index = top.layer.getFrameIndex(iframeId);
            top.layer.close(index);
        }
    });
}

/**
    打开文本弹出对话窗口并获取选中行的值
    参数说明：
    1.BackFiles：需要回显的字段字符串(如果当前字段与表中字段不一致时用(页面字段名称=弹框中字段名称))
    2.width:弹出窗口的宽度
    3.height:弹出窗口的高度
    4.FunBack:回调函数
    5.multiselect:是否多选
    5.where:筛选条件
**/
//数据弹框(组织机构用户)
function selectClickNew(BackFiles, width, height, FunBack, multiselect, where) {
    $.modalOpen({
        id: "SelectFormNew",
        title: "窗口",
        url: "/Common/SelectFormNew?multiselect=" + multiselect + "&where=" + where,
        width: width,
        height: height,
        callBack: function (iframeId) {
            var BackData = top.frames[iframeId].BackData(multiselect);
            if (BackData == "" || BackData == null || BackData == undefined) {
                return false;
            }
            var a = BackFiles.split(",");
            for (var key in BackData[0]) {
                var $id = $('#' + key);
                if ($id.length > 0) {
                    $id.val(BackData[0][key]);
                }
                else {
                    for (var i = 0; i < a.length; i++) {
                        var b = a[i].split("=");
                        if (b.length == 2) {
                            $("#" + b[0] + "").val(BackData[0][b[1]]);
                        } else {
                            $("#" + a[i] + "").val(BackData[0][a[i]]);
                        }
                    }
                }
            }
            if (FunBack != "" && FunBack != null && FunBack != undefined) {
                //回调函数
                FunBack(BackData);
            }
            var index = top.layer.getFrameIndex(iframeId);
            top.layer.close(index);
        }
    });
}


//自动转换数字金额为大小写中文字符,返回大小写中文字符串，最大处理到999兆
function changeMoneyToChinese(money) {
    var cnNums = new Array("零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖"); //汉字的数字
    var cnIntRadice = new Array("", "拾", "佰", "仟"); //基本单位
    var cnIntUnits = new Array("", "万", "亿", "兆"); //对应整数部分扩展单位
    var cnDecUnits = new Array("角", "分", "毫", "厘"); //对应小数部分单位
    var cnInteger = "整"; //整数金额时后面跟的字符
    var cnIntLast = "元"; //整型完以后的单位
    var maxNum = 999999999999999.9999; //最大处理的数字

    var IntegerNum; //金额整数部分
    var DecimalNum; //金额小数部分
    var ChineseStr = ""; //输出的中文金额字符串
    var parts; //分离金额后用的数组，预定义

    if (money == "") {
        if (money == 0) {

        } else {
            return "";
        }
    }

    money = parseFloat(money);
    //alert(money);
    if (money >= maxNum) {
        $.alert('超出最大处理数字');
        return "";
    }
    if (money == 0) {
        ChineseStr = cnNums[0] + cnIntLast + cnInteger;
        //document.getElementById("show").value=ChineseStr;
        return ChineseStr;
    }
    money = money.toString(); //转换为字符串
    if (money.indexOf(".") == -1) {
        IntegerNum = money;
        DecimalNum = '';
    } else {
        parts = money.split(".");
        IntegerNum = parts[0];
        DecimalNum = parts[1].substr(0, 4);
    }
    if (parseInt(IntegerNum, 10) > 0) {//获取整型部分转换
        var zeroCount = 0;
        var IntLen = IntegerNum.length;
        for (i = 0; i < IntLen; i++) {
            n = IntegerNum.substr(i, 1);
            p = IntLen - i - 1;
            q = p / 4;
            m = p % 4;
            if (n == "0") {
                zeroCount++;
            } else {
                if (zeroCount > 0) {
                    ChineseStr += cnNums[0];
                }
                zeroCount = 0; //归零
                ChineseStr += cnNums[parseInt(n)] + cnIntRadice[m];
            }
            if (m == 0 && zeroCount < 4) {
                ChineseStr += cnIntUnits[q];
            }
        }
        ChineseStr += cnIntLast;
        //整型部分处理完毕
    }
    if (DecimalNum != '') {//小数部分
        var decLen = DecimalNum.length;
        for (i = 0; i < decLen; i++) {
            n = DecimalNum.substr(i, 1);
            if (n != '0') {
                ChineseStr += cnNums[Number(n)] + cnDecUnits[i];
            }
        }
    }
    if (ChineseStr == '') {
        ChineseStr += cnNums[0] + cnIntLast + cnInteger;
    }
    else if (DecimalNum == '') {
        ChineseStr += cnInteger;
    }
    return ChineseStr;
}

function delHtmlTag(str) {
    if (str == undefined) {
        return "";
    } else {
        str = str.toString().replace("&nbsp;", "");
        return str.replace(/<[^>]+>/g, ""); //去掉所有的html标记
    }
}
function nbsptrim(s) {
    s = s.toString().replace(/ /ig, "");
    return s.toString().replace(/&nbsp;/ig, "");
}
//设置光标在文本末
function setFocus(obj) {
    var len = obj.value.length;
    if (document.selection) {
        var sel = obj.createTextRange();
        sel.moveStart('character', len);
        sel.collapse();
        sel.select();
    } else if (typeof obj.selectionStart == 'number' && typeof obj.selectionEnd == 'number') {
        obj.selectionStart = obj.selectionEnd = len;
    }
    if (obj.localName != "select") {
        obj.select();
    }
}

//赋值时间
function setData(data) { //获得当前日期
    var d = new Date();
    var vYear = d.getFullYear();
    var vMon = d.getMonth() + 1;
    var vDay = d.getDate();
    var today = vYear + "-" + (vMon < 10 ? "0" + vMon : vMon) + "-" + (vDay < 10 ? "0" + vDay : vDay);
    $("#" + data + "").val(today);
}

//获得当前日期
function GetData() {
    var d = new Date();
    var vYear = d.getFullYear();
    var vMon = d.getMonth() + 1;
    var vDay = d.getDate();
    var today = vYear + "-" + (vMon < 10 ? "0" + vMon : vMon) + "-" + (vDay < 10 ? "0" + vDay : vDay);
    return today;
}

//格式化日期
Date.prototype.Format = function (fmt) {
    var o = {
        "M+": this.getMonth() + 1, //月份 
        "d+": this.getDate(), //日 
        "H+": this.getHours(), //小时 
        "m+": this.getMinutes(), //分 
        "s+": this.getSeconds(), //秒 
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
        "S": this.getMilliseconds() //毫秒 
    };
    if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
}
//格式化列表中的时间
function formatDatebox(value) {
    var IsAny = checkDate(value);
    if (IsAny) {
        return /\d{4}-\d{1,2}-\d{1,2}/g.exec(value);
        //var dt = new Date(value);
        //var aa = dt.Format("yyyy-MM-dd");
        //return aa;
        //return dt.Format("yyyy-MM-dd");//扩展的Date的format方法(上述插件实现)
    } else {
        if (value == null || value == "null" || value == "NULL") {
            value = " ";
        }
        return value;
    }
}

function formatterDateTime2(value) {
    if (value == null || value == '') {
        return '';
    }
    var dt;
    if (value instanceof Date) {
        dt = value;
    } else {
        dt = new Date(value);
    }
    return dt.Format("yyyy-MM-dd HH:mm"); //扩展的Date的format方法(上述插件实现)  
}
function formatterDateTime1(value) {
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

/**
* 验证日期格式
* @param date
* @return {boolean}
*/
function checkDate(date) {
    var reg = /^[1-9]\d{3}-(0[1-9]|1[0-2])-(0[1-9]|[1-2][0-9]|3[0-1])\s+(20|21|22|23|[0-1]\d):[0-5]\d:[0-5]\d$/;
    var regExp = new RegExp(reg);
    if (!regExp.test(date)) {
        return false;
    } else { return true }
}

function ZDate(dt) {
    if (dt) {
        return (new Date(Date.parse(dt.replace(/-/g, "/"))));
    }
    return value;
}


function ZString1(val, row) {
    if (val != null) {
        var date = new Date(val);
        var year = date.getFullYear(),
            yue = date.getMonth() + 1,
            day = date.getDate(),
            shi = date.getHours(),
            fen = date.getMinutes(),
            miao = date.getSeconds();
        var dt = year + '-' + (yue < 10 ? "0" + yue : yue) + '-' + (day < 10 ? "0" + day : day) + ' ' + (shi < 10 ? "0" + shi : shi) + ':' + (fen < 10 ? "0" + fen : fen) + ':' + (miao < 10 ? "0" + miao : miao);
        return dt;
    }
}


function myBrowser() {
    var userAgent = navigator.userAgent; //取得浏览器的userAgent字符串

    var isOpera = userAgent.indexOf("Opera") > -1;

    if (isOpera) {

        return "Opera"

    }; //判断是否Opera浏览器

    if (userAgent.indexOf("Firefox") > -1) {

        return "FF";

    } //判断是否Firefox浏览器

    if (userAgent.indexOf("Chrome") > -1) {

        return "Chrome";

    }

    if (userAgent.indexOf("Safari") > -1) {

        return "Safari";

    } //判断是否Safari浏览器

    if (userAgent.indexOf("compatible") > -1 && userAgent.indexOf("MSIE") > -1 && !isOpera) {

        return "IE";

    }; //判断是否IE浏览器
}