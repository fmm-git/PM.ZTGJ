
var _lineInfo;         //存储连线信息
var _globalcanvas;     //存储设计器对象
var _processData;
var _flowCode;//流程编码
var _formCode;//表单编码

$(function () {
    //if (parent.$('#iframeFlowDesign') == null || _readOnly == "true") {
    //    $('#BtnDesign').hide();
    //    $('#processMenu').remove();//删除右键菜单
    //}
    _flowCode = $("#FlowCode").val();
    _formCode = $("#FormCode").val();
    var alertModal = $('#alertModal'), attributeModal = $("#attributeModal");
    //消息提示
    mAlert = function (messages, s) {
        if (!messages) messages = "";
        if (!s) s = 5000;
        alertModal.find(".modal-body").html(messages);
        alertModal.modal('toggle');
        setTimeout(function () { alertModal.modal("hide") }, s);
    }

    //属性设置
    attributeModal.on("hidden", function () {
        $(this).removeData("modal");//移除数据，防止缓存
    });
    ajaxModal = function (url, fn) {
        url += url.indexOf('?') ? '&' : '?';
        url += '_t=' + new Date().getTime();
        attributeModal.modal({
            remote: url
        })
        //加载完成执行
        if (fn) {
            attributeModal.on('shown', fn);
        }
    }

    function page_reload() {
        location.reload();
    }
    /*步骤数据*/
    var processData = getFlowJson();
    if (processData == null) {
        //新建流程默认保存发起人、结束节点
        $.post('/Flow/Flow/LoadingNode', { FlowCode: _flowCode }, function (result) { });
        processData = {
            "list": [
                { "id": "0", "flow_id": "" + _flowCode + "", "icon": "icon-00", "process_name": "开始", "process_to": "9999", "style": "left:100px;top:68px;text-align:center;" },
                { "id": "9999", "flow_id": "" + _flowCode + "", "icon": "icon-9999", "process_name": "结束", "process_to": "", "style": "left:660px;top:68px;" }
            ]
        }
    }

    /*创建流程设计器*/
    var _canvas = $("#flowdesign_canvas").Flowdesign({
        "processData": processData,
        canvasMenus: {
            "fresh": function (t) {
                Refresh();
            }
        }
        /*步骤右键*/
        , processMenus: {
            "addson": function (t) {
                var activeId = _canvas.getActiveId();//右键当前的ID
                if (activeId != '9999') {
                    var mLeft = t.offsetLeft, mTop = t.offsetTop + 160;
                    var ID = parseInt(activeId) + 1;
                    layer.open({
                        type: 2,
                        title: "新增节点",
                        //area: ['820px', $(top.document).height() - 30 + "px"],
                        area: ['80%', '80%'],
                        content: "/WorkFlow/Flow/FlowNodeDefineNew?FormCode=" + _formCode,
                        btn: ['确定', '关闭'],
                        yes: function (index, layero) {
                            debugger;
                            var Result = layero.find("iframe")[0].contentWindow.BackData();
                            Result["NodeLeft"] = mLeft;
                            Result["NodeTop"] = mTop;
                            Result["CurrentNode"] = activeId;
                            Result["ID"] = ID;
                            Result["FlowCode"] = _flowCode;
                            $.post('/Flow/Flow/AddNodeNew', { NodeInfo: JSON.stringify(Result) }, function (data) {
                                if ($.parseJSON(data).state == "success") {
                                    mAlert("保存成功", 5000);
                                    Refresh();
                                }
                                else {
                                    mAlert("保存失败", 10000);
                                }
                            });
                        }, cancel: function (index) { }
                    });
                }
                else {
                    mAlert("结束节点不能添加子节点", 5000);
                }
            },
            "delete": function (t) {
                var activeId = _canvas.getActiveId();//右键当前的ID
                if (activeId == 0 || activeId == 9999) {
                    mAlert("不允许删除该节点", 10000);
                }
                else {
                    $("#confirm").modal('toggle');
                    $("#cancelOK").bind('click', function () {
                        $.post('/Flow/Flow/DeleteNodeNew', { FlowCode: _flowCode, FlowNodeCode: activeId }, function (data) {
                            if ($.parseJSON(data).state == "success") {
                                Refresh();
                            }
                            else {
                                mAlert("删除失败", 10000);
                            }
                        });
                    });
                }
            },
            "attribute": function (t) {
                var flownodecode = _canvas.getActiveId();
                var mLeft = t.offsetLeft, mTop = t.offsetTop + 160;
                layer.open({
                    type: 2,
                    title: "节点定义",
                    //area: ['820px', $(top.document).height() - 30 + "px"],
                    area: ['95%', '95%'],
                    content: "/WorkFlow/Flow/FlowNodeDefineNew?type=edit&FlowCode=" + _flowCode + "&FlowNodeCode=" + flownodecode + "&FormCode=" + _formCode,
                    btn: ['确定', '关闭'],
                    yes: function (index, layero) {
                        var Result = layero.find("iframe")[0].contentWindow.BackData();
                        Result["NodeLeft"] = mLeft;
                        Result["NodeTop"] = mTop;
                        Result["CurrentNode"] = flownodecode;
                        Result["ID"] = flownodecode;
                        Result["FlowCode"] = _flowCode;
                        $.post('/Flow/Flow/UpdateNodeNew', { NodeInfo: JSON.stringify(Result) }, function (data) {
                            if ($.parseJSON(data).state == "success") {
                                mAlert("更新成功", 10000);
                                Refresh();
                            }
                            else {
                                mAlert("更新失败", 10000);
                            }
                        });
                    }, cancel: function (index) { }
                });
            },
            //"where": function (t) {
            //    var flownodecode = _canvas.getActiveId();
            //    $.post("/WorkFlow/Flow/MathNodeJudgeCriter", { FlowCode: _flowCode, FlowNodeCode: flownodecode }, function (data) {
            //        var json_result = JSON.parse(data);
            //        if (json_result.result == "True") {
            //            layer.open({
            //                type: 2,
            //                title: "节点条件",
            //                area: ['820px', $(top.document).height() - 30 + "px"],
            //                content: "/WorkFlow/Flow/FlowNodeJudgeCriteria?FlowNodeCode=" + flownodecode + "&FlowCode=" + _flowCode,
            //                btn: ['确定', '关闭'],
            //                yes: function (index, layero) {
            //                    var Result = layero.find("iframe")[0].contentWindow.BackData();
            //                    Result["FlowNodeCode"] = flownodecode;
            //                    $.post("/WorkFlow/Flow/AddJudgeCriteria", { JcInfo: JSON.stringify(Result) }, function (data) {
            //                        Refresh();
            //                    });
            //                    layer.close(index);
            //                }, cancel: function (index) { }
            //            });
            //        }
            //        else {
            //            mAlert("该节点没有平行节点，不能设置节点执行条件", 10000);
            //        }
            //    });
            //},
            "error": function (t) {
                var flownodecode = _canvas.getActiveId();
                var mLeft = t.offsetLeft, mTop = t.offsetTop + 160;
                layer.open({
                    type: 2,
                    title: "预警条件",
                    area: ['95%', '95%'],
                    content: "/WorkFlow/Flow/FlowNodeEarlyWarningCondition?FlowCode=" + _flowCode + "&FlowNodeCode=" + flownodecode,
                    btn: ['关闭'],
                    cancel: function (index) { }
                });
            }
        }
        , fnRepeat: function () {
            mAlert("步骤连接重复，请重新连接");
        }
        , fnClick: function () {

        }
        //双击处理事件
        , fnDbClick: function (nodeid) {
        }
    });

    _globalcanvas = _canvas;
    _processData = processData;

    /*保存*/
    $("#leipi_save").bind('click', function () {
        SaveNodeInfo(_canvas, true);
    });
    /*清除*/
    $("#leipi_clear").bind('click', function () {
        if (_canvas.clear()) {
            mAlert("清空连接成功，请重新设置连接。");
        } else {
            mAlert("清空连接失败");
        }
    });
});

