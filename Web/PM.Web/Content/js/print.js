var keyValue = $.request("keyValue");
var et = $.request("et");
var keyValue1 = $.request("keyValue1");
$(function () {
    top.$(".content-iframe").css("background-color", "#f9f9f9");
    var date = new Date();
    var year = date.getFullYear();
    if (!!keyValue) {
        $.ajax({
            url: "/Distribution/DistributionEnt/GetFormJson",
            data: { keyValue: keyValue, keyValue1: keyValue1 },
            dataType: "json",
            async: false,
            success: function (data) {
                $("h1").text(data.Item1[0].FHDW + " 半成品构件配送签收单");
                $("#shdw").text("收货单位：" + data.Item2[0].SiteName);
                $("#psbh").text("配送编号：" + data.Item1[0].DistributionCode);
                $("#ddh").text(data.Item2[0].TypeCode);
                $("#fhdw").text("发货单位：" + data.Item1[0].FHDW);
                $("#qsdw").text("签收单位：" + data.Item2[0].SiteName);
                $("#Cph").text("运输车牌：" + data.Item1[0].CarCph);
                $("#Sj").text("司机：" + data.Item1[0].CarUser);
                //$("#SjTel").text("司机电话：" + data.Item2[0].SiteName);
                var Num = 0;
                var SumQ = 0;
                var html = "";
                for (var i = 0; i < data.Item3.length; i++) {
                    if (data.Item3[i].PackagesNumber==null)
                    {
                        data.Item3[i].PackagesNumber = 0;
                    }
                    Num = Num + 1;
                    SumQ += Number(data.Item3[i].UnitWeight * data.Item3[i].SingletonWeight * data.Item3[i].Number);
                    html += "<tr class='trclass' style='border: 1px solid #070606;'>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + Num + "</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + data.Item2[0].SiteName + data.Item2[0].TypeName + data.Item2[0].TypeCode + "</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + data.Item3[i].MaterialName + "</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + data.Item3[i].Standard + "</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + data.Item3[i].UnitWeight + "</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + data.Item3[i].SingletonWeight + "</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + data.Item3[i].GjNumber + "</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + data.Item3[i].PackNumber + "</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + data.Item3[i].PackagesNumber + "</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + data.Item3[i].Number + "</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + Number(data.Item3[i].UnitWeight * data.Item3[i].SingletonWeight * data.Item3[i].Number).toFixed(5) + "</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>未计加工损耗量</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + data.Item3[i].Manufactor + "</td>" +
                        "<td class='tdclass1' style='border: 1px solid #070606;'>" + data.Item3[i].HeatNo + "</td></tr>";
                }
                $("#hj").text(SumQ.toFixed(5));
                $("#bt").after(html);
                $("#rq").append('<td class="tdclass1" colspan="5" style="border:1px solid #070606;">' + year + '年&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;月&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;日</td><td class="tdclass1" colspan="4" style="border:1px solid #070606;">' + year + '年&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;月&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;日</td><td class="tdclass1" colspan="5" style="border:1px solid #070606;">' + year + '年&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;月&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;日</td>');
            }
        });
    }
    if (et == "打印") {
        $("#ExAndPrint").append('<a class="btn btn-default " onclick="btn_print()"><i class="fa fa-print" style="margin-right: 5px; font-size: 13px;"></i>打印</a>');
    } else
    {
        $("#ExAndPrint").append('<a class="btn btn-default " onclick="exportExcel()"><i class="fa fa-print" style="margin-right: 5px; font-size: 13px;"></i>导出</a>');
    }
});

//打印
function btn_print() {
    $.ajax({
        url: "/Distribution/DistributionEnt/AddCount",
        data: {keyValue:keyValue},
        dataType: "json",
        async: false,
        success: function (data) {

        }
    });
    var bdhtml = window.document.body.innerHTML;
    var sprnstr = "<!--startprint-->";
    var eprnstr = "<!--endprint-->";
    var prnhtml = bdhtml.substr(bdhtml.indexOf(sprnstr) + 17);
    prnhtml = prnhtml.substring(0, prnhtml.indexOf(eprnstr));
    if (getExplorer() == "IE") {
        pagesetup_null();
    }
    window.document.body.innerHTML = prnhtml;
    window.print();
}

//导出

function exportExcel() {
    if (getExplorer() == 'IE') {
        var curTbl = document.getElementById("DTable");
        var oXL = new ActiveXObject("Excel.Application");
        var oWB = oXL.Workbooks.Add();
        var xlsheet = oWB.Worksheets(1);
        var sel = document.body.createTextRange();
        sel.moveToElementText(curTbl);
        sel.select();
        sel.execCommand("Copy");
        xlsheet.Paste();
        oXL.Visible = true;
        try {
            var fname = oXL.Application.GetSaveAsFilename("Excel.xls", "Excel Spreadsheets (*.xls), *.xls");
        } catch (e) {
            print("Nested catch caught " + e);
        } finally {
            oWB.SaveAs(fname);
            oWB.Close(savechanges = false);
            oXL.Quit();
            oXL = null;
            idTmr = window.setInterval("Cleanup();", 1);
        }
    }
    else {
        tableToExcel("DTable")
    }
}

var tableToExcel = (function () {
    var uri = 'data:application/vnd.ms-excel;base64,',
      template = '<html><head><meta charset="UTF-8"></head><body><table>{table}</table></body></html>',
      base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))) },
      format = function (s, c) {
          return s.replace(/{(\w+)}/g,
            function (m, p) { return c[p]; })
      }
    return function (table, name) {
        if (!table.nodeType) table = document.getElementById(table)
        var ctx = { worksheet: name || 'Worksheet', table: table.innerHTML }
        window.location.href = uri + base64(format(template, ctx))
    }
})()

function Cleanup() {
    window.clearInterval(idTmr);
    CollectGarbage();
}

//获取浏览器类型
function getExplorer() {
    var explorer = window.navigator.userAgent;
    //ie   
    if (explorer.indexOf("MSIE") >= 0) {
        return "IE";
    }
        //firefox   
    else if (explorer.indexOf("Firefox") >= 0) {
        return "Firefox";
    }
        //Chrome  
    else if (explorer.indexOf("Chrome") >= 0) {
        return "Chrome";
    }
        //Opera  
    else if (explorer.indexOf("Opera") >= 0) {
        return "Opera";
    }
        //Safari  
    else if (explorer.indexOf("Safari") >= 0) {
        return "Safari";
    }
}

//IE设置去除页眉页脚  
function pagesetup_null() {
    var hkey_root, hkey_path, hkey_key;
    hkey_root = "HKEY_CURRENT_USER";
    hkey_path = "\\Software\\Microsoft\\Internet Explorer\\PageSetup\\";
    try {
        var RegWsh = new ActiveXObject("WScript.Shell");
        hkey_key = "header";
        RegWsh.RegWrite(hkey_root + hkey_path + hkey_key, "");
        hkey_key = "footer";
        RegWsh.RegWrite(hkey_root + hkey_path + hkey_key, "");
    } catch (e) { }
}

