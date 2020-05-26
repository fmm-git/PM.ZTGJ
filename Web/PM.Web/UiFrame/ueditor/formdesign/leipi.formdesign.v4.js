/*
* 设计器私有的配置说明 
* 一
* UE.leipiFormDesignUrl  插件路径
* 
* 二
*UE.getEditor('myFormDesign',{
*          toolleipi:true,//是否显示，设计器的清单 tool
*/
UE.leipiFormDesignUrl = 'formdesign';
/**
 * 文本框
 * @command textfield
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'textfield');
 * ```
 */
UE.plugins['text'] = function () {
    var me = this, thePlugins = 'text';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/text.aspx',
                name: thePlugins,
                editor: this,
                title: '文本框',
                cssRules: "width:440px;height:180px;",
                buttons: [
				{
				    className: 'edui-okbutton',
				    label: '确定',
				    onclick: function () {
				        dialog.close(true);
				    }
				},
				{
				    className: 'edui-cancelbutton',
				    label: '取消',
				    onclick: function () {
				        dialog.close(false);
				    }
				}]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins, me.tableName);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/input/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
				'<nobr>文本框: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {
                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });
};
/**
 * 文本弹出框
 * @command textfield
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'textfield');
 * ```
 */
UE.plugins['textdialog'] = function () {
    var me = this, thePlugins = 'textdialog';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/textdialog.aspx',
                name: thePlugins,
                editor: this,
                title: '文本弹出框',
                cssRules: "width:490px;height:240px;",
                buttons: [
				{
				    className: 'edui-okbutton',
				    label: '确定',
				    onclick: function () {
				        dialog.close(true);
				    }
				},
				{
				    className: 'edui-cancelbutton',
				    label: '取消',
				    onclick: function () {
				        dialog.close(false);
				    }
				}]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins, me.tableName);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                if (this.anchorEl.nextSibling) {
                    baidu.editor.dom.domUtils.remove(this.anchorEl.nextSibling, false);
                }
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/input/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
				'<nobr>文本弹出框: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {
                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });
};
/**
 * 数字框
 * @command textfield
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'textfield');
 * ```
 */
UE.plugins['number'] = function () {
    var me = this, thePlugins = 'number';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/number.aspx',
                name: thePlugins,
                editor: this,
                title: '数字框',
                cssRules: "width:440px;height:180px;",
                buttons: [
				{
				    className: 'edui-okbutton',
				    label: '确定',
				    onclick: function () {
				        dialog.close(true);
				    }
				},
				{
				    className: 'edui-cancelbutton',
				    label: '取消',
				    onclick: function () {
				        dialog.close(false);
				    }
				}]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins, me.tableName);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/input/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
				'<nobr>数字框: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {
                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });
};
/**
 * 日期框
 * @command textfield
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'textfield');
 * ```
 */
UE.plugins['datebox'] = function () {
    var me = this, thePlugins = 'datebox';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/datebox.aspx',
                name: thePlugins,
                editor: this,
                title: '日期框',
                cssRules: "width:440px;height:180px;",
                buttons: [
				{
				    className: 'edui-okbutton',
				    label: '确定',
				    onclick: function () {
				        dialog.close(true);
				    }
				},
				{
				    className: 'edui-cancelbutton',
				    label: '取消',
				    onclick: function () {
				        dialog.close(false);
				    }
				}]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins, me.tableName);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/input/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
				'<nobr>日期框: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {
                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });
};
/**
 * 分页符
 * @command textfield
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'textfield');
 * ```
 */
UE.plugins['tab'] = function () {
    var me = this, thePlugins = 'tab';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/tab.aspx',
                name: thePlugins,
                editor: this,
                title: '分页符',
                cssRules: "width:440px;height:180px;",
                buttons: [
				{
				    className: 'edui-okbutton',
				    label: '确定',
				    onclick: function () {
				        dialog.close(true);
				    }
				},
				{
				    className: 'edui-cancelbutton',
				    label: '取消',
				    onclick: function () {
				        dialog.close(false);
				    }
				}]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins, me.tableName);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/input/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
				'<nobr>分页符: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {
                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });
};

