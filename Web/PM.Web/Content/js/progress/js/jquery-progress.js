; (function () {
    $.fn.startProgress = function (data) {
        /*初始化*/
        var ele = this;
        ele.append('<div style="color:blue;font-size:14px;padding-left:50px;padding-top:30px;">订单编号:'+data.OrderCode+'</div>');
        //ele.append('<div class="yixif">'
        //+ '<span class="yixiaof">订单接收</span><br/>'
        //+ '<img src="/Content/js/progress/images/position.png" class="dingwei"></div>'
        //+ '<div class="xianpa">'
        //+ '<div class="hengxian">'
        //+ '<div class="hongxian"></div>'
        //+ '</div></div>'
        //+ '<div class="flexbox djwai"></div>');
        if (data.hasPay == "-1") {
            ele.append('<div class="yixif">'
            + '<span class="yixiaof"></span><br/>'
            + '</div>'
            + '<div class="xianpa">'
            + '<div class="hengxian">'
            + '<div class="hongxian"></div>'
            + '</div></div>'
            + '<div class="flexbox djwai"></div>');
        } else {
            ele.append('<div class="yixif">'
            + '<span class="yixiaof">订单接收</span><br/>'
            + '<img src="/Content/js/progress/images/position.png" class="dingwei"></div>'
            + '<div class="xianpa">'
            + '<div class="hengxian">'
            + '<div class="hongxian"></div>'
            + '</div></div>'
            + '<div class="flexbox djwai"></div>');
        }
        if (data && data.nodes && (data.hasPay || data.hasPay == 0)) {
            /*currentNode当前节点数;overprogress超所在的区间下限进度;temp临时变量;redLine进度条长度占比;*/
            var hStr = '', currentNode = 0, overProgress = 0, temp = 0, redLine = 0;
            /*构造节点数据*/
            for (var node in data.nodes) {
                if (data.nodes[node].progress <= data.hasPay) {
                    currentNode += 1;
                    temp = data.nodes[node].progress;
                }
                overProgress = data.hasPay - temp;
                var data1 = data.nodes[node].progressDate;
                if (data.nodes[node].progressDate == null || data.nodes[node].progressDate == undefined) {
                    data1 = "";
                }
                hStr = '<div class="yifen dengji ' + (node >= 0 ? 'gray' : '') + '" onclick="OnClickType(\'' + data.nodes[node].name + '\',\'' + data.OrderCode+'\')">'
                    + '<img src="/Content/js/progress/images/juyuandian.png">'
                    + '<div class="jucol">' + data.nodes[node].name + '</div>'
                    + '<div class="jucol">' + data1 + '</div>'
                    + '</div>';
                ele.find(".djwai").append(hStr);
            }
            /*计算进度条位置*/
            if (currentNode && currentNode > 0 && currentNode <= data.nodes.length) {
                if (currentNode == data.nodes.length) {
                    redLine = 100;
                } else {
                    redLine = (currentNode - 1) / (data.nodes.length - 1);
                    redLine += (overProgress / (data.nodes[currentNode].progress
                        - data.nodes[currentNode - 1].progress)) * (1 / (data.nodes.length - 1));
                    redLine = redLine * 100;
                    redLine = redLine > 100 ? 100 : redLine;
                    redLine = redLine < 0 ? 0 : redLine;
                }
            }
            /*进度条滑动特效*/
            var t = 1000, i = 0, speedup = 14, startspeed = 1, currentProgress = 0;
            setTimeout(function () {
                var timer = setInterval(function () {
                    if (i <= 500) {
                        i += startspeed + ((i / 500) * speedup);
                    } else if (i > 500) {
                        i += startspeed + speedup - +(((i - 500) / 500) * speedup);
                    }
                    currentProgress = parseInt(i * data.progressnew / t);
                    if (i >= t) { i = t; currentProgress = data.progressnew; }
                    setPosition((redLine * (i / t)), currentProgress);
                    if (i >= t) { clearInterval(timer); }
                }, 16.67);
            }, 1000);
        }
        /*设置进度条当前位置*/
        function setPosition(p, progress) {
            for (var n in data.nodes) {
                if (p / 100 >= n / (data.nodes.length - 1)) {
                    $(".yifen").eq(n).removeClass('gray');
                }
            }
            ele.find(".dingwei").css("left", p + '%');
            ele.find(".hongxian").css("width", p + '%');
            ele.find(".yixiaof").css("left", p + '%');
            if (data.hasname == "订单接收") {
                ele.find(".yixiaof").text('已接收' + progress + '%');
            }
            if (data.hasname == "领料完成") {
                ele.find(".yixiaof").text('已领料' + progress + '%');
            }
            else if (data.hasname == "加工完成") {
                ele.find(".yixiaof").text('已加工' + progress + '%');
            }
            else if (data.hasname == "配送完成") {
                ele.find(".yixiaof").text('已配送' + progress + '%');
            }
            else if (data.hasname == "签收完成") {
                ele.find(".yixiaof").text('已签收' + progress + '%');
            }
        }
    }
})();
//; (function () {
//    $.fn.startProgress = function (data) {
//        /*初始化*/
//        var ele = this;
//        ele.append('<div class="yixif">'
//                   + '<span class="yixiaof">订单接收</span><br/>'
//                   + '<img src="/Content/js/progress/images/position.png" class="dingwei"></div>'
//                   + '<div class="xianpa">'
//                   + '<div class="hengxian">'
//                   + '<div class="hongxian"></div>'
//                   + '</div></div>'
//                   + '<div class="flexbox djwai"></div>');
//        if (data && data.nodes && data.hasPay) {
//            /*currentNode当前节点数;overProgress超所在的区间下限额度;temp临时变量;redLine进度条长度占比;*/
//            var hStr = '', currentNode = 0, overProgress = 0, temp = 0, redLine = 0;
//            /*构造节点数据*/
//            for (var node in data.nodes) {
//                if (data.nodes[node].progress <= data.hasPay) {
//                    currentNode += 1;
//                    temp = data.nodes[node].progress;
//                }
//                overProgress = data.hasPay - temp;
//                hStr = '<div class="yifen dengji ' + (node > 0 ? 'gray' : '') + '">'
//                    + '<img src="/Content/js/progress/images/juyuandian.png">'
//                    + '<div class="jucol">' + data.nodes[node].name + '</div>'
//                    + '<div class="jucol">' + data.nodes[node].progressDate + '</div>'
//                    + '</div>';
//                ele.find(".djwai").append(hStr);
//            }
//            /*计算进度条位置*/
//            if (currentNode && currentNode > 0 && currentNode <= data.nodes.length) {
//                if (currentNode == data.nodes.length) {
//                    redLine = 25;
//                } else {
//                    redLine = (currentNode - 1) / (data.nodes.length - 1);
//                    redLine += (overProgress / (data.nodes[currentNode].progress
//                        - data.nodes[currentNode - 1].progress)) * (1 / (data.nodes.length - 1));
//                    redLine = redLine * 25;
//                    redLine = redLine > 25 ? 25 : redLine;
//                    redLine = redLine < 0 ? 0 : redLine;
//                }
//            }
//            setPosition(redLine, data.hasPay);
//        }
//        /*设置进度条当前位置*/
//        function setPosition(p, progress) {
//            for (var n in data.nodes) {
//                if (p / 100 >= n / (data.nodes.length - 1)) {
//                    $(".yifen").eq(n).removeClass('gray');
//                }
//            }
//            ele.find(".dingwei").css("left", p + '%');
//            ele.find(".hongxian").css("width", p + '%');
//            ele.find(".yixiaof").css("left", p + '%');
//            if (data.hasname == "订单接收") {
//                ele.find(".yixiaof").text('已领料' + progress + '%');
//            }
//            else if (data.hasname == "领料完成") {
//                ele.find(".yixiaof").text('已加工' + progress + '%');
//            }
//            else if (data.hasname == "加工完成") {
//                ele.find(".yixiaof").text('已配送' + progress + '%');
//            }
//            else if (data.hasname == "签收确认") {
//                ele.find(".yixiaof").text('已签收' + progress + '%');
//            }
//        }
//    }
//})();