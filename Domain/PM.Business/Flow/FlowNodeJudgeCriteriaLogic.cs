using Dos.ORM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PM.Common;
using PM.DataAccess.Flow;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace PM.Business.Flow
{
    public class FlowNodeJudgeCriteriaLogic
    {
        FlowNodeJudgeCriteriaDA dao = new FlowNodeJudgeCriteriaDA();
        /// <summary>
        /// 通过流程编码找到对应的业务单据数据存储表
        /// </summary>
        /// <param name="FlowCode">流程编码</param>
        /// <returns></returns>
        public string GetTableNameByFlowCode(string FlowCode)
        {
            return dao.GetTableNameByFlowCode(FlowCode);
        }
        /// <summary>
        /// 添加条件
        /// </summary>
        /// <param name="JudgeCriteriaInfo"></param>
        /// <returns></returns>
        public AjaxResult AddJudgeCriteria(string JudgeCriteriaInfo)
        {
            JObject json = (JObject)JsonConvert.DeserializeObject(JudgeCriteriaInfo);
            JArray array = (JArray)json["jclist"];
            List<TbFlowNodeJudgeCriteria> list = new List<TbFlowNodeJudgeCriteria>();
            for (int i = 0; i < array.Count; i++)
            {
                JObject obj = (JObject)array[i];
                TbFlowNodeJudgeCriteria model = new TbFlowNodeJudgeCriteria();
                model.FlowCode = Convert.ToString(json["FlowCode"]);
                model.FlowNodeCode = Convert.ToString(json["FlowNodeCode"]);
                model.PhysicalTableName = Convert.ToString(json["TableName"]);
                model.FieldCode = Convert.ToString(obj["FieldCode"]);
                model.JudgeSymbol = Convert.ToString(obj["JudgeSymbol"]);
                model.BeforeBrackets = "";
                model.JudgeValue = Convert.ToString(obj["JudgeValue"]);
                model.LastBrackets = "";
                model.JudgeRelation = Convert.ToString(obj["JudgeRelation"]);
                model.FieldName = Convert.ToString(obj["FieldName"]);
                model.FieldType = Convert.ToString(obj["FieldType"]);
                list.Add(model);
            }
            if (FlowNodeJudgeCriteriaDA.Insert(list) > 0)
                return AjaxResult.Success();
            else return AjaxResult.Error();
        }
        public string GetFields(string FlowCode)
        {
            string tableName = GetTableNameByFlowCode(FlowCode);
            //实体对象
            var filemodel = Assembly.Load("PM.DataEntity").CreateInstance("PM.DataEntity." + tableName);
            //获取实体列中返回所有字段的方法
            MethodInfo mi = filemodel.GetType().GetMethod("GetFields");
            //获取实体类中该方法所需参数
            ParameterInfo[] paramsInfo = mi.GetParameters();
            //实例化实体类中方法所需参数
            Object[] paras = new Object[] { };
            object obj = Activator.CreateInstance(filemodel.GetType());
            //获取实体类中所有字段信息
            Field[] fields = (Field[])mi.Invoke(obj, paras);

            List<FlowFiledInfo> list = new List<FlowFiledInfo>();
            foreach (Field F in fields)
            {
                FlowFiledInfo ffi = new FlowFiledInfo();
                string fileName = string.Format(F.FieldName, "", "");
                ffi.tableName = tableName;
                ffi.filedName = fileName;
                ffi.filedDescription = F.Description;
                ffi.fieldType = (filemodel.GetType().GetProperty(fileName).PropertyType).Name;
                list.Add(ffi);
            }
            return JsonConvert.SerializeObject(list);
        }
        /// <summary>
        /// 获取运算符
        /// </summary>
        /// <returns></returns>
        public string GetOperator()
        {
            List<Option> list = new List<Option>();
            list.Add(new Option("","--无--"));
            list.Add(new Option("=", "等于"));
            list.Add(new Option(">", "大于"));
            list.Add(new Option("=>", "大于等于"));
            list.Add(new Option("<", "小于"));
            list.Add(new Option("<=", "小于等于"));
            list.Add(new Option("like", "包含"));
            list.Add(new Option("notlike", "不包含"));
            return JsonConvert.SerializeObject(list);
        }
        /// <summary>
        /// 关联关系
        /// </summary>
        /// <returns></returns>
        public string GetLogical()
        {
            List<Option> list = new List<Option>();
            list.Add(new Option("", "--无--"));
            list.Add(new Option("and","并且"));
            list.Add(new Option("or", "或者"));
            return JsonConvert.SerializeObject(list);
        }
        public string GetJudgeCriteriaByFlowNode(string FlowCode, string FlowNodeCode)
        {
            return JsonConvert.SerializeObject(dao.GetJudgeCriteriaByFlowNode(FlowCode, FlowNodeCode));
        }
        public string MathNodeJudgeCriter(string FlowCode, string FlowNodeCode)
        {
            bool flag = dao.MathNodeJudgeCriter(FlowCode,FlowNodeCode);
            return "{\"result\":\""+flag+"\"}";
        }
    }
    #region 实体
    public class Option
    {
        public Option(string value, string text)
        {
            this.ValueInfo = value;
            this.TextInfo = text;
        }
        /// <summary>
        /// 存储值
        /// </summary>
        public string ValueInfo { get; set; }
        /// <summary>
        /// 显示值
        /// </summary>
        public string TextInfo { get; set; }
    }
    //字段属性
    public class FlowFiledInfo
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string tableName { get; set; }
        /// <summary>
        /// 字段名称
        /// </summary>
        public string filedName { get; set; }
        /// <summary>
        /// 字段描述
        /// </summary>
        public string filedDescription { get; set; }
        /// <summary>
        /// 字段类型
        /// </summary>
        public string fieldType { get; set; }
    }
    #endregion
}