/**
 * 文本
 * @command textfield
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'textfield');
 * ```
 */
UE.plugins['label'] = function () {
    var me = this, thePlugins = 'label';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/label.aspx',
                name: thePlugins,
                editor: this,
                title: '文本',
                cssRules: "width:370px;height:70px;",
                buttons: [
				{
				    className: 'edui-okbutton',
				    label: '确定',
				    onclick: function () {
				        dialog.close(true);
				    }
				},
				{
				    className: 'edui-cancelbutton',
				    label: '取消',
				    onclick: function () {
				        dialog.close(false);
				    }
				}]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins, me.tableName);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/label/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
				'<nobr>文本: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {
                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });
};

/**
 * 复选框
 * @command checkbox
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'checkbox');
 * ```
 */

UE.plugins['checkbox'] = function () {
    var me = this, thePlugins = 'checkbox';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/checkbox.aspx',
                name: thePlugins,
                editor: this,
                title: '复选框',
                cssRules: "width:250px;height:180px;",
                buttons: [
                {
                    className: 'edui-okbutton',
                    label: '确定',
                    onclick: function () {
                        dialog.close(true);
                    }
                },
                {
                    className: 'edui-cancelbutton',
                    label: '取消',
                    onclick: function () {
                        dialog.close(false);
                    }
                }]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins, me.tableName);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/input|span/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
                '<nobr>复选框: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {
                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });
};


/**
 * 多行文本框
 * @command textarea
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'textarea');
 * ```
 */
UE.plugins['textarea'] = function () {
    var me = this, thePlugins = 'textarea';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/textarea.aspx',
                name: thePlugins,
                editor: this,
                title: '多行文本框',
                cssRules: "width:440px;height:180px;",
                buttons: [
                {
                    className: 'edui-okbutton',
                    label: '确定',
                    onclick: function () {
                        dialog.close(true);
                    }
                },
                {
                    className: 'edui-cancelbutton',
                    label: '取消',
                    onclick: function () {
                        dialog.close(false);
                    }
                }]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins, me.tableName);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/input/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
                '<nobr>多行文本框: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {
                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });
};
/**
 * 下拉菜单
 * @command select
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'select');
 * ```
 */
UE.plugins['combobox'] = function () {
    var me = this, thePlugins = 'combobox';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/combobox.aspx',
                name: thePlugins,
                editor: this,
                title: '下拉菜单',
                cssRules: "width:490px;height:180px;",
                buttons: [
                {
                    className: 'edui-okbutton',
                    label: '确定',
                    onclick: function () {
                        dialog.close(true);
                    }
                },
                {
                    className: 'edui-cancelbutton',
                    label: '取消',
                    onclick: function () {
                        dialog.close(false);
                    }
                }]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins, me.tableName);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/select|span/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
                '<nobr>下拉菜单: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {

                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });

};
/**
 * 下拉树
 * @command select
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'select');
 * ```
 */
UE.plugins['combotree'] = function () {
    var me = this, thePlugins = 'combotree';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/combotree.aspx',
                name: thePlugins,
                editor: this,
                title: '下拉树',
                cssRules: "width:490px;height:180px;",
                buttons: [
                {
                    className: 'edui-okbutton',
                    label: '确定',
                    onclick: function () {
                        dialog.close(true);
                    }
                },
                {
                    className: 'edui-cancelbutton',
                    label: '取消',
                    onclick: function () {
                        dialog.close(false);
                    }
                }]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins, me.tableName);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/select|span/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
                '<nobr>下拉树: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {

                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });

};
/**
 * 下拉字典
 * @command select
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'select');
 * ```
 */
