function CommonView(options) {
    var defaults = {
        id: null,//弹框Id
        divId: null,//列表Id
        title: '删除',//弹框标题
        url: '',//地址
        anyUrl: '',//验证url
        isbtn: true,//是否有按钮
        isBack: true,//是否回调
        isAny: true,//是否验证数据
        isdel: false//是否删除
    };
    var options = $.extend(defaults, options);
    var str = options.title.substring(0, 2)
    var msg = "请选择要" + str + "的信息";
    options.title = options.title + "信息";
    if (options.divId == null) {
        options.divId = "#gridList";
    }
    var keyValue = $(options.divId).getGridParam("selrow");
    if (keyValue != "" && keyValue != null && keyValue != undefined) {
        //判断数据行是否符合当前登录人
        //if (top.clients.user != $("#gridList").jqGridRowValue().InsertUserCode && defaults.isBack) {
        //    $.modalMsg("该数据不属于当前登录人创建,不能进行此操作", "warning"); return false;
        //}
        options.url = options.url + "?keyValue=" + keyValue;
        var flag = true;
        if (options.isAny) {
            var examinestatus = $(options.divId).jqGridRowValue().Examinestatus;
            if (examinestatus != undefined) {
                if (examinestatus != "未发起" && examinestatus != "已退回") {
                    flag = false;
                    $.modalMsg("信息正在审核中或已审核完成,不能进行此操作", "warning"); return false;
                }
            }
            //验证是否可以进行操作
            if (options.anyUrl != "") {
                $.ajax({
                    url: options.anyUrl,
                    data: { keyValue: keyValue },
                    dataType: "json",
                    async: false,
                    success: function (data) {
                        if (data.state != "success") {
                            flag = false;
                            $.modalAlert(data.message, data.state);
                            return false;
                        }
                    }
                });
            }
        }
        if (flag) {
            if (options.isdel) {//删除信息
                $.deleteForm({
                    url: options.url,
                    param: { keyValue: keyValue },
                    success: function () {
                        $.currentWindow().$("#gridList").trigger("reloadGrid");
                    }
                });
            }
            else {
                if (typeof (CommonOpen) == 'function') {
                    CommonOpen(options.id, options.title, options.url, options.isbtn, options.isBack);
                }
                else {
                    CommonOpenForLC({
                        id: options.id,
                        title: options.title,
                        url: options.url,
                        isbtn: options.isbtn,
                        isBack: options.isBack
                    });
                }
            }
        }
    } else { $.modalMsg(msg, "warning"); return false; }
}

function CommonOpenAdd(options) {
    var defaults = {
        id: null,//弹框Id
        title: '',//弹框标题
        url: '',//地址
        isbtn: true,//是否有按钮
        isBack: true,//是否回调
        isAny: true,//是否验证数据
        windowType: 1,//1普通添加窗口 2流程窗口 3导入窗口
        btnText: '确认并发起',
    };
    var flag = true;
    var options = $.extend(defaults, options);
    ////判断页面上左边的组织机构是否存在
    //if (document.getElementById("leftgridList")) {
    //    //存在 
    //    var keyValue = $("#leftgridList").getGridParam("selrow");
    //    var CompanyId = "";
    //    if (keyValue == "" || keyValue == null || keyValue == undefined) {
    //        if (top.clients.orgType == "1") {
    //            flag = false;
    //            $.modalMsg("请先选择左边的组织机构信息", "warning");
    //            return false;
    //        }
    //    } else {
    //        top.clients.projectId = $("#leftgridList").jqGridRowValue().ProjectId;
    //        CompanyId = $("#leftgridList").jqGridRowValue().CompanyCode;
    //    }
    //}
    //判断页面上左边的组织机构是否存在
    if (document.getElementById("leftgridList")) {
        //存在 
        var keyValue = $("#leftgridList").getGridParam("selrow");
        var CompanyId = "";
        if (keyValue) {
            top.clients.projectId = $("#leftgridList").jqGridRowValue().ProjectId;
            CompanyId = $("#leftgridList").jqGridRowValue().CompanyCode;
        }
    }
    if (flag) {
        if (options.windowType == 1) { //添加
            if (typeof (CommonOpen) == 'function') {
                CommonOpen(options.id, options.title, options.url, options.isbtn, options.isBack);
            }
            else {
                CommonOpenForLC({
                    id: options.id,
                    title: options.title,
                    url: options.url,
                    isbtn: options.isbtn,
                    isBack: options.isBack
                });
            }
        }
        else if (options.windowType == 2) {//流程
            CommonOpenForLC(options);
        }
        else if (options.windowType == 3) {//导入
            inpuWindow(options);
        }
    }
}

function inpuWindow(options) {
    if (!top.clients.projectId) {
        $.modalMsg("请先选择左边的组织机构信息", "warning");
        return false;
    }
    var keyValue = "&keyValue=" + top.clients.projectId;
    $.modalOpen({
        id: "InputNew",
        title: options.title,
        url: "/Common/InputNew?submitUrl=" + options.url + keyValue,
        width: "600px",
        height: "500px",
        btn: null
    });
}

