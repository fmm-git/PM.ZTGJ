﻿(function ($) {
    var defaults = {
        processData: {},//步骤节点数据
   
        /*右键菜单样式*/
        menuStyle: {
            border: '1px solid #5a6377',
            minWidth: '150px',
            padding: '5px 0'
        },
        processMenus: {
            "one": function (t) { alert('步骤右键') }
        },
        fnDbClick: function (nodeid) {
            alert("双击");
        },
        itemStyle: {
            fontFamily: 'verdana',
            color: '#333',
            border: '0',
            /*borderLeft:'5px solid #fff',*/
            padding: '5px 40px 5px 20px'
        },
        itemHoverStyle: {
            border: '0',
            /*borderLeft:'5px solid #49afcd',*/
            color: '#fff',
            backgroundColor: '#5a6377'
        },
        //这是连接线路的绘画样式
        connectorPaintStyle: {
            lineWidth: 2,
            strokeStyle: "#49afcd",
            joinstyle: "round"
        },
        //鼠标经过样式
        connectorHoverStyle: {
            lineWidth: 3,
            strokeStyle: "#da4f49"
        }

    };/*defaults end*/

    var initEndPoints = function () {
        $(".process-flag").each(function (i, e) {
            var p = $(e).parent();
            jsPlumb.makeSource($(e), {
                parent: p,
                anchor: "Continuous",
                endpoint: ["Dot", { radius: 1 }],
                connector: ["Flowchart", { stub: [5, 5] }],
                connectorStyle: defaults.connectorPaintStyle,
                hoverPaintStyle: defaults.connectorHoverStyle,
                dragOptions: {},
                maxConnections: -1
            });
        });
    }

    /*设置隐藏域保存关系信息*/
    var aConnections = [];
    var setConnections = function (conn, remove) {
        if (!remove) aConnections.push(conn);
        else {
            var idx = -1;
            for (var i = 0; i < aConnections.length; i++) {
                if (aConnections[i] == conn) {
                    idx = i; break;
                }
            }
            if (idx != -1) aConnections.splice(idx, 1);
        }
        if (aConnections.length > 0) {
            var s = "";
            for (var j = 0; j < aConnections.length; j++) {
                var from = $('#' + aConnections[j].sourceId).attr('process_id');
                var target = $('#' + aConnections[j].targetId).attr('process_id');
                s = s + "<input type='hidden' value=\"" + from + "," + target + "\">";
            }
            $('#leipi_process_info').html(s);
        } else {
            $('#leipi_process_info').html('');
        }
        jsPlumb.repaintEverything();//重画
    };

    /*Flowdesign 命名纯粹为了美观，而不是 formDesign */
    $.fn.Flowdesign = function (options) {
        var _canvas = $(this);
        //右键步骤的步骤号
        _canvas.append('<input type="hidden" id="leipi_active_id" value="0"/><input type="hidden" id="leipi_copy_id" value="0"/>');
        _canvas.append('<div id="leipi_process_info"></div>');


        /*配置*/
        $.each(options, function (i, val) {
            if (typeof val == 'object' && defaults[i])
                $.extend(defaults[i], val);
            else
                defaults[i] = val;
        });
        /*画布右键绑定*/
        var contextmenu = {
            bindings: defaults.canvasMenus,
            menuStyle: defaults.menuStyle,
            itemStyle: defaults.itemStyle,
            itemHoverStyle: defaults.itemHoverStyle
        }
        $(this).contextMenu('canvasMenu', contextmenu);

        jsPlumb.importDefaults({
            DragOptions: { cursor: 'pointer' },
            EndpointStyle: { fillStyle: '#225588' },
            Endpoint: ["Dot", { radius: 1 }],
            ConnectionOverlays: [
                ["Arrow", { location: 1 }],
                ["Label", {
                    location: 0.1,
                    id: "label",
                    cssClass: "aLabel"
                }]
            ],
            Anchor: 'Continuous',
            ConnectorZIndex: 5,
            HoverPaintStyle: defaults.connectorHoverStyle
        });
        if ($.browser.msie && $.browser.version < '9.0') { //ie9以下，用VML画图
            jsPlumb.setRenderMode(jsPlumb.VML);
        } else { //其他浏览器用SVG
            jsPlumb.setRenderMode(jsPlumb.SVG);
        }


        //初始化原步骤
        var lastProcessId = 0;
        var processData = defaults.processData;
        if (processData.list) {
            $.each(processData.list, function (i, row) {

                var nodeDiv = document.createElement('div');
                var nodeId = "window" + row.id, badge = 'badge-inverse', icon = 'icon-star';
                if (lastProcessId == 0)//第一步
                {
                    badge = 'badge-info';
                    icon = 'icon-play';
                }
                if (row.icon) {
                    icon = row.icon;
                }
                $(nodeDiv).attr("id", nodeId)
                .attr("style", row.style)
                .attr("process_to", row.process_to)
                .attr("process_id", row.id)
                .addClass("process-step btn btn-small")
                .html(row.process_name + '<br/><span class="process-flag badge ' + badge + '" style="background:#f5f5f5"><i class="' + icon + ' icon-white"></i></span>&nbsp;')
                .mousedown(function (e) {
                    if (e.which == 3) { //右键绑定
                        _canvas.find('#leipi_active_id').val(row.id);
                        contextmenu.bindings = defaults.processMenus
                        $(this).contextMenu('processMenu', contextmenu);
                    }
                });
                _canvas.append(nodeDiv);
                //索引变量
                lastProcessId = row.id;
            });//each
        }


        var timeout = null;
        //点击或双击事件,这里进行了一个单击事件延迟，因为同时绑定了双击事件
        $(".process-step").live('dblclick', function () {
            clearTimeout(timeout);
            defaults.fnDbClick(this.id);
        });

        initEndPoints();

        jsPlumb.makeTarget(jsPlumb.getSelector(".process-step"), {
            dropOptions: { hoverClass: "hover", activeClass: "active" },
            anchor: "Continuous",
            maxConnections: -1,
            endpoint: ["Dot", { radius: 1 }],
            paintStyle: { fillStyle: "#ec912a", radius: 1 },
            hoverPaintStyle: this.connectorHoverStyle,
            beforeDrop: function (params) {
                var j = 0;
                $('#leipi_process_info').find('input').each(function (i) {
                    var str = $('#' + params.sourceId).attr('process_id') + ',' + $('#' + params.targetId).attr('process_id');
                    if (str == $(this).val()) {
                        j++;
                        return;
                    }
                })
                if (j > 0) {
                    defaults.fnRepeat();
                    return false;
                } else {
                    return true;
                }
            }
        });
        //reset  start
        var _canvas_design = function () {

            //连接关联的步骤
            $('.process-step').each(function (i) {
                var id = $(this).attr('process_id');
                var nodeId = "window" + id;
                var prcsto = $(this).attr('process_to');
                var toArr = prcsto.split(",");
                $.each(toArr, function (j, n) {
                    if (n != '' && n != 0) {
                        jsPlumb.connect({
                            source: nodeId,
                            target: "window" + n
                            /* ,labelStyle : { cssClass:"component label" }
                             ,label : id +" - "+ n*/
                        });
                    }
                })
            });
        }//_canvas_design end reset 
        _canvas_design();

        //-----外部调用----------------------
        //flowdesign_canvas 不传也能获取？
        var Flowdesign = {
            getActiveId: function () {
                return _canvas.find("#leipi_active_id").val();
            },
            copy: function (active_id) {
                if (!active_id)
                    active_id = _canvas.find("#leipi_active_id").val();

                _canvas.find("#leipi_copy_id").val(active_id);
                return true;
            },
            paste: function () {
                return _canvas.find("#leipi_copy_id").val();
            },
            getProcessInfo: function () {
                try {
                    /*连接关系*/
                    var aProcessData = {};
                    $("#leipi_process_info input[type=hidden]").each(function (i) {
                        var processVal = $(this).val().split(",");
                        if (processVal.length == 2) {
                            if (!aProcessData[processVal[0]]) {
                                aProcessData[processVal[0]] = { "top": 0, "left": 0, "process_to": [] };
                            }
                            aProcessData[processVal[0]]["process_to"].push(processVal[1]);
                        }
                    })
                    /*位置*/
                    _canvas.find("div.process-step").each(function (i) { //生成Json字符串，发送到服务器解析
                        if ($(this).attr('id')) {
                            var pId = $(this).attr('process_id');
                            var pLeft = parseInt($(this).css('left'));
                            var pTop = parseInt($(this).css('top'));
                            if (!aProcessData[pId]) {
                                aProcessData[pId] = { "top": 0, "left": 0, "process_to": [] };
                            }
                            aProcessData[pId]["top"] = pTop;
                            aProcessData[pId]["left"] = pLeft;

                        }
                    })
                    return JSON.stringify(aProcessData);
                } catch (e) {
                    return '';
                }

            },
            clear: function () {
                try {

                    jsPlumb.detachEveryConnection();
                    jsPlumb.deleteEveryEndpoint();
                    $('#leipi_process_info').html('');
                    jsPlumb.repaintEverything();
                    return true;
                } catch (e) {
                    return false;
                }
            }, refresh: function () {
                try {
                    //jsPlumb.reset();
                    this.clear();
                    _canvas_design();
                    return true;
                } catch (e) {
                    return false;
                }
            }
        };
        return Flowdesign;


    }//$.fn
})(jQuery);