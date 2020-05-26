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

function fdate(id)
{
    var aa = new Date(id.replace(/-/g, '/')).Format("yyyy-MM-dd");
    return aa;
}

//去左空格
function ltrim(s) {
    return s.toString().replace(/(^\s*)/g, "");
}
//去右空格;
function rtrim(s) {
    return s.toString().replace(/(\s*$)/g, "");
}


/**
当前时间
*/
function CurrentTime() {
    var date = new Date();
    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    if (month < 10) month = '0' + month;
    var day = date.getDate();
    if (day < 10) day = '0' + day;
    var hour = date.getHours();
    if (hour < 10) hour = '0' + hour;
    var minute = date.getMinutes();
    if (minute < 10) minute = '0' + minute;
    var second = date.getSeconds();
    if (second < 10) second = '0' + second;
    return year + '-' + month + '-' + day + ' ' + hour + ':' + minute + ':' + second;
}

/*
* 星期
*/
function CurrentDay() {
    var date = new Date();
    if (date.getDay() === 1) {
        return "星期一";
    } else if (date.getDay() === 2) {
        return "星期二";
    } else if (date.getDay() === 3) {
        return "星期三";
    } else if (date.getDay() === 4) {
        return "星期四";
    } else if (date.getDay() === 5) {
        return "星期五";
    } else if (date.getDay() === 6) {
        return "星期六";
    } else if (date.getDay() === 0) {
        return "星期日";
    }
    return "星期一";
}

//获取当前月份
function getNewMonth() {
    var dt = new Date();
    dt.setDate(1);
    dt.setMonth(dt.getMonth() + 1);
    cdt = new Date(dt.getTime() - 1000 * 60 * 60 * 24);
    return cdt.getFullYear() + "-" + (Number(cdt.getMonth()) + 1);
}
//获取当前月的最后一天
function getLastDay() {
    var dt = new Date();
    dt.setDate(1);
    dt.setMonth(dt.getMonth() + 1);
    cdt = new Date(dt.getTime() - 1000 * 60 * 60 * 24);
    return cdt.getFullYear() + "-" + (Number(cdt.getMonth()) + 1) + "-" + cdt.getDate();
}
//获取当时间
function getNowFormatDate() {
    var date = new Date();
    var seperator1 = "-";
    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    var strDate = date.getDate();
    if (month >= 1 && month <= 9) {
        month = "0" + month;
    }
    if (strDate >= 0 && strDate <= 9) {
        strDate = "0" + strDate;
    }
    var currentdate = year + seperator1 + month + seperator1 + strDate;
    return currentdate;
}

//通过差值与一个时间获取另外一个时间
function addByTransDate(dateParameter, num) {
    var translateDate = "", dateString = "", monthString = "", dayString = "";
    translateDate = dateParameter.replace("-", "/").replace("-", "/");
    var newDate = new Date(translateDate);
    newDate = newDate.valueOf();
    newDate = newDate + num * 24 * 60 * 60 * 1000; //备注 如果是往前计算日期则为减号 否则为加号
    newDate = new Date(newDate);
    //如果月份长度少于2，则前加 0 补位  
    if ((newDate.getMonth() + 1).toString().length == 1) {
        monthString = 0 + "" + (newDate.getMonth() + 1).toString();
    } else {
        monthString = (newDate.getMonth() + 1).toString();
    }
    //如果天数长度少于2，则前加 0 补位  
    if (newDate.getDate().toString().length == 1) {

        dayString = 0 + "" + newDate.getDate().toString();
    } else {

        dayString = newDate.getDate().toString();
    }
    dateString = newDate.getFullYear() + "-" + monthString + "-" + dayString;
    return dateString;
}

/**
文本框只允许输入数字
**/
function IsNumber(obj) {
    $("#" + obj).bind("contextmenu", function () {
        return false;
    });
    $("#" + obj).css('ime-mode', 'disabled');
    $("#" + obj).keypress(function (e) {
        if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
            return false;
        }
    });
}