UE.plugins['combocustom'] = function () {
    var me = this, thePlugins = 'combocustom';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/combocustom.aspx',
                name: thePlugins,
                editor: this,
                title: '下拉字典',
                cssRules: "width:490px;height:180px;",
                buttons: [
                {
                    className: 'edui-okbutton',
                    label: '确定',
                    onclick: function () {
                        dialog.close(true);
                    }
                },
                {
                    className: 'edui-cancelbutton',
                    label: '取消',
                    onclick: function () {
                        dialog.close(false);
                    }
                }]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins, me.tableName);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/select|span/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
                '<nobr>下拉字典: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {

                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });

};
/**
 * 下拉表格
 * @command select
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'select');
 * ```
 */
UE.plugins['combogrid'] = function () {
    var me = this, thePlugins = 'combogrid';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/combogrid.aspx',
                name: thePlugins,
                editor: this,
                title: '下拉表格',
                cssRules: "width:490px;height:180px;",
                buttons: [
                {
                    className: 'edui-okbutton',
                    label: '确定',
                    onclick: function () {
                        dialog.close(true);
                    }
                },
                {
                    className: 'edui-cancelbutton',
                    label: '取消',
                    onclick: function () {
                        dialog.close(false);
                    }
                }]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins, me.tableName);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/select|span/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
                '<nobr>下拉表格: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {

                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });

};
/**
 * 列表控件
 * @command listctrl
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'qrcode');
 * ```
 */
UE.plugins['datagrid'] = function () {
    var me = this, thePlugins = 'datagrid';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/datagrid.aspx',
                name: thePlugins,
                editor: this,
                title: '明细表',
                cssRules: "width:1000px;height:400px;",
                buttons: [
                {
                    className: 'edui-okbutton',
                    label: '确定',
                    onclick: function () {
                        dialog.close(true);
                    }
                },
                {
                    className: 'edui-cancelbutton',
                    label: '取消',
                    onclick: function () {
                        dialog.close(false);
                    }
                }]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                var oNode = this.anchorEl;
                $.ajax({
                    type: "post",
                    url: '/api/common/clearandbatinsert/AddFormControl',
                    data: {
                        "FormCode": this.editor.formCode,
                        "DelTableName": oNode.getAttribute('id'),
                        "DelFunCode": "DeleteFormControls",
                        "OtherItemCUDFunCode": "DeleteFormSubTable",
                        "data": ""
                    },
                    success: function (data) {
                        if (data.success == 'true') {

                        }
                    },
                    dataType: "json",
                    async: false
                });
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/table|tr/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
                '<nobr>明细表: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {
                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });
};
/**
 * 公式
 * @command textfield
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'textfield');
 * ```
 */
UE.plugins['expression'] = function () {
    var me = this, thePlugins = 'expression';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/expression.aspx',
                name: thePlugins,
                editor: this,
                title: '公式',
                cssRules: "width:550px;height:240px;",
                buttons: [
				{
				    className: 'edui-okbutton',
				    label: '确定',
				    onclick: function () {
				        dialog.close(true);
				    }
				},
				{
				    className: 'edui-cancelbutton',
				    label: '取消',
				    onclick: function () {
				        dialog.close(false);
				    }
				}]
            });
            dialog.render();
            dialog.open();
        }
    };
};

/**
 * 批量添加配置
 * @command textfield
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'textfield');
 * ```
 */
UE.plugins['bataddconfig'] = function () {
    var me = this, thePlugins = 'bataddconfig';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/bataddconfig.aspx',
                name: thePlugins,
                editor: this,
                title: '批量添加配置',
                cssRules: "width:490px;height:200px;",
                buttons: [
				{
				    className: 'edui-okbutton',
				    label: '确定',
				    onclick: function () {
				        dialog.close(true);
				    }
				},
				{
				    className: 'edui-cancelbutton',
				    label: '取消',
				    onclick: function () {
				        dialog.close(false);
				    }
				}]
            });
            dialog.render();
            dialog.open();
        }
    };
};

/**
 * 验证规则配置
 * @command textfield
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'textfield');
 * ```
 */
