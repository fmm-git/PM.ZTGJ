//上传
function CommonUpload() {
    $.modalOpen({
        id: "Index",
        title: "附件上传",
        url: "/Attachment/Index",
        width: "640px",
        height: "460px",
        btn: null
    });

}
//下载
function CommonDownload(fileID) {
    //单个文件下载
    if (fileID != null && fileID != undefined && fileID != "") {
        var rowData = jQuery("#AttachmentList").jqGrid("getRowData", fileID);
        //window.open("/Attachment/FileDownload?FileUrl="+encodeURI(rowData.FileStoragePath) + "&FileName=" + encodeURI(rowData.FileName));
        window.open("/Attachment/FileDownload?FileID=" + encodeURI(rowData.FileID));
    } else {
        var rows = jQuery("#AttachmentList").jqGrid('getGridParam', 'selarrrow');//获取选中的所有行
        if (rows.length > 0) {
            $.each(rows, function (i, item) {
                var rowData = jQuery("#AttachmentList").jqGrid("getRowData", item);
                //window.open("/Attachment/FileDownload?FileUrl="+encodeURI(rowData.FileStoragePath) +"&FileName=" + encodeURI(rowData.FileName));
                window.open("/Attachment/FileDownload?FileID=" + encodeURI(rowData.FileID));
            });
        } else {
            $.modalMsg("未选择下载附件", "warning");
            return false;
        }
    }
}
//删除文件
function CommonDeleteFile(fileID) {
    //单个文件删除
    if (fileID != null && fileID != undefined && fileID != "") {
        var rowData = jQuery("#AttachmentList").jqGrid("getRowData", fileID);
        $.deleteForm({
            prompt: "你确定要删除此数据?为了保证数据的一致,删除之后请重新提交表单。",
            url: "/Attachment/FileDelete",
            param: { FileID: fileID, FileStoragePath: encodeURI(rowData.FileStoragePath) },
            success: function () {
                $("#AttachmentList").trigger("reloadGrid");
                getDelFileID(fileID);
            }
        });
    } else {
        var rows = jQuery("#AttachmentList").jqGrid('getGridParam', 'selarrrow');//获取选中的所有行
        if (rows.length > 0) {
            var FileIDs = "";
            var FileStoragePath = "";
            $.each(rows, function (i, item) {
                var rowData = jQuery("#AttachmentList").jqGrid("getRowData", item);
                FileIDs += item + ",";
                FileStoragePath += encodeURI(rowData.FileStoragePath) + ",";
            });
            $.deleteForm({
                prompt: "你确定要删除此数据?为了保证数据的一致,删除之后请重新提交表单。",
                url: "/Attachment/FileDelete",
                param: { FileID: FileIDs, FileStoragePath: FileStoragePath },
                success: function () {
                    $("#AttachmentList").trigger("reloadGrid");
                    getDelFileID(FileIDs);
                }
            });
        } else {
            $.modalMsg("请选择要删除的信息", "warning");
            return false;
        }
    }
}

//获取附件列表删除后的Id 绑定到页面
function getDelFileID(delFileIDs) {
    var viewFileIds = $("#Enclosure").val();
    if (viewFileIds != "" && viewFileIds != "") {
        var FileID = $("#Enclosure").val().split(",");//现保留的附件Id
        var arr = delFileIDs.split(",");//要删除的附件Id
        $(arr).each(function (index, item) {
            if (item != "") {
                FileID.splice($.inArray(item, FileID), 1);
            }
        });
        $("#Enclosure").val(FileID.join(","));
    }
}

//上传附件 返回附件Id 绑定到页面
function GetFileIDC(FileID) {
    if ($("#Enclosure").val() != "") { FileID = FileID == null ? $("#Enclosure").val() : $("#Enclosure").val() + "," + FileID; }
    $("#Enclosure").val(FileID);
    $("#AttachmentList").jqGrid('setGridParam', {
        url: "/Attachment/GetAttachmentJson",
        postData: { FileID: $("#Enclosure").val() },
    }).trigger('reloadGrid');
}


function defaultformatterNameC(value, options, cell) {
    var a = value.slice(4);
    return a;
}
function defaultformatterC(value, options, cell) {
    return '<a href="javascript:void(0);" style="color:blue" onclick="CommonDownload(\'' + value + '\')">下载</a>&nbsp;&nbsp;<a href="javascript:void(0);" style="color:blue" onclick="CommonDeleteFile(\'' + value + '\')">删除</a>'
}
function defaultformatterD(value, options, cell) {
    return '<a href="javascript:void(0);" style="color:blue" onclick="CommonDownload(\'' + value + '\')">下载</a>'
}

