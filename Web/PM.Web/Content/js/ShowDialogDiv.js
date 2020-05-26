//高级查询
var DateTime1 = "";
$(function () {
    DateTime1 = $("#DistributionTime").val();
})

//打开高级查询窗口
function ShowDialogDiv() {
    $("#DialogDiv").slideDown();
}
//关闭高级查询窗口
function CloseDialogDiv() {
    $("#DialogDiv").slideUp();
}
