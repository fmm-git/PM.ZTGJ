(function ($) {

    var fullToolbars = ['source','undo', 'redo', 'fullscreen', 'eraser', '|', 'bold', 'italic', 'underline', 'strikethrough', 'forecolor', 'bgcolor', '|', 'quote', 'fontsize', 'indent', 'lineheight', '|', 'unorderlist', 'orderlist', '|', 'alignleft', 'aligncenter', 'alignright', '|', 'img', 'video'];//'source','|','emotion', 'fontfamily','head', 'link', 'unlink','|', 'table', 'insertcode',, 'location'

    //上传地址
    //var uploadUrl = $.BaseUrl.Img + "/uploader";
    var uploadUrl = '/Attachment/UploadFileNoBas';

    var initFunc = function (editor) {
        editor.hasContents = function () {
            var content = editor.$txt.formatText();//去除了空格 换行等
            return content.length > 0;
        }

        editor.getContentTxt = function () {
            return editor.$txt.text();
        }

        editor.getContent = function () {
            return editor.$txt.html();
        }

        editor.getEncodeContent = function () {
            return $("<div/>").text(editor.getContent()).html();
        }

        return editor;
    }

    //自定义上传
    var uploadInit = function () {
        var editor = this;
        var btnId = editor.customUploadBtnId;
        var containerId = editor.customUploadContainerId;

        $("#" + btnId).Uploader({
            container: containerId,
            fileUploaded: function (file) {
                try {
                    // 插入到编辑器中
                    editor.command(null, 'insertHtml', '<img src="' + file + '" style="max-width:100%;"/>');
                } catch (ex) {
                    // 此处可不写代码
                } finally {
                    // 隐藏进度条
                    editor.hideUploadProgress();
                }
            },
            uploadProgress: function (percent) {
                editor.showUploadProgress(percent);
            }
        });
    }

    $.fn.extend({
        PowerEditor: function (settings) {
            var defaults = {
                width: "100%",
                height: "480px"
                //maxHeight: "400px"
            }

            var options = $.extend({}, defaults, settings);
            $(this).css(options);

            wangEditor.config.printLog = false;//关闭打印log

            var editor = new wangEditor($(this).get(0));

            editor.config.menus = fullToolbars;
            //editor.config.mapAk = "";
            editor.config.menuFixed = true;// 菜单栏fixed  editor.config.menuFixed = 50;
            editor.config.zindex = 20000;
            editor.config.jsFilter = false;//关闭js过滤
            editor.config.pasteFilter = false;//取消粘贴过滤
            editor.config.pasteText = true;
            editor.config.uploadImgUrl = uploadUrl;
            editor.config.hideLinkImg = false;//隐藏网络图片

            //editor.config.customUpload = true;  // 配置自定义上传
            //editor.config.customUploadInit = uploadInit;  // 配置上传事件
            editor.config.uploadImgMaxSize = 3 * 1024 * 1024;
            editor.config.uploadImgMaxLength = 5;
            editor.config.uploadFileName = 'Filedata';
            editor.create();

            return initFunc(editor);
        }
    });
})(jQuery);