function CommonOpenForLC(options) {
    var defaults = {
        id: null,//弹框Id
        title: '删除',//弹框标题
        url: '',//地址
        isbtn: true,//是否有按钮
        isBack: true,//是否回调
        width: "55%",
        height: "600px",
        btnText: "确认并发起",
    };
    var options = $.extend(defaults, options);
    var data = OpenForLC();
    if (options.title == "新增到货入库信息") {
        $.modalOpen({
            id: options.id,
            title: options.title,
            url: options.url,
            width: data.width,
            height: data.height,
            btn: options.isbtn ? ['',options.btnText, '关闭'] : null,
            btnclass: ['', 'btn btn-primary', 'btn btn-danger'],
            callBack: options.isBack ? function (iframeId) {
                top.frames[iframeId].submitForm();
            } : null,
            callBack2: options.isBack ? function (iframeId) {
                top.frames[iframeId].submitForm2();
            } : null
        });
    } else {
        $.modalOpen({
            id: options.id,
            title: options.title,
            url: options.url,
            width: data.width,
            height: data.height,
            btn: options.isbtn ? ['确认', options.btnText, '关闭'] : null,
            btnclass: ['btn btn-primary', 'btn btn-primary', 'btn btn-danger'],
            callBack: options.isBack ? function (iframeId) {
                top.frames[iframeId].submitForm();
            } : null,
            callBack2: options.isBack ? function (iframeId) {
                top.frames[iframeId].submitForm2();
            } : null
        });
    }
    
}


//获取左侧组织机构编码
function getOrganizationalCode(code) {
    //判断当前节点是否有下级且唯一
    var siteCode = getSiteCode(code);
    var projectId = $("#leftgridList").jqGrid('getRowData', code).ProjectId;
    var param = $(".search").GetSearchCondition();
    param.SiteCode = siteCode;
    param.ProjectId = projectId;
    $("#gridList").jqGrid('setGridParam', {
        postData: param,
        page: 1
    }).trigger("reloadGrid");
    return siteCode;
}
function getSiteCode2(code) {
    var siteCode = code;
    var tr = $("#leftgridList").find('td[title=' + code + ']').parent();
    if (tr.length == 2) {
        var tr1 = $("#leftgridList").find('td[title=' + $(tr[(tr.length - 1)]).attr("id") + ']').parent();
        if (tr1.length == 1) {
            siteCode = $(tr[0]).attr("id");
        } else if (tr1.length >= 2) {
            siteCode = $(tr1[0]).attr("id");
        }
        else {
            siteCode = $(tr[(tr.length - 1)]).attr("id");
        }
        return siteCode;
    }
    else {
        return siteCode;
    }
}

function getSiteCode(code) {
    var siteCode = code;
    var tr = $("#leftgridList").find('td[title=' + code + ']').parent();
    if (tr.length == 2) {
        return getSiteCode($(tr[1]).attr("id"));
    }
    else {
        return siteCode;
    }
}

$(function () {
    //列表搜索框
    $("#SearchType").change(function () {
        var ss = $(this).children('option:selected').val();
        if (ss == "") {
            // return;
            $(this).siblings().addClass("hidSearchContent");
        }
        var consh = $(".SearchContent");
        $.each(consh, function (i, item) {
            $(item).addClass("hidSearchContent");
        });
        var cons = $("[name='" + ss + "']");
        $.each(cons, function (i, item) {
            $(item).removeClass("hidSearchContent");
        });
    });

    $("#btn_searchOne").click(function () {
        var postData = $(".search").formSerialize();
        for (x in postData) {
            if (x != "ProjectId") {
                postData[x] = "";
            }
        }
        var ss = $("#SearchType").children('option:selected').val();
        if (ss != "") {
            var cons = $("[name='" + ss + "']");
            $.each(cons, function (i, item) {
                postData[$(item)[0].id] = $(item).val();
            });
        }
        $("#gridList").jqGrid('setGridParam', {
            postData: postData,
            page: 1
        }).trigger('reloadGrid');
        if (typeof (SetParamData) == 'function') {
            SetParamData(postData);
        }
    });
});

function findDimensions() //函数：获取尺寸
{
    //var winWidth = 0;
    //var windivWidth = 0;
    //var winleft1 = 0;
    //var winleft2 = 0;
    //if (window.parent.document.body.offsetWidth) {//获取浏览器的宽度
    //    winWidth = window.parent.document.body.offsetWidth;
    //}
    //var left1 = window.parent.document.getElementById('nav-col');
    //winleft1 = left1.offsetWidth;
    //var left2 = document.getElementsByClassName('ui-layout-west');
    //winleft2 = left2[0].offsetWidth;
    //var content = document.getElementsByClassName('divwidth');
    //windivWidth = content[0].offsetWidth + content[1].offsetWidth;
    //var winallWidth = Number(windivWidth) + Number(winleft1) + Number(winleft2);
    //return winWidth + "/" + winallWidth;
    var rightwidth1 = 0;
    var rightwidth2 = 0;
    var content1 = document.getElementsByClassName('divwidth1');
    rightwidth1 = content1[0].offsetWidth;
    var content2 = document.getElementsByClassName('divwidth2');
    rightwidth2 = content2[0].offsetWidth + content2[1].offsetWidth+50;
    return rightwidth1 + "/" + rightwidth2;
}