//获取流程图JSON字符串
function getFlowJson() {
    var flowjson = "";
    $.ajax({
        type: "POST",
        url: '/Flow/Flow/GetNodeList',
        data: { FlowCode: _flowCode },
        async: false, //设为false就是同步请求
        cache: false,
        success: function (data) {
            var json = eval(data);
            //对获取的字段进行拼接
            for (var i in json) {
                if (json.length == 1) {
                    flowjson += '"list": {"' + json[i].FlowNodeCode + '": { "id": "' + json[i].FlowNodeCode + '", "flow_id": "' + json[i].FlowCode + '", "icon": "' + json[i].icon + '", "process_name": "' + json[i].FlowNodeName + '", "process_to": "' + json[i].processData + '", "style": "left:' + json[i].NodeLeft + 'px;top:' + json[i].NodeTop + 'px;color:' + json[i].background + '" }}';
                }
                else if (i == 0) {
                    flowjson += '"list": {"' + json[i].FlowNodeCode + '": { "id": "' + json[i].FlowNodeCode + '", "flow_id": "' + json[i].FlowCode + '", "icon": "' + json[i].icon + '", "process_name": "' + json[i].FlowNodeName + '", "process_to": "' + json[i].processData + '", "style": "left:' + json[i].NodeLeft + 'px;top:' + json[i].NodeTop + 'px;color:' + json[i].background + '" },';
                }
                    //如果是最后一个
                else if (i == json.length - 1) {
                    flowjson += '"' + json[i].FlowNodeCode + '": { "id": "' + json[i].FlowNodeCode + '", "flow_id": "' + json[i].FlowCode + '", "icon": "' + json[i].icon + '", "process_name": "' + json[i].FlowNodeName + '", "process_to": "' + json[i].processData + '", "style": "left:' + json[i].NodeLeft + 'px;top:' + json[i].NodeTop + 'px;color:' + json[i].background + '" }}';
                }
                else {
                    flowjson += '"' + json[i].FlowNodeCode + '": { "id": "' + json[i].FlowNodeCode + '", "flow_id": "' + json[i].FlowCode + '", "icon": "' + json[i].icon + '", "process_name": "' + json[i].FlowNodeName + '", "process_to": "' + json[i].processData + '", "style": "left:' + json[i].NodeLeft + 'px;top:' + json[i].NodeTop + 'px;color:' + json[i].background + '" },';
                }
            }
        }
    });
    if (flowjson == "") {
        return null;
    } else {
        return $.parseJSON("{" + flowjson + "}");
    }
}