/**
只能输入数字和小数点
**/
function IsMoney(obj) {
    $("#" + obj).bind("contextmenu", function () {
        return false;
    });
    $("#" + obj).css('ime-mode', 'disabled');
    $("#" + obj).bind("keydown", function (e) {
        var key = window.event ? e.keyCode : e.which;
        if (isFullStop(key)) {
            return $(this).val().indexOf('.') < 0;
        }
        return (isSpecialKey(key)) || ((isNumber(key) && !e.shiftKey));
    });
    function isNumber(key) {
        return key >= 48 && key <= 57;
    }
    function isSpecialKey(key) {
        return key == 8 || key == 46 || (key >= 37 && key <= 40) || key == 35 || key == 36 || key == 9 || key == 13 || (key >= 96 && key <= 105);
    }
    function isFullStop(key) {
        return key == 190 || key == 110;
    }
}

/**
* 金额格式(保留2位小数)后格式化成金额形式
*/
function FormatCurrency(num) {
    num = num.toString().replace(/\$|\,/g, '');
    if (isNaN(num))
        num = "0";
    sign = (num == (num = Math.abs(num)));
    num = Math.floor(num * 100 + 0.50000000001);
    cents = num % 100;
    num = Math.floor(num / 100).toString();
    if (cents < 10)
        cents = "0" + cents;
    for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3) ; i++)
        num = num.substring(0, num.length - (4 * i + 3)) + '' +
                num.substring(num.length - (4 * i + 3));
    return (((sign) ? '' : '-') + num + '.' + cents);
}

//保留三位小数    
//功能：将浮点数四舍五入，取小数点后3位   
function ToDecimal(x) {
    var f = parseFloat(x);
    if (isNaN(f)) {
        return 0;
    }
    f = Math.round(x * 1000) / 1000;
    return f;
}

//保留多少位小数点（动态设置）
function ToDecimalByNum(value, num) {
    var f = parseFloat(value);
    if (isNaN(f)) {
        return 0;
    }
    var zhi = 1;
    for (var a = 0; a < num; a++) {
        zhi = zhi * 10;
    }
    f = Math.round(value * zhi) / zhi;
    return f;
}



//js获取传递参数
function getUrlParams(name) {
    var param = "";
    var strFullPath = window.document.location.href;
    var data = strFullPath.split("?");
    if (data.length > 1) {
        var params = data[1].split("&");
        $.each(params, function (i, item) {
            var strValue = item.split("=");
            if (strValue[0] && strValue[0] == name) {
                param = strValue[1];
            }
        });
    }
    return param;
}

/*
刷新当前页面
*/
function Replace() {
    location.reload();
    return false;
}
/*
href跳转页面
*/
function Urlhref(url) {
    location.href = url;
    return false;
}

/**
 * GUID生成
 * @returns {} 
 */
function newGuid() {
    var guid = "";
    for (var i = 1; i <= 32; i++) {
        var n = Math.floor(Math.random() * 16.0).toString(16);
        guid += n;
        if ((i == 8) || (i == 12) || (i == 16) || (i == 20))
            guid += "-";
    }
    return guid;
}

$(function () {
    //文本框禁用改变背景色
    $("input[disabled='disabled'],textarea[disabled='disabled']").each(function () {
        $(this).addClass("disabled-bgcolor");
    });
    //下拉框禁用改变背景色
    $("select[disabled]").each(function () {
        $(this).parent().find(".select2-selection__rendered").addClass("disabled-bgcolor");
    });
});

function EditColor(id,cls)
{
    if (id != "")
    {
        //文本框改变背景色
        $(id).each(function () {
            $(this).removeClass("disabled-bgcolor");
        });
    }
    if (cls != "")
    {
        //下拉框改变背景色
        $(cls).each(function () {
            $(this).parent().find(".select2-selection__rendered").removeClass("disabled-bgcolor");
        });
    }
}
function EditColor1(id, cls) {
    if (id != "")
    {
        //文本框禁用改变背景色
        $(id).each(function () {
            $(this).addClass("disabled-bgcolor");
        });
    }
    if (cls != "")
    {
        //下拉框禁用改变背景色
        $(cls).each(function () {
            $(this).parent().find(".select2-selection__rendered").addClass("disabled-bgcolor");
        });
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