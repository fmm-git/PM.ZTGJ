/*
@license

dhtmlxGantt v.3.3.0 dhtmlx.com
This software can be used only as part of dhtmlx.com site.
You are not allowed to use it on any other site

(c) Dinamenta, UAB.
*/
Gantt.plugin(function(t){t.config.highlight_critical_path=!1,t._criticalPathHandler=function(){t.config.highlight_critical_path&&t.render()},t.attachEvent("onAfterLinkAdd",t._criticalPathHandler),t.attachEvent("onAfterLinkUpdate",t._criticalPathHandler),t.attachEvent("onAfterLinkDelete",t._criticalPathHandler),t.attachEvent("onAfterTaskAdd",t._criticalPathHandler),t.attachEvent("onAfterTaskUpdate",t._criticalPathHandler),t.attachEvent("onAfterTaskDelete",t._criticalPathHandler),t.isCriticalTask=function(t){
if(t){var e=arguments[1]||{};if(this._isTask(t)){if(this._isProjectEnd(t))return!0;e[t.id]=!0;for(var a=this._getSuccessors(t),n=0;n<a.length;n++){var i=this.getTask(a[n].task);if(this._getSlack(t,i,a[n].link)<=0&&!e[i.id]&&this.isCriticalTask(i,e))return!0}}return!1}},t.isCriticalLink=function(e){return this.isCriticalTask(t.getTask(e.source))},t.getSlack=function(t,e){for(var a=[],n={},i=0;i<t.$source.length;i++)n[t.$source[i]]=!0;for(var i=0;i<e.$target.length;i++)n[e.$target[i]]&&a.push(e.$target[i]);
for(var r=[],i=0;i<a.length;i++)r.push(this._getSlack(t,e,this.getLink(a[i]).type));return Math.min.apply(Math,r)},t._getSlack=function(t,e,a){if(null===a)return 0;var n=null,i=null,r=this.config.links,s=this.config.types;return n=a!=r.finish_to_finish&&a!=r.finish_to_start||this._get_safe_type(t.type)==s.milestone?t.start_date:t.end_date,i=a!=r.finish_to_finish&&a!=r.start_to_finish||this._get_safe_type(e.type)==s.milestone?e.start_date:e.end_date,this.calculateDuration(n,i)},t._getProjectEnd=function(){
var e=t.getTaskByTime();return e=e.sort(function(t,e){return+t.end_date>+e.end_date?1:-1}),e.length?e[e.length-1].end_date:null},t._isProjectEnd=function(t){return!this._hasDuration(t.end_date,this._getProjectEnd())},t._isTask=function(e){return!(e.type&&e.type==t.config.types.project||e.$no_start||e.$no_end)},t._isProject=function(t){return!this._isTask(t)},t._formatSuccessors=function(t,e){for(var a=[],n=0;n<t.length;n++)a.push(this._formatSuccessor(t[n],e));return a},t._formatSuccessor=function(t,e){
return{task:t,link:e}},t._getSuccessors=function(e){var a=[];if(t._isProject(e))a=a.concat(t._formatSuccessors(this.getChildren(e.id),null));else for(var n=e.$source,i=0;i<n.length;i++){var r=this.getLink(n[i]);if(this.isTaskExists(r.target)){var s=this.getTask(r.target);this._isTask(s)?a.push(t._formatSuccessor(r.target,r.type)):a=a.concat(t._formatSuccessors(this.getChildren(s.id),r.type))}}return a}});
//# sourceMappingURL=../sources/ext/dhtmlxgantt_critical_path.js.map