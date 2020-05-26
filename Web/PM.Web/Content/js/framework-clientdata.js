var clients = [];
$(function () {
    
    clients = $.clientsInit();
})
$.clientsInit = function () {
    var dataJson = {
        //dataItems: [],
        //organize: [],
        //role: [],
        //duty: [],
        user: [],
        projectId: "",
        orgType: "",
        companyId:"",
        authorizeMenu: []
        //authorizeButton: []
    };
    var init = function () {
        $.ajax({
            url: "/Menu/GetClientsDataJson",
            type: "get",
            dataType: "json",
            async: false,
            success: function (data) {
                //dataJson.dataItems = data.dataItems;
                //dataJson.organize = data.organize;
                //dataJson.role = data.role;
                //dataJson.duty = data.duty;
                dataJson.user = data.user;
                dataJson.projectId = data.projectId;
                dataJson.orgType = data.orgType;
                dataJson.companyId = data.companyId;
                dataJson.authorizeMenu = eval(data.authorizeMenu);
                //dataJson.authorizeButton = data.authorizeButton;
            }
        });
    }
    init();
    return dataJson;
}