UE.plugins['validrulelist'] = function () {
    var me = this, thePlugins = 'validrulelist';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/validrulelist.aspx',
                name: thePlugins,
                editor: this,
                title: '验证规则配置',
                cssRules: "width:600px;height:240px;",
                buttons: [
				{
				    className: 'edui-okbutton',
				    label: '确定',
				    onclick: function () {
				        dialog.close(true);
				    }
				},
				{
				    className: 'edui-cancelbutton',
				    label: '取消',
				    onclick: function () {
				        dialog.close(false);
				    }
				}]
            });
            dialog.render();
            dialog.open();
        }
    };
};

/**
 * 验证规则
 * @command textfield
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'textfield');
 * ```
 */
UE.plugins['validruleconfig'] = function () {
    var me = this, thePlugins = 'validruleconfig';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/validruleconfig.aspx',
                name: thePlugins,
                editor: this,
                title: '验证规则',
                cssRules: "width:700px;height:340px;",
                buttons: [
				{
				    className: 'edui-okbutton',
				    label: '确定',
				    onclick: function () {
				        dialog.close(true);
				    }
				},
				{
				    className: 'edui-cancelbutton',
				    label: '取消',
				    onclick: function () {
				        dialog.close(false);
				    }
				}]
            });
            dialog.render();
            dialog.open();
        }
    };
};

/**
 * 附件列表
 * @command listctrl
 * @method execCommand
 * @param { String } cmd 命令字符串
 * @example
 * ```javascript
 * editor.execCommand( 'qrcode');
 * ```
 */
UE.plugins['attachment'] = function () {
    var me = this, thePlugins = 'attachment';
    me.commands[thePlugins] = {
        execCommand: function () {
            var dialog = new UE.ui.Dialog({
                iframeUrl: this.options.UEDITOR_HOME_URL + UE.leipiFormDesignUrl + '/attachment.aspx',
                name: thePlugins,
                editor: this,
                title: '附件列表',
                cssRules: "width:350px;height:200px;",
                buttons: [
                {
                    className: 'edui-okbutton',
                    label: '确定',
                    onclick: function () {
                        dialog.close(true);
                    }
                },
                {
                    className: 'edui-cancelbutton',
                    label: '取消',
                    onclick: function () {
                        dialog.close(false);
                    }
                }]
            });
            dialog.render();
            dialog.open();
        }
    };
    var popup = new baidu.editor.ui.Popup({
        editor: this,
        content: '',
        className: 'edui-bubble',
        _edittext: function () {
            baidu.editor.plugins[thePlugins].editdom = popup.anchorEl;
            me.execCommand(thePlugins);
            this.hide();
        },
        _delete: function () {
            if (window.confirm('确认删除该控件吗？')) {
                var oNode = this.anchorEl;
                if (this.anchorEl.nextSibling) {
                    baidu.editor.dom.domUtils.remove(this.anchorEl.nextSibling, false);
                }
                if (this.anchorEl.nextSibling) {
                    baidu.editor.dom.domUtils.remove(this.anchorEl.nextSibling, false);
                }
                baidu.editor.dom.domUtils.remove(this.anchorEl, false);
            }
            this.hide();
        }
    });
    popup.render();
    me.addListener('mouseover', function (t, evt) {
        evt = evt || window.event;
        var el = evt.target || evt.srcElement;
        var leipiPlugins = el.getAttribute('leipiplugins');
        if (/table/ig.test(el.tagName) && leipiPlugins == thePlugins) {
            var html = popup.formatHtml(
                '<nobr>附件列表: <span onclick=$$._edittext() class="edui-clickable">编辑</span>&nbsp;&nbsp;<span onclick=$$._delete() class="edui-clickable">删除</span></nobr>');
            if (html) {
                popup.getDom('content').innerHTML = html;
                popup.anchorEl = el;
                popup.showAnchor(popup.anchorEl);
            } else {
                popup.hide();
            }
        }
    });
};