$(function () {
    //在整个正文内容DIV前面加上展开导航列表的图标
    $("#layout").before('<i class="fa fa-chevron-right" id="icon_right" style="position: absolute; z-index: 999; margin-top: 21px; cursor: pointer;position:fixed; display:none" title="展开导航列表"></i>');

    //在左侧导航列表的右边加上收起导航列表的图标
    $(".ui-layout-west").before('<i class="fa fa-chevron-left" id="icon_left" style="position:absolute; z-index:999; margin-top:12px;margin-left:170px; cursor:pointer" title="收起导航列表"></i>');

    //右侧收起按钮鼠标移入事件
    $("#icon_left").click("bind", function () {
        $(this).css("display", "none");
        $(".ui-layout-west").animate({ display: 'none', opacity: '0' }, 'slow', function () {
            if (typeof charreflow == 'function') {
                charreflow();
            }
        });//左侧导航表格隐藏
        $("#icon_right").css("display", "block");//显示展开导航列表图标
        $(".ui-layout-center").animate({ left: "0px", width: "100%" }, "slow");//Jqgrid表格宽度变为100%
        $("#gridList").setGridWidth($(window).width());//jqgrid表格随窗口大小改变而改变
    })

    //左侧展开按钮鼠标移出事件
    $("#icon_right").click("bind", function () {
        $.reload();//因左侧导航列表在隐藏期间，正文jqgrid表格数据可能已发生改变，所以调用刷新事件，获取最新数据
    })
})