//刷新界面方法
function Refresh() {
    location.reload();
}

//删除连线后，删除连线关系数据
function FlowNodeDelete(stratNode, endNode) {
    var processData = "";
    //获取起始节点信息，取出当前节点的下级数据用于更新
    $.post('@Url.Action("GridListApi", "Flow", new { FunCode = "GetFlowLineStart" })', { FlowCode: _flowCode, FlowNodeCode: stratNode.replace("window", "") }, function (data) {
        var s = eval(data)[0].processData.split(',');
        for (var i = 0; i < s.length; i++) {
            if (s[i] != endNode.replace("window", "")) {
                processData += s[i] + ",";
            }
        }
        processData = processData.substring(0, processData.length - 1);
        //更新起始节点下级关系，删除关系表对应关系
        $.post('@Url.Action("ItemCUD", "Flow", new { FunCode = "FlowLineDelete" })', { FlowCode: _flowCode, ParentNodeCode: stratNode.replace("window", ""), ChildNodeCode: endNode.replace("window", ""), processData: processData, FlowNodeCode: stratNode.replace("window", "") }, function (data) {
            //alert(data);
        });
    });
    //alert(stratNode.replace("window", "") + "      " + endNode.replace("window", ""));
}

//连线之后，添加节点关系数据
function FlowLineAdd(stratNode, endNode) {
    stratNode = stratNode.replace("window", "");
    endNode = endNode.replace("window", "");
    var processData = "";
    //获取起始节点信息，取出当前节点的下级数据用于更新
    $.post('/Flow/Flow/LineAdd', { Para: JSON.stringify({ FlowCode: _flowCode, StartNode: stratNode, EndNode: endNode }) }, function (data) {

    });
}

//保存节点位置信息
function SaveNodeInfo(_canvas, _flag) {
    $.post('/Flow/Flow/UpdataFlowNodeUI', { FlowCode: _flowCode, processInfo: _canvas.getProcessInfo() }, function (data) {
        var result = $.parseJSON(data);
        if (result.state != "success") {
            $("#alertModal").find(".modal-body").html(result.message);
            $("#alertModal").modal('toggle');
        }
        else {
            $("#alertModal").find(".modal-body").html(result.message);
            $("#alertModal").modal('toggle');
            setTimeout(function () { $("#alertModal").modal("hide") }, 5000);
        }
    });
}

//删除连线处理方法
function btnDeleteLine() {
    $("#winMenu").window('close');
    $.messager.confirm('确认', '你确定删除连接吗？', function (r) {
        if (r) {
            FlowNodeDelete(_lineInfo.sourceId, _lineInfo.targetId);
            jsPlumb.detach(_lineInfo);
        }
    });
}
//驳回的连线方法
//startId:连线开始节点ID
//endId:连线结束节点ID
//labelInfo:连线上label显示内容
function RejectedConnection(startId, endId, labelInfo) {
    jsPlumb.connect({
        source: startId,
        target: endId,
        label: labelInfo
    });
}
////通过类型取部门 
////加工厂：1 经理部：2 分部：3 工区：4 工点：5
//function GetDepartment(orgType)
//{
//    $("#Loading").modal('toggle');
//    if (orgType == "" || orgType == null || orgType == undefined) { orgType = 2;}
//    $.ajax({
//        type: 'POST',
//        url: '/Flow/Flow/GetDepartmentByOrgType',
//        data: { OrgType: orgType },
//        async: false,
//        cache: false,
//        success: function (data) {
//            $("#Loading").modal('hide');
//            $("#departmentlist  tr:not(:first)").html("");
//            $("#rolelist  tr:not(:first)").html("");
//            var str_json = JSON.parse(data);
//            if (str_json.length > 0) {
//                for (var i = 0; i < str_json.length; i++) {
//                    var tr = "";
//                    tr += '<tr id="' + str_json[i].DepartmentCode + '" onclick="getRole(this)" class="odd gradeX">';
//                    tr += '<td><input type="hidden" value="' + str_json[i].DepartmentCode + '">' + str_json[i].DepartmentName + '</td>';
//                    tr += "</tr>";
//                    $("#departmentlist").append(tr);
//                }
//            }
//            else {
//                var tr = "";
//                tr += '<tr class="odd gradeX">';
//                tr += '<td >暂无相关数据</td>';
//                tr += "</tr>";
//                $("#departmentlist").append(tr);
//            }
//        }
//    });
//}
////通过部门编码获取部门下相应的角色
//function getRole(tr)
//{
//    var dcode = tr.id;
//    $.ajax({
//        type: 'POST',
//        url: '/Flow/Flow/GetRoleByDepCode',
//        data: { DepCode: dcode },
//        async: false,
//        cache: false,
//        success: function (data) {
//            var role = JSON.parse(data);
//            $("#rolelist  tr:not(:first)").html("");
//            if (role.length > 0) {
//                for (var i = 0; i < role.length; i++) {
//                    var tr = "";
//                    tr += '<tr class="odd gradeX">';
//                    tr += '<td><input type="checkbox" onclick="SureRole(this)" name="' + role[i].RoleName + '" class="checkboxes" value="' + role[i].RoleCode + '" /></td>';
//                    tr += '<td>' + role[i].RoleName + '</td>';
//                    tr += "</tr>";
//                    $("#rolelist").append(tr);
//                }
//            }
//            else {
//                var tr = "";
//                tr += '<tr class="odd gradeX">';
//                tr += '<td  colspan="2">暂无相关数据</td>';
//                tr += "</tr>";
//                $("#rolelist").append(tr);
//            }
//        }
//    });
//}

//function SureRole(r)
//{
//    var RoleID = r.value;
//    if (r.checked) {
//        var RoleIds = $("#DataID").val();
//        var RoleName = $("#rolenum").html();
//        if (RoleIds.length > 0)
//        {
//            RoleIds + ","+RoleID;
//        }
//        else
//        {
//            RoleIds += RoleID;
//        }

//        if (RoleName == "未选择") {
//            $("#rolenum").html("已选择" + r.name);
//        }
//        else {
//            RoleName +="、"+r.name;
//            $("#rolenum").html(RoleName);
//        }
//        $("#DataID").val(RoleIds);
//    }
//    else {
//        var RoleName = $("#rolenum").html();
//        var RoleIds = $("#DataID").val();
//        if (RoleName.indexOf("、" + r.name) > 0)
//        {
//            RoleName = RoleName.replace("、" + r.name, "");
//        }
//        else if (RoleName.indexOf(r.name + "、") > 0) {
//            RoleName = RoleName.replace(r.name + "、", "");
//        }
//        else {
//            RoleName = RoleName.replace(r.name, "");
//        }
//        if (RoleName == "已选择")
//            $("#rolenum").html("未选择");
//        else $("#rolenum").html(RoleName);

//        if (RoleIds.indexOf("," + RoleID)>0)
//        {
//            RoleIds = RoleIds.replace("," + RoleID,"");
//        }
//        else if (RoleIds.indexOf(RoleID + ",") > 0) {
//            RoleIds = RoleIds.replace(RoleID + ",", "");
//        }
//        else {
//            RoleIds = RoleIds.replace(RoleID, "");
//        }
//        $("#DataID").val(RoleIds);

//    }
//}
//function GetParameter(activeId, ID,mLeft, mTop)
//{
//    debugger;
//    var Result = {};
//    Result["NodeLeft"] = mLeft;
//    Result["NodeTop"] = mTop;
//    Result["CurrentNode"] = activeId;
//    Result["ID"] = ID;
//    Result["FlowCode"] = _flowCode;
//    var Name = $("#FlowNodeName").val();
//    Result["Name"] = Name;
//    var AllApproval = $("#AllApproval").is(":checked") ? 1 : 0;
//    Result["AllApproval"] = AllApproval;
//    var FreeCandidates = $("#FreeCandidates").is(":checked") ? 1 : 0;
//    Result["FreeCandidates"] = FreeCandidates;
//    var BlankNode = $("#BlankNode").is(":checked") ? 1 : 0;
//    Result["BlankNode"] = BlankNode;
//    var SPList = [];


//    var DataID = $("#DataID").val().split(',');
//    var PersonnelSource;
//    if ($("#ZDR").is(":checked"))
//    { 
//        PersonnelSource = "Originator";
//        var sp = {};
//        sp["ActionType"] = "0";
//        sp["PersonnelSource"] = PersonnelSource;
//        sp["PersonnelCode"] = "0";
//        SPList.push(sp);
//    }
//    else if ($("#DL").is(":checked"))
//    {
//        PersonnelSource = "DepartmentLeader";
//        var sp = {};
//        sp["ActionType"] = "0";
//        sp["PersonnelSource"] = PersonnelSource;
//        sp["PersonnelCode"] = "0";
//        SPList.push(sp);
//    }
//    else if ($("#ROLE").is(":checked"))
//    {
//        PersonnelSource = "Role";
//        for (var i = 0; i < DataID.length; i++) {
//            var sp = {};
//            sp["ActionType"] = "0";
//            sp["PersonnelSource"] = PersonnelSource;
//            sp["PersonnelCode"] = DataID[i];
//            SPList.push(sp);
//        }
//    }
//    else if ($("#FREE").is(":checked"))
//    {
//        PersonnelSource = "Personnel";
//        for (var i = 0; i < DataID.length; i++) {
//            var sp = {};
//            sp["ActionType"] = "0";
//            sp["PersonnelSource"] = PersonnelSource;
//            sp["PersonnelCode"] = DataID[i];
//            SPList.push(sp);
//        }
//    }
//    Result["SPLIST"] = SPList;
//    //var CSID = $("#CSID").val().split(',');
//    //for (var i = 0; i < CSID.length; i++)
//    //{
//    //    var cs = {};
//    //    cs["ActionType"] = "-1";
//    //    cs["PersonnelSource"] = "Role";
//    //    cs["PersonnelCode"] = CSID[i];
//    //    SPList.push(sp);
//    //}
//    return JSON.stringify(Result);
//}
