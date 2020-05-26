//发起流程
//DataId 业务数据ID
//FormCode 业务表单编码
//Examinestatus 单据审批状态
//DataCode 业务单据编号
//ProjectId 项目编号
//LoginUserCode 当前登录人
//OtherParma 其他参数
function examination(DataId, FormCode, Examinestatus, DataCode, ProjectId, LoginUserCode, OtherParma) {
    var flag = true;
    if (DataId) {
        //查找流程定义
        var OtherParmaNew = "";
        if (OtherParma) {//判断是否传入了其他参数
            if (OtherParma == "BuildingSteel") {//建筑钢筋
                OtherParmaNew = "建筑钢筋";
            } else if (OtherParma == "SectionSteel") {//型钢
                OtherParmaNew = "型钢";
            } else {
                OtherParmaNew = OtherParma;
            }
        }
        $.ajax({
            type: 'Get',
            url: '/OA/MyApproval/GetFlowDefine',
            data: { FormCode: FormCode, ProjectId: ProjectId, OtherParma: OtherParmaNew },
            async: false,
            cache: false,
            success: function (data) {
                var datajson = JSON.parse(data);
                if (datajson.length > 0) {
                    flag=LaunchFlow(FormCode, datajson[0].FlowCode, DataId, DataCode, datajson[0].FormName, LoginUserCode);
                } else {
                    $.modalMsg("流程未定义", "warning");
                    flag = false;
                    return false;
                }
            }
        });
    } else {
        $.modalMsg("请选择要发起流程的单据", "warning");
        flag = false;
        return false;
    }
    return flag;
    //var rowData = $("#gridList").jqGridRowValue();
    //if (id != "" && id != null && id != undefined) {
    //    if (examinestatus == "审核完成") {
    //        $.modalMsg("该数据处于" + examinestatus + "不能重复发起流程", "warning");
    //        return false;
    //    }
    //    else if (examinestatus == "审批中" || examinestatus == "已退回") {
    //        $.modalOpen({
    //            id: "Serach",
    //            title: "查看审批流程",
    //            url: "/Flow/PageDealFlow/Index?FormCode=" + formcode + "&FormDataCode=" + id,
    //            width: "60%",
    //            height: "450px",
    //            btn: null,
    //            callBack: function (iframeId) {
    //                top.frames[iframeId].submitForm();
    //                $.reload();
    //            }
    //        });
    //    }
    //    else {
    //        $.modalOpen({
    //            id: "ChangeFlow",
    //            title: "选择要发起的审批流程",
    //            url: "/Flow/PageDealFlow/ChangeFlow?FormCode=" + formcode + "&name=" + dataname + "&id=" + id + "&ProjectId=" + rowData.ProjectId,
    //            width: "50%",
    //            height: "450px",
    //            btn: ['发起流程', '关闭'],
    //            callBack: function (iframeId) {
    //                top.frames[iframeId].submitForm();
    //                $.reload();
    //            }
    //        });
    //    }
    //}
    //else {
    //    $.modalMsg("请选择要发起流程的单据", "warning");
    //    return false;
    //}
}

function SeeApproval(FormCode, DataId) {
    $.ajax({
        url: "/OA/MyApproval/GetDataExaminestatus",
        data: { FormCode: FormCode, DataId: DataId },
        dataType: "json",
        async: false,
        success: function (data) {
            if (data.length > 0) {
                $.modalOpen({
                    id: "Approval",
                    title: "审批单据信息",
                    url: "/OA/MyApproval/SeeApproval?FlowPerformID=" + data[0].FlowPerformID + "&ID=" + data[0].FormDataCode + "&FormCode=" + data[0].FormCode,
                    width: "85%",
                    height: "85%",
                    btn: null
                });
            }
        }
    });
}

//发起流程
function LaunchFlow(FormCode, FlowCode, DataId, DataCode, FormName, LoginUserCode) {
    $.loading(true, '正在发起流程...');
    var flag = true;
    var FlowTitle = '';
    FlowTitle = '关于《' + DataCode + '》的' + FormName + '审批';
    $.ajax({
        type: 'POST',
        url: '/OA/MyApproval/InitFlowPerformNew',
        data: { param: JSON.stringify({ FormCode: FormCode, FlowCode: FlowCode, FormDataCode: DataId, UserCode: LoginUserCode, FlowTitle: FlowTitle, FlowLevel: '0' }) },
        async: false,
        cache: false,
        success: function (data) {
            $.loading(false);
            var str_json = JSON.parse(data);
            if (str_json.state == "success") {
                $.modalMsg("发起流程成功", "success");
                if (FormCode!="InOrder") {
                    $.reload();
                }
                return true;
            }
            else {
                $.modalMsg("发起流程失败", "warning");
                flag = false;
                return false;
            }
        }
    });
    return flag;
}

//发起流程（编辑页面 确认并发起）
function examinationForAdd(options) {
    var defaults = {
        id: "",
        dataname: "",
        formcode: "",
        success: null
    };
    var options = $.extend(defaults, options);
    var a = top.frames;
    var b = "";
    if (options.formcode == "RawMonthDemandSupplyPlan") {
        b = "ChangeFlow";
    }
    $.modalOpen({
        id: "ChangeFlow",
        title: "选择要发起的审批流程",
        url: "/Flow/PageDealFlow/ChangeFlow?FormCode=" + options.formcode + "&name=" + options.dataname + "&id=" + options.id,
        width: "50%",
        height: "450px",
        btn: ['发起流程', '关闭'],
        callBack: function (iframeId) {
            if (options.formcode == "RawMonthDemandSupplyPlan") {
                a[b].submitForm();
            } else {
                top.frames[iframeId].submitForm();
            }
            options.success(iframeId);
        }
    });
}
//数据授权
function btn_other5() {
    var dataID = 0;
    var ids = $("#gridList").jqGrid('getGridParam', 'selarrrow');
    if (ids.length > 1) {
        $.modalMsg("只能选择一条数据", "warning");
        return false;
    }
    else if (ids.length == 0) {
        var keyValue = $("#gridList").getGridParam("selrow");
        if (keyValue == "" || keyValue == null || keyValue == undefined) {
            $.modalMsg("请选择数据", "warning"); return false;
        }
    }
    var url = $('#gridList').jqGrid('getGridParam', 'url');
    var arr = url.split('/');
    var menuCode = arr[2];
    authorizeUser(menuCode);
}
//menuCode 业务表单编码
function authorizeUser(menuCode) {
    var rowData = $("#gridList").jqGridRowValue();
    if (jQuery.isArray(rowData)) {
        rowData = rowData[0];
    }
    var dataID = rowData.ID;
    if (dataID == undefined) {
        dataID = rowData.id;
    }
    if (dataID == undefined || dataID == 0) {
        $.modalMsg("数据信息错误", "warning"); return false;
    }
    $.modalOpen({
        id: "Form",
        title: "人员数据授权",
        url: "/SystemManage/AuthorizeUser/Form?menuCode=" + menuCode + "&dataID=" + dataID,
        width: "50%",
        height: "650px",
        btn: ['确认', '关闭'],
        callBack: function (iframeId) {
            top.frames[iframeId].submitForm();
        }
    });
}