//转换文件大小
function conver(value, options, cell) {
    var size = "";
    if (value < 0.1 * 1024) { //如果小于0.1KB转化成B
        size = value.toFixed(2) + "B";
    } else if (value < 0.1 * 1024 * 1024) {//如果小于0.1MB转化成KB
        size = (value / 1024).toFixed(2) + "KB";
    } else if (value < 0.1 * 1024 * 1024 * 1024) { //如果小于0.1GB转化成MB
        size = (value / (1024 * 1024)).toFixed(2) + "MB";
    } else { //其他转化成GB
        size = (value / (1024 * 1024 * 1024)).toFixed(2) + "GB";
    }

    var sizestr = size + "";
    var len = sizestr.indexOf("\.");
    var dec = sizestr.substr(len + 1, 2);
    if (dec == "00") {//当小数点后为00时 去掉小数部分
        return sizestr.substring(0, len) + sizestr.substr(len + 3, 2);
    }
    return sizestr;
}


//----------------新附件上传-------------------//

//生成附件GUID
function generateUUID() {
    var d = new Date().getTime();
    var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = (d + Math.random() * 16) % 16 | 0;
        d = Math.floor(d / 16);
        return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
    return uuid;
};

//上传附件
function UplaodFile(fileId, DataId, menuTable) {
    var FJ;
    if (!fileId) {
        FJ = generateUUID();
    }
    else {
        FJ = fileId;
    }
    //var fun = function (data) {
    //    if (data.length) {
    //        top.frames["Form"].$("#uplaodFileTitle").html("已上传");
    //        top.frames["Form"].$("#uplaodFilelook").show();
    //        top.frames["Form"].$("#Enclosure").val(data[0].keyID);
    //    }
    //}
    $.modalOpen({
        id: "Upload",
        title: "文件上传",
        //url: "/Attachment/Upload?keyID=" + FJ + "&fun=" + escape(fun.toString()) + "&DataId="+DataId+"&menuTable=" + menuTable,
        url: "/Attachment/Upload?keyID=" + FJ + "&DataId=" + DataId + "&menuTable=" + menuTable + "&index=-111",
        width: "70%",
        height: "70%",
        btn: null
    });
}

//上传附件
function UplaodFileItem(fileId, DataId, menuTable, index) {
    var FJ;
    if (!fileId) {
        FJ = generateUUID();
    }
    else {
        FJ = fileId;
    }
    //var fun = function (data,index) {
    //    if (data.length) {
    //        top.frames["Form"].$("#uplaodFileTitle-"+index).html("已上传");
    //        top.frames["Form"].$("#uplaodFilelook-"+index).show();
    //        top.frames["Form"].$("#Enclosure-" + index).val(data[0].keyID);
    //    } 
    //}
    $.modalOpen({
        id: "Upload",
        title: "文件上传",
        url: "/Attachment/Upload?keyID=" + FJ + "&DataId=" + DataId + "&menuTable=" + menuTable + "&index=" + index,
        width: "70%",
        height: "70%",
        btn: null
    });
}
//查看文件
function showFile(fileId, type, menuTable, IsItem, index1) {
    if (!fileId) return;
    $.modalOpen({
        id: 'UploadFileList',
        title: "文件查看",
        url: "/Attachment/UploadFileList?ids=" + fileId + "&type=" + type + "&menuTable=" + menuTable + "&IsItem=" + IsItem + "&index1="+index1,
        width: "80%",
        height: "80%",
        btn: null
    });
}

//删除文件
function DelFile(id, menuTable, IsItem, index1) {
    if (id) {
        $.deleteForm({
            prompt: "你确定要删除此附件?",
            url: "/Attachment/Del",
            param: { id: id, menuTable: menuTable },
            success: function (data) {
                $.modalAlert(data.message);
                window.location.reload(true);
                if (typeof (top.frames["Form"].delEnclosure) == 'function') {
                    top.frames["Form"].delEnclosure();
                }
                var a = $("#div_file").children().length;
                if ((a - 1) <= 0) {
                    if (IsItem && menuTable == "MonthCostHeSuan") {
                        top.frames["Form"].$("#uplaodFileTitle-" + index1).html("未上传");
                        top.frames["Form"].$("#uplaodFilelook-" + index1).hide();
                        var dataItem = top.frames["Form"].$("#gridListCost #" + index1 + ">td");
                        dataItem[8].innerText ="";
                    } else {
                        top.frames["Form"].$("#Enclosure").val("");
                        top.frames["Form"].$("#uplaodFileTitle").html("未上传");
                        top.frames["Form"].$("#uplaodFilelook").hide();
                    }
                    
                }
            }
        });
    } else {
        $.modalAlert("附件信息错误！！");
    }
}

//下载附件
function DownFile(id) {
    //$.ajax({
    //    url: "/Attachment/GetDownFileModel",
    //    data: { id: id },
    //    success: function (data) {
    //        if (data) {
    //            //window.location = data;
    //            window.location.target = "_blank";
    //            window.location.href = data;
    //        } else {
    //            $.modalAlert("附件信息错误！！");
    //        }
    //    }
    //});
    window.open("/Attachment/FileDownloadNew?FileID=" + encodeURI(id));
}