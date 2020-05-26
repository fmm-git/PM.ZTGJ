//根据分部编号联动查询工区信息
function Workarea() {
    var BranchCode = $("#BranchCode").val();
    if (BranchCode == "" || BranchCode == null) {
        $.modalMsg("您好，请先选择分部信息！", "warning");
    }
    else {
        selectClick('win_workarea', '/RawMaterial/RMProductionMaterial/WorkAreaNameCodeSelect&keyValue=BranchCode/' + BranchCode, 'Grid', 'WorkAreaCode', 'BranchCode', 'WorkAreaCode,WorkAreaName', '50%', '450px')
    }
}

//根据工区编号联动查询站点信息
function Site() {
    var WorkAreaCode = $("#WorkAreaCode").val();
    if (WorkAreaCode == "" || WorkAreaCode == null) {
        $.modalMsg("您好，请先选择工区信息！", "warning");
    }
    else {
        selectClick('win_Site', '/RawMaterial/RMProductionMaterial/SiteNameCodeSelect&keyValue=WorkAreaCode/' + WorkAreaCode, 'Grid', 'SiteCode', '', 'SiteCode,SiteName', '50%', '450px')
    }
}

//根据站点编号联动查询加工厂信息
function ProcessingFactory() {
    var SiteCode = $("#SiteCode").val();
    if (SiteCode == "" || SiteCode == null) {
        $.modalMsg("您好，请先选择站点信息！", "warning");
    }
    else {
        selectClick('win_ProcessingFactoryCode', '/RawMaterial/RMProductionMaterial/ProcessingFactoryNameCodeSelect&keyValue=SiteCode/' + SiteCode, 'Grid', 'ProcessFactoryCode', '', 'ProcessFactoryCode,ProcessFactoryName', '50%', '450px')
    }
}