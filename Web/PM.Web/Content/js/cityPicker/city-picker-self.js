(function ($, chineseDistricts) {

    'use strict';

    if (typeof chineseDistricts === 'undefined') {
        throw new Error('ChineseDistricts must be included first!');
    }

    var NAMESPACE = 'citypicker';
    var EVENT_CHANGE = 'change.' + NAMESPACE;
    var PROVINCE = 'province';
    var CITY = 'city';
    var DISTRICT = 'district';

    var CityPicker = function (element, options) {
        this.$element = $(element);
        this.$dropdown = null;
        this.options = $.extend({}, CityPicker.DEFAULTS, $.isPlainObject(options) && options);

        if (this.options.level < 1)
            this.options.level = 1;

        if (this.options.level > 3)
            this.options.level = 3;

        var levelArr = [PROVINCE, CITY, DISTRICT];
        this.options.level = levelArr[this.options.level - 1];

        this.active = false;
        this.dems = [];
        this.needBlur = false;
        this.init();
    }

    CityPicker.prototype = {
        constructor: CityPicker,

        init: function () {

            this.defineDems();

            this.render();

            this.bind();

            this.active = true;
        },

        render: function () {
            var p = this.getPosition(),
                placeholder = this.$element.attr('placeholder') || this.options.placeholder,
                textspan = '<span class="city-picker-span" style="' +
                    this.getWidthStyle(p.width) + 'height:' +
                    p.height + 'px;line-height:' + (p.height - 1) + 'px;">' +
                    (placeholder ? '<span class="placeholder">' + placeholder + '</span>' : '') +
                    '<span class="title"></span><div class="arrow"></div>' + '</span>',

                dropdown = '<div class="city-picker-dropdown" style="left:0px;top:100%;' +
                    this.getWidthStyle(p.width, true) + '">' +
                    '<div class="city-select-wrap">' +
                    '<div class="city-select-tab">' +
                    '<a class="active" data-count="province">省份</a>' +
                    (this.includeDem('city') ? '<a data-count="city">城市</a>' : '') +
                    (this.includeDem('district') ? '<a data-count="district">区县</a>' : '') +
                    '<input type="button" value="清空" class="city-picker-clear">' + '</div>' +
                    '<div class="city-select-content">' +
                    '<div class="city-select province" data-count="province"></div>' +
                    (this.includeDem('city') ? '<div class="city-select city" data-count="city"></div>' : '') +
                    (this.includeDem('district') ? '<div class="city-select district" data-count="district"></div>' : '') +
                    '</div></div>';

            this.$element.addClass('city-picker-input');
            this.$textspan = $(textspan).insertAfter(this.$element);
            this.$dropdown = $(dropdown).insertAfter(this.$textspan);
            var $select = this.$dropdown.find('.city-select');

            // setup this.$province, this.$city and/or this.$district object
            $.each(this.dems, $.proxy(function (i, type) {
                this['$' + type] = $select.filter('.' + type + '');
            }, this));

            this.refresh();
        },

        refresh: function (force) {
            // clean the data-item for each $select
            var $select = this.$dropdown.find('.city-select');
            $select.data('item', null);
            // parse value from value of the target $element
            var val = this.$element.val() || '';
            val = val.split('/');
            $.each(this.dems, $.proxy(function (i, type) {
                if (val[i] && i < val.length) {
                    this.options[type] = val[i];
                } else if (force) {
                    this.options[type] = '';
                }
                this.output(type, true);
            }, this));
            this.tab(PROVINCE);
            this.feedText();
            this.feedVal();
        },

        defineDems: function () {
            var stop = false;
            $.each([PROVINCE, CITY, DISTRICT], $.proxy(function (i, type) {
                if (!stop) {
                    this.dems.push(type);
                }
                if (type === this.options.level) {
                    stop = true;
                }
            }, this));
        },

        includeDem: function (type) {
            return $.inArray(type, this.dems) !== -1;
        },

        getPosition: function () {
            var p, h, w, s, pw;
            p = this.$element.position();
            s = this.getSize(this.$element);
            h = s.height;
            w = s.width;
            if (this.options.responsive) {
                pw = this.$element.offsetParent().width();
                if (pw) {
                    w = w / pw;
                    if (w > 0.99) {
                        w = 1;
                    }
                    w = w * 100 + '%';
                }
            }

            return {
                top: p.top || 0,
                left: p.left || 0,
                height: h,
                width: w
            };
        },

        getSize: function ($dom) {
            var $wrap, $clone, sizes;
            if (!$dom.is(':visible')) {
                $wrap = $("<div />").appendTo($("body"));
                $wrap.css({
                    "position": "absolute !important",
                    "visibility": "hidden !important",
                    "display": "block !important"
                });

                $clone = $dom.clone().appendTo($wrap);

                sizes = {
                    width: $clone.outerWidth(),
                    height: $clone.outerHeight()
                };

                $wrap.remove();
            } else {
                sizes = {
                    width: $dom.outerWidth(),
                    height: $dom.outerHeight()
                };
            }

            return sizes;
        },

        getWidthStyle: function (w, dropdown) {
            if (this.options.responsive && !$.isNumeric(w)) {
                return 'width:' + w + ';';
            } else {
                return 'width:' + (dropdown ? Math.max(320, w) : w) + 'px;';
            }
        },

        bind: function () {
            var $this = this;

            if (this.options.code) {
                $this.$element.val(this.options.code);
            }

            $(document).on('click', (this._mouteclick = function (e) {
                //$this.$element.text();
             
                var $target = $(e.target);
                var $dropdown, $span, $input;
                if ($target.is('.city-picker-span')) {
                    $span = $target;
                } else if ($target.is('.city-picker-span *')) {
                    $span = $target.parents('.city-picker-span');
                }
                if ($target.is('.city-picker-input')) {
                    $input = $target;
                  
                }
                if ($target.is('.city-picker-dropdown')) {
                    $dropdown = $target;
                 
                } else if ($target.is('.city-picker-dropdown *')) {
                    $dropdown = $target.parents('.city-picker-dropdown');
                    //动态添加所选城市(实现添加城市,直辖市处理，港澳台处理，国外处理）
                   
                    if ($this.getVal().indexOf('/') > 0) {
                        var valText = $this.getVal();
                        var arr = valText.split('/');
                        var cityList = "天津市,北京市,上海市,重庆市";
                        var city = arr[0].trim();
                        if (cityList.indexOf(city) >= 0) {
                            var appendText = '<li class="city-item">' + arr[0] + '<i class="search-choice-close11">x</i></li>' + '<input name="citylist" type="text" style="display:none" value="' + arr[0] + '">';
                            $(".city-list").append(appendText);
                            //添加城市数目
                            var numCity = $(".city-list").find("li").length;
                            $("#cityNum").html("("+numCity+")");

                        } else {
                            var appendText0 = '<li class="city-item">' + arr[1] + '<i class="search-choice-close11">x</i></li>' + '<input name="citylist" type="text" style="display:none" value="' + arr[1] + '">';
                            $(".city-list").append(appendText0);
                            //添加城市数目
                            var numCity0 = $(".city-list").find("li").length;
                            $("#cityNum").html("(" + numCity0 + ")");
                        }                      
                    } else {
                        var cityList1 = "香港,澳门,台湾";
                        var valText1 = $this.getVal();
                        if (cityList1.indexOf(valText1) >= 0 && valText1 != null && valText1 != "") {
                            var appendText1 = '<li class="city-item">' + valText1 + '<i class="search-choice-close11">x</i></li>' + '<input name="citylist" type="text" style="display:none" value="' + valText1 + '">';
                            $(".city-list").append(appendText1);
                            //添加城市数目
                            var numCity1 = $(".city-list").find("li").length;
                            $("#cityNum").html("(" + numCity1 + ")");
                        }
                    }

                 
                }
                if ((!$input && !$span && !$dropdown) ||
                     ($span && $span.get(0) !== $this.$textspan.get(0)) ||
                     ($input && $input.get(0) !== $this.$element.get(0)) ||
                     ($dropdown && $dropdown.get(0) !== $this.$dropdown.get(0))) {
                    $this.close(true);
                }

            }));

            this.$element.on('change', (this._changeElement = $.proxy(function () {
                this.close(true);
                this.refresh(true);
            }, this))).on('focus', (this._focusElement = $.proxy(function () {
                this.needBlur = true;
                this.open();
            }, this))).on('blur', (this._blurElement = $.proxy(function () {
                if (this.needBlur) {
                    this.needBlur = false;
                    this.close(true);
                }
            }, this)));

            this.$textspan.on('click', function (e) {
                var $target = $(e.target), type;
                $this.needBlur = false;
                if ($target.is('.select-item')) {
                    type = $target.data('count');
                    $this.open(type);
                } else {
                    if ($this.$dropdown.is(':visible')) {
                        $this.close();
                    } else {
                        $this.open();
                    }
                }
                $this.$element.blur();
            }).on('mousedown', function () {
                $this.needBlur = false;
            });

            this.$dropdown.on('click', '.city-select a', function () {
                var $select = $(this).parents('.city-select');
                var $active = $select.find('a.active');
                var last = $select.next().length === 0;
                $active.removeClass('active');
                $(this).addClass('active');
                if ($active.data('code') !== $(this).data('code')) {
                    $select.data('item', {
                        address: $(this).attr('title'), code: $(this).data('code')
                    });
                    $(this).trigger(EVENT_CHANGE);
                    $this.feedText();
                    $this.feedVal();
                    if (last) {
                        $this.close();
                    }
                }
                if ($(this).data().code) {
                    $this.$element.val($(this).data().code);
                }
                $this.$element.blur();
            }).on('click', '.city-select-tab a', function () {
                if (!$(this).hasClass('active')) {
                    var type = $(this).data('count');
                    $this.tab(type);
                }
            }).on('mousedown', function () {
                $this.needBlur = false;
            }).on("click", ".city-picker-clear", function () {
                $this.$element.val("");
                $this.reset(true);
                $this.$element.blur();
            });

            if (this.$province) {
                this.$province.on(EVENT_CHANGE, (this._changeProvince = $.proxy(function () {
                    this.output(CITY);
                    this.output(DISTRICT);
                    this.tab(CITY);
                }, this)));
            }

            if (this.$city) {
                this.$city.on(EVENT_CHANGE, (this._changeCity = $.proxy(function () {
                    this.output(DISTRICT);
                    this.tab(DISTRICT);
                }, this)));
            }
        },

        open: function (type) {
            type = type || PROVINCE;
            this.$dropdown.show();
            this.$textspan.addClass('open').addClass('focus');
            this.tab(type);
        },

        close: function (blur) {
            this.$dropdown.hide();
            this.$textspan.removeClass('open');
            if (blur) {
                this.$textspan.removeClass('focus');
            }
        },

        unbind: function () {

            $(document).off('click', this._mouteclick);

            this.$element.off('change', this._changeElement);
            this.$element.off('focus', this._focusElement);
            this.$element.off('blur', this._blurElement);

            this.$textspan.off('click');
            this.$textspan.off('mousedown');

            this.$dropdown.off('click');
            this.$dropdown.off('mousedown');

            if (this.$province) {
                this.$province.off(EVENT_CHANGE, this._changeProvince);
            }

            if (this.$city) {
                this.$city.off(EVENT_CHANGE, this._changeCity);
            }
        },

        getText: function () {
            var text = '';
            this.$dropdown.find('.city-select')
                .each(function () {
                    var item = $(this).data('item'),
                        type = $(this).data('count');
                    if (item) {
                        text += ($(this).hasClass('province') ? '' : '/') + '<span class="select-item" data-count="' +
                            type + '" data-code="' + item.code + '">' + item.address + '</span>';
                    }
                });
            return text;
        },

        getPlaceHolder: function () {
            return this.$element.attr('placeholder') || this.options.placeholder;
        },

        feedText: function () {
            var text = this.getText();
            if (text) {
                this.$textspan.find('>.placeholder').hide();
                this.$textspan.find('>.title').html(this.getText()).show();
            } else {
                this.$textspan.find('>.placeholder').text(this.getPlaceHolder()).show();
                this.$textspan.find('>.title').html('').hide();
            }
        },

        getVal: function () {
            var text = '';
            this.$dropdown.find('.city-select')
                .each(function () {
                    var item = $(this).data('item');
                    if (item) {
                        text += ($(this).hasClass('province') ? '' : '/') + item.address;
                    }
                });
            return text;
        },

        feedVal: function () {
            this.$element.val(this.getVal());
        },

        output: function (type, isInit) {
            var options = this.options;
            //var placeholders = this.placeholders;
            var $select = this['$' + type];
            var data = type === PROVINCE ? {} : [];
            var item;
            var districts;
            var code;
            var matched = null;
            var value;

            if (!$select || !$select.length) {
                return;
            }

            item = $select.data('item');
            value = (item ? item.address : null) || options[type];

            code = (
                type === PROVINCE ? 86 :
                    type === CITY ? this.$province && this.$province.find('.active').data('code') :
                        type === DISTRICT ? this.$city && this.$city.find('.active').data('code') : code
            );

            //处理code选中的问题
            if (isInit && $.trim(options.code)) {
                if (type === PROVINCE) {
                    code = 86;
                } else if (type === CITY) {
                    code = parseInt(options.code.toString().substr(0, 2));
                } else {
                    code = parseInt(options.code.toString().substr(0, 4));
                }
            }

            districts = $.isNumeric(code) ? chineseDistricts[code] : null;

            if ($.isPlainObject(districts)) {
                $.each(districts, function (code, address) {
                    var provs;
                    if (type === PROVINCE) {
                        provs = [];
                        var provinceCode = parseInt(options.code.toString().substr(0, 2));
                        for (var i = 0; i < address.length; i++) {
                            if (address[i].address === value || address[i].code === provinceCode) {
                                matched = {
                                    code: address[i].code,
                                    address: address[i].address
                                };
                            }
                            provs.push({
                                code: address[i].code,
                                address: address[i].address,
                                selected: address[i].address === value || address[i].code === provinceCode
                            });
                        }
                        data[code] = provs;
                    } else {
                        var cityCode = options.code.toString().substr(0, 4);

                        if (address === value || code === cityCode || code === options.code.toString()) {
                            matched = {
                                code: code,
                                address: address
                            };
                        }
                        data.push({
                            code: code,
                            address: address,
                            selected: address === value || code === cityCode || code === options.code.toString()
                        });
                    }
                });
            }

            $select.html(type === PROVINCE ? this.getProvinceList(data) :
                this.getList(data, type));
            $select.data('item', matched);
        },

        getProvinceList: function (data) {
            var list = [],
                $this = this,
                simple = this.options.simple;

            $.each(data, function (i, n) {
                list.push('<dl class="clearfix">');
                list.push('<dt>' + i + '</dt><dd>');
                $.each(n, function (j, m) {
                    list.push(
                        '<a' +
                        ' title="' + (m.address || '') + '"' +
                        ' data-code="' + (m.code || '') + '"' +
                        ' class="' +
                        (m.selected ? ' active' : '') +
                        '">' +
                        (simple ? $this.simplize(m.address, PROVINCE) : m.address) +
                        '</a>');
                });
                list.push('</dd></dl>');
            });

            return list.join('');
        },

        getList: function (data, type) {
            var list = [],
                $this = this,
                simple = this.options.simple;
            list.push('<dl class="clearfix"><dd>');

            $.each(data, function (i, n) {
                list.push(
                    '<a' +
                    ' title="' + (n.address || '') + '"' +
                    ' data-code="' + (n.code || '') + '"' +
                    ' class="' +
                    (n.selected ? ' active' : '') +
                    '">' +
                    (simple ? $this.simplize(n.address, type) : n.address) +
                    '</a>');
            });
            list.push('</dd></dl>');

            return list.join('');
        },

        simplize: function (address, type) {
            address = address || '';
            if (type === PROVINCE) {
                return address.replace(/[省,市,自治区,壮族,回族,维吾尔]/g, '');
            } else if (type === CITY) {
                return address.replace(/[市,地区,回族,蒙古,苗族,白族,傣族,景颇族,藏族,彝族,壮族,傈僳族,布依族,侗族]/g, '')
                    .replace('哈萨克', '').replace('自治州', '').replace(/自治县/, '');
            } else if (type === DISTRICT) {
                return address.length > 2 ? address.replace(/[市,区,县,旗]/g, '') : address;
            }
        },

        tab: function (type) {
            var $selects = this.$dropdown.find('.city-select');
            var $tabs = this.$dropdown.find('.city-select-tab > a');
            var $select = this['$' + type];
            var $tab = this.$dropdown.find('.city-select-tab > a[data-count="' + type + '"]');
            if ($select) {
                $selects.hide();
                $select.show();
                $tabs.removeClass('active');
                $tab.addClass('active');
            }
        },

        reset: function (isClear, options) {
            if (isClear) {
                this.options.city = '';
                this.options.code = '';
                this.options.district = '';
                this.options.province = '';
            }

            if (options) {
                this.options = $.extend(this.options, options);
            }

            this.$element.val(null).trigger('change');
        },

        destroy: function () {
            this.unbind();
            this.$element.removeData(NAMESPACE).removeClass('city-picker-input');
            this.$textspan.remove();
            this.$dropdown.remove();
        }
    };

    CityPicker.DEFAULTS = {
        simple: false,
        responsive: false,
        placeholder: '请选择地区',
        level: '3',//层级
        code: '',//初始化code
        province: '',
        city: '',
        district: ''
    };

    CityPicker.setDefaults = function (options) {
        $.extend(CityPicker.DEFAULTS, options);
    };

    // Save the other citypicker
    CityPicker.other = $.fn.citypicker;

    // Register as jQuery plugin
    $.fn.citypicker = function (option) {
        var args = [].slice.call(arguments, 1);
        var cityPicker;

        this.each(function () {
            var $this = $(this);
            var data = $this.data(NAMESPACE);
            var options;
            var fn;

            if (!data) {
                if (/destroy/.test(option)) {
                    return;
                }

                options = $.extend({}, $this.data(), $.isPlainObject(option) && option);

                //reset,args[1]为自定义参数
                if (args.length > 1) {
                    options = $.extend(options, args[1]);
                }

                cityPicker = new CityPicker(this, options);
                $this.data(NAMESPACE, (data = cityPicker));
            }

            if (typeof option === 'string' && $.isFunction(fn = data[option])) {
                fn.apply(data, args);
            }
        });

        var $this = this;
        return {
            get: function () {
                var selData = [];
                var selects = $this.next('span').find(".select-item");
                $.each(selects, function (index, item) {
                    var data = {};
                    data.text = $(item).text();
                    data.value = $(item).data('code');
                    selData.push(data);
                });
                return selData;
            },
            destroy: function () {
                $this.citypicker('destroy');
                return this;
            },
            reset: function (options) {
                $this.citypicker('destroy');
                $this.citypicker("reset", false, options);
                return this;
            },
            enable: function () {
                $this.citypicker('bind');
            },
            disable: function () {
                $this.citypicker('unbind');
            },
            getText: function () {
                var selText = '';
                var selects = $this.next('span').find(".select-item");
                $.each(selects, function (index, item) {
                    selText += $(item).text() + "/";
                });
                return selText.length > 1 ? selText.substr(0, selText.length - 1) : "";
            },
            getVal: function () {
                return $this.val();
            }
        }
    };

    $.fn.citypicker.Constructor = CityPicker;
    $.fn.citypicker.setDefaults = CityPicker.setDefaults;

    // No conflict
    $.fn.citypicker.noConflict = function () {
        $.fn.citypicker = CityPicker.other;
        return this;
    };

    $(function () {
        $('[data-toggle="city-picker"]').citypicker();
    });
})(jQuery, { 86: { 'A-G': [{ code: 33, address: '安徽省' }, { code: 10, address: '北京市' }, { code: 60, address: '重庆市' }, { code: 34, address: '福建省' }, { code: 84, address: '甘肃省' }, { code: 40, address: '广东省' }, { code: 41, address: '广西省' }, { code: 63, address: '贵州省' }], 'H-K': [{ code: 42, address: '海南省' }, { code: 12, address: '河北省' }, { code: 22, address: '黑龙江省' }, { code: 70, address: '河南省' }, { code: 71, address: '湖北省' }, { code: 72, address: '湖南省' }, { code: 31, address: '江苏省' }, { code: 36, address: '江西省' }, { code: 21, address: '吉林省' }], 'L-S': [{ code: 20, address: '辽宁省' }, { code: 13, address: '内蒙古' }, { code: 82, address: '宁夏' }, { code: 81, address: '青海省' }, { code: 32, address: '山东省' }, { code: 30, address: '上海市' }, { code: 14, address: '山西省' }, { code: 80, address: '陕西省' }, { code: 61, address: '四川省' }], 'T-Z': [{ code: 11, address: '天津市' }, { code: 83, address: '新疆' }, { code: 64, address: '西藏' }, { code: 62, address: '云南省' }, { code: 35, address: '浙江省' }], '': [{ code: 90, address: '香港' }, { code: 91, address: '澳门' }, { code: 92, address: '台湾' }, { code: 93, address: '国外' }], }, 10: { 1001: "东城区", 1002: "西城区", 1005: "朝阳区", 1006: "海淀区", 1007: "丰台区", 1008: "石景山区", 1009: "顺义区", 1010: "昌平区", 1011: "门头沟区", 1012: "通州区", 1013: "房山区", 1014: "大兴区", 1015: "延庆县", 1016: "怀柔区", 1017: "平谷区", 1018: "密云县", 1019: "亦庄开发区" }, 11: { 1101: "和平区", 1102: "河北区", 1103: "河东区", 1104: "河西区", 1105: "南开区", 1106: "红桥区", 1107: "滨海新区", 1110: "东丽区", 1111: "西青区", 1112: "津南区", 1113: "北辰区", 1114: "武清区", 1115: "宝坻区", 1116: "蓟县", 1117: "宁河县", 1118: "静海县", 1119: "开发区" }, 12: { 1201: "石家庄市", 1202: "秦皇岛市", 1203: "唐山市", 1204: "张家口市", 1205: "廊坊市", 1206: "衡水市", 1207: "保定市", 1208: "承德市", 1209: "邢台市", 1210: "沧州市", 1211: "邯郸市" }, 13: { 1301: "呼和浩特市", 1302: "包头市", 1303: "呼伦贝尔市", 1304: "兴安盟", 1305: "通辽市", 1306: "赤峰市", 1307: "锡林郭勒盟", 1308: "乌兰察布市", 1309: "鄂尔多斯市", 1310: "巴彦淖尔市", 1311: "乌海市", 1312: "阿拉善盟", 1313: "满洲里", 1314: "二连浩特" }, 14: { 1401: "太原市", 1402: "大同市", 1403: "阳泉市", 1404: "长治市", 1405: "晋城市", 1406: "朔州市", 1407: "忻州市", 1408: "晋中市", 1409: "临汾市", 1410: "吕梁市", 1411: "运城市" }, 20: { 2001: "沈阳市", 2002: "大连市", 2003: "鞍山市", 2004: "锦州市", 2005: "丹东市", 2006: "盘锦市", 2007: "铁岭市", 2008: "抚顺市", 2009: "营口市", 2010: "辽阳市", 2011: "阜新市", 2012: "本溪市", 2013: "朝阳市", 2014: "葫芦岛市" }, 21: { 2101: "长春市", 2102: "吉林市", 2103: "四平市", 2104: "辽源市", 2105: "通化市", 2106: "白山市", 2107: "松原市", 2108: "白城市", 2109: "延边州" }, 22: { 2201: "哈尔滨市", 2202: "齐齐哈尔市", 2203: "鸡西市", 2204: "鹤岗市", 2205: "双鸭山市", 2206: "大庆市", 2207: "伊春市", 2208: "佳木斯市", 2209: "七台河市", 2210: "牡丹江市", 2211: "黑河市", 2212: "绥化市", 2213: "大兴安岭地区" }, 30: { 3001: "宝山区", 3002: "金山区", 3004: "长宁区", 3005: "静安区", 3006: "青浦区", 3007: "崇明县", 3009: "松江区", 3010: "奉贤区", 3011: "浦东新区", 3012: "杨浦区", 3013: "虹口区", 3014: "普陀区", 3015: "闸北区", 3016: "黄浦区", 3017: "闵行区", 3018: "徐汇区", 3019: "嘉定区" }, 31: { 3101: "南京市", 3102: "苏州市", 3103: "无锡市", 3104: "镇江市", 3105: "扬州市", 3106: "南通市", 3107: "常州市", 3108: "徐州市", 3109: "连云港市", 3110: "盐城市", 3111: "淮安市", 3112: "泰州市", 3113: "宿迁市" }, 32: { 3201: "济南市", 3202: "青岛市", 3203: "烟台市", 3204: "淄博市", 3205: "泰安市", 3206: "潍坊市", 3207: "济宁市", 3208: "枣庄市", 3209: "德州市", 3210: "威海市", 3211: "日照市", 3212: "莱芜市", 3213: "滨州市", 3214: "东营市", 3215: "聊城市", 3216: "菏泽市", 3217: "临沂市" }, 33: { 3301: "合肥市", 3302: "芜湖市", 3303: "蚌埠市", 3304: "马鞍山市", 3305: "淮北市", 3306: "铜陵市", 3307: "安庆市", 3308: "黄山市", 3309: "滁州市", 3310: "宿州市", 3311: "池州市", 3312: "淮南市", 3313: "巢湖市", 3314: "阜阳市", 3315: "六安市", 3316: "宣城市", 3317: "亳州市" }, 34: { 3401: "福州市", 3402: "厦门市", 3403: "漳州市", 3404: "泉州市", 3405: "莆田市", 3406: "三明市", 3407: "南平市", 3408: "龙岩市", 3409: "宁德市" }, 35: { 3501: "杭州市", 3502: "宁波市", 3503: "温州市", 3504: "嘉兴市", 3505: "湖州市", 3506: "绍兴市", 3507: "金华市", 3508: "衢州市", 3509: "舟山市", 3510: "台州市", 3511: "丽水市" }, 36: { 3601: "南昌市", 3602: "景德镇市", 3603: "萍乡市", 3604: "九江市", 3605: "新余市", 3606: "鹰潭市", 3607: "赣州市", 3608: "吉安市", 3609: "宜春市", 3610: "抚州市", 3611: "上饶市" }, 40: { 4001: "广州市", 4002: "深圳市", 4003: "珠海市", 4004: "汕头市", 4005: "韶关市", 4006: "河源市", 4007: "梅州市", 4008: "惠州市", 4009: "汕尾市", 4010: "东莞市", 4011: "中山市", 4012: "江门市", 4013: "佛山市", 4014: "阳江市", 4015: "湛江市", 4016: "茂名市", 4017: "肇庆市", 4018: "清远市", 4019: "潮州市", 4020: "揭阳市", 4021: "云浮市" }, 41: { 4101: "南宁市", 4102: "柳州市", 4103: "桂林市", 4104: "梧州市", 4105: "北海市", 4106: "防城港市", 4107: "钦州市", 4108: "贵港市", 4109: "玉林市", 4110: "百色市", 4111: "贺州市", 4112: "河池市", 4113: "来宾市", 4114: "崇左市" }, 42: { 4201: "海口市", 4202: "三亚市", 4203: "琼海市", 4204: "儋州市", 4205: "文昌市", 4206: "东方市", 4207: "五指山市", 4208: "万宁市", 4209: "定安县", 4210: "屯昌县", 4211: "澄迈县", 4212: "临高县", 4213: "陵水县", 4214: "琼中县", 4215: "保亭县", 4216: "乐东县", 4217: "昌江县", 4218: "白沙县", 4219: "洋浦" }, 60: { 6001: "万州区", 6002: "涪陵区", 6003: "渝中区", 6004: "大渡口区", 6005: "江北区", 6006: "沙坪坝区", 6007: "九龙坡区", 6008: "南岸区", 6009: "北碚区", 6010: "万盛区", 6011: "双桥区", 6012: "渝北区", 6013: "巴南区", 6014: "黔江区", 6015: "长寿区", 6016: "江津区", 6017: "合川区", 6018: "永川区", 6019: "南川区", 6020: "綦江县", 6021: "潼南县", 6022: "铜梁县", 6023: "大足县", 6024: "荣昌县", 6025: "璧山县", 6026: "梁平县", 6027: "城口县", 6028: "丰都县", 6029: "垫江县", 6030: "武隆县", 6031: "忠县", 6032: "开县", 6033: "云阳县", 6034: "奉节县", 6035: "巫山县", 6036: "巫溪县", 6037: "石柱县", 6038: "秀山县", 6039: "酉阳县", 6040: "彭水苗族县", 6041: "开发区" }, 61: { 6101: "成都市", 6102: "德阳市", 6103: "绵阳市", 6104: "眉山市", 6105: "泸州市", 6106: "南充市", 6107: "自贡市", 6108: "内江市", 6109: "宜宾市", 6110: "乐山市", 6111: "雅安市", 6112: "遂宁市", 6113: "达州市", 6114: "巴中市", 6115: "广元市", 6116: "广安市", 6117: "资阳市", 6118: "阿坝州", 6119: "攀枝花市", 6120: "甘孜州", 6121: "凉山州" }, 62: { 6201: "昆明市", 6202: "曲靖市", 6203: "玉溪市", 6204: "昭通市", 6205: "楚雄州", 6206: "红河哈尼族州", 6207: "丽江市", 6208: "迪庆州", 6209: "文山州", 6210: "西双版纳州", 6211: "普洱市", 6212: "大理", 6213: "保山市", 6214: "德宏", 6215: "怒江", 6216: "临沧市" }, 63: { 6301: "贵阳市", 6302: "遵义市", 6303: "六盘水市", 6304: "安顺市", 6305: "黔西南", 6306: "黔南", 6307: "铜仁地区", 6308: "毕节地区", 6309: "黔东南" }, 64: { 6401: "拉萨市", 6402: "日喀则地区", 6403: "山南地区", 6404: "林芝地区", 6405: "昌都地区", 6406: "那曲地区", 6407: "阿里地区" }, 70: { 7001: "郑州市", 7002: "洛阳市", 7003: "开封市", 7004: "安阳市", 7005: "新乡市", 7006: "濮阳市", 7007: "焦作市", 7008: "鹤壁市", 7009: "三门峡市", 7010: "商丘市", 7011: "许昌市", 7012: "漯河市", 7013: "平顶山市", 7014: "驻马店市", 7015: "周口市", 7016: "南阳市", 7017: "信阳市", 7018: "济源市" }, 71: { 7101: "武汉市", 7102: "黄石市", 7103: "襄阳市", 7104: "十堰市", 7105: "荆州市", 7106: "宜昌市", 7107: "荆门市", 7108: "鄂州市", 7109: "孝感市", 7110: "黄冈市", 7111: "咸宁市", 7112: "随州市", 7113: "恩施", 7114: "潜江市", 7115: "仙桃市", 7116: "天门市", 7117: "神农架林区" }, 72: { 7201: "长沙市", 7202: "株洲市", 7203: "湘潭市", 7204: "衡阳市", 7205: "益阳市", 7206: "常德市", 7207: "岳阳市", 7208: "邵阳市", 7209: "郴州市", 7210: "娄底市", 7211: "永州市", 7212: "怀化市", 7213: "张家界市", 7214: "湘西" }, 80: { 8001: "西安市", 8002: "宝鸡市", 8003: "咸阳市", 8004: "渭南市", 8005: "延安市", 8006: "榆林市", 8007: "铜川市", 8008: "汉中市", 8009: "安康市", 8010: "商洛市", 8011: "杨凌" }, 81: { 8101: "西宁市", 8102: "海东地区", 8103: "海南州", 8104: "海北州", 8105: "海西蒙古族州", 8106: "黄南州", 8107: "果洛州", 8108: "玉树州" }, 82: { 8201: "银川市", 8202: "石嘴山市", 8203: "吴忠市", 8204: "固原市", 8205: "中卫市" }, 83: { 8301: "乌鲁木齐市", 8302: "伊犁州", 8303: "阿勒泰地区", 8304: "塔城地区", 8305: "博尔塔拉", 8306: "昌吉", 8307: "吐鲁番地区", 8308: "巴音郭楞", 8309: "哈密地区", 8310: "和田地区", 8311: "阿克苏地区", 8312: "克孜勒苏", 8313: "喀什地区", 8314: "克拉玛依市", 8315: "石河子市", 8316: "五家渠市", 8317: "阿拉尔市", 8318: "图木舒克市" }, 84: { 8401: "兰州市", 8402: "天水市", 8403: "嘉峪关市", 8404: "武威市", 8405: "金昌市", 8406: "酒泉市", 8407: "张掖市", 8408: "庆阳市", 8409: "平凉市", 8410: "白银市", 8411: "定西市", 8412: "陇南市", 8413: "临夏", 8414: "甘南州" }, 90: {}, 91: {}, 92: {}, 93: {}, 1201: { 120102: "长安区", 120103: "桥东区", 120104: "桥西区", 120105: "新华区", 120106: "井陉矿区", 120107: "裕华区", 120108: "井陉县", 120109: "正定县", 120110: "栾城县", 120111: "行唐县", 120112: "灵寿县", 120113: "高邑县", 120114: "深泽县", 120115: "赞皇县", 120116: "无极县", 120117: "平山县", 120118: "元氏县", 120119: "赵县", 120120: "辛集市", 120121: "藁城市", 120122: "晋州市", 120123: "新乐市", 120124: "鹿泉市", 120125: "高新区" }, 1202: { 120202: "海港区", 120203: "山海关区", 120204: "北戴河区", 120205: "青龙县", 120206: "昌黎县", 120207: "抚宁县", 120208: "卢龙县", 120209: "开发区" }, 1203: { 120302: "路南区", 120303: "路北区", 120304: "古冶区", 120305: "开平区", 120306: "丰南区", 120307: "丰润区", 120308: "滦县", 120309: "滦南县", 120310: "乐亭县", 120311: "迁西县", 120312: "玉田县", 120313: "唐海县", 120314: "遵化市", 120315: "迁安市" }, 1204: { 120402: "桥东区", 120403: "桥西区", 120404: "宣化区", 120405: "下花园区", 120406: "宣化县", 120407: "张北县", 120408: "康保县", 120409: "沽源县", 120410: "尚义县", 120411: "蔚县", 120412: "阳原县", 120413: "怀安县", 120414: "万全县", 120415: "怀来县", 120416: "涿鹿县", 120417: "赤城县", 120418: "崇礼县" }, 1205: { 120502: "安次区", 120503: "广阳区", 120504: "固安县", 120505: "永清县", 120506: "香河县", 120507: "大城县", 120508: "文安县", 120509: "大厂县", 120510: "霸州市", 120511: "三河市", 120512: "开发区" }, 1206: { 120602: "桃城区", 120603: "枣强县", 120604: "武邑县", 120605: "武强县", 120606: "饶阳县", 120607: "安平县", 120608: "故城县", 120609: "景县", 120610: "阜城县", 120611: "冀州市", 120612: "深州市" }, 1207: { 120701: "新市区", 120702: "北市区", 120703: "南市区", 120705: "满城县", 120706: "清苑县", 120707: "涞水县", 120708: "阜平县", 120709: "徐水县", 120710: "定兴县", 120711: "唐县", 120712: "高阳县", 120713: "容城县", 120714: "涞源县", 120715: "望都县", 120716: "安新县", 120717: "易县", 120718: "曲阳县", 120719: "蠡县", 120720: "顺平县", 120721: "博野县", 120722: "雄县", 120723: "涿州市", 120724: "定州市", 120725: "安国市", 120726: "高碑店市", 120727: "高新区" }, 1208: { 120802: "双桥区", 120803: "双滦区", 120804: "鹰手营子", 120805: "承德县", 120806: "兴隆县", 120807: "平泉县", 120808: "滦平县", 120809: "隆化县", 120810: "丰宁县", 120811: "宽城县", 120812: "围场县" }, 1209: { 120902: "桥东区", 120903: "桥西区", 120904: "邢台县", 120905: "临城县", 120906: "内丘县", 120907: "柏乡县", 120908: "隆尧县", 120909: "任县", 120910: "南和县", 120911: "宁晋县", 120912: "巨鹿县", 120913: "新河县", 120914: "广宗县", 120915: "平乡县", 120916: "威县", 120917: "清河县", 120918: "临西县", 120919: "南宫市", 120920: "沙河市" }, 1210: { 121002: "新华区", 121003: "运河区", 121004: "沧县", 121005: "青县", 121006: "东光县", 121007: "海兴县", 121008: "盐山县", 121009: "肃宁县", 121010: "南皮县", 121011: "吴桥县", 121012: "献县", 121013: "孟村县", 121014: "泊头市", 121015: "任丘市", 121016: "黄骅市", 121017: "河间市", 121018: "开发区" }, 1211: { 121102: "邯山区", 121103: "丛台区", 121104: "复兴区", 121105: "峰峰矿区", 121106: "邯郸县", 121107: "临漳县", 121108: "成安县", 121109: "大名县", 121110: "涉县", 121111: "磁县", 121112: "肥乡县", 121113: "永年县", 121114: "邱县", 121115: "鸡泽县", 121116: "广平县", 121117: "馆陶县", 121118: "魏县", 121119: "曲周县", 121120: "武安市" }, 1301: { 130102: "新城区", 130103: "回民区", 130104: "玉泉区", 130105: "赛罕区", 130106: "土默特左旗", 130107: "托克托县", 130108: "和林格尔县", 130109: "清水河县", 130110: "武川县", 130111: "开发区" }, 1302: { 130202: "东河区", 130203: "昆都仑区", 130204: "青山区", 130205: "石拐区", 130206: "白云鄂博", 130207: "九原区", 130208: "土默特右旗", 130209: "固阳县", 130210: "达茂联合旗", 130211: "高新区" }, 1303: { 130302: "海拉尔区", 130303: "阿荣旗", 130304: "莫旗", 130305: "鄂伦春自治旗", 130306: "鄂温克旗", 130307: "陈巴尔虎旗", 130308: "新巴尔虎左旗", 130309: "新巴尔虎右旗", 130311: "牙克石市", 130312: "扎兰屯市", 130313: "额尔古纳市", 130314: "根河市" }, 1304: { 130401: "乌兰浩特市", 130402: "阿尔山市", 130403: "科右前旗", 130404: "科右中旗", 130405: "扎赉特旗", 130406: "突泉县" }, 1305: { 130502: "科尔沁区", 130503: "科左中旗", 130504: "科左后旗", 130505: "开鲁县", 130506: "库伦旗", 130507: "奈曼旗", 130508: "扎鲁特旗", 130509: "霍林郭勒市" }, 1306: { 130602: "红山区", 130603: "元宝山区", 130604: "松山区", 130605: "阿鲁科尔沁旗", 130606: "巴林左旗", 130607: "巴林右旗", 130608: "林西县", 130609: "克什克腾旗", 130610: "翁牛特旗", 130611: "喀喇沁旗", 130612: "宁城县", 130613: "敖汉旗" }, 1307: { 130702: "锡林浩特市", 130703: "阿巴嘎旗", 130704: "苏尼特左旗", 130705: "苏尼特右旗", 130706: "东乌珠穆沁旗", 130707: "西乌珠穆沁旗", 130708: "太仆寺旗", 130709: "镶黄旗", 130710: "正镶白旗", 130711: "正蓝旗", 130712: "多伦县" }, 1308: { 130802: "集宁区", 130803: "卓资县", 130804: "化德县", 130805: "商都县", 130806: "兴和县", 130807: "凉城县", 130808: "察右前旗", 130809: "察右中旗", 130810: "察右后旗", 130811: "四子王旗", 130812: "丰镇市" }, 1309: { 130902: "东胜区", 130903: "达拉特旗", 130904: "准格尔旗", 130905: "鄂托克前旗", 130906: "鄂托克旗", 130907: "杭锦旗", 130908: "乌审旗", 130909: "伊金霍洛旗" }, 1310: { 131002: "临河区", 131003: "五原县", 131004: "磴口县", 131005: "乌拉特前旗", 131006: "乌拉特中旗", 131007: "乌拉特后旗", 131008: "杭锦后旗" }, 1311: { 131102: "海勃湾区", 131103: "海南区", 131104: "乌达区" }, 1312: { 131201: "阿拉善左旗", 131202: "阿拉善右旗", 131203: "额济纳旗" }, 1401: { 140102: "小店区", 140103: "迎泽区", 140104: "杏花岭区", 140105: "尖草坪区", 140106: "万柏林区", 140107: "晋源区", 140108: "清徐县", 140109: "阳曲县", 140110: "娄烦县", 140111: "古交市", 140112: "开发区" }, 1402: { 140202: "城区", 140203: "矿区", 140204: "南郊区", 140205: "新荣区", 140206: "阳高县", 140207: "天镇县", 140208: "广灵县", 140209: "灵丘县", 140210: "浑源县", 140211: "左云县", 140212: "大同县", 140213: "开发区" }, 1403: { 140302: "城区", 140303: "矿区", 140304: "郊区", 140305: "平定县", 140306: "盂县" }, 1404: { 140402: "城区", 140403: "郊区", 140404: "长治县", 140405: "襄垣县", 140406: "屯留县", 140407: "平顺县", 140408: "黎城县", 140409: "壶关县", 140410: "长子县", 140411: "武乡县", 140412: "沁县", 140413: "沁源县", 140414: "潞城市" }, 1405: { 140502: "城区", 140503: "沁水县", 140504: "阳城县", 140505: "陵川县", 140506: "泽州县", 140507: "高平市" }, 1406: { 140602: "朔城区", 140603: "平鲁区", 140604: "山阴县", 140605: "应县", 140606: "右玉县", 140607: "怀仁县" }, 1407: { 140702: "忻府区", 140703: "定襄县", 140704: "五台县", 140705: "代县", 140706: "繁峙县", 140707: "宁武县", 140708: "静乐县", 140709: "神池县", 140710: "五寨县", 140711: "岢岚县", 140712: "河曲县", 140713: "保德县", 140714: "偏关县", 140715: "原平市" }, 1408: { 140802: "榆次区", 140803: "榆社县", 140804: "左权县", 140805: "和顺县", 140806: "昔阳县", 140807: "寿阳县", 140808: "太谷县", 140809: "祁县", 140810: "平遥县", 140811: "灵石县", 140812: "介休市" }, 1409: { 140902: "尧都区", 140903: "曲沃县", 140904: "翼城县", 140905: "襄汾县", 140906: "洪洞县", 140907: "古县", 140908: "安泽县", 140909: "浮山县", 140910: "吉县", 140911: "乡宁县", 140912: "大宁县", 140913: "隰县", 140914: "永和县", 140915: "蒲县", 140916: "汾西县", 140917: "侯马市", 140918: "霍州市" }, 1410: { 141002: "离石区", 141003: "文水县", 141004: "交城县", 141005: "兴县", 141006: "临县", 141007: "柳林县", 141008: "石楼县", 141009: "岚县", 141010: "方山县", 141011: "中阳县", 141012: "交口县", 141013: "孝义市", 141014: "汾阳市" }, 1411: { 141102: "盐湖区", 141103: "临猗县", 141104: "万荣县", 141105: "闻喜县", 141106: "稷山县", 141107: "新绛县", 141108: "绛县", 141109: "垣曲县", 141110: "夏县", 141111: "平陆县", 141112: "芮城县", 141113: "永济市", 141114: "河津市" }, 2001: { 200102: "和平区", 200103: "沈河区", 200104: "大东区", 200105: "皇姑区", 200106: "铁西区", 200107: "苏家屯区", 200108: "东陵区", 200109: "沈北新区", 200110: "于洪区", 200111: "辽中县", 200112: "康平县", 200113: "法库县", 200114: "新民市", 200115: "浑南新区" }, 2002: { 200202: "中山区", 200203: "西岗区", 200204: "沙河口区", 200205: "甘井子区", 200206: "旅顺口区", 200207: "金州区", 200208: "长海县", 200209: "瓦房店市", 200210: "普兰店市", 200211: "庄河市", 200212: "开发区" }, 2003: { 200302: "铁东区", 200303: "铁西区", 200304: "立山区", 200305: "千山区", 200306: "台安县", 200307: "岫岩县", 200308: "海城市", 200309: "高新区" }, 2004: { 200402: "古塔区", 200403: "凌河区", 200404: "太和区", 200405: "黑山县", 200406: "义县", 200407: "凌海市", 200408: "北镇市", 200409: "开发区" }, 2005: { 200502: "元宝区", 200503: "振兴区", 200504: "振安区", 200505: "宽甸县", 200506: "东港市", 200507: "凤城市" }, 2006: { 200602: "双台子区", 200603: "兴隆台区", 200604: "大洼县", 200605: "盘山县" }, 2007: { 200702: "银州区", 200703: "清河区", 200704: "铁岭县", 200705: "西丰县", 200706: "昌图县", 200707: "调兵山市", 200708: "开原市" }, 2008: { 200802: "新抚区", 200803: "东洲区", 200804: "望花区", 200805: "顺城区", 200806: "抚顺县", 200807: "新宾县", 200808: "清原县" }, 2009: { 200902: "站前区", 200904: "鲅鱼圈区", 200905: "老边区", 200906: "盖州市", 200907: "大石桥市", 200908: "开发区" }, 2010: { 201002: "白塔区", 201003: "文圣区", 201004: "宏伟区", 201005: "弓长岭区", 201006: "太子河区", 201007: "辽阳县", 201008: "灯塔市", 201009: "高新区" }, 2011: { 201102: "海州区", 201103: "新邱区", 201104: "太平区", 201105: "清河门区", 201106: "细河区", 201107: "阜新县", 201108: "彰武县" }, 2012: { 201202: "平山区", 201203: "溪湖区", 201204: "明山区", 201205: "南芬区", 201206: "本溪县", 201207: "桓仁县" }, 2013: { 201302: "双塔区", 201303: "龙城区", 201304: "朝阳县", 201305: "建平县", 201306: "喀左县", 201307: "北票市", 201308: "凌源市" }, 2014: { 201402: "连山区", 201403: "龙港区", 201404: "南票区", 201405: "绥中县", 201406: "建昌县", 201407: "兴城市" }, 2101: { 210102: "南关区", 210103: "宽城区", 210104: "朝阳区", 210105: "二道区", 210106: "绿园区", 210107: "双阳区", 210108: "农安县", 210109: "九台市", 210110: "榆树市", 210111: "德惠市", 210112: "开发区" }, 2102: { 210202: "昌邑区", 210203: "龙潭区", 210204: "船营区", 210205: "丰满区", 210206: "永吉县", 210207: "蛟河市", 210208: "桦甸市", 210209: "舒兰市", 210210: "磐石市", 210211: "开发区" }, 2103: { 210302: "铁西区", 210303: "铁东区", 210304: "梨树县", 210305: "伊通县", 210306: "公主岭市", 210307: "双辽市", 210308: "开发区" }, 2104: { 210402: "龙山区", 210403: "西安区", 210404: "东丰县", 210405: "东辽县" }, 2105: { 210502: "东昌区", 210503: "二道江区", 210504: "通化县", 210505: "辉南县", 210506: "柳河县", 210507: "梅河口市", 210508: "集安市" }, 2106: { 210602: "八道江区", 210603: "江源区", 210604: "抚松县", 210605: "靖宇县", 210606: "长白县", 210607: "临江市" }, 2107: { 210702: "宁江区", 210703: "前郭县", 210704: "长岭县", 210705: "乾安县", 210706: "扶余县" }, 2108: { 210802: "洮北区", 210803: "镇赉县", 210804: "通榆县", 210805: "洮南市", 210806: "大安市" }, 2109: { 210901: "延吉市", 210902: "图们市", 210903: "敦化市", 210904: "珲春市", 210905: "龙井市", 210906: "和龙市", 210907: "汪清县", 210908: "安图县", 210909: "高新区" }, 2201: { 220102: "道里区", 220103: "南岗区", 220104: "道外区", 220105: "平房区", 220106: "松北区", 220107: "香坊区", 220108: "呼兰区", 220109: "阿城区", 220110: "依兰县", 220111: "方正县", 220112: "宾县", 220113: "巴彦县", 220114: "木兰县", 220115: "通河县", 220116: "延寿县", 220117: "双城市", 220118: "尚志市", 220119: "五常市", 220120: "开发区" }, 2202: { 220202: "龙沙区", 220203: "建华区", 220204: "铁锋区", 220205: "昂昂溪区", 220206: "富拉尔基区", 220207: "碾子山区", 220208: "梅里斯区", 220209: "龙江县", 220210: "依安县", 220211: "泰来县", 220212: "甘南县", 220213: "富裕县", 220214: "克山县", 220215: "克东县", 220216: "拜泉县", 220217: "讷河市", 220218: "高新区" }, 2203: { 220302: "鸡冠区", 220303: "恒山区", 220304: "滴道区", 220305: "梨树区", 220306: "城子河区", 220307: "麻山区", 220308: "鸡东县", 220309: "虎林市", 220310: "密山市" }, 2204: { 220402: "向阳区", 220403: "工农区", 220404: "南山区", 220405: "兴安区", 220406: "东山区", 220407: "兴山区", 220408: "萝北县", 220409: "绥滨县" }, 2205: { 220502: "尖山区", 220503: "岭东区", 220504: "四方台区", 220505: "宝山区", 220506: "集贤县", 220507: "友谊县", 220508: "宝清县", 220509: "饶河县" }, 2206: { 220602: "萨尔图区", 220603: "龙凤区", 220604: "让胡路区", 220605: "红岗区", 220606: "大同区", 220607: "肇州县", 220608: "肇源县", 220609: "林甸县", 220610: "杜尔伯特县", 220611: "高新区" }, 2207: { 220702: "伊春区", 220703: "南岔区", 220704: "友好区", 220705: "西林区", 220706: "翠峦区", 220707: "新青区", 220708: "美溪区", 220709: "金山屯区", 220710: "五营区", 220711: "乌马河区", 220712: "汤旺河区", 220713: "带岭区", 220714: "乌伊岭区", 220715: "红星区", 220716: "上甘岭区", 220717: "嘉荫县", 220718: "铁力市" }, 2208: { 220802: "向阳区", 220803: "前进区", 220804: "东风区", 220805: "郊区", 220806: "桦南县", 220807: "桦川县", 220808: "汤原县", 220809: "抚远县", 220810: "同江市", 220811: "富锦市" }, 2209: { 220902: "新兴区", 220903: "桃山区", 220904: "茄子河区", 220905: "勃利县" }, 2210: { 221002: "东安区", 221003: "阳明区", 221004: "爱民区", 221005: "西安区", 221006: "东宁县", 221007: "林口县", 221008: "绥芬河市", 221009: "海林市", 221010: "宁安市", 221011: "穆棱市" }, 2211: { 221102: "爱辉区", 221103: "嫩江县", 221104: "逊克县", 221105: "孙吴县", 221106: "北安市", 221107: "五大连池市" }, 2212: { 221202: "北林区", 221203: "望奎县", 221204: "兰西县", 221205: "青冈县", 221206: "庆安县", 221207: "明水县", 221208: "绥棱县", 221209: "安达市", 221210: "肇东市", 221211: "海伦市" }, 2213: { 221301: "加格达奇区", 221302: "松岭区", 221303: "新林区", 221304: "呼中区", 221305: "呼玛县", 221306: "塔河县", 221307: "漠河县" }, 3101: { 310102: "玄武区", 310103: "白下区", 310104: "秦淮区", 310105: "建邺区", 310106: "鼓楼区", 310107: "下关区", 310108: "浦口区", 310109: "栖霞区", 310110: "雨花台区", 310111: "江宁区", 310112: "六合区", 310113: "溧水县", 310114: "高淳县", 310115: "开发区" }, 3102: { 310202: "沧浪区", 310203: "平江区", 310204: "金阊区", 310205: "虎丘区", 310206: "吴中区", 310207: "相城区", 310208: "常熟市", 310209: "张家港市", 310210: "昆山市", 310211: "吴江市", 310212: "太仓市", 310213: "工业园区", 310214: "高新区" }, 3103: { 310302: "崇安区", 310303: "南长区", 310304: "北塘区", 310305: "锡山区", 310306: "惠山区", 310307: "滨湖区", 310308: "江阴市", 310309: "宜兴市", 310310: "高新区" }, 3104: { 310402: "京口区", 310403: "润州区", 310404: "丹徒区", 310405: "丹阳市", 310406: "扬中市", 310407: "句容市", 310408: "开发区" }, 3105: { 310502: "广陵区", 310503: "邗江区", 310505: "宝应县", 310506: "仪征市", 310507: "高邮市", 310508: "江都区", 310509: "开发区" }, 3106: { 310602: "崇川区", 310603: "港闸区", 310604: "海安县", 310605: "如东县", 310606: "启东市", 310607: "如皋市", 310608: "通州市", 310609: "海门市", 310610: "开发区" }, 3107: { 310702: "天宁区", 310703: "钟楼区", 310704: "戚墅堰区", 310705: "新北区", 310706: "武进区", 310707: "溧阳市", 310708: "金坛市" }, 3108: { 310802: "鼓楼区", 310803: "云龙区", 310804: "九里区", 310805: "贾汪区", 310806: "泉山区", 310807: "丰县", 310808: "沛县", 310809: "铜山县", 310810: "睢宁县", 310811: "新沂市", 310812: "邳州市", 310813: "开发区" }, 3109: { 310902: "连云区", 310903: "新浦区", 310904: "海州区", 310905: "赣榆县", 310906: "东海县", 310907: "灌云县", 310908: "灌南县", 310909: "开发区" }, 3110: { 311002: "亭湖区", 311003: "盐都区", 311004: "响水县", 311005: "滨海县", 311006: "阜宁县", 311007: "射阳县", 311008: "建湖县", 311009: "东台市", 311010: "大丰市", 311011: "开发区" }, 3111: { 311102: "清河区", 311103: "楚州区", 311104: "淮阴区", 311105: "清浦区", 311106: "涟水县", 311107: "洪泽县", 311108: "盱眙县", 311109: "金湖县", 311110: "开发区" }, 3112: { 311202: "海陵区", 311203: "高港区", 311204: "兴化市", 311205: "靖江市", 311206: "泰兴市", 311207: "姜堰市", 311208: "高新区" }, 3113: { 311302: "宿城区", 311303: "宿豫区", 311304: "沭阳县", 311305: "泗阳县", 311306: "泗洪县" }, 3201: { 320102: "历下区", 320103: "市中区", 320104: "槐荫区", 320105: "天桥区", 320106: "历城区", 320107: "长清区", 320108: "平阴县", 320109: "济阳县", 320110: "商河县", 320111: "章丘市", 320112: "高新区" }, 3202: { 320202: "市南区", 320203: "市北区", 320204: "四方区", 320205: "黄岛区", 320206: "崂山区", 320207: "李沧区", 320208: "城阳区", 320209: "胶州市", 320210: "即墨市", 320211: "平度市", 320212: "胶南市", 320213: "莱西市", 320214: "开发区" }, 3203: { 320302: "芝罘区", 320303: "福山区", 320304: "牟平区", 320305: "莱山区", 320306: "长岛县", 320307: "龙口市", 320308: "莱阳市", 320309: "莱州市", 320310: "蓬莱市", 320311: "招远市", 320312: "栖霞市", 320313: "海阳市", 320314: "高新区", 320315: "开发区" }, 3204: { 320402: "淄川区", 320403: "张店区", 320404: "博山区", 320405: "临淄区", 320406: "周村区", 320407: "桓台县", 320408: "高青县", 320409: "沂源县", 320410: "高新区" }, 3205: { 320502: "泰山区", 320503: "岱岳区", 320504: "宁阳县", 320505: "东平县", 320506: "新泰市", 320507: "肥城市" }, 3206: { 320602: "潍城区", 320603: "寒亭区", 320604: "坊子区", 320605: "奎文区", 320606: "临朐县", 320607: "昌乐县", 320608: "青州市", 320609: "诸城市", 320610: "寿光市", 320611: "安丘市", 320612: "高密市", 320613: "昌邑市", 320614: "开发区" }, 3207: { 320702: "市中区", 320703: "任城区", 320704: "微山县", 320705: "鱼台县", 320706: "金乡县", 320707: "嘉祥县", 320708: "汶上县", 320709: "泗水县", 320710: "梁山县", 320711: "曲阜市", 320712: "兖州市", 320713: "邹城市", 320714: "高新区" }, 3208: { 320802: "市中区", 320803: "薛城区", 320804: "峄城区", 320805: "台儿庄区", 320806: "山亭区", 320807: "滕州市", 320808: "高新区" }, 3209: { 320902: "德城区", 320903: "陵县", 320904: "宁津县", 320905: "庆云县", 320906: "临邑县", 320907: "齐河县", 320908: "平原县", 320909: "夏津县", 320910: "武城县", 320911: "乐陵市", 320912: "禹城市" }, 3210: { 321002: "环翠区", 321003: "文登市", 321004: "荣成市", 321005: "乳山市", 321006: "开发区" }, 3211: { 321102: "东港区", 321103: "岚山区", 321104: "五莲县", 321105: "莒县", 321106: "开发区" }, 3212: { 321202: "莱城区", 321203: "钢城区" }, 3213: { 321302: "滨城区", 321303: "惠民县", 321304: "阳信县", 321305: "无棣县", 321306: "沾化县", 321307: "博兴县", 321308: "邹平县" }, 3214: { 321402: "东营区", 321403: "河口区", 321404: "垦利县", 321405: "利津县", 321406: "广饶县", 321407: "开发区" }, 3215: { 321502: "东昌府区", 321503: "阳谷县", 321504: "莘县", 321505: "茌平县", 321506: "东阿县", 321507: "冠县", 321508: "高唐县", 321509: "临清市", 321510: "高新区" }, 3216: { 321602: "牡丹区", 321603: "曹县", 321604: "单县", 321605: "成武县", 321606: "巨野县", 321607: "郓城县", 321608: "鄄城县", 321609: "定陶县", 321610: "东明县" }, 3217: { 321702: "兰山区", 321703: "罗庄区", 321704: "河东区", 321705: "沂南县", 321706: "郯城县", 321707: "沂水县", 321708: "苍山县", 321709: "费县", 321710: "平邑县", 321711: "莒南县", 321712: "蒙阴县", 321713: "临沭县", 321714: "开发区" }, 3301: { 330102: "瑶海区", 330103: "庐阳区", 330104: "蜀山区", 330105: "包河区", 330106: "长丰县", 330107: "肥东县", 330108: "肥西县", 330109: "开发区" }, 3302: { 330202: "镜湖区", 330203: "弋江区", 330204: "鸠江区", 330205: "三山区", 330206: "芜湖县", 330207: "繁昌县", 330208: "南陵县", 330209: "开发区" }, 3303: { 330302: "龙子湖区", 330303: "蚌山区", 330304: "禹会区", 330305: "淮上区", 330306: "怀远县", 330307: "五河县", 330308: "固镇县", 330309: "高新区" }, 3304: { 330402: "金家庄区", 330403: "花山区", 330404: "雨山区", 330405: "当涂县", 330406: "开发区" }, 3305: { 330502: "杜集区", 330503: "相山区", 330504: "烈山区", 330505: "濉溪县" }, 3306: { 330602: "铜官山区", 330603: "狮子山区", 330604: "郊区", 330605: "铜陵县", 330606: "开发区" }, 3307: { 330702: "迎江区", 330703: "大观区", 330704: "宜秀区", 330705: "怀宁县", 330706: "枞阳县", 330707: "潜山县", 330708: "太湖县", 330709: "宿松县", 330710: "望江县", 330711: "岳西县", 330712: "桐城市", 330713: "开发区" }, 3308: { 330802: "屯溪区", 330803: "黄山区", 330804: "徽州区", 330805: "歙县", 330806: "休宁县", 330807: "黟县", 330808: "祁门县" }, 3309: { 330902: "琅琊区", 330903: "南谯区", 330904: "来安县", 330905: "全椒县", 330906: "定远县", 330907: "凤阳县", 330908: "天长市", 330909: "明光市" }, 3310: { 331002: "埇桥区", 331003: "砀山县", 331004: "萧县", 331005: "灵璧县", 331006: "泗县" }, 3311: { 331102: "贵池区", 331103: "东至县", 331104: "石台县", 331105: "青阳县" }, 3312: { 331202: "大通区", 331203: "田家庵区", 331204: "谢家集区", 331205: "八公山区", 331206: "潘集区", 331207: "凤台县" }, 3313: { 331302: "居巢区", 331303: "庐江县", 331304: "无为县", 331305: "含山县", 331306: "和县" }, 3314: { 331402: "颍州区", 331403: "颍东区", 331404: "颍泉区", 331405: "临泉县", 331406: "太和县", 331407: "阜南县", 331408: "颍上县", 331409: "界首市" }, 3315: { 331502: "金安区", 331503: "裕安区", 331504: "寿县", 331505: "霍邱县", 331506: "舒城县", 331507: "金寨县", 331508: "霍山县" }, 3316: { 331602: "宣州区", 331603: "郎溪县", 331604: "广德县", 331605: "泾县", 331606: "绩溪县", 331607: "旌德县", 331608: "宁国市" }, 3317: { 331702: "谯城区", 331703: "涡阳县", 331704: "蒙城县", 331705: "利辛县" }, 3401: { 340102: "鼓楼区", 340103: "台江区", 340104: "仓山区", 340105: "马尾区", 340106: "晋安区", 340107: "闽侯县", 340108: "连江县", 340109: "罗源县", 340110: "闽清县", 340111: "永泰县", 340112: "平潭县", 340113: "福清市", 340114: "长乐市", 340115: "开发区" }, 3402: { 340202: "思明区", 340203: "海沧区", 340204: "湖里区", 340205: "集美区", 340206: "同安区", 340207: "翔安区", 340208: "开发区" }, 3403: { 340302: "芗城区", 340303: "龙文区", 340304: "云霄县", 340305: "漳浦县", 340306: "诏安县", 340307: "长泰县", 340308: "东山县", 340309: "南靖县", 340310: "平和县", 340311: "华安县", 340312: "龙海市", 340313: "开发区" }, 3404: { 340402: "鲤城区", 340403: "丰泽区", 340404: "洛江区", 340405: "泉港区", 340406: "惠安县", 340407: "安溪县", 340408: "永春县", 340409: "德化县", 340410: "金门县", 340411: "石狮市", 340412: "晋江市", 340413: "南安市", 340414: "开发区" }, 3405: { 340502: "城厢区", 340503: "涵江区", 340504: "荔城区", 340505: "秀屿区", 340506: "仙游县" }, 3406: { 340602: "梅列区", 340603: "三元区", 340604: "明溪县", 340605: "清流县", 340606: "宁化县", 340607: "大田县", 340608: "尤溪县", 340609: "沙县", 340610: "将乐县", 340611: "泰宁县", 340612: "建宁县", 340613: "永安市" }, 3407: { 340702: "延平区", 340703: "顺昌县", 340704: "浦城县", 340705: "光泽县", 340706: "松溪县", 340707: "政和县", 340708: "邵武市", 340709: "武夷山市", 340710: "建瓯市", 340711: "建阳市" }, 3408: { 340802: "新罗区", 340803: "长汀县", 340804: "永定县", 340805: "上杭县", 340806: "武平县", 340807: "连城县", 340808: "漳平市" }, 3409: { 340902: "蕉城区", 340903: "霞浦县", 340904: "古田县", 340905: "屏南县", 340906: "寿宁县", 340907: "周宁县", 340908: "柘荣县", 340909: "福安市", 340910: "福鼎市" }, 3501: { 350102: "上城区", 350103: "下城区", 350104: "江干区", 350105: "拱墅区", 350106: "西湖区", 350107: "滨江区", 350108: "萧山区", 350109: "余杭区", 350110: "桐庐县", 350111: "淳安县", 350112: "建德市", 350113: "富阳市", 350114: "临安市", 350115: "开发区" }, 3502: { 350202: "海曙区", 350203: "江东区", 350204: "江北区", 350205: "北仑区", 350206: "镇海区", 350207: "鄞州区", 350208: "象山县", 350209: "宁海县", 350210: "余姚市", 350211: "慈溪市", 350212: "奉化市", 350213: "开发区" }, 3503: { 350302: "鹿城区", 350303: "龙湾区", 350304: "瓯海区", 350305: "洞头县", 350306: "永嘉县", 350307: "平阳县", 350308: "苍南县", 350309: "文成县", 350310: "泰顺县", 350311: "瑞安市", 350312: "乐清市", 350313: "开发区" }, 3504: { 350402: "南湖区", 350403: "秀洲区", 350404: "嘉善县", 350405: "海盐县", 350406: "海宁市", 350407: "平湖市", 350408: "桐乡市", 350409: "开发区" }, 3505: { 350502: "吴兴区", 350503: "南浔区", 350504: "德清县", 350505: "长兴县", 350506: "安吉县", 350507: "开发区" }, 3506: { 350602: "越城区", 350603: "绍兴县", 350604: "新昌县", 350605: "诸暨市", 350606: "上虞市", 350607: "嵊州市", 350608: "开发区" }, 3507: { 350702: "婺城区", 350703: "金东区", 350704: "武义县", 350705: "浦江县", 350706: "磐安县", 350707: "兰溪市", 350708: "义乌市", 350709: "东阳市", 350710: "永康市", 350711: "开发区" }, 3508: { 350802: "柯城区", 350803: "衢江区", 350804: "常山县", 350805: "开化县", 350806: "龙游县", 350807: "江山市" }, 3509: { 350902: "定海区", 350903: "普陀区", 350904: "岱山县", 350905: "嵊泗县" }, 3510: { 351002: "椒江区", 351003: "黄岩区", 351004: "路桥区", 351005: "玉环县", 351006: "三门县", 351007: "天台县", 351008: "仙居县", 351009: "温岭市", 351010: "临海市" }, 3511: { 351102: "莲都区", 351103: "青田县", 351104: "缙云县", 351105: "遂昌县", 351106: "松阳县", 351107: "云和县", 351108: "庆元县", 351109: "景宁县", 351110: "龙泉市" }, 3601: { 360102: "东湖区", 360103: "西湖区", 360104: "青云谱区", 360105: "湾里区", 360106: "青山湖区", 360107: "南昌县", 360108: "新建县", 360109: "安义县", 360110: "进贤县", 360111: "开发区" }, 3602: { 360202: "昌江区", 360203: "珠山区", 360204: "浮梁县", 360205: "乐平市", 360206: "高新区" }, 3603: { 360302: "安源区", 360303: "湘东区", 360304: "莲花县", 360305: "上栗县", 360306: "芦溪县", 360307: "开发区" }, 3604: { 360402: "庐山区", 360403: "浔阳区", 360404: "九江县", 360405: "武宁县", 360406: "修水县", 360407: "永修县", 360408: "德安县", 360409: "星子县", 360410: "都昌县", 360411: "湖口县", 360412: "彭泽县", 360413: "瑞昌市", 360414: "开发区" }, 3605: { 360502: "渝水区", 360503: "分宜县", 360504: "高新区" }, 3606: { 360602: "月湖区", 360603: "余江县", 360604: "贵溪市" }, 3607: { 360702: "章贡区", 360703: "赣县", 360704: "信丰县", 360705: "大余县", 360706: "上犹县", 360707: "崇义县", 360708: "安远县", 360709: "龙南县", 360710: "定南县", 360711: "全南县", 360712: "宁都县", 360713: "于都县", 360714: "兴国县", 360715: "会昌县", 360716: "寻乌县", 360717: "石城县", 360718: "瑞金市", 360719: "南康市", 360720: "开发区" }, 3608: { 360802: "吉州区", 360803: "青原区", 360804: "吉安县", 360805: "吉水县", 360806: "峡江县", 360807: "新干县", 360808: "永丰县", 360809: "泰和县", 360810: "遂川县", 360811: "万安县", 360812: "安福县", 360813: "永新县", 360814: "井冈山市" }, 3609: { 360902: "袁州区", 360903: "奉新县", 360904: "万载县", 360905: "上高县", 360906: "宜丰县", 360907: "靖安县", 360908: "铜鼓县", 360909: "丰城市", 360910: "樟树市", 360911: "高安市" }, 3610: { 361002: "临川区", 361003: "南城县", 361004: "黎川县", 361005: "南丰县", 361006: "崇仁县", 361007: "乐安县", 361008: "宜黄县", 361009: "金溪县", 361010: "资溪县", 361011: "东乡县", 361012: "广昌县" }, 3611: { 361102: "信州区", 361103: "上饶县", 361104: "广丰县", 361105: "玉山县", 361106: "铅山县", 361107: "横峰县", 361108: "弋阳县", 361109: "余干县", 361110: "鄱阳县", 361111: "万年县", 361112: "婺源县", 361113: "德兴市", 361114: "开发区" }, 4001: { 400102: "荔湾区", 400103: "越秀区", 400104: "海珠区", 400105: "天河区", 400106: "白云区", 400107: "黄埔区", 400108: "番禺区", 400109: "花都区", 400110: "南沙区", 400111: "萝岗区", 400112: "增城市", 400113: "从化市", 400114: "开发区" }, 4002: { 400202: "罗湖区", 400203: "福田区", 400204: "南山区", 400205: "宝安区", 400206: "龙岗区", 400207: "盐田区", 400208: "高新区" }, 4003: { 400302: "香洲区", 400303: "斗门区", 400304: "金湾区", 400305: "高新区" }, 4004: { 400402: "龙湖区", 400403: "金平区", 400404: "濠江区", 400405: "潮阳区", 400406: "潮南区", 400407: "澄海区", 400408: "南澳县" }, 4005: { 400502: "武江区", 400503: "浈江区", 400504: "曲江区", 400505: "始兴县", 400506: "仁化县", 400507: "翁源县", 400508: "乳源县", 400509: "新丰县", 400510: "乐昌市", 400511: "南雄市" }, 4006: { 400602: "源城区", 400603: "紫金县", 400604: "龙川县", 400605: "连平县", 400606: "和平县", 400607: "东源县" }, 4007: { 400702: "梅江区", 400703: "梅县", 400704: "大埔县", 400705: "丰顺县", 400706: "五华县", 400707: "平远县", 400708: "蕉岭县", 400709: "兴宁市" }, 4008: { 400802: "惠城区", 400803: "惠阳区", 400804: "博罗县", 400805: "惠东县", 400806: "龙门县", 400807: "高新区" }, 4009: { 400902: "城区", 400903: "海丰县", 400904: "陆河县", 400905: "陆丰市" }, 4010: { 401001: "高新区" }, 4011: { 401101: "高新区" }, 4012: { 401202: "蓬江区", 401203: "江海区", 401204: "新会区", 401205: "台山市", 401206: "开平市", 401207: "鹤山市", 401208: "恩平市", 401209: "高新区" }, 4013: { 401302: "禅城区", 401303: "南海区", 401304: "顺德区", 401305: "三水区", 401306: "高明区", 401307: "高新区" }, 4014: { 401402: "江城区", 401403: "阳西县", 401404: "阳东县", 401405: "阳春市" }, 4015: { 401502: "赤坎区", 401503: "霞山区", 401504: "坡头区", 401505: "麻章区", 401506: "遂溪县", 401507: "徐闻县", 401508: "廉江市", 401509: "雷州市", 401510: "吴川市", 401511: "开发区" }, 4016: { 401602: "茂南区", 401603: "茂港区", 401604: "电白县", 401605: "高州市", 401606: "化州市", 401607: "信宜市" }, 4017: { 401702: "端州区", 401703: "鼎湖区", 401704: "广宁县", 401705: "怀集县", 401706: "封开县", 401707: "德庆县", 401708: "高要市", 401709: "四会市", 401710: "高新区" }, 4018: { 401802: "清城区", 401803: "佛冈县", 401804: "阳山县", 401805: "连山县", 401806: "连南县", 401807: "清新县", 401808: "英德市", 401809: "连州市" }, 4019: { 401902: "湘桥区", 401903: "潮安县", 401904: "饶平县" }, 4020: { 402002: "榕城区", 402003: "揭东县", 402004: "揭西县", 402005: "惠来县", 402006: "普宁市" }, 4021: { 402102: "云城区", 402103: "新兴县", 402104: "郁南县", 402105: "云安县", 402106: "罗定市" }, 4101: { 410102: "兴宁区", 410103: "青秀区", 410104: "江南区", 410105: "西乡塘区", 410106: "良庆区", 410107: "邕宁区", 410108: "武鸣县", 410109: "隆安县", 410110: "马山县", 410111: "上林县", 410112: "宾阳县", 410113: "横县", 410114: "开发区" }, 4102: { 410202: "城中区", 410203: "鱼峰区", 410204: "柳南区", 410205: "柳北区", 410206: "柳江县", 410207: "柳城县", 410208: "鹿寨县", 410209: "融安县", 410210: "融水县", 410211: "三江县", 410212: "高新区" }, 4103: { 410302: "秀峰区", 410303: "叠彩区", 410304: "象山区", 410305: "七星区", 410306: "雁山区", 410307: "阳朔县", 410308: "临桂县", 410309: "灵川县", 410310: "全州县", 410311: "兴安县", 410312: "永福县", 410313: "灌阳县", 410314: "龙胜县", 410315: "资源县", 410316: "平乐县", 410317: "荔蒲县", 410318: "恭城县", 410319: "高新区" }, 4104: { 410402: "万秀区", 410403: "蝶山区", 410404: "长洲区", 410405: "苍梧县", 410406: "藤县", 410407: "蒙山县", 410408: "岑溪市" }, 4105: { 410502: "海城区", 410503: "银海区", 410504: "铁山港区", 410505: "合浦县" }, 4106: { 410602: "港口区", 410603: "防城区", 410604: "上思县", 410605: "东兴市" }, 4107: { 410702: "钦南区", 410703: "钦北区", 410704: "灵山县", 410705: "浦北县", 410706: "开发区" }, 4108: { 410802: "港北区", 410803: "港南区", 410804: "覃塘区", 410805: "平南县", 410806: "桂平市" }, 4109: { 410902: "玉州区", 410903: "容县", 410904: "陆川县", 410905: "博白县", 410906: "兴业县", 410907: "北流市" }, 4110: { 411002: "右江区", 411003: "田阳县", 411004: "田东县", 411005: "平果县", 411006: "德保县", 411007: "靖西县", 411008: "那坡县", 411009: "凌云县", 411010: "乐业县", 411011: "田林县", 411012: "西林县", 411013: "隆林县" }, 4111: { 411102: "八步区", 411103: "昭平县", 411104: "钟山县", 411105: "富川县" }, 4112: { 411202: "金城江区", 411203: "南丹县", 411204: "天峨县", 411205: "凤山县", 411206: "东兰县", 411207: "罗城县", 411208: "环江县", 411209: "巴马县", 411210: "都安县", 411211: "大化县", 411212: "宜州市" }, 4113: { 411302: "兴宾区", 411303: "忻城县", 411304: "象州县", 411305: "武宣县", 411306: "金秀县", 411307: "合山市" }, 4114: { 411402: "江洲区", 411403: "扶绥县", 411404: "宁明县", 411405: "龙州县", 411406: "大新县", 411407: "天等县", 411408: "凭祥市" }, 4201: { 420102: "秀英区", 420103: "龙华区", 420104: "琼山区", 420105: "美兰区", 420106: "高新区" }, 4219: { 421902: "开发区" }, 6101: { 610102: "锦江区", 610103: "青羊区", 610104: "金牛区", 610105: "武侯区", 610106: "成华区", 610107: "龙泉驿区", 610108: "青白江区", 610109: "新都区", 610110: "温江区", 610111: "金堂县", 610112: "双流县", 610113: "郫县", 610114: "大邑县", 610115: "蒲江县", 610116: "新津县", 610117: "都江堰市", 610118: "彭州市", 610119: "邛崃市", 610120: "崇州市", 610121: "高新区", 610122: "开发区" }, 6102: { 610202: "旌阳区", 610203: "中江县", 610204: "罗江县", 610205: "广汉市", 610206: "什邡市", 610207: "绵竹市", 610208: "开发区" }, 6103: { 610302: "涪城区", 610303: "游仙区", 610304: "三台县", 610305: "盐亭县", 610306: "安县", 610307: "梓潼县", 610308: "北川县", 610309: "平武县", 610310: "江油市", 610311: "高新区" }, 6104: { 610402: "东坡区", 610403: "仁寿县", 610404: "彭山县", 610405: "洪雅县", 610406: "丹棱县", 610407: "青神县" }, 6105: { 610502: "江阳区", 610503: "纳溪区", 610504: "龙马潭区", 610505: "泸县", 610506: "合江县", 610507: "叙永县", 610508: "古蔺县" }, 6106: { 610602: "顺庆区", 610603: "高坪区", 610604: "嘉陵区", 610605: "南部县", 610606: "营山县", 610607: "蓬安县", 610608: "仪陇县", 610609: "西充县", 610610: "阆中市" }, 6107: { 610702: "自流井区", 610703: "贡井区", 610704: "大安区", 610705: "沿滩区", 610706: "荣县", 610707: "富顺县", 610708: "高新区" }, 6108: { 610802: "市中区", 610803: "东兴区", 610804: "威远县", 610805: "资中县", 610806: "隆昌县" }, 6109: { 610902: "翠屏区", 610903: "宜宾县", 610904: "南溪县", 610905: "江安县", 610906: "长宁县", 610907: "高县", 610908: "珙县", 610909: "筠连县", 610910: "兴文县", 610911: "屏山县" }, 6110: { 611002: "市中区", 611003: "沙湾区", 611004: "五通桥区", 611005: "金口河区", 611006: "犍为县", 611007: "井研县", 611008: "夹江县", 611009: "沐川县", 611010: "峨边县", 611011: "马边县", 611012: "峨眉山市" }, 6111: { 611102: "雨城区", 611103: "名山县", 611104: "荥经县", 611105: "汉源县", 611106: "石棉县", 611107: "天全县", 611108: "芦山县", 611109: "宝兴县" }, 6112: { 611202: "船山区", 611203: "安居区", 611204: "蓬溪县", 611205: "射洪县", 611206: "大英县" }, 6113: { 611302: "通川区", 611303: "达县", 611304: "宣汉县", 611305: "开江县", 611306: "大竹县", 611307: "渠县", 611308: "万源市" }, 6114: { 611402: "巴州区", 611403: "通江县", 611404: "南江县", 611405: "平昌县" }, 6115: { 611502: "市中区", 611503: "元坝区", 611504: "朝天区", 611505: "旺苍县", 611506: "青川县", 611507: "剑阁县", 611508: "苍溪县" }, 6116: { 611602: "广安区", 611603: "岳池县", 611604: "武胜县", 611605: "邻水县", 611606: "华蓥市", 611607: "开发区" }, 6117: { 611702: "雁江区", 611703: "安岳县", 611704: "乐至县", 611705: "简阳市" }, 6118: { 611801: "汶川县", 611802: "理县", 611803: "茂县", 611804: "松潘县", 611805: "九寨沟县", 611806: "金川县", 611807: "小金县", 611808: "黑水县", 611809: "马尔康县", 611810: "壤塘县", 611811: "阿坝县", 611812: "若尔盖县", 611813: "红原县" }, 6119: { 611902: "东区", 611903: "西区", 611904: "仁和区", 611905: "米易县", 611906: "盐边县" }, 6120: { 612001: "康定县", 612002: "泸定县", 612003: "丹巴县", 612004: "九龙县", 612005: "雅江县", 612006: "道孚县", 612007: "炉霍县", 612008: "甘孜县", 612009: "新龙县", 612010: "德格县", 612011: "白玉县", 612012: "石渠县", 612013: "色达县", 612014: "理塘县", 612015: "巴塘县", 612016: "乡城县", 612017: "稻城县", 612018: "得荣县" }, 6121: { 612101: "西昌市", 612102: "木里县", 612103: "盐源县", 612104: "德昌县", 612105: "会理县", 612106: "会东县", 612107: "宁南县", 612108: "普格县", 612109: "布拖县", 612110: "金阳县", 612111: "昭觉县", 612112: "喜德县", 612113: "冕宁县", 612114: "越西县", 612115: "甘洛县", 612116: "美姑县", 612117: "雷波县" }, 6201: { 620102: "五华区", 620103: "盘龙区", 620104: "官渡区", 620105: "西山区", 620106: "东川区", 620107: "呈贡县", 620108: "晋宁县", 620109: "富民县", 620110: "宜良县", 620111: "石林县", 620112: "嵩明县", 620113: "禄劝县", 620114: "寻甸回族县", 620115: "安宁市", 620116: "开发区" }, 6202: { 620202: "麒麟区", 620203: "马龙县", 620204: "陆良县", 620205: "师宗县", 620206: "罗平县", 620207: "富源县", 620208: "会泽县", 620209: "沾益县", 620210: "宣威市", 620211: "开发区" }, 6203: { 620302: "红塔区", 620303: "江川县", 620304: "澄江县", 620305: "通海县", 620306: "华宁县", 620307: "易门县", 620308: "峨山县", 620309: "新平县", 620310: "元江哈尼族县" }, 6204: { 620402: "昭阳区", 620403: "鲁甸县", 620404: "巧家县", 620405: "盐津县", 620406: "大关县", 620407: "永善县", 620408: "绥江县", 620409: "镇雄县", 620410: "彝良县", 620411: "威信县", 620412: "水富县" }, 6205: { 620501: "楚雄市", 620502: "双柏县", 620503: "牟定县", 620504: "南华县", 620505: "姚安县", 620506: "大姚县", 620507: "永仁县", 620508: "元谋县", 620509: "武定县", 620510: "禄丰县" }, 6206: { 620601: "个旧市", 620602: "开远市", 620603: "蒙自县", 620604: "屏边县", 620605: "建水县", 620606: "石屏县", 620607: "弥勒县", 620608: "泸西县", 620609: "元阳县", 620610: "红河县", 620611: "金平县", 620612: "绿春县", 620613: "河口县" }, 6207: { 620702: "古城区", 620703: "玉龙县", 620704: "永胜县", 620705: "华坪县", 620706: "宁蒗县" }, 6208: { 620801: "香格里拉县", 620802: "德钦县", 620803: "维西县" }, 6209: { 620901: "文山县", 620902: "砚山县", 620903: "西畴县", 620904: "麻栗坡县", 620905: "马关县", 620906: "丘北县", 620907: "广南县", 620908: "富宁县" }, 6210: { 621001: "景洪市", 621002: "勐海县", 621003: "勐腊县" }, 6211: { 621102: "思茅区", 621103: "宁洱哈尼族县", 621104: "墨江县", 621105: "景东县", 621106: "景谷傣族县", 621107: "镇沅县", 621108: "江城哈尼族县", 621109: "孟连县", 621110: "澜沧县", 621111: "西盟县" }, 6212: { 621201: "大理市", 621202: "漾濞县", 621203: "祥云县", 621204: "宾川县", 621205: "弥渡县", 621206: "南涧县", 621207: "巍山彝族县", 621208: "永平县", 621209: "云龙县", 621210: "洱源县", 621211: "剑川县", 621212: "鹤庆县" }, 6213: { 621302: "隆阳区", 621303: "施甸县", 621304: "腾冲县", 621305: "龙陵县", 621306: "昌宁县" }, 6214: { 621401: "瑞丽市", 621402: "潞西市", 621403: "梁河县", 621404: "盈江县", 621405: "陇川县" }, 6215: { 621501: "泸水县", 621502: "福贡县", 621503: "贡山县", 621504: "兰坪县" }, 6216: { 621602: "临翔区", 621603: "凤庆县", 621604: "云县", 621605: "永德县", 621606: "镇康县", 621607: "双江县", 621608: "耿马傣族县", 621609: "沧源县" }, 6301: { 630102: "南明区", 630103: "云岩区", 630104: "花溪区", 630105: "乌当区", 630106: "白云区", 630107: "小河区", 630108: "开阳县", 630109: "息烽县", 630110: "修文县", 630111: "清镇市", 630112: "开发区" }, 6302: { 630202: "红花岗区", 630203: "汇川区", 630204: "遵义县", 630205: "桐梓县", 630206: "绥阳县", 630207: "正安县", 630208: "道真仡佬族县", 630209: "务川仡佬族县", 630210: "凤冈县", 630211: "湄潭县", 630212: "余庆县", 630213: "习水县", 630214: "赤水市", 630215: "仁怀市", 630216: "开发区" }, 6303: { 630301: "钟山区", 630302: "六枝特区", 630303: "水城县", 630304: "盘县" }, 6304: { 630402: "西秀区", 630403: "平坝县", 630404: "普定县", 630405: "镇宁布依族县", 630406: "关岭布依族县", 630407: "紫云县" }, 6305: { 630501: "兴义市", 630502: "兴仁县", 630503: "普安县", 630504: "晴隆县", 630505: "贞丰县", 630506: "望谟县", 630507: "册亨县", 630508: "安龙县" }, 6306: { 630601: "都匀市", 630602: "福泉市", 630603: "荔波县", 630604: "贵定县", 630605: "瓮安县", 630606: "独山县", 630607: "平塘县", 630608: "罗甸县", 630609: "长顺县", 630610: "龙里县", 630611: "惠水县", 630612: "三都县" }, 6307: { 630701: "铜仁市", 630702: "江口县", 630703: "玉屏县", 630704: "石阡县", 630705: "思南县", 630706: "印江县", 630707: "德江县", 630708: "沿河县", 630709: "松桃县", 630710: "万山特区" }, 6308: { 630801: "毕节市", 630802: "大方县", 630803: "黔西县", 630804: "金沙县", 630805: "织金县", 630806: "纳雍县", 630807: "威宁县", 630808: "赫章县" }, 6309: { 630901: "凯里市", 630902: "黄平县", 630903: "施秉县", 630904: "三穗县", 630905: "镇远县", 630906: "岑巩县", 630907: "天柱县", 630908: "锦屏县", 630909: "剑河县", 630910: "台江县", 630911: "黎平县", 630912: "榕江县", 630913: "从江县", 630914: "雷山县", 630915: "麻江县", 630916: "丹寨县" }, 6401: { 640102: "城关区", 640103: "林周县", 640104: "当雄县", 640105: "尼木县", 640106: "曲水县", 640107: "堆龙德庆县", 640108: "达孜县", 640109: "墨竹工卡县", 640110: "开发区" }, 6402: { 640201: "日喀则市", 640202: "南木林县", 640203: "江孜县", 640204: "定日县", 640205: "萨迦县", 640206: "拉孜县", 640207: "昂仁县", 640208: "谢通门县", 640209: "白朗县", 640210: "仁布县", 640211: "康马县", 640212: "定结县", 640213: "仲巴县", 640214: "亚东县", 640215: "吉隆县", 640216: "聂拉木县", 640217: "萨嘎县", 640218: "岗巴县" }, 6403: { 640301: "乃东县", 640302: "扎囊县", 640303: "贡嘎县", 640304: "桑日县", 640305: "琼结县", 640306: "曲松县", 640307: "措美县", 640308: "洛扎县", 640309: "加查县", 640310: "隆子县", 640311: "错那县", 640312: "浪卡子县" }, 6404: { 640401: "林芝县", 640402: "工布江达县", 640403: "米林县", 640404: "墨脱县", 640405: "波密县", 640406: "察隅县", 640407: "朗县" }, 6405: { 640501: "昌都县", 640502: "江达县", 640503: "贡觉县", 640504: "类乌齐县", 640505: "丁青县", 640506: "察雅县", 640507: "八宿县", 640508: "左贡县", 640509: "芒康县", 640510: "洛隆县", 640511: "边坝县" }, 6406: { 640601: "那曲县", 640602: "嘉黎县", 640603: "比如县", 640604: "聂荣县", 640605: "安多县", 640606: "申扎县", 640607: "索县", 640608: "班戈县", 640609: "巴青县", 640610: "尼玛县" }, 6407: { 640701: "普兰县", 640702: "札达县", 640703: "噶尔县", 640704: "日土县", 640705: "革吉县", 640706: "改则县", 640707: "措勤县" }, 7001: { 700102: "中原区", 700103: "二七区", 700104: "管城回族区", 700105: "金水区", 700106: "上街区", 700107: "惠济区", 700108: "中牟县", 700109: "巩义市", 700110: "荥阳市", 700111: "新密市", 700112: "新郑市", 700113: "登封市", 700114: "经济开发区", 700115: "郑东新区", 700116: "高新区" }, 7002: { 700202: "老城区", 700203: "西工区", 700204: "瀍河回族区", 700205: "涧西区", 700206: "吉利区", 700207: "洛龙区", 700208: "孟津县", 700209: "新安县", 700210: "栾川县", 700211: "嵩县", 700212: "汝阳县", 700213: "宜阳县", 700214: "洛宁县", 700215: "伊川县", 700216: "偃师市", 700217: "高新区" }, 7003: { 700302: "龙亭区", 700303: "顺河回族区", 700304: "鼓楼区", 700305: "禹王台区", 700306: "金明区", 700307: "杞县", 700308: "通许县", 700309: "尉氏县", 700310: "开封县", 700311: "兰考县", 700312: "开发区" }, 7004: { 700402: "文峰区", 700403: "北关区", 700404: "殷都区", 700405: "龙安区", 700406: "安阳县", 700407: "汤阴县", 700408: "滑县", 700409: "内黄县", 700410: "林州市", 700411: "高新区" }, 7005: { 700502: "红旗区", 700503: "卫滨区", 700504: "凤泉区", 700505: "牧野区", 700506: "新乡县", 700507: "获嘉县", 700508: "原阳县", 700509: "延津县", 700510: "封丘县", 700511: "长垣县", 700512: "卫辉市", 700513: "辉县市" }, 7006: { 700602: "华龙区", 700603: "清丰县", 700604: "南乐县", 700605: "范县", 700606: "台前县", 700607: "濮阳县" }, 7007: { 700702: "解放区", 700703: "中站区", 700704: "马村区", 700705: "山阳区", 700706: "修武县", 700707: "博爱县", 700708: "武陟县", 700709: "温县", 700710: "沁阳市", 700711: "孟州市" }, 7008: { 700802: "鹤山区", 700803: "山城区", 700804: "淇滨区", 700805: "浚县", 700806: "淇县", 700807: "开发区" }, 7009: { 700902: "湖滨区", 700903: "渑池县", 700904: "陕县", 700905: "卢氏县", 700906: "义马市", 700907: "灵宝市" }, 7010: { 701002: "梁园区", 701003: "睢阳区", 701004: "民权县", 701005: "睢县", 701006: "宁陵县", 701007: "柘城县", 701008: "虞城县", 701009: "夏邑县", 701010: "永城市" }, 7011: { 701102: "魏都区", 701103: "许昌县", 701104: "鄢陵县", 701105: "襄城县", 701106: "禹州市", 701107: "长葛市", 701108: "开发区" }, 7012: { 701202: "源汇区", 701203: "郾城区", 701204: "召陵区", 701205: "舞阳县", 701206: "临颍县", 701207: "开发区" }, 7013: { 701302: "新华区", 701303: "卫东区", 701304: "石龙区", 701305: "湛河区", 701306: "宝丰县", 701307: "叶县", 701308: "鲁山县", 701309: "郏县", 701310: "舞钢市", 701311: "汝州市" }, 7014: { 701402: "驿城区", 701403: "西平县", 701404: "上蔡县", 701405: "平舆县", 701406: "正阳县", 701407: "确山县", 701408: "泌阳县", 701409: "汝南县", 701410: "遂平县", 701411: "新蔡县" }, 7015: { 701502: "川汇区", 701503: "扶沟县", 701504: "西华县", 701505: "商水县", 701506: "沈丘县", 701507: "郸城县", 701508: "淮阳县", 701509: "太康县", 701510: "鹿邑县", 701511: "项城市" }, 7016: { 701602: "宛城区", 701603: "卧龙区", 701604: "南召县", 701605: "方城县", 701606: "西峡县", 701607: "镇平县", 701608: "内乡县", 701609: "淅川县", 701610: "社旗县", 701611: "唐河县", 701612: "新野县", 701613: "桐柏县", 701614: "邓州市", 701615: "高新区" }, 7017: { 701702: "浉河区", 701703: "平桥区", 701704: "罗山县", 701705: "光山县", 701706: "新县", 701707: "商城县", 701708: "固始县", 701709: "潢川县", 701710: "淮滨县", 701711: "息县" }, 7101: { 710102: "江岸区", 710103: "江汉区", 710104: "楚口区", 710105: "汉阳区", 710106: "武昌区", 710107: "青山区", 710108: "洪山区", 710109: "东西湖区", 710110: "汉南区", 710111: "蔡甸区", 710112: "江夏区", 710113: "黄陂区", 710114: "新洲区", 710115: "开发区" }, 7102: { 710202: "黄石港区", 710203: "西塞山区", 710204: "下陆区", 710205: "铁山区", 710206: "阳新县", 710207: "大冶市", 710208: "开发区" }, 7103: { 710302: "襄城区", 710303: "樊城区", 710304: "襄州区", 710305: "南漳县", 710306: "谷城县", 710307: "保康县", 710308: "老河口市", 710309: "枣阳市", 710310: "宜城市", 710311: "开发区" }, 7104: { 710402: "茅箭区", 710403: "张湾区", 710404: "郧县", 710405: "郧西县", 710406: "竹山县", 710407: "竹溪县", 710408: "房县", 710409: "丹江口市" }, 7105: { 710503: "荆州区", 710504: "公安县", 710505: "监利县", 710506: "江陵县", 710507: "石首市", 710508: "洪湖市", 710509: "松滋市" }, 7106: { 710602: "西陵区", 710603: "伍家岗区", 710604: "点军区", 710605: "猇亭区", 710606: "夷陵区", 710607: "远安县", 710608: "兴山县", 710609: "秭归县", 710610: "长阳县", 710611: "五峰县", 710612: "宜都市", 710613: "当阳市", 710614: "枝江市", 710615: "高新区" }, 7107: { 710702: "东宝区", 710703: "掇刀区", 710704: "京山县", 710705: "沙洋县", 710706: "钟祥市" }, 7108: { 710802: "梁子湖区", 710803: "华容区", 710804: "鄂城区" }, 7109: { 710902: "孝南区", 710903: "孝昌县", 710904: "大悟县", 710905: "云梦县", 710906: "应城市", 710907: "安陆市", 710908: "汉川市" }, 7110: { 711002: "黄州区", 711003: "团风县", 711004: "红安县", 711005: "罗田县", 711006: "英山县", 711007: "浠水县", 711008: "蕲春县", 711009: "黄梅县", 711010: "麻城市", 711011: "武穴市" }, 7111: { 711102: "咸安区", 711103: "嘉鱼县", 711104: "通城县", 711105: "崇阳县", 711106: "通山县", 711107: "赤壁市" }, 7112: { 711202: "曾都区", 711203: "广水市" }, 7113: { 711301: "恩施市", 711302: "利川市", 711303: "建始县", 711304: "巴东县", 711305: "宣恩县", 711306: "咸丰县", 711307: "来凤县", 711308: "鹤峰县" }, 7201: { 720102: "芙蓉区", 720103: "天心区", 720104: "岳麓区", 720105: "开福区", 720106: "雨花区", 720107: "长沙县", 720108: "望城县", 720109: "宁乡县", 720110: "浏阳市", 720111: "开发区" }, 7202: { 720202: "荷塘区", 720203: "芦淞区", 720204: "石峰区", 720205: "天元区", 720206: "株洲县", 720207: "攸县", 720208: "茶陵县", 720209: "炎陵县", 720210: "醴陵市", 720211: "高新区" }, 7203: { 720302: "雨湖区", 720303: "岳塘区", 720304: "湘潭县", 720305: "湘乡市", 720306: "韶山市", 720307: "高新区" }, 7204: { 720402: "珠晖区", 720403: "雁峰区", 720404: "石鼓区", 720405: "蒸湘区", 720406: "南岳区", 720407: "衡阳县", 720408: "衡南县", 720409: "衡山县", 720410: "衡东县", 720411: "祁东县", 720412: "耒阳市", 720413: "常宁市" }, 7205: { 720502: "资阳区", 720503: "赫山区", 720504: "南县", 720505: "桃江县", 720506: "安化县", 720507: "沅江市", 720508: "高新区" }, 7206: { 720602: "武陵区", 720603: "鼎城区", 720604: "安乡县", 720605: "汉寿县", 720606: "澧县", 720607: "临澧县", 720608: "桃源县", 720609: "石门县", 720610: "津市市", 720611: "开发区" }, 7207: { 720702: "岳阳楼区", 720703: "云溪区", 720704: "君山区", 720705: "岳阳县", 720706: "华容县", 720707: "湘阴县", 720708: "平江县", 720709: "汨罗市", 720710: "临湘市", 720711: "开发区" }, 7208: { 720802: "双清区", 720803: "大祥区", 720804: "北塔区", 720805: "邵东县", 720806: "新邵县", 720807: "邵阳县", 720808: "隆回县", 720809: "洞口县", 720810: "绥宁县", 720811: "新宁县", 720812: "城步县", 720813: "武冈市" }, 7209: { 720902: "北湖区", 720903: "苏仙区", 720904: "桂阳县", 720905: "宜章县", 720906: "永兴县", 720907: "嘉禾县", 720908: "临武县", 720909: "汝城县", 720910: "桂东县", 720911: "安仁县", 720912: "资兴市" }, 7210: { 721002: "娄星区", 721003: "双峰县", 721004: "新化县", 721005: "冷水江市", 721006: "涟源市" }, 7211: { 721102: "零陵区", 721103: "冷水滩区", 721104: "祁阳县", 721105: "东安县", 721106: "双牌县", 721107: "道县", 721108: "江永县", 721109: "宁远县", 721110: "蓝山县", 721111: "新田县", 721112: "江华县" }, 7212: { 721202: "鹤城区", 721203: "中方县", 721204: "沅陵县", 721205: "辰溪县", 721206: "溆浦县", 721207: "会同县", 721208: "麻阳县", 721209: "新晃县", 721210: "芷江县", 721211: "靖州苗族县", 721212: "通道县", 721213: "洪江市" }, 7213: { 721302: "永定区", 721303: "武陵源区", 721304: "慈利县", 721305: "桑植县" }, 7214: { 721401: "吉首市", 721402: "泸溪县", 721403: "凤凰县", 721404: "花垣县", 721405: "保靖县", 721406: "古丈县", 721407: "永顺县", 721408: "龙山县" }, 8001: { 800102: "新城区", 800103: "碑林区", 800104: "莲湖区", 800105: "灞桥区", 800106: "未央区", 800107: "雁塔区", 800108: "阎良区", 800109: "临潼区", 800110: "长安区", 800111: "蓝田县", 800112: "周至县", 800113: "户县", 800114: "高陵县", 800115: "经济开发区", 800116: "高新区" }, 8002: { 800202: "渭滨区", 800203: "金台区", 800204: "陈仓区", 800205: "凤翔县", 800206: "岐山县", 800207: "扶风县", 800208: "眉县", 800209: "陇县", 800210: "千阳县", 800211: "麟游县", 800212: "凤县", 800213: "太白县", 800214: "高新区" }, 8003: { 800302: "秦都区", 800304: "渭城区", 800305: "三原县", 800306: "泾阳县", 800307: "乾县", 800308: "礼泉县", 800309: "永寿县", 800310: "彬县", 800311: "长武县", 800312: "旬邑县", 800313: "淳化县", 800314: "武功县", 800315: "兴平市" }, 8004: { 800402: "临渭区", 800403: "华县", 800404: "潼关县", 800405: "大荔县", 800406: "合阳县", 800407: "澄城县", 800408: "蒲城县", 800409: "白水县", 800410: "富平县", 800411: "韩城市", 800412: "华阴市", 800413: "高新区" }, 8005: { 800502: "宝塔区", 800503: "延长县", 800504: "延川县", 800505: "子长县", 800506: "安塞县", 800507: "志丹县", 800508: "吴起县", 800509: "甘泉县", 800510: "富县", 800511: "洛川县", 800512: "宜川县", 800513: "黄龙县", 800514: "黄陵县" }, 8006: { 800602: "榆阳区", 800603: "神木县", 800604: "府谷县", 800605: "横山县", 800606: "靖边县", 800607: "定边县", 800608: "绥德县", 800609: "米脂县", 800610: "佳县", 800611: "吴堡县", 800612: "清涧县", 800613: "子洲县" }, 8007: { 800702: "王益区", 800703: "印台区", 800704: "耀州区", 800705: "宜君县" }, 8008: { 800802: "汉台区", 800803: "南郑县", 800804: "城固县", 800805: "洋县", 800806: "西乡县", 800807: "勉县", 800808: "宁强县", 800809: "略阳县", 800810: "镇巴县", 800811: "留坝县", 800812: "佛坪县" }, 8009: { 800902: "汉滨区", 800903: "汉阴县", 800904: "石泉县", 800905: "宁陕县", 800906: "紫阳县", 800907: "岚皋县", 800908: "平利县", 800909: "镇坪县", 800910: "旬阳县", 800911: "白河县" }, 8010: { 801002: "商州区", 801003: "洛南县", 801004: "丹凤县", 801005: "商南县", 801006: "山阳县", 801007: "镇安县", 801008: "柞水县" }, 8011: { 801101: "农业示范区" }, 8101: { 810102: "城东区", 810103: "城中区", 810104: "城西区", 810105: "城北区", 810106: "大通县", 810107: "湟中县", 810108: "湟源县", 810109: "开发区" }, 8102: { 810201: "平安县", 810202: "民和县", 810203: "乐都县", 810204: "互助县", 810205: "化隆县", 810206: "循化县" }, 8103: { 810301: "共和县", 810302: "同德县", 810303: "贵德县", 810304: "兴海县", 810305: "贵南县" }, 8104: { 810401: "门源县", 810402: "祁连县", 810403: "海晏县", 810404: "刚察县" }, 8105: { 810501: "格尔木市", 810502: "德令哈市", 810503: "乌兰县", 810504: "都兰县", 810505: "天峻县" }, 8106: { 810601: "同仁县", 810602: "尖扎县", 810603: "泽库县", 810604: "河南县" }, 8107: { 810701: "玛沁县", 810702: "班玛县", 810703: "甘德县", 810704: "达日县", 810705: "久治县", 810706: "玛多县" }, 8108: { 810801: "玉树县", 810802: "杂多县", 810803: "称多县", 810804: "治多县", 810805: "囊谦县", 810806: "曲麻莱县" }, 8201: { 820102: "兴庆区", 820103: "西夏区", 820104: "金凤区", 820105: "永宁县", 820106: "贺兰县", 820107: "灵武市", 820108: "开发区" }, 8202: { 820202: "大武口区", 820203: "惠农区", 820204: "平罗县" }, 8203: { 820302: "利通区", 820303: "盐池县", 820304: "同心县", 820305: "青铜峡市" }, 8204: { 820402: "原州区", 820403: "西吉县", 820404: "隆德县", 820405: "泾源县", 820406: "彭阳县" }, 8205: { 820502: "沙坡头区", 820503: "中宁县", 820504: "海原县" }, 8301: { 830102: "天山区", 830103: "沙依巴克区", 830105: "水磨沟区", 830106: "头屯河区(开发区)", 830107: "达坂城区", 830108: "米东区", 830109: "乌鲁木齐县", 830110: "新市区(高新区)" }, 8302: { 830201: "伊宁市", 830202: "奎屯市", 830203: "伊宁县", 830204: "察布查尔", 830205: "霍城县", 830206: "巩留县", 830207: "新源县", 830208: "昭苏县", 830209: "特克斯县", 830210: "尼勒克县" }, 8303: { 830301: "阿勒泰市", 830302: "布尔津县", 830303: "富蕴县", 830304: "福海县", 830305: "哈巴河县", 830306: "青河县", 830307: "吉木乃县" }, 8304: { 830401: "塔城市", 830402: "乌苏市", 830403: "额敏县", 830404: "沙湾县", 830405: "托里县", 830406: "裕民县", 830407: "和布克赛尔" }, 8305: { 830501: "博乐市", 830502: "精河县", 830503: "温泉县" }, 8306: { 830601: "昌吉市", 830602: "阜康市", 830603: "呼图壁县", 830604: "玛纳斯县", 830605: "奇台县", 830606: "吉木萨尔县", 830607: "木垒县", 830608: "高新区" }, 8307: { 830701: "吐鲁番市", 830702: "鄯善县", 830703: "托克逊县" }, 8308: { 830801: "库尔勒市", 830802: "轮台县", 830803: "尉犁县", 830804: "若羌县", 830805: "且末县", 830806: "焉耆县", 830807: "和静县", 830808: "和硕县", 830809: "博湖县" }, 8309: { 830901: "哈密市", 830902: "巴里坤县", 830903: "伊吾县" }, 8310: { 831001: "和田市", 831002: "和田县", 831003: "墨玉县", 831004: "皮山县", 831005: "洛浦县", 831006: "策勒县", 831007: "于田县", 831008: "民丰县" }, 8311: { 831101: "阿克苏市", 831102: "温宿县", 831103: "库车县", 831104: "沙雅县", 831105: "新和县", 831106: "拜城县", 831107: "乌什县", 831108: "阿瓦提县", 831109: "柯坪县" }, 8312: { 831201: "阿图什市", 831202: "阿克陶县", 831203: "阿合奇县", 831204: "乌恰县" }, 8313: { 831301: "喀什市", 831302: "疏附县", 831303: "疏勒县", 831304: "英吉沙县", 831305: "泽普县", 831306: "莎车县", 831307: "叶城县", 831308: "麦盖提县", 831309: "岳普湖县", 831310: "伽师县", 831311: "巴楚县", 831312: "塔什库尔干" }, 8314: { 831402: "独山子区", 831403: "克拉玛依区", 831404: "白碱滩区", 831405: "乌尔禾区" }, 8401: { 840102: "城关区", 840103: "七里河区", 840104: "西固区", 840105: "安宁区", 840106: "红古区", 840107: "永登县", 840108: "皋兰县", 840109: "榆中县", 840110: "开发区" }, 8402: { 840202: "秦州区", 840203: "麦积区", 840204: "清水县", 840205: "秦安县", 840206: "甘谷县", 840207: "武山县", 840208: "张家川县", 840209: "开发区" }, 8404: { 840402: "凉州区", 840403: "民勤县", 840404: "古浪县", 840405: "天祝县" }, 8405: { 840502: "金川区", 840503: "永昌县", 840504: "开发区" }, 8406: { 840602: "肃州区", 840603: "金塔县", 840604: "瓜州县", 840605: "肃北县", 840606: "阿克塞县", 840607: "玉门市", 840608: "敦煌市" }, 8407: { 840702: "甘州区", 840703: "肃南县", 840704: "民乐县", 840705: "临泽县", 840706: "高台县", 840707: "山丹县" }, 8408: { 840802: "西峰区", 840803: "庆城县", 840804: "环县", 840805: "华池县", 840806: "合水县", 840807: "正宁县", 840808: "宁县", 840809: "镇原县" }, 8409: { 840902: "崆峒区", 840903: "泾川县", 840904: "灵台县", 840905: "崇信县", 840906: "华亭县", 840907: "庄浪县", 840908: "静宁县" }, 8410: { 841002: "白银区", 841003: "平川区", 841004: "靖远县", 841005: "会宁县", 841006: "景泰县", 841007: "高新区" }, 8411: { 841102: "安定区", 841103: "通渭县", 841104: "陇西县", 841105: "渭源县", 841106: "临洮县", 841107: "漳县", 841108: "岷县" }, 8412: { 841202: "武都区", 841203: "成县", 841204: "文县", 841205: "宕昌县", 841206: "康县", 841207: "西和县", 841208: "礼县", 841209: "徽县", 841210: "两当县" }, 8413: { 841301: "临夏市", 841302: "临夏县", 841303: "康乐县", 841304: "永靖县", 841305: "广河县", 841306: "和政县", 841307: "东乡族自治县", 841308: "积石山县" }, 8414: { 841401: "合作市", 841402: "临潭县", 841403: "卓尼县", 841404: "舟曲县", 841405: "迭部县", 841406: "玛曲县", 841407: "碌曲县", 841408: "夏河县" } });