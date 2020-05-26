//原材料名称查询
$(function () {
    $("#MaterialNameSelect").bindSelect({
        url: "/Distribution/DistributionPlan/MaterialNameSelect",
        id: "MaterialName",
        text: "MaterialName"
    });

    //原材料名称值改变事件
    $("#MaterialNameSelect").change("bind", function () {
        var MaterialNameSelect = $(this).find("option:selected").val();
        $("#MaterialName").val(MaterialNameSelect);
        $("#MaterialNames").val(MaterialNameSelect);
    })
})

//组织机构树加标签
var retLabaItemData = new Array();
function GetEachData(code, leftData) {
    //获取所有子级site
    var codeList = new Array();
    codeList.push(code);
    EachData(codeList);
    var codeTep = new Array();
    $.each(retLabaItemData, function (i, v) {
        var reportData = $.grep(leftData, function (data) {
            return data.OrgCode == v;
        });
        reportData.map((item) => codeTep.push({
            LabList: item.LabList
        }));
    });
    var tdhtml = "";
    if (codeTep.length == 0) { return tdhtml; }
    var retData = new Array();
    codeTep[0].LabList.map((item) => retData.push({
        Color: item.Color,
        Name: item.Name,
        Value: 0
    }));
    $.each(retData, function (x, j) {
        j.Value = 0;
        $.each(codeTep, function (i, v) {
            j.Value += v.LabList[x].Value
        });
    });
    tdhtml += "</br>";
    tdhtml += "<div style='text-align:center;font-size:12px;color:#AAAAAA'>";
    $.each(retData, function (i, v) {
        tdhtml += "<span>" + v.Name + " <label style='color:" + v.Color + ";'>" + v.Value + "</label></span> / ";
    });
    tdhtml = tdhtml.substring(0, tdhtml.lastIndexOf('/'));
    tdhtml += "</div>";
    return tdhtml;
}
function EachData(retCode) {
    //获取子级所有site
    var codeTep = new Array();
    $.each(retCode, function (i, v) {
        var reportData = $.grep(treeData, function (data) {
            return data.ParentCompanyCode == v;
        });
        reportData.map((item) => codeTep.push(item.CompanyCode));
    });
    if (codeTep.length == 0) {
        retLabaItemData = retCode;
        return;
    } else {
        EachData(codeTep)
    }
}