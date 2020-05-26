/**
 * 扩展说明：
 *  1、maskt 显示遮罩
 *      1.1 参数说明
 *          target:     要加载遮罩的目标对象
 *          loadMsg:    遮罩显示信息
 *
 *
 *  2、unmask 关闭遮罩
 *      2.1 参数说明
 *          target:     要加载遮罩的目标对象
 *
 */
(function ($) {
    function addCss(id, content) {
        if ($('#' + id).length > 0) return;
        return $('<style>' + content + '</style>').attr('id', id).attr('type', 'text/css').appendTo('head');
    }

    $.extend({
        mask: function (opts) {
            opts = opts || {};
            var options = $.extend({}, { target: 'body', loadMsg: $.fn.datagrid.defaults.loadMsg }, opts);
            //this.unmask(options);

            if (options.target != 'body' && $(options.target).css('position') == 'static') {
                $(options.target).addClass('mask-relative');
            }

            var mask = $("<div class=\"datagrid-mask\" style=\"display:block;\"></div>").appendTo(options.target);
            var msg = $("<div class=\"datagrid-mask-msg\" style=\"display:block; left: 50%;\"></div>").html(options.loadMsg).appendTo(options.target);
            setTimeout(function () {
                msg.css("marginLeft", -msg.outerWidth() / 2);
            }, 5);

            var css = '.mask-relative {position: relative !important;}';
            addCss('mask_css', css);
        },
        unmask: function (options) {
            var target = options.target || 'body';
            $(">div.datagrid-mask-msg", target).remove();
            $(">div.datagrid-mask", target).remove();
            $(options.target).removeClass('mask-relative');
            //setTimeout(function () {
            //    $(">div.datagrid-mask-msg", target).remove();
            //    $(">div.datagrid-mask", target).remove();
            //    $(options.target).removeClass('mask-relative');
            //}, 1000);
        }
    });

    /**
 * 通用错误提示
 * 用于datagrid/treegrid/tree/combogrid/combobox/form加载数据出错时的操作
 */
    var easyuiErrorFunction = function (XMLHttpRequest) {
        if ($.messager.progress != undefined) {
            $.messager.progress('close');
        }        
        //$.messager.alert('错误提示', '错误详情(' + XMLHttpRequest.status + '):</br>' + XMLHttpRequest.responseText);
        var errMsg;
        if (XMLHttpRequest.status === 0)
        {
            errMsg = "连接服务器失败。";
        }
        else
        {
            errMsg = '错误详情(' + XMLHttpRequest.status + '):</br>' + XMLHttpRequest.responseText;
        }
        $.messager.show({
            title: '错误提示',
            msg: errMsg,
            showType: 'slide',
            style: {
                right: '',
                top: document.body.scrollTop + document.documentElement.scrollTop,
                bottom: ''
            }
        });
    };

    $.fn.form.defaults.onLoadError = easyuiErrorFunction;
    $.fn.panel.defaults.onLoadError = easyuiErrorFunction;
    $.fn.combobox.defaults.onLoadError = easyuiErrorFunction;
    $.fn.combotree.defaults.onLoadError = easyuiErrorFunction;
    $.fn.combogrid.defaults.onLoadError = easyuiErrorFunction;
    $.fn.datagrid.defaults.onLoadError = easyuiErrorFunction;
    $.fn.propertygrid.defaults.onLoadError = easyuiErrorFunction;
    $.fn.tree.defaults.onLoadError = easyuiErrorFunction;
    $.fn.treegrid.defaults.onLoadError = easyuiErrorFunction;
    $.ajaxSetup({ error: easyuiErrorFunction });
})(jQuery);
