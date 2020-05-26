using Dos.ORM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataAccess.Flow;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business
{
    public class FlowNodeUILogic
    {
        FlowNodeUIDA dao = new FlowNodeUIDA();
        public AjaxResult UpdateUI(List<TbFlowNodeUI> list)
        {
            int result = FlowNodeUIDA.Update(list);
            if (result > 0)
                return AjaxResult.Success();
            else return AjaxResult.Error();
        }
        public AjaxResult UpdataFlowNodeUI(string FlowCode, string json)
        {
            List<string> processEnd = new List<string>();
            bool isVieation = true;
            List<TbFlowNodeUI> list = dao.GetList(FlowCode);
            JObject jo = (JObject)JsonConvert.DeserializeObject(json);
            foreach (TbFlowNodeUI nodeui in list)
            {
                JObject node = (JObject)jo[nodeui.FlowNodeCode];
                nodeui.NodeTop = Convert.ToString(node["top"]).Replace("px","");
                nodeui.NodeLeft = Convert.ToString(node["left"]).Replace("px","");
                JArray array = (JArray)node["process_to"];
                string processData = string.Empty;
                foreach(string p in array)
                {
                    if (!processEnd.Contains(p)) processEnd.Add(p);
                    processData += p+",";
                }
                if (processData.Length > 0)
                    nodeui.processData = processData.Substring(0, processData.Length - 1);
                else nodeui.processData = processData;
                
            }
            foreach (TbFlowNodeUI ui in list)
            {
                if (
                    (ui.FlowNodeCode != "9999" && ui.processData.Length == 0)
                    ||(ui.FlowNodeCode!="0"&&!processEnd.Contains(ui.FlowNodeCode))
                    )
                {
                    isVieation = false;
                    break;
                }
            }
            if (isVieation)
            {
                int result = FlowNodeUIDA.Update(list);
                if (result > 0)
                {
                    new FlowNodeDA().UpdateNodeRelation(FlowCode);
                    return AjaxResult.Success("节点保存成功");
                }
                else { 
                    return AjaxResult.Error("节点保存失败"); 
                }
            }
            else
            {
                return AjaxResult.Error("流程中只能存在一个开始节点和一个结束节点");
            }
        }

        public AjaxResult AddNodeUI(List<TbFlowNodeUI> list)
        {
            int result = dao.AddNodeUI(list);
            if (result > 0) return AjaxResult.Success();
            else return AjaxResult.Error();
        }
        public AjaxResult LineAdd(string FlowCode,string strNode,string endNode)
        {
            int result = dao.LineAdd(FlowCode,strNode,endNode);
            if (result > 0){return AjaxResult.Success();}else{return AjaxResult.Error();}
        }
    }
}
