using Newtonsoft.Json;
using PM.Collector;
using PM.DAL;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PM.Business
{
    public class FlowPerformManageLogic
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SQLDBCnnString"].ConnectionString;
        //private string userCode = HttpContext.Current.Session[UserInfo.USER_CODE].ToString().Trim();//用户代码
        private string userCodeLogin = "yuanming";//用户代码

        private SqlTransHelper transHelper = null;

        public FlowPerformManageLogic()
        {
            if (transHelper == null) transHelper = new SqlTransHelper(connStr);
            transHelper.BeginTrans();
        }
        /// <summary>
        /// 发起流程
        /// </summary>
        /// <param name="FlowCode">流程代码</param>
        /// <param name="FormCode">表单代码</param>
        /// <param name="FormDataCode">单据号</param>
        /// <param name="FlowLevel">级别</param>
        /// <param name="FlowTitle">主题</param>   
        /// <param name="FreeNodeUser">执行人</param>
        /// <param name="FinalCutoffTime">流程截止时间</param>
        public void InitFlowPerform(string FlowCode, string FormCode, string FormDataCode, string FlowLevel, string FlowTitle, string FreeNodeUser, string FinalCutoffTime)
        {
            string performID = string.Empty;//流程执行流水号
            DataTable dtPerformID = (DataTable)transHelper.ExcuteDataTable(CommandType.Text, "Select Top 1 CodePrefix+RIGHT('0000000000' + CAST(CodeValue AS VARCHAR), 10) as FlowPerformID From TbFlowCode where CodeType='FlowPerformID' ", null);
            if (dtPerformID == null || dtPerformID.Rows.Count < 1)
            {
                transHelper.Rollback();
                throw new Exception("获取流程执行流水号失败");
            }
            performID = dtPerformID.Rows[0]["FlowPerformID"].ToString();

            SqlParameter[] paramater = new SqlParameter[] { 
                new SqlParameter("@FlowCode",FlowCode),
                new SqlParameter("@FlowPerformID",performID),
                new SqlParameter("@FormCode",FormCode),
                new SqlParameter("@FormDataCode",FormDataCode),
                new SqlParameter("@UserCode",userCodeLogin),
                new SqlParameter("@FlowTitle",FlowTitle),
                new SqlParameter("@FlowLevel",FlowLevel),
                new SqlParameter("@FinalCutoffTime",FinalCutoffTime)
            };
            int result = ExecuteDAL.SqlHelper.ExecuteNoneQueryByProc("proc_InitFlowPerform", paramater);
            if (result <= 0)
            {
                throw new Exception("初始化流程执行信息失败!");
            }
            else
            {
                LaunchFlow(performID, 0, DateTime.Now, -1, FreeNodeUser);
            }
        }

        /// <summary>
        /// 发出流程
        /// </summary>
        /// <param name="FlowPerformID">流程执行流水号</param>
        ///<param name="ParentNodeCode">发起（推送）流程节点</param>
        ///<param name="processTime">当前发起时间</param>
        ///<param name="FreeNodeUser">自由选人节点的执行人</param>
        ///<param name="reviewNodeCode">重审发出节点，默认-1，为正常流程</param>        
        public void LaunchFlow(string FlowPerformID, int ParentNodeCode, DateTime processTime, int reviewNodeCode = -1, string FreeNodeUser = "")
        {
            try
            {
                string strChildNode = "";//流程将要推送消息节点
                SqlParameter[] childNodeParas = null;

                //保存没有执行人的自由选人节点的执行人
                DataTable dtFreeNodeUser = new DataTable();
                #region 自由选人
                if (FreeNodeUser != "")
                {
                    dtFreeNodeUser = JsonConvert.DeserializeObject<DataTable>(FreeNodeUser);//JSON方式提交的数据必须是单个表结构
                    string strNodeUser = "Insert Into TbFlowPerformNodePersonnel (FlowPerformID, FlowCode, FlowNodeCode, ActionType, PersonnelSource, PersonnelCode) Values (@FlowPerformID,@FlowCode,@FlowNodeCode,0,'Personnel', @PersonnelCode)";
                    SqlParameter[] freeNodeParas = new SqlParameter[] {
                            new SqlParameter("@FlowPerformID",FlowPerformID),
                            new SqlParameter("@FlowCode",SqlDbType.VarChar),
                            new SqlParameter("@FlowNodeCode",SqlDbType.Int),
                            new SqlParameter("@PersonnelCode",SqlDbType.VarChar)
                        };
                    for (var r = 0; r < dtFreeNodeUser.Rows.Count; r++)
                    {
                        freeNodeParas[1].Value = dtFreeNodeUser.Rows[r]["FlowCode"];
                        freeNodeParas[2].Value = dtFreeNodeUser.Rows[r]["FlowNodeCode"];
                        string PersonnelCodes = dtFreeNodeUser.Rows[r]["PersonnelCode"].ToString();
                        string[] pCodes = PersonnelCodes.Split(',');
                        for (var i = 0; i < pCodes.Length; i++)
                        {
                            freeNodeParas[3].Value = pCodes[i].ToString();
                            transHelper.ExecuteNonQuery(CommandType.Text, strNodeUser, freeNodeParas);
                        }
                    }
                }
                #endregion


                //是退回重审
                if (reviewNodeCode == ParentNodeCode)
                {
                    #region 退回重审节点
                    //获取流程定义的回退重审属性
                    DataTable dtRollbackAttribute = transHelper.ExcuteDataTable(CommandType.Text, "select RollbackAttribute from  TbFlowDefine where FlowCode=(select FlowCode from TbFlowPerform where FlowPerformID=@FlowPerformID )",
                        new SqlParameter[] { new SqlParameter("@FlowPerformID", FlowPerformID) });

                    if (dtRollbackAttribute == null || dtRollbackAttribute.Rows.Count <= 0)
                    {
                        transHelper.Rollback();
                        throw new Exception("获取执行流程的重审回退属性错误");
                    }
                    string rollbackAttribute = dtRollbackAttribute.Rows[0]["RollbackAttribute"].ToString().Trim();
                    if (rollbackAttribute == "0")
                    {//退回指定节点重审（指定节点重审后，再推送到最初退回流程的节点）
                        string strNodeRelation = "select a.ParentNodeCode,a.ChildNodeCode,b.FlowNodeState from TbFlowPerformNodeRelation a left join TbFlowPerformNode b on "
                                + "a.FlowPerformID=b.FlowPerformID and a.ChildNodeCode=b.FlowNodeCode where a.FlowPerformID=@FlowPerformID";

                        DataTable dtNodeRelation = transHelper.ExcuteDataTable(CommandType.Text, strNodeRelation, new SqlParameter[] { new SqlParameter("@FlowPerformID", FlowPerformID) });

                        if (dtNodeRelation == null || dtNodeRelation.Rows.Count <= 0)
                        {
                            transHelper.Rollback();
                            throw new Exception("获取流程节点关系失败");
                        }
                        DataRow[] childNodes = null;
                        childNodes = dtNodeRelation.Select("ParentNodeCode=" + ParentNodeCode);

                        int rollbackNode = PushNode(childNodes, ParentNodeCode, FlowPerformID, processTime, dtNodeRelation);

                        strChildNode = "select " + rollbackNode + " as ChildNodeCode";

                        reviewNodeCode = -1;
                    }
                    else if (rollbackAttribute == "1")
                    {
                        //退回指定节点之前重审(非空节点)
                        //strChildNode = "select ChildNodeCode from TbFlowPerformNodeRelation where ParentNodeCode=0 and FlowPerformID=@FlowPerformID";
                        strChildNode = " select a.ChildNodeCode from TbFlowPerformNodeRelation  a inner join TbFlowPerformNode b " +
                                     "on a.FlowPerformID=b.FlowPerformID and a.FlowCode=b.FlowCode and a.ChildNodeCode=b.FlowNodeCode " +
                                     "where b.BlankNode=0 and a.ParentNodeCode=0 and a.FlowPerformID=@FlowPerformID";
                        childNodeParas = new SqlParameter[] {
                            new SqlParameter("@FlowPerformID",FlowPerformID)
                        };
                    }
                    else
                    {
                        //退回指定节点之后重审(非空节点)
                        // strChildNode = "select ChildNodeCode from TbFlowPerformNodeRelation where FlowPerformID=@FlowPerformID and ParentNodeCode=@ParentNodeCode ";//获取指定流程节点的子节点
                        strChildNode = "select a.ChildNodeCode from TbFlowPerformNodeRelation  a inner join TbFlowPerformNode b " +
                                        "on a.FlowPerformID=b.FlowPerformID and a.FlowCode=b.FlowCode and a.ChildNodeCode=b.FlowNodeCode " +
                                        "where b.BlankNode=0 and a.FlowPerformID=@FlowPerformID and a.ParentNodeCode=@ParentNodeCode ";
                        childNodeParas = new SqlParameter[] {
                            new SqlParameter("@FlowPerformID",FlowPerformID),
                            new SqlParameter("@ParentNodeCode",ParentNodeCode)
                        };
                    }
                    #endregion
                }
                else
                {
                    #region 非退回重审节点

                    strChildNode = "select a.ChildNodeCode,b.BlankNode from TbFlowPerformNodeRelation  a inner join TbFlowPerformNode b " +
                                    "on a.FlowPerformID=b.FlowPerformID and a.FlowCode=b.FlowCode and a.ChildNodeCode=b.FlowNodeCode " +
                                    "where a.FlowPerformID=@FlowPerformID and a.ParentNodeCode=@ParentNodeCode ";
                    childNodeParas = new SqlParameter[] {
                            new SqlParameter("@FlowPerformID",FlowPerformID),
                            new SqlParameter("@ParentNodeCode",ParentNodeCode)
                        };

                    DataTable dtNotStated = transHelper.ExcuteDataTable(CommandType.Text, "select FlowNodeCode from TbFlowPerformNode where FlowNodeCode in" +
                   "(select ChildNodeCode from TbFlowPerformNodeRelation where FlowPerformID=@FlowPerformID and ParentNodeCode=@ParentNodeCode) and BlankNode=0 and FlowNodeState=0 and FlowPerformID=@FlowPerformID", childNodeParas);

                    if (dtNotStated == null || dtNotStated.Rows.Count <= 0) reviewNodeCode = -1;//不是重审节点
                    #endregion
                }

                //获取下一步审批节点
                List<ChildNoteModel> flowNodeCode = new List<ChildNoteModel>();//存放不满足判定条件的节点代码
                List<ChildNoteModel> matchNodeCode = new List<ChildNoteModel>();//存放满足判定条件的节点代码
                List<ChildNoteModel> withoutCriteria = new List<ChildNoteModel>();//存放无判定条件的节点代码

                DataTable tbChildNode = transHelper.ExcuteDataTable(CommandType.Text, strChildNode, childNodeParas);//获取发起节点的子节点

                if (tbChildNode != null && tbChildNode.Rows.Count > 0)
                {
                    for (int i = 0; i < tbChildNode.Rows.Count; i++)
                    {

                        ChildNoteModel model = new ChildNoteModel();
                        model.childNodeCode = Convert.ToInt32(tbChildNode.Rows[i]["ChildNodeCode"]);
                        model.blankNode = Convert.ToInt32(tbChildNode.Rows[i]["BlankNode"]) == 1 ? true : false;
                        if (model.childNodeCode == 9999)
                        {
                            #region 下一节点为结束节点
                            ChangeFlowNodeState(FlowPerformID, model.childNodeCode, 3, processTime);//将节点状态标识成已完成

                            //子节点是流程结束节点时跳过并将流程状态设置为9，已完成
                            ChangeFlowState(FlowPerformID, 9);
                            transHelper.Commit();
                            return;
                            #endregion
                        }
                        else
                        {
                            #region 下一节点为非结束节点
                            Tuple<string, int, string> formInfo = GetPerformFormCode(FlowPerformID);
                            SqlParameter[] sqlParams = new SqlParameter[] {
                                new SqlParameter("@FlowPerformID", FlowPerformID),
                                new SqlParameter("@FormCode", formInfo.Item1),
                                new SqlParameter("@id",formInfo.Item2)
                            };
                            string strSelect = GetStrSql(sqlParams);//不包含判定条件的查询字符串
                            DataTable dtCriteria = transHelper.ExcuteDataTable(CommandType.Text, string.Format("select * from TbFlowPerformNodeJudgeCriteria where FlowPerformID='{0}' and FlowNodeCode={1}", FlowPerformID, tbChildNode.Rows[i]["ChildNodeCode"]), null);
                            if (dtCriteria != null && dtCriteria.Rows.Count > 0)
                            {
                                bool result = IsMatchCriteria(dtCriteria, strSelect, sqlParams);
                                if (result)
                                {
                                    if (!matchNodeCode.Contains(model)) matchNodeCode.Add(model);
                                }
                                else
                                {
                                    if (!flowNodeCode.Contains(model)) flowNodeCode.Add(model);
                                }
                            }
                            else if (dtCriteria == null || (dtCriteria != null && dtCriteria.Rows.Count == 0))
                            {
                                if (!withoutCriteria.Contains(model)) withoutCriteria.Add(model);
                            }
                            #endregion
                        }
                    }
                }
                else
                {
                    throw new Exception("未找到下一审批节点");
                }
                string msgTitle = GetMessageTitle(FlowPerformID);
                //向所有满足条件的审批人提示审批信息

                string sql = "select 1 from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID and FlowNodeCode in (select ParentNodeCode from TbFlowPerformNodeRelation where ChildNodeCode=@ChildNodeCode) and PerformState=-1";
                DataTable dt = transHelper.ExcuteDataTable(CommandType.Text, sql, new SqlParameter[] { new SqlParameter("@FlowPerformID", FlowPerformID), new SqlParameter("@ChildNodeCode", ParentNodeCode) });
                bool isfinally = dt != null && dt.Rows.Count > 0 ? false : true;
                if (!isfinally)
                    ChangeFlowNodeState(FlowPerformID, ParentNodeCode, 3, processTime);
                else
                {
                    for (int i = 0; i < matchNodeCode.Count; i++)
                    {

                        if (matchNodeCode[i].blankNode)
                        {
                            LaunchFlow(FlowPerformID, matchNodeCode[i].childNodeCode, processTime, -1, FreeNodeUser);
                        }
                        else
                        {
                            ChangeFlowNodeState(FlowPerformID, matchNodeCode[i].childNodeCode, 1, processTime);//将接收消息的节点状态设置为1，执行中
                            ChangeFlowState(FlowPerformID, 0);//将流程状态设置为0，执行中
                            CreatePerformMessage(msgTitle, matchNodeCode[i].childNodeCode, FlowPerformID, 0, ParentNodeCode, processTime, reviewNodeCode);//发出审批提示消息
                        }
                    }
                    //向所有无条件节点的审批人添加审批信息
                    for (int i = 0; i < withoutCriteria.Count; i++)
                    {
                        if (withoutCriteria[i].blankNode)
                        {
                            LaunchFlow(FlowPerformID, withoutCriteria[i].childNodeCode, processTime, -1, FreeNodeUser);
                        }
                        else
                        {
                            ChangeFlowNodeState(FlowPerformID, withoutCriteria[i].childNodeCode, 1, processTime);//将接收消息的节点状态设置为1，执行中
                            ChangeFlowState(FlowPerformID, 0);//将流程状态设置为0，执行中
                            CreatePerformMessage(msgTitle, withoutCriteria[i].childNodeCode, FlowPerformID, 0, ParentNodeCode, processTime, reviewNodeCode);//发出审批提示消息
                            //向手机端推送消息
                            //PushPhone(FlowPerformID, Convert.ToString(withoutCriteria[i].childNodeCode));
                        }
                    }
                }


                StringBuilder strIn = new StringBuilder();
                //将禁用节点修改状态
                for (int i = 0; i < flowNodeCode.Count; i++)
                {
                    strIn.Append(flowNodeCode[i].childNodeCode + ",");
                    if (i == flowNodeCode.Count - 1)
                        transHelper.ExecuteNonQuery(CommandType.Text, "update TbFlowPerformNode set FlowNodeState=8 where FlowPerformID=@FlowPerformID and FlowNodeCode in(" + strIn.ToString().Substring(0, strIn.ToString().Length - 1) + ")", new SqlParameter[] { new SqlParameter("@FlowPerformID", FlowPerformID) });
                }

                if (ParentNodeCode == 0) ChangeFlowNodeState(FlowPerformID, ParentNodeCode, 3, processTime);
                transHelper.Commit();
            }
            catch (Exception ex)
            {
                transHelper.Rollback();
                throw ex;
            }
            finally
            {
                transHelper.Close();
            }
            #region 注释__原来实现
            //try
            //{
            //    string strChildNode = "";//流程将要推送消息节点
            //    SqlParameter[] childNodeParas = null;

            //    //保存没有执行人的自由选人节点的执行人
            //    DataTable dtFreeNodeUser = new DataTable();
            //    if (FreeNodeUser != "")
            //    {
            //        dtFreeNodeUser = JsonConvert.DeserializeObject<DataTable>(FreeNodeUser);//JSON方式提交的数据必须是单个表结构
            //        string strNodeUser = "Insert Into TbFlowPerformNodePersonnel (FlowPerformID, FlowCode, FlowNodeCode, ActionType, PersonnelSource, PersonnelCode) Values (@FlowPerformID,@FlowCode,@FlowNodeCode,0,'Personnel', @PersonnelCode)";
            //        SqlParameter[] freeNodeParas = new SqlParameter[] {
            //                new SqlParameter("@FlowPerformID",FlowPerformID),
            //                new SqlParameter("@FlowCode",SqlDbType.VarChar),
            //                new SqlParameter("@FlowNodeCode",SqlDbType.Int),
            //                new SqlParameter("@PersonnelCode",SqlDbType.VarChar)
            //            };
            //        for (var r = 0; r < dtFreeNodeUser.Rows.Count; r++)
            //        {
            //            freeNodeParas[1].Value = dtFreeNodeUser.Rows[r]["FlowCode"];
            //            freeNodeParas[2].Value = dtFreeNodeUser.Rows[r]["FlowNodeCode"];
            //            string PersonnelCodes = dtFreeNodeUser.Rows[r]["PersonnelCode"].ToString();
            //            string[] pCodes = PersonnelCodes.Split(',');
            //            for (var i = 0; i < pCodes.Length; i++)
            //            {
            //                freeNodeParas[3].Value = pCodes[i].ToString();
            //                transHelper.ExecuteNonQuery(CommandType.Text, strNodeUser, freeNodeParas);
            //            }
            //        }
            //    }

            //    if (reviewNodeCode == ParentNodeCode)
            //    { //退回重审节点

            //        //获取流程定义的回退重审属性
            //        DataTable dtRollbackAttribute = transHelper.ExcuteDataTable(CommandType.Text, "select RollbackAttribute from  TbFlowDefine where FlowCode=(select FlowCode from TbFlowPerform where FlowPerformID=@FlowPerformID )",
            //            new SqlParameter[] { new SqlParameter("@FlowPerformID", FlowPerformID) });

            //        if (dtRollbackAttribute == null || dtRollbackAttribute.Rows.Count <= 0)
            //        {
            //            transHelper.Rollback();
            //            throw new Exception("获取执行流程的重审回退属性错误");
            //        }
            //        string rollbackAttribute = dtRollbackAttribute.Rows[0]["RollbackAttribute"].ToString().Trim();
            //        if (rollbackAttribute == "0")
            //        {//退回指定节点重审（指定节点重审后，再推送到最初退回流程的节点）
            //            string strNodeRelation = "select a.ParentNodeCode,a.ChildNodeCode,b.FlowNodeState from TbFlowPerformNodeRelation a left join TbFlowPerformNode b on "
            //                    + "a.FlowPerformID=b.FlowPerformID and a.ChildNodeCode=b.FlowNodeCode where a.FlowPerformID=@FlowPerformID";

            //            DataTable dtNodeRelation = transHelper.ExcuteDataTable(CommandType.Text, strNodeRelation, new SqlParameter[] { new SqlParameter("@FlowPerformID", FlowPerformID) });

            //            if (dtNodeRelation == null || dtNodeRelation.Rows.Count <= 0)
            //            {
            //                transHelper.Rollback();
            //                throw new Exception("获取流程节点关系失败");
            //            }
            //            DataRow[] childNodes = null;
            //            childNodes = dtNodeRelation.Select("ParentNodeCode=" + ParentNodeCode);

            //            int rollbackNode = PushNode(childNodes, ParentNodeCode, FlowPerformID, processTime, dtNodeRelation);

            //            strChildNode = "select " + rollbackNode + " as ChildNodeCode";

            //            reviewNodeCode = -1;
            //        }
            //        else if (rollbackAttribute == "1")
            //        {
            //            //退回指定节点之前重审(非空节点)
            //            //strChildNode = "select ChildNodeCode from TbFlowPerformNodeRelation where ParentNodeCode=0 and FlowPerformID=@FlowPerformID";
            //            strChildNode = " select a.ChildNodeCode from TbFlowPerformNodeRelation  a inner join TbFlowPerformNode b " +
            //                         "on a.FlowPerformID=b.FlowPerformID and a.FlowCode=b.FlowCode and a.ChildNodeCode=b.FlowNodeCode " +
            //                         "where b.BlankNode=0 and a.ParentNodeCode=0 and a.FlowPerformID=@FlowPerformID";
            //            childNodeParas = new SqlParameter[] {
            //                new SqlParameter("@FlowPerformID",FlowPerformID)
            //            };
            //        }
            //        else
            //        {
            //            //退回指定节点之后重审(非空节点)
            //            // strChildNode = "select ChildNodeCode from TbFlowPerformNodeRelation where FlowPerformID=@FlowPerformID and ParentNodeCode=@ParentNodeCode ";//获取指定流程节点的子节点
            //            strChildNode = "select a.ChildNodeCode from TbFlowPerformNodeRelation  a inner join TbFlowPerformNode b " +
            //                            "on a.FlowPerformID=b.FlowPerformID and a.FlowCode=b.FlowCode and a.ChildNodeCode=b.FlowNodeCode " +
            //                            "where b.BlankNode=0 and a.FlowPerformID=@FlowPerformID and a.ParentNodeCode=@ParentNodeCode ";
            //            childNodeParas = new SqlParameter[] {
            //                new SqlParameter("@FlowPerformID",FlowPerformID),
            //                new SqlParameter("@ParentNodeCode",ParentNodeCode)
            //            };
            //        }
            //    }
            //    else
            //    {//(非空节点)
            //        //strChildNode = "select ChildNodeCode from TbFlowPerformNodeRelation where FlowPerformID=@FlowPerformID and ParentNodeCode=@ParentNodeCode ";//获取指定流程节点的子节点
            //        strChildNode = "select a.ChildNodeCode from TbFlowPerformNodeRelation  a inner join TbFlowPerformNode b " +
            //                        "on a.FlowPerformID=b.FlowPerformID and a.FlowCode=b.FlowCode and a.ChildNodeCode=b.FlowNodeCode " +
            //                        "where b.BlankNode=0 and a.FlowPerformID=@FlowPerformID and a.ParentNodeCode=@ParentNodeCode ";
            //        childNodeParas = new SqlParameter[] {
            //                new SqlParameter("@FlowPerformID",FlowPerformID),
            //                new SqlParameter("@ParentNodeCode",ParentNodeCode)
            //            };

            //        //未开始执行节点
            //        //DataTable dtNotStated = transHelper.ExcuteDataTable(CommandType.Text, "select FlowNodeCode from TbFlowPerformNode where FlowNodeCode in" +
            //        //"(select ChildNodeCode from TbFlowPerformNodeRelation where FlowPerformID=@FlowPerformID and ParentNodeCode=@ParentNodeCode) and FlowNodeState=0 and FlowPerformID=@FlowPerformID", childNodeParas);

            //        DataTable dtNotStated = transHelper.ExcuteDataTable(CommandType.Text, "select FlowNodeCode from TbFlowPerformNode where FlowNodeCode in" +
            //       "(select ChildNodeCode from TbFlowPerformNodeRelation where FlowPerformID=@FlowPerformID and ParentNodeCode=@ParentNodeCode) and BlankNode=0 and FlowNodeState=0 and FlowPerformID=@FlowPerformID", childNodeParas);

            //        if (dtNotStated == null || dtNotStated.Rows.Count <= 0) reviewNodeCode = -1;//不是重审节点
            //    }

            //    //获取下一步审批节点
            //    DataTable tbChildNode = transHelper.ExcuteDataTable(CommandType.Text, strChildNode, childNodeParas);//获取发起节点的子节点



            //    List<int> flowNodeCode = new List<int>();//存放不满足判定条件的节点代码
            //    List<int> matchNodeCode = new List<int>();//存放满足判定条件的节点代码
            //    List<int> withoutCriteria = new List<int>();//存放无判定条件的节点代码
            //    if (tbChildNode == null || tbChildNode.Rows.Count <= 0)
            //    {
            //        transHelper.Rollback();
            //        return;//无子节点
            //    }

            //    DataTable dtCriteria;//判定条件
            //    int childNodeCode = 0;
            //    bool matchCriteria = true;

            //    Tuple<string, int, string> formInfo = GetPerformFormCode(FlowPerformID);
            //    SqlParameter[] sqlParams = new SqlParameter[] {
            //        new SqlParameter("@FlowPerformID", FlowPerformID),
            //        new SqlParameter("@FormCode", formInfo.Item1),
            //        new SqlParameter("@id",formInfo.Item2)
            //    };
            //    string strSelect = GetStrSql(sqlParams);//不包含判定条件的查询字符串
            //    if (strSelect == "")
            //    {
            //        transHelper.Rollback();
            //        return;
            //    }
            //    #region 获取所有空节点,执行空节点的判定条件；若条件通过，将流程推送到空节点的下一个节点，再检查其它非空节点；若条件未通过，执行其它非空节点（将该空节点状态改为8已禁止 ）
            //    DataTable dt_BlankNode = transHelper.ExcuteDataTable(CommandType.Text, "select FlowNodeCode from TbFlowPerformNode where FlowPerformID=@FlowPerformID  and " +
            //        " BlankNode=1  and FlowNodeCode in(select ChildNodeCode from TbFlowPerformNodeRelation where FlowPerformID=@FlowPerformID and ParentNodeCode=@ParentNodeCode)",
            //        new SqlParameter[] { 
            //            new SqlParameter("@FlowPerformID",FlowPerformID),
            //            new SqlParameter("@ParentNodeCode",ParentNodeCode)
            //        }
            //        );
            //    if (dt_BlankNode != null && dt_BlankNode.Rows.Count > 0)
            //    {//存在空节点
            //        int blankNode = 0;
            //        foreach (DataRow blankNodeRow in dt_BlankNode.Rows)
            //        {
            //            blankNode = int.Parse(blankNodeRow["FlowNodeCode"].ToString());

            //            dtCriteria = transHelper.ExcuteDataTable(CommandType.Text,
            //                string.Format("select * from TbFlowPerformNodeJudgeCriteria where FlowPerformID='{0}' and FlowNodeCode={1}",
            //                FlowPerformID, blankNode), null);//返回指定节点的判定条件

            //            if (dtCriteria == null) { matchCriteria = true; }

            //            if (dtCriteria != null || dtCriteria.Rows.Count > 0)
            //            {
            //                matchCriteria = IsMatchCriteria(dtCriteria, strSelect, sqlParams);//判断是否满足判定条件
            //            }

            //            if (matchCriteria)
            //            { //空节点判定条件通过
            //                ChangeFlowNodeState(FlowPerformID, blankNode, 3, processTime);//节点状态改为 已完成（3）	
            //                LaunchFlow(FlowPerformID, blankNode, processTime, -1, FreeNodeUser);

            //            }
            //            else
            //            {
            //                ChangeFlowNodeState(FlowPerformID, blankNode, 8, processTime);//节点状态改为 已禁止（8）	
            //            }

            //            dtCriteria = null;
            //        }

            //    }
            //    #endregion

            //    foreach (DataRow drChildNode in tbChildNode.Rows)
            //    {
            //        childNodeCode = int.Parse(drChildNode[0].ToString());

            //        if (childNodeCode == 9999)
            //        {
            //            //CreatePerformMessage(GetMessageTitle(FlowPerformID), childNodeCode, FlowPerformID, 99, -1, DateTime.Now, -1);
            //            ChangeFlowNodeState(FlowPerformID, childNodeCode, 3, processTime);//将节点状态标识成已完成

            //            //子节点是流程结束节点时跳过并将流程状态设置为9，已完成
            //            ChangeFlowState(FlowPerformID, 9);

            //            transHelper.Commit();
            //            return;
            //        };

            //        //获取判断条件Sql
            //        string judgeCriteria = "select * from TbFlowPerformNodeJudgeCriteria where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode";
            //        dtCriteria = transHelper.ExcuteDataTable(CommandType.Text, judgeCriteria, new SqlParameter[] {
            //             new SqlParameter("@FlowPerformID",FlowPerformID),
            //             new SqlParameter("@FlowNodeCode",childNodeCode)});//返回指定节点的判定条件

            //        if (dtCriteria == null || dtCriteria.Rows.Count <= 0)
            //        {
            //            //无判定条件
            //            if (!withoutCriteria.Contains(childNodeCode)) withoutCriteria.Add(childNodeCode);
            //        }
            //        else
            //        {
            //            matchCriteria = IsMatchCriteria(dtCriteria, strSelect, sqlParams);//判断是否满足判定条件
            //            if (!matchCriteria)
            //            {
            //                if (!flowNodeCode.Contains(childNodeCode))
            //                {
            //                    flowNodeCode.Add(childNodeCode);//添加不满足判定条件的节点代码
            //                }
            //            }
            //            else
            //            {
            //                if (!matchNodeCode.Contains(childNodeCode)) matchNodeCode.Add(childNodeCode);//添加满足判定条件的节点代码
            //            }
            //        }
            //    }

            //    #region  判断子节点的所有父节点是否审批完成，完成子节点可用；反之
            //    string parentNodeStr = "select a.ParentNodeCode from TbFlowPerformNodeRelation a left join TbFlowPerformNode b on a.FlowPerformID=b.FlowPerformID and b.FlowNodeCode=a.ParentNodeCode " +
            //                   "where a.FlowPerformID=@FlowPerformID and a.ChildNodeCode=@ChildNodeCode and a.ParentNodeCode!=@ParentNodeCode and b.FlowNodeState not in (3,7,8)";
            //    List<int> disableNode = new List<int>();//需禁用的节点
            //    List<int> enableNode = new List<int>();//可用节点
            //    SqlParameter[] parentParams = new SqlParameter[] { 
            //        new SqlParameter("@FlowPerformID", SqlDbType.VarChar),
            //        new SqlParameter("@ChildNodeCode", SqlDbType.Int),
            //        new SqlParameter("@ParentNodeCode", SqlDbType.Int) };
            //    parentParams[0].Value = FlowPerformID;
            //    parentParams[2].Value = ParentNodeCode;//发起节点
            //    DataTable dtParentNode = null;//未完成审批的父节点
            //    foreach (DataRow drChildNode in tbChildNode.Rows)
            //    {
            //        childNodeCode = int.Parse(drChildNode[0].ToString());
            //        // if (childNodeCode == 9999)continue;//子节点是流程结束节点时跳过
            //        parentParams[1].Value = childNodeCode;
            //        dtParentNode = transHelper.ExcuteDataTable(CommandType.Text, parentNodeStr, parentParams);
            //        if (dtParentNode != null && dtParentNode.Rows.Count == 0)
            //        {
            //            if (flowNodeCode.Contains(childNodeCode))//节点不满足判断条件
            //            {
            //                //添加到不可用节点
            //                if (!disableNode.Contains(childNodeCode))
            //                    disableNode.Add(childNodeCode);
            //            }
            //            else
            //            {
            //                #region 判断同级最后一个流程是否审核通过，不然不让通过
            //                //获取所有未审核的同级元素
            //                string sqlStr = "select p.id,p.FlowPerformID,p.FlowNodeCode,n.FlowNodeName,u.UserCode,u.UserName,case UserType when 0 then '执行人' when 1 then '抄送人' end as UserType,p.PerformState from TbFlowPerformOpinions p left join TbFlowPerformNode n on p.FlowPerformID=n.FlowPerformID and p.FlowNodeCode=n.FlowNodeCode left join TbUser u on p.UserCode=u.UserCode where p.FlowPerformID=@FlowPerformID and p.FlowNodeCode in (select ChildNodeCode from TbFlowPerformNodeRelation where ParentNodeCode in (select ParentNodeCode from TbFlowPerformNodeRelation a where a.FlowPerformID=@FlowPerformID and ChildNodeCode=@ParentNodeCode) and FlowPerformID=@FlowPerformID)";
            //                SqlParameter[] parParams = new SqlParameter[] {
            //                                    new SqlParameter("@FlowPerformID",FlowPerformID),
            //                                    new SqlParameter("@ParentNodeCode",ParentNodeCode)
            //                                };
            //                //所有节点
            //                DataTable dataTables = transHelper.ExcuteDataTable(CommandType.Text, sqlStr, parParams);
            //                //未审核节点
            //                DataTable ExistsTable = transHelper.ExcuteDataTable(CommandType.Text, sqlStr + " and p.PerformState=-1 and p.UserType !=1", parParams);
            //                //有数据判断，没有数据表示开始流程，直接跳过
            //                if (dataTables != null && dataTables.Rows.Count > 0)
            //                {
            //                    //判断登录人和最后一条记录是否一样，是的话审核通过，添加节点，反之
            //                    string userCode = HttpContext.Current.Session[UserInfo.USER_CODE].ToString().Trim();//用户代码
            //                    string dataLastUserCode = dataTables.Rows[dataTables.Rows.Count - 1]["UserCode"].ToString();
            //                    if (dataLastUserCode == userCode)
            //                    {
            //                        if (ExistsTable.Rows.Count > 0)
            //                        {
            //                            transHelper.Rollback();
            //                            throw new Exception("" + ExistsTable.Rows[0]["FlowNodeName"] + "-" + ExistsTable.Rows[0]["UserType"] + "-" + ExistsTable.Rows[0]["UserName"] + " 还未同意审批，请先通知审批通过！");
            //                        }
            //                        else
            //                        {
            //                            //所有父节点完成审核并且节点满足判断条件，当前子节点可用
            //                            //添加到可用节点
            //                            if (withoutCriteria.Count > 0)
            //                            {
            //                                foreach (int i in withoutCriteria)
            //                                {
            //                                    if (i == childNodeCode)
            //                                    {
            //                                        enableNode.Add(i);
            //                                    }
            //                                }
            //                            }
            //                            else if (!enableNode.Contains(childNodeCode)) enableNode.Add(childNodeCode);
            //                        }
            //                    }
            //                    else
            //                    {
            //                        if (!enableNode.Contains(childNodeCode)) enableNode.Add(childNodeCode);
            //                    }
            //                }
            //                else
            //                {
            //                    //所有父节点完成审核并且节点满足判断条件，当前子节点可用
            //                    //添加到可用节点
            //                    if (withoutCriteria.Count > 0)
            //                    {
            //                        foreach (int i in withoutCriteria)
            //                        {
            //                            if (i == childNodeCode)
            //                            {
            //                                enableNode.Add(i);
            //                            }
            //                        }
            //                    }
            //                    else if (!enableNode.Contains(childNodeCode)) enableNode.Add(childNodeCode);
            //                }
            //                #endregion
            //            }
            //        }
            //        else
            //        {
            //            //添加到不可用节点
            //            //if (!disableNode.Contains(childNodeCode))
            //            //    disableNode.Add(childNodeCode);

            //            #region 判断同级最后一个流程是否审核通过，不然不让通过
            //            //获取所有未审核的同级元素
            //            string sqlStr = "select p.id,p.FlowPerformID,p.FlowNodeCode,n.FlowNodeName,u.UserCode,u.UserName,case UserType when 0 then '执行人' when 1 then '抄送人' end as UserType,p.PerformState from TbFlowPerformOpinions p left join TbFlowPerformNode n on p.FlowPerformID=n.FlowPerformID and p.FlowNodeCode=n.FlowNodeCode left join TbUser u on p.UserCode=u.UserCode where p.FlowPerformID=@FlowPerformID and p.FlowNodeCode in (select ParentNodeCode from TbFlowPerformNodeRelation where ChildNodeCode in (select ChildNodeCode from TbFlowPerformNodeRelation where FlowPerformID=@FlowPerformID and ParentNodeCode=@ParentNodeCode) and FlowPerformID=@FlowPerformID)";
            //            SqlParameter[] parParams = new SqlParameter[] {
            //                                    new SqlParameter("@FlowPerformID",FlowPerformID),
            //                                    new SqlParameter("@ParentNodeCode",ParentNodeCode)
            //                                };
            //            //所有节点
            //            DataTable dataTables = transHelper.ExcuteDataTable(CommandType.Text, sqlStr, parParams);
            //            //未审核节点
            //            DataTable ExistsTable = transHelper.ExcuteDataTable(CommandType.Text, sqlStr + " and p.PerformState=-1 and p.UserType !=1", parParams);
            //            //有数据判断，没有数据表示开始流程，直接跳过
            //            if (dataTables != null && dataTables.Rows.Count > 0)
            //            {
            //                //判断登录人和最后一条记录是否一样，是的话审核通过，添加节点，反之
            //                string userCode = HttpContext.Current.Session[UserInfo.USER_CODE].ToString().Trim();//用户代码
            //                string dataLastUserCode = dataTables.Rows[dataTables.Rows.Count - 1]["UserCode"].ToString();
            //                if (dataLastUserCode == userCode)
            //                {
            //                    if (ExistsTable.Rows.Count > 0)
            //                    {
            //                        transHelper.Rollback();
            //                        throw new Exception("" + ExistsTable.Rows[0]["FlowNodeName"] + "-" + ExistsTable.Rows[0]["UserType"] + "-" + ExistsTable.Rows[0]["UserName"] + " 还未同意审批，请先通知审批通过！");
            //                    }
            //                    else
            //                    {
            //                        if (!enableNode.Contains(childNodeCode) && withoutCriteria.Count > 0)
            //                            enableNode.Add(childNodeCode);
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                if (!enableNode.Contains(childNodeCode) && withoutCriteria.Count > 0)
            //                    enableNode.Add(childNodeCode);
            //            }
            //            #endregion

            //            //if (!enableNode.Contains(childNodeCode) && withoutCriteria.Count > 0)
            //            //    enableNode.Add(childNodeCode);
            //        }
            //    }
            //    #endregion

            //    #region 设置禁用节点，并对可用节点添加推送消息
            //    StringBuilder strIn = new StringBuilder();
            //    if (disableNode.Count > 0)
            //    {
            //        for (int i = 0; i < disableNode.Count; i++)
            //        {
            //            if (i == 0) strIn.Append(disableNode[i]);
            //            else strIn.Append("," + disableNode[i]);
            //        }

            //        transHelper.ExecuteNonQuery(CommandType.Text, "update TbFlowPerformNode set FlowNodeState=8 where FlowPerformID=@FlowPerformID and FlowNodeCode in(" + strIn.ToString() + ")", sqlParams);
            //    }

            //    if (enableNode.Count > 0)
            //    {
            //        //写推送消息
            //        string msgTitle = GetMessageTitle(FlowPerformID);
            //        int msgType = 0;//消息类型
            //        if (reviewNodeCode > -1) msgType = 3;//重审提示消息
            //        for (int i = 0; i < enableNode.Count; i++)
            //        {
            //            ChangeFlowNodeState(FlowPerformID, enableNode[i], 1, processTime);//将接收消息的节点状态设置为1，执行中

            //            CreatePerformMessage(msgTitle, enableNode[i], FlowPerformID, msgType, ParentNodeCode, processTime, reviewNodeCode);//发出审批提示消息
            //        }
            //        ChangeFlowState(FlowPerformID, 0);//将流程状态设置为0，执行中

            //    }

            //    if (ParentNodeCode == 0) ChangeFlowNodeState(FlowPerformID, ParentNodeCode, 3, processTime);//审批意见处理后将该流程节点状态改为 已完成（3）


            //    #endregion

            //    transHelper.Commit();
            //}
            //catch (Exception ex)
            //{
            //    transHelper.Rollback();
            //    throw ex;
            //}
            //finally
            //{
            //    transHelper.Close();
            //}
            #endregion

        }

        //------------修改-----------------//
        public void LaunchEndFlow(string FlowPerformID, int ParentNodeCode, DateTime processTime, int reviewNodeCode = -1, string FreeNodeUser = "")
        {
            try
            {

                string strChildNode = "";//流程将要推送消息节点
                SqlParameter[] childNodeParas = null;

                //保存没有执行人的自由选人节点的执行人
                DataTable dtFreeNodeUser = new DataTable();
                strChildNode = "select a.ChildNodeCode from TbFlowPerformNodeRelation  a inner join TbFlowPerformNode b " +
                                    "on a.FlowPerformID=b.FlowPerformID and a.FlowCode=b.FlowCode and a.ChildNodeCode=b.FlowNodeCode " +
                                    "where b.BlankNode=0 and a.FlowPerformID=@FlowPerformID and a.ParentNodeCode=@ParentNodeCode ";
                childNodeParas = new SqlParameter[] {
                            new SqlParameter("@FlowPerformID",FlowPerformID),
                            new SqlParameter("@ParentNodeCode",ParentNodeCode)
                        };
                DataTable dtNotStated = transHelper.ExcuteDataTable(CommandType.Text, "select FlowNodeCode from TbFlowPerformNode where FlowNodeCode in" +
               "(select ChildNodeCode from TbFlowPerformNodeRelation where FlowPerformID=@FlowPerformID and ParentNodeCode=@ParentNodeCode) and BlankNode=0 and FlowNodeState=0 and FlowPerformID=@FlowPerformID", childNodeParas);

                if (dtNotStated == null || dtNotStated.Rows.Count <= 0) reviewNodeCode = -1;//不是重审节点

                //获取下一步审批节点

                DataTable tbChildNode = transHelper.ExcuteDataTable(CommandType.Text, strChildNode, childNodeParas);//获取发起节点的子节点

                List<int> flowNodeCode = new List<int>();//存放不满足判定条件的节点代码
                List<int> matchNodeCode = new List<int>();//存放满足判定条件的节点代码
                List<int> withoutCriteria = new List<int>();//存放无判定条件的节点代码
                if (tbChildNode == null || tbChildNode.Rows.Count <= 0)
                {
                    transHelper.Rollback();
                    return;//无子节点
                }


                DataTable dtCriteria;//判定条件

                int childNodeCode = 0;
                bool matchCriteria = true;

                Tuple<string, int, string> formInfo = GetPerformFormCode(FlowPerformID);
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@FlowPerformID", FlowPerformID),
                    new SqlParameter("@FormCode", formInfo.Item1),
                    new SqlParameter("@id",formInfo.Item2)
                };
                string strSelect = GetStrSql(sqlParams);//不包含判定条件的查询字符串
                if (strSelect == "")
                {
                    transHelper.Rollback();
                    return;
                }

                #region 获取所有空节点,执行空节点的判定条件；若条件通过，将流程推送到空节点的下一个节点，再检查其它非空节点；若条件未通过，执行其它非空节点（将该空节点状态改为8已禁止 ）
                DataTable dt_BlankNode = transHelper.ExcuteDataTable(CommandType.Text, "select FlowNodeCode from TbFlowPerformNode where FlowPerformID=@FlowPerformID  and " +
                    " BlankNode=1  and FlowNodeCode in(select ChildNodeCode from TbFlowPerformNodeRelation where FlowPerformID=@FlowPerformID and ParentNodeCode=@ParentNodeCode)",
                    new SqlParameter[] { 
                        new SqlParameter("@FlowPerformID",FlowPerformID),
                        new SqlParameter("@ParentNodeCode",ParentNodeCode)
                    }
                    );
                if (dt_BlankNode != null || dt_BlankNode.Rows.Count > 0)
                {//存在空节点
                    int blankNode = 0;
                    foreach (DataRow blankNodeRow in dt_BlankNode.Rows)
                    {
                        blankNode = int.Parse(blankNodeRow["FlowNodeCode"].ToString());

                        dtCriteria = transHelper.ExcuteDataTable(CommandType.Text,
                            string.Format("select * from TbFlowPerformNodeJudgeCriteria where FlowPerformID='{0}' and FlowNodeCode={1}",
                            FlowPerformID, blankNode), null);//返回指定节点的判定条件

                        if (dtCriteria == null) { matchCriteria = true; }

                        if (dtCriteria != null || dtCriteria.Rows.Count > 0)
                        {
                            matchCriteria = IsMatchCriteria(dtCriteria, strSelect, sqlParams);//判断是否满足判定条件
                        }

                        if (matchCriteria)
                        { //空节点判定条件通过
                            ChangeFlowNodeState(FlowPerformID, blankNode, 3, processTime);//节点状态改为 已完成（3）	
                            LaunchFlow(FlowPerformID, blankNode, processTime, -1, FreeNodeUser);

                        }
                        else
                        {
                            ChangeFlowNodeState(FlowPerformID, blankNode, 8, processTime);//节点状态改为 已禁止（8）	
                        }

                        dtCriteria = null;
                    }

                }
                #endregion


                foreach (DataRow drChildNode in tbChildNode.Rows)
                {
                    childNodeCode = int.Parse(drChildNode[0].ToString());

                    if (childNodeCode == 9999)
                    {
                        ChangeFlowNodeState(FlowPerformID, childNodeCode, 2, processTime);//将节点状态标识成已完成

                        //子节点是流程结束节点时跳过并将流程状态设置为9，已完成
                        ChangeFlowState(FlowPerformID, 2);

                        transHelper.Commit();
                        return;
                    };


                    //获取判断条件Sql
                    string judgeCriteria = "select * from TbFlowPerformNodeJudgeCriteria where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode";
                    dtCriteria = transHelper.ExcuteDataTable(CommandType.Text, judgeCriteria, new SqlParameter[] {
                         new SqlParameter("@FlowPerformID",FlowPerformID),
                         new SqlParameter("@FlowNodeCode",childNodeCode)});//返回指定节点的判定条件


                    if (dtCriteria == null || dtCriteria.Rows.Count <= 0)
                    {
                        //无判定条件
                        if (!withoutCriteria.Contains(childNodeCode)) withoutCriteria.Add(childNodeCode);
                    }
                    else
                    {
                        matchCriteria = IsMatchCriteria(dtCriteria, strSelect, sqlParams);//判断是否满足判定条件
                        if (!matchCriteria)
                        {
                            if (!flowNodeCode.Contains(childNodeCode))
                            {
                                flowNodeCode.Add(childNodeCode);//添加不满足判定条件的节点代码
                            }
                        }
                        else
                        {
                            if (!matchNodeCode.Contains(childNodeCode)) matchNodeCode.Add(childNodeCode);//添加满足判定条件的节点代码
                        }
                    }
                }

                #region  判断子节点的所有父节点是否审批完成，完成子节点可用；反之
                string parentNodeStr = "select a.ParentNodeCode from TbFlowPerformNodeRelation a left join TbFlowPerformNode b on a.FlowPerformID=b.FlowPerformID and b.FlowNodeCode=a.ParentNodeCode " +
                                    "where a.FlowPerformID=@FlowPerformID and a.ChildNodeCode=@ChildNodeCode and a.ParentNodeCode!=@ParentNodeCode and b.FlowNodeState not in (3,7,8)";
                List<int> disableNode = new List<int>();//需禁用的节点
                List<int> enableNode = new List<int>();//可用节点
                SqlParameter[] parentParams = new SqlParameter[] { 
                    new SqlParameter("@FlowPerformID", SqlDbType.VarChar),
                    new SqlParameter("@ChildNodeCode", SqlDbType.Int),
                    new SqlParameter("@ParentNodeCode", SqlDbType.Int) };
                parentParams[0].Value = FlowPerformID;
                parentParams[2].Value = ParentNodeCode;//发起节点
                DataTable dtParentNode = null;//未完成审批的父节点
                foreach (DataRow drChildNode in tbChildNode.Rows)
                {
                    childNodeCode = int.Parse(drChildNode[0].ToString());
                    // if (childNodeCode == 9999)continue;//子节点是流程结束节点时跳过
                    parentParams[1].Value = childNodeCode;
                    dtParentNode = transHelper.ExcuteDataTable(CommandType.Text, parentNodeStr, parentParams);
                    if (dtParentNode != null && dtParentNode.Rows.Count == 0)
                    {
                        if (flowNodeCode.Contains(childNodeCode))//节点不满足判断条件
                        {
                            //添加到不可用节点
                            if (!disableNode.Contains(childNodeCode))
                                disableNode.Add(childNodeCode);
                        }
                        else
                        {
                            //所有父节点完成审核并且节点满足判断条件，当前子节点可用
                            //添加到可用节点
                            if (!enableNode.Contains(childNodeCode)) enableNode.Add(childNodeCode);
                        }
                    }
                    else
                    {
                        //添加到不可用节点
                        if (!disableNode.Contains(childNodeCode))
                            disableNode.Add(childNodeCode);
                    }
                }
                #endregion

                #region 设置禁用节点，并对可用节点添加推送消息
                StringBuilder strIn = new StringBuilder();
                if (disableNode.Count > 0)
                {
                    for (int i = 0; i < disableNode.Count; i++)
                    {
                        if (i == 0) strIn.Append(disableNode[i]);
                        else strIn.Append("," + disableNode[i]);
                    }

                    transHelper.ExecuteNonQuery(CommandType.Text, "update TbFlowPerformNode set FlowNodeState=8 where FlowPerformID=@FlowPerformID and FlowNodeCode in(" + strIn.ToString() + ")", sqlParams);

                }

                if (enableNode.Count > 0)
                {//写推送消息
                    string msgTitle = GetMessageTitle(FlowPerformID);
                    int msgType = 0;//消息类型
                    if (reviewNodeCode > -1) msgType = 3;//重审提示消息
                    for (int i = 0; i < enableNode.Count; i++)
                    {
                        ChangeFlowNodeState(FlowPerformID, enableNode[i], 1, processTime);//将接收消息的节点状态设置为1，执行中

                        CreatePerformMessage(msgTitle, enableNode[i], FlowPerformID, msgType, ParentNodeCode, processTime, reviewNodeCode);//发出审批提示消息
                    }
                    ChangeFlowState(FlowPerformID, 0);//将流程状态设置为0，执行中

                }

                if (ParentNodeCode == 0) ChangeFlowNodeState(FlowPerformID, ParentNodeCode, 3, processTime);//审批意见处理后将该流程节点状态改为 已完成（3）


                #endregion

                transHelper.Commit();
            }
            catch (Exception ex)
            {
                transHelper.Rollback();
                throw ex;
            }
            finally
            {
                transHelper.Close();
            }


        }
        //------------修改-----------------//


        /// <summary>
        /// 退回节点重审，向不需要审核节点推送消息
        /// </summary>
        /// <param name="childNodes">推送消息节点信息</param>
        /// <param name="ParentNodeCode">父节点</param>
        /// <param name="FlowPerformID"></param>
        /// <param name="processTime"></param>
        /// <param name="dtNodeRelation">该流程节点关系</param>
        /// <returns>rollbackNode 退回流程节点</returns>
        private int PushNode(DataRow[] childNodes, int ParentNodeCode, string FlowPerformID, DateTime processTime, DataTable dtNodeRelation)
        {
            int rollbackNode = -1;
            int nodeState = -1;//节点状态
            int childNode = -1;
            if (childNodes != null)
            {
                string msgTitle = GetMessageTitle(FlowPerformID);
                foreach (DataRow item in childNodes)
                {
                    nodeState = int.Parse(item["FlowNodeState"].ToString().Trim());
                    childNode = int.Parse(item["ChildNodeCode"].ToString().Trim());
                    if (nodeState != 0 && nodeState != 1 && nodeState != 2)
                    {//0未开始，1执行中，2退回
                        CreatePerformMessage(msgTitle, childNode, FlowPerformID, 4, ParentNodeCode, processTime, -1, false);//不创建审批意见,消息类型4（其他人修订）
                        rollbackNode = PushNode(dtNodeRelation.Select("ParentNodeCode=" + childNode), childNode, FlowPerformID, processTime, dtNodeRelation);

                    }
                    else if (nodeState == 1)
                    {
                        CreatePerformMessage(msgTitle, childNode, FlowPerformID, 4, ParentNodeCode, processTime, -1, false);//不创建审批意见,消息类型4（其他人修订）
                        break;
                    }
                    else if (nodeState == 2)
                    {
                        rollbackNode = childNode;
                        //CreatePerformMessage("", childNode, FlowPerformID, 4, ParentNodeCode, processTime, -1);//需创建审批意见
                        break;
                    }
                }
            }
            return rollbackNode;
        }

        /// <summary>
        /// 判断是否满足判定条件
        /// </summary>
        /// <returns>true满足，false 不满足判定条件</returns>
        private bool IsMatchCriteria(DataTable dtCriteria, string strSelect, SqlParameter[] sqlParams)
        {

            bool result = true;
            StringBuilder strCriteria = new StringBuilder(" and ");
            //判定条件
            string tableName = "";
            string fieldName = "";
            string judgeSymbol = "";//判断符合
            string BBrackets = "";//前括号
            string LBrackets = "";//后括号
            string judgeValue = "";//值
            string judgeRelation = "";//关联关系
            try
            {
                #region 判定条件
                foreach (DataRow drCriteria in dtCriteria.Rows)
                {
                    tableName = "V_" + drCriteria["PhysicalTableName"].ToString().Trim();
                    fieldName = drCriteria["FieldCode"].ToString().Trim();
                    judgeSymbol = drCriteria["JudgeSymbol"].ToString().Trim();
                    BBrackets = drCriteria["BeforeBrackets"].ToString().Trim();
                    LBrackets = drCriteria["LastBrackets"].ToString().Trim();
                    judgeValue = drCriteria["JudgeValue"].ToString().Trim();
                    judgeRelation = drCriteria["JudgeRelation"].ToString().Trim();


                    switch (judgeSymbol)
                    {
                        case "like":
                            strCriteria.AppendFormat(" {0} {1}.{2} like '%{3}%' {4} {5}", BBrackets, tableName, fieldName, judgeValue, LBrackets, judgeRelation);
                            break;
                        case "leftLike":
                            strCriteria.AppendFormat(" {0} {1}.{2} like '%{3}' {4} {5}", BBrackets, tableName, fieldName, judgeValue, LBrackets, judgeRelation);
                            break;
                        case "rightLike":
                            strCriteria.AppendFormat(" {0} {1}.{2} like '{3}%' {4} {5}", BBrackets, tableName, fieldName, judgeValue, LBrackets, judgeRelation);
                            break;
                        case "notLike":
                            strCriteria.AppendFormat(" {0} {1}.{2} not like '%{3}%' {4} {5}", BBrackets, tableName, fieldName, judgeValue, LBrackets, judgeRelation);
                            break;
                        case "notLeftLike":
                            strCriteria.AppendFormat(" {0} {1}.{2} not like '%{3}' {4} {5}", BBrackets, tableName, fieldName, judgeValue, LBrackets, judgeRelation);
                            break;
                        case "notRightLike":
                            strCriteria.AppendFormat(" {0} {1}.{2} not like '{3}%' {4} {5}", BBrackets, tableName, fieldName, judgeValue, LBrackets, judgeRelation);
                            break;
                        default:
                            strCriteria.AppendFormat(" {0} {1}.{2}{3}'{4}' {5} {6}", BBrackets, tableName, fieldName, judgeSymbol, judgeValue, LBrackets, judgeRelation);
                            break;

                    }
                }
                #endregion

                if (strSelect != "")
                {
                    strSelect += strCriteria.ToString();

                    DataTable query = transHelper.ExcuteDataTable(CommandType.Text, strSelect, sqlParams);

                    if (query == null || query.Rows.Count <= 0)
                    {
                        result = false;//不满足判定条件
                    }
                    else
                    {
                        if (int.Parse(query.Rows[0][0].ToString()) <= 0)
                        {
                            result = false;//不满足判定条件
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                throw new Exception("检查流程节点是否满足进入条件错误：" + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 返回查询字符串，不包含判定条件
        /// </summary>
        /// <returns></returns>
        private string GetStrSql(SqlParameter[] sqlParams)
        {
            StringBuilder strSelect = new StringBuilder("");

            //获取form表单主从表都存在的字段，和表名
            string formTableStr = "select a.PhysicalTableName,a.FieldCode,b.MainSubTable from TbSysTableFields a left join TbFormTable b on a.PhysicalTableName=b.PhysicalTableName where b.FormCode=@FormCode" +
            " and a.FieldCode not in(select FieldCode from TbSysTableFields f left join TbFormTable t on f.PhysicalTableName=t.PhysicalTableName where t.FormCode=@FormCode group by FieldCode having count(FieldCode)>1);" +
            "select PhysicalTableName,MainSubTable from TbFormTable where FormCode=@FormCode";
            try
            {

                DataSet dsFields = transHelper.ExcuteDataSet(CommandType.Text, formTableStr, sqlParams);

                if (dsFields != null)
                {
                    //dsFields.Tables[1] 表单关联表主从表
                    DataTable formTable = dsFields.Tables[1];
                    if (formTable == null && formTable.Rows.Count <= 0) return "";//无关联表单

                    DataTable tb_Fields = dsFields.Tables[0];
                    //dsFields.Tables[0] 关联表单主从表关联字段
                    if (tb_Fields != null && tb_Fields.Rows.Count > 0)
                    {
                        //获取主表和从表关联字段
                        DataRow[] tbMainRelationFields = tb_Fields.Select("MainSubTable=0");
                        //从表和主表关联字段
                        DataRow[] tbSubRelationFields = tb_Fields.Select("MainSubTable=1");

                        StringBuilder strWhere = new StringBuilder("");

                        foreach (DataRow itemMain in tbMainRelationFields)
                        {
                            foreach (DataRow itemSub in tbSubRelationFields)
                            {
                                if (itemMain["FieldCode"].ToString() == itemSub["FieldCode"].ToString())
                                {
                                    if (strWhere.ToString() != "")
                                    {
                                        strWhere.Append(" and ");
                                    }
                                    else
                                    {
                                        strWhere.Append(" where ");
                                    }
                                    strWhere.AppendFormat("{0}.{1}={2}.{3}", "V_" + itemMain["PhysicalTableName"].ToString(), itemMain["FieldCode"].ToString(), "V_" + itemSub["PhysicalTableName"].ToString(), itemSub["FieldCode"].ToString());
                                }
                            }
                        }

                        //追加ID过滤条件
                        if (strWhere.ToString() != "")
                        {
                            strWhere.Append(" and ");
                        }
                        else
                        {
                            strWhere.Append(" where ");
                        }

                        //主表.id=@id
                        strWhere.AppendFormat("{0}.id=@id", "V_" + formTable.Select("MainSubTable=0")[0]["PhysicalTableName"]);

                        foreach (DataRow drTable in formTable.Rows)
                        {
                            if (strSelect.ToString() == "")
                            {
                                strSelect.Append("select count(0) from V_" + drTable["PhysicalTableName"].ToString());
                            }
                            else
                            {
                                strSelect.Append(",V_" + drTable["PhysicalTableName"].ToString());
                            }
                        }

                        if (strSelect.ToString() != "") { strSelect.Append(strWhere.ToString()); }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("获取流程管理表单查询字符串错误：" + ex.Message);
            }
            return strSelect.ToString();
        }


        /// <summary>
        ///  创建执行消息,和初始化审批意见信息
        /// </summary>
        /// <param name="msgTitle">消息标题</param>
        /// <param name="FlowNodeCode">推送消息流程节点</param>
        /// <param name="FlowPerformID">流程执行流水号</param>
        /// <param name="messageType">消息类型</param>
        /// <param name="parentNodeCode">当前发出消息流程节点</param>
        /// <param name="processTime">发起时间</param>
        /// <param name="reviewNodeCode">重审发出节点代码</param>
        /// <param name="isCreateOpinion">是否创建审批意见</param>
        public void CreatePerformMessage(string msgTitle, int FlowNodeCode, string FlowPerformID, int messageType, int parentNodeCode, DateTime processTime, int reviewNodeCode, bool isCreateOpinion = true)
        {
            Hashtable cacheValues = new Hashtable();

            #region 创建消息,写（消息接收人员表）推送消息、审批意见信息

            string messageID = string.Empty;//流程执行消息ID

            messageID = GetMessageID();//获取消息流水号

            try
            {
                if (string.IsNullOrEmpty(messageID)) return;

                string strPerformMsg = "if not exists(select 1 from TbFlowPerformMessage where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode) insert into TbFlowPerformMessage(messageID,messageCreateTime,messageType,messageTitle,messageContent,FlowPerformID,FlowNodeCode)" +
                        "values(@messageID,@processTime,@messageType,@messageTitle,@messageContent,@FlowPerformID,@FlowNodeCode)";

                SqlParameter[] parsMsg = new SqlParameter[] {
                    new SqlParameter("@messageID",SqlDbType.VarChar),
                    new SqlParameter("@messageType",SqlDbType.Int),
                    new SqlParameter("@messageTitle",SqlDbType.VarChar),
                    new SqlParameter("@messageContent",SqlDbType.VarChar),
                    new SqlParameter("@FlowPerformID",SqlDbType.VarChar),
                    new SqlParameter("@FlowNodeCode",SqlDbType.Int),
                    new SqlParameter("@processTime",SqlDbType.DateTime)
                };

                parsMsg[0].Value = messageID;
                parsMsg[1].Value = messageType;//-------------------------需判断---------------------
                parsMsg[2].Value = msgTitle;
                parsMsg[3].Value = msgTitle;//上个步骤的执行人+审批意见
                parsMsg[4].Value = FlowPerformID;
                parsMsg[5].Value = FlowNodeCode;
                parsMsg[6].Value = processTime;

                transHelper.ExecuteNonQuery(CommandType.Text, strPerformMsg, parsMsg);

                //                String sqlStr = "";
                //                DataSet dataset = new DataSet();
                //                if (messageType == 3 || messageType == 99 ||messageType == 1) 
                //                { //推送流程终止消息到之前的所有节点
                //                    sqlStr = @"select * from (select a.*,b.FlowNodeName from (
                //                                select a.*,b.FlowCode from TbFlowPerformOpinions a 
                //                                left join TbFlowPerform b on a.FlowPerformID = b.FlowPerformID 
                //                            ) a left join TbFlowNode b on a.FlowNodeCode = b.FlowNodeCode and a.FlowCode = b.FlowCode 
                //                            inner join (
                //                                select MAX(id) id,FlowPerformID,UserCode from TbFlowPerformOpinions group by FlowPerformID,UserCode
                //                            ) c on a.id = c.id
                //                            union all
                //                            select -1,FlowPerformID,0,BeginTime,UserCode,0,1,BeginTime,'',-1,'',FlowCode,'发起人' from TbFlowPerform 
                //                            ) a where a.FlowPerformID = @FlowPerformID and UserCode != @UserCode";
                //                    int newSize = parsMsg.Length+1;
                //                    Array.Resize(ref parsMsg, newSize);
                //                    parsMsg[newSize-1] = new SqlParameter("@UserCode", SqlDbType.VarChar);
                //                    parsMsg[newSize - 1].Value = messageType == 1 ? "" : HttpContext.Current.Session[UserInfo.USER_CODE].ToString();
                //                    dataset = transHelper.ExcuteDataSet(CommandType.Text, sqlStr, parsMsg);

                //                    if (messageType == 3) parsMsg[3].Value = "【" + msgTitle + "】该流程已被终止！";
                //                    else if (messageType == 1) parsMsg[3].Value = "【" + msgTitle + "】该流程已被发起人【" + HttpContext.Current.Session[UserInfo.USER_NAME].ToString() + "】撤销！";
                //                    else parsMsg[3].Value = "【" + msgTitle + "】该流程已审核完成！";

                //                    parsMsg[0].Value = GetMessageID();
                //                    WritePersonnels(parsMsg[0].Value.ToString(), dataset.Tables[0], messageType);//写入消息接收人员
                //                    transHelper.ExecuteNonQuery(CommandType.Text, strPerformMsg, parsMsg);
                //                    transHelper.Commit();
                //                    return;
                //                }
                //                else if (messageType==2)
                //                { //推送流程回退消息到之前的所有节点，除了回退的目标节点
                //                    sqlStr = @"select * from (select a.*,b.FlowNodeName from (
                //                                select a.*,b.FlowCode from TbFlowPerformOpinions a 
                //                                left join TbFlowPerform b on a.FlowPerformID = b.FlowPerformID 
                //                            ) a left join TbFlowNode b on a.FlowNodeCode = b.FlowNodeCode and a.FlowCode = b.FlowCode 
                //                            inner join (
                //                                select MAX(id) id,FlowPerformID,UserCode from TbFlowPerformOpinions group by FlowPerformID,UserCode
                //                            ) c on a.id = c.id
                //                            union all
                //                            select -1,FlowPerformID,0,BeginTime,UserCode,0,1,BeginTime,'',-1,'',FlowCode,'发起人' from TbFlowPerform 
                //                            ) a where a.FlowPerformID = @FlowPerformID and a.FlowNodeCode != @FlowNodeCode";
                //                    dataset = transHelper.ExcuteDataSet(CommandType.Text, sqlStr, parsMsg);

                //                    String FlowNodeName = transHelper.ExecuteScalar(CommandType.Text, @"select FlowNodeName from TbFlowPerform a left join TbFlowNode b on a.FlowCode = b.FlowCode 
                //                                where FlowNodeCode = @FlowNodeCode and a.FlowPerformID = @FlowPerformID", parsMsg).ToString();
                //                    parsMsg[3].Value = "【" + msgTitle + "】该流程已退回到节点【" + FlowNodeName + "】！";

                //                    parsMsg[0].Value = GetMessageID();
                //                    WritePersonnels(parsMsg[0].Value.ToString(), dataset.Tables[0], messageType);//写入消息接收人员
                //                    transHelper.ExecuteNonQuery(CommandType.Text, strPerformMsg, parsMsg);
                //                }

                if (messageType == 10) return;//消息为附言，只创建消息，流程节点代码为空


                //写消息接收人员
                cacheValues["nodeCode"] = FlowNodeCode;
                cacheValues["performID"] = FlowPerformID;

                string personnelSource = string.Empty;//人员来源（类型）
                string personnelCode = string.Empty;//人员代码
                int userType = 0;//人员类型（0执行人，1抄送人）
                DataSet dsPersonnel = null;//人员集合
                transHelper.Commit();

                #region 获取流程发起人，接收消息人员

                string userCode = "";//发起人
                DataSet dsPerformPersonnel = null;//接收消息节点配置人员

                if (parentNodeCode == 0)//从流程发起节点发出流程
                {
                    userCode = userCodeLogin;
                }
                else
                {//流程审批过程中

                    DataTable dtUserCode = transHelper.ExcuteDataTable(CommandType.Text, "select UserCode from TbFlowPerform where FlowPerformID=@FlowPerformID", new SqlParameter[] { new SqlParameter("@FlowPerformID", FlowPerformID) });

                    if (dtUserCode != null && dtUserCode.Rows.Count > 0)
                    {
                        userCode = dtUserCode.Rows[0]["UserCode"].ToString().Trim();
                    }
                    else
                    {
                        throw new Exception("获取发起人失败");
                    }
                }

                if (FlowNodeCode == 0)
                {//推送消息到发起人

                    dsPerformPersonnel = new DataSet();
                    DataTable dt = new DataTable();
                    dt.Columns.Add("ActionType", typeof(int));
                    dt.Columns.Add("PersonnelSource", typeof(string));
                    dt.Columns.Add("PersonnelCode", typeof(string));
                    DataRow dr = dt.NewRow();
                    dr["ActionType"] = 0;//执行人
                    dr["PersonnelSource"] = "Originator";//人员来源（发起人）
                    dr["PersonnelCode"] = userCode;//人员代码
                    dt.Rows.Add(dr);
                    dsPerformPersonnel.Tables.Add(dt);
                }
                else
                {

                    dsPerformPersonnel = (DataSet)Collector.AshxSql.AutomaticCollection("GetNodePersonnel", cacheValues, UserInfo.CONNECTION_STRING, true);
                    cacheValues.Clear();
                    if (dsPerformPersonnel == null || dsPerformPersonnel.Tables[0].Rows.Count < 1)
                    {
                        throw new Exception("未配置节点执行人员");
                    }
                }

                #endregion

                //遍历节点执行人员
                foreach (DataRow item in dsPerformPersonnel.Tables[0].Rows)
                {
                    personnelSource = item["PersonnelSource"].ToString().Trim();
                    personnelCode = item["PersonnelCode"].ToString();
                    userType = int.Parse(item["ActionType"].ToString().Trim());
                    cacheValues.Clear();
                    switch (personnelSource)
                    {
                        case "Originator":// 发起人

                            WriteSinglePersonnel(messageID, userCode, userType);
                            if (isCreateOpinion) CreateSinglePerformOpinion(FlowPerformID, FlowNodeCode, userCode, userType, processTime, parentNodeCode, reviewNodeCode);//初始化审批意见信息

                            break;
                        case "DepartmentLeader":// 部门主管
                            cacheValues["Originator"] = userCode;
                            DataSet dsDeparLeader = (DataSet)Collector.AshxSql.AutomaticCollection("GetDepartmentLeader", cacheValues, UserInfo.CONNECTION_STRING, true);//获取部门主管
                            if (dsDeparLeader == null || dsDeparLeader.Tables[0].Rows.Count < 1) break;

                            WriteSinglePersonnel(messageID, dsDeparLeader.Tables[0].Rows[0]["DepartmentLeader"].ToString(), userType);

                            if (isCreateOpinion) CreateSinglePerformOpinion(FlowPerformID, FlowNodeCode, dsDeparLeader.Tables[0].Rows[0]["DepartmentLeader"].ToString(), userType, processTime, parentNodeCode, reviewNodeCode);//初始化审批意见信息

                            break;
                        case "DepartmentSecLeader":// 部门副主管

                            cacheValues["Originator"] = userCode;
                            DataSet dsDeparSecLeader = (DataSet)Collector.AshxSql.AutomaticCollection("GetDepartmentLeader", cacheValues, UserInfo.CONNECTION_STRING, true);//部门副主管
                            if (dsDeparSecLeader == null || dsDeparSecLeader.Tables[0].Rows.Count < 1) break;

                            WriteSinglePersonnel(messageID, dsDeparSecLeader.Tables[0].Rows[0]["DepartmentSecLeader"].ToString(), userType);

                            if (isCreateOpinion) CreateSinglePerformOpinion(FlowPerformID, FlowNodeCode, dsDeparSecLeader.Tables[0].Rows[0]["DepartmentSecLeader"].ToString(), userType, processTime, parentNodeCode, reviewNodeCode);//初始化审批意见信息

                            break;
                        case "Department":// 部门
                            cacheValues["depar_Code"] = personnelCode;
                            dsPersonnel = (DataSet)Collector.AshxSql.AutomaticCollection("GetDepartmentUsers", cacheValues, UserInfo.CONNECTION_STRING, true);//获取部门用户
                            if (dsPersonnel == null || dsPersonnel.Tables[0].Rows.Count < 1) break;

                            WritePersonnels(messageID, dsPersonnel.Tables[0], userType);//写入消息接收人员

                            if (isCreateOpinion) CreatePerformOpinions(FlowPerformID, FlowNodeCode, dsPersonnel.Tables[0], userType, processTime, parentNodeCode, reviewNodeCode);//初始化审批意见信息

                            break;
                        case "Role":// 角色
                            cacheValues["RoleCode"] = personnelCode;
                            dsPersonnel = (DataSet)Collector.AshxSql.AutomaticCollection("GetRoleUsers", cacheValues, UserInfo.CONNECTION_STRING, true);//获取角色用户
                            if (dsPersonnel == null || dsPersonnel.Tables[0].Rows.Count < 1) break;

                            WritePersonnels(messageID, dsPersonnel.Tables[0], userType);//写入消息接收人员

                            if (isCreateOpinion) CreatePerformOpinions(FlowPerformID, FlowNodeCode, dsPersonnel.Tables[0], userType, processTime, parentNodeCode, reviewNodeCode);//初始化审批意见信息

                            break;
                        case "PositionCode":// 职位
                            cacheValues["PositionCode"] = personnelCode;
                            dsPersonnel = (DataSet)Collector.AshxSql.AutomaticCollection("GetPositionUsers", cacheValues, UserInfo.CONNECTION_STRING, true);//获取职位用户
                            if (dsPersonnel == null || dsPersonnel.Tables[0].Rows.Count < 1) break;

                            WritePersonnels(messageID, dsPersonnel.Tables[0], userType);//写入消息接收人员

                            if (isCreateOpinion) CreatePerformOpinions(FlowPerformID, FlowNodeCode, dsPersonnel.Tables[0], userType, processTime, parentNodeCode, reviewNodeCode);//初始化审批意见信息

                            break;
                        case "Personnel"://  操作员

                            WriteSinglePersonnel(messageID, personnelCode, userType);

                            if (isCreateOpinion) CreateSinglePerformOpinion(FlowPerformID, FlowNodeCode, personnelCode, userType, processTime, parentNodeCode, reviewNodeCode);//初始化审批意见信息
                            break;
                        case "ProjectManager":// 项目经理
                            cacheValues["Originator"] = userCode;
                            cacheValues["FlowPerformID"] = FlowPerformID;
                            DataSet dsProjectManager = (DataSet)Collector.AshxSql.AutomaticCollection("GetProjectManager", cacheValues, UserInfo.CONNECTION_STRING, true);//获取部门主管
                            if (dsProjectManager == null || dsProjectManager.Tables[0].Rows.Count < 1) break;

                            WriteSinglePersonnel(messageID, dsProjectManager.Tables[0].Rows[0]["ProjectManager"].ToString(), userType);

                            if (isCreateOpinion) CreateSinglePerformOpinion(FlowPerformID, FlowNodeCode, dsProjectManager.Tables[0].Rows[0]["ProjectManager"].ToString(), userType, processTime, parentNodeCode, reviewNodeCode);//初始化审批意见信息

                            break;
                        case "PerformDep":// 按项目承建部门
                            cacheValues["Originator"] = userCode;
                            cacheValues["FlowPerformID"] = FlowPerformID;
                            cacheValues["personnelCode"] = personnelCode;
                            DataSet PerformDep = (DataSet)Collector.AshxSql.AutomaticCollection("GetDepLeaderForPerformDep", cacheValues, UserInfo.CONNECTION_STRING, true);//获取部门主管
                            if (PerformDep == null || PerformDep.Tables[0].Rows.Count < 1) break;
                            if (Int32.Parse(personnelCode) == 9460)
                            {
                                userType = (int)PerformDep.Tables[0].Rows[0]["type"];
                                WriteSinglePersonnel(messageID, PerformDep.Tables[0].Rows[0]["DepartmentLeader"].ToString(), userType);

                                if (isCreateOpinion) CreateSinglePerformOpinion(FlowPerformID, FlowNodeCode, PerformDep.Tables[0].Rows[0]["DepartmentLeader"].ToString(), userType, processTime, parentNodeCode, reviewNodeCode);//初始化审批意见信息
                            }
                            else
                            {
                                //if (PerformDep.Tables[0].Rows[0]["Leader"].ToString() == null || PerformDep.Tables[0].Rows[0]["Leader"].ToString().Equals("NULL"))
                                //{
                                //    break;
                                //}
                                WriteSinglePersonnel(messageID, PerformDep.Tables[0].Rows[0]["Leader"].ToString(), userType);

                                if (isCreateOpinion) CreateSinglePerformOpinion(FlowPerformID, FlowNodeCode, PerformDep.Tables[0].Rows[0]["Leader"].ToString(), userType, processTime, parentNodeCode, reviewNodeCode);//初始化审批意见信息
                            }


                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            #endregion
        }

        /// <summary>
        /// 执行流程节点事件
        /// </summary>
        private void ExecuteNodeEvent(int flowNodeCode, string flowPerformID, int StateCode, DateTime processTime)
        {
            string cmdStr = "Select cmdText From TbFlowPerformNodeEvent a left join TbSysCommand b on a.fmCode=b.fmCode and a.ActionCode=b.ActionCode " +
                " Where a.FlowPerformID=@FlowPerformID and a.FlowNodeCode=@FlowNodeCode and a.StateCode=@StateCode";

            Tuple<string, int, string> formInfo = GetPerformFormCode(flowPerformID);//获取FormCode,FormDataCode,FlowCode
            try
            {
                DataTable dt_Cmd = transHelper.ExcuteDataTable(CommandType.Text, cmdStr,
                            new SqlParameter[] { 
                                new SqlParameter("@FlowPerformID",flowPerformID),
                                new SqlParameter("@FlowNodeCode",flowNodeCode),
                                new SqlParameter("@StateCode",StateCode)
                                //new SqlParameter("@fmCode",formInfo.Item1)
                                });

                if (dt_Cmd != null && dt_Cmd.Rows.Count > 0)
                { //存在节点事件
                    for (int i = 0; i < dt_Cmd.Rows.Count; i++)
                    {
                        transHelper.ExecuteNonQuery(CommandType.Text, dt_Cmd.Rows[i]["cmdText"].ToString(),
                                                    new SqlParameter[] {
                                                    new SqlParameter("@fmCode",formInfo.Item1),
                                                    new SqlParameter("@id",formInfo.Item2),
                                                    new SqlParameter("@FlowCode",formInfo.Item3),                            
                                                    new SqlParameter("@FlowPerformID",flowPerformID),
                                                    new SqlParameter("@FlowNodeCode",flowNodeCode),
                                                    new SqlParameter("@ProcessTime",processTime),
                                                    new SqlParameter("@UserCode",userCodeLogin)
                                                    });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("流程节点事件执行失败:" + ex.Message);
            }
        }

        /// <summary>
        /// 执行流程事件
        /// </summary>
        private void ExecuteFlowEvent(string flowPerformID, int StateCode, DateTime processTime)
        {
            string cmdStr = "Select cmdText From TbFlowPerformEvent a left join TbSysCommand b on a.fmCode=b.fmCode and a.ActionCode=b.ActionCode " +
                " Where a.FlowPerformID=@FlowPerformID and  a.StateCode=@StateCode";

            Tuple<string, int, string> formInfo = GetPerformFormCode(flowPerformID);//获取FormCode,FormDataCode,FlowCode
            try
            {
                DataTable dt_Cmd = transHelper.ExcuteDataTable(CommandType.Text, cmdStr,
                            new SqlParameter[] { 
                                new SqlParameter("@FlowPerformID",flowPerformID),                                
                                new SqlParameter("@StateCode",StateCode)
                                //new SqlParameter("@fmCode",formInfo.Item1)
                                });

                if (dt_Cmd != null && dt_Cmd.Rows.Count > 0)
                { //存在流程事件
                    for (int i = 0; i < dt_Cmd.Rows.Count; i++)
                    {
                        transHelper.ExecuteNonQuery(CommandType.Text, dt_Cmd.Rows[i]["cmdText"].ToString(),
                            new SqlParameter[] {
                            new SqlParameter("@fmCode",formInfo.Item1),
                            new SqlParameter("@id",formInfo.Item2),
                            new SqlParameter("@FlowCode",formInfo.Item3),                            
                            new SqlParameter("@FlowPerformID",flowPerformID),                            
                            new SqlParameter("@ProcessTime",processTime),
                            new SqlParameter("@UserCode",userCodeLogin)
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("流程流程事件执行失败:" + ex.Message);
            }
        }

        /// <summary>
        /// 修改流程状态（或办文状态）
        /// </summary>
        /// <param name="FlowPerformID">流程执行流水号</param>
        /// <param name="FlowState">流程状态</param>
        private void ChangeFlowState(string FlowPerformID, int FlowState)
        {

            string sql = "";
            string sqlOffice = "";//更新办文状态sql
            sqlOffice = "update TbOfficeInfo set OfficeState=2 where id=(select FormDataCode from TbFlowPerform where FlowPerformID=@FlowPerformID and FormCode='formOfficeInfo')";
            if (FlowState == 3 || FlowState == 9 || FlowState == 1)
            {
                sql = "update TbFlowPerform set FlowState=@FlowState,EndTime=getdate() where FlowPerformID=@FlowPerformID";
            }
            else
            {
                sql = "update TbFlowPerform set FlowState=@FlowState where FlowPerformID=@FlowPerformID";
            }
            try
            {

                transHelper.ExecuteNonQuery(CommandType.Text, sql, new SqlParameter[] { new SqlParameter("@FlowPerformID", FlowPerformID), new SqlParameter("@FlowState", FlowState) });
                //更新办文的状态为已完成
                transHelper.ExecuteNonQuery(CommandType.Text, sqlOffice, new SqlParameter[] { new SqlParameter("@FlowPerformID", FlowPerformID) });

                ExecuteFlowEvent(FlowPerformID, FlowState, DateTime.Now);
            }
            catch (Exception ex)
            {
                throw new Exception("修改流程状态错误：" + ex.Message);
            }

        }

        /// <summary>
        /// 修改流程节点状态
        /// </summary>
        /// <param name="connStr">数据库连接字符串</param>
        /// <param name="performID">流程执行流水号</param>
        /// <param name="flowNodeCode">流程节点代码</param>
        private void ChangeFlowNodeState(string performID, int flowNodeCode, int nodeState, DateTime processTime)
        {

            string sql = "update TbFlowPerformNode set FlowNodeState=@FlowNodeState where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode";
            try
            {
                SqlParameter[] paras = new SqlParameter[] { 
                    new SqlParameter("@FlowNodeState",SqlDbType.Int),
                    new SqlParameter("@FlowPerformID",SqlDbType.VarChar),
                    new SqlParameter("@FlowNodeCode",SqlDbType.Int)
                };
                paras[0].Value = nodeState;
                paras[1].Value = performID;
                paras[2].Value = flowNodeCode;

                transHelper.ExecuteNonQuery(CommandType.Text, sql, paras);

                ExecuteNodeEvent(flowNodeCode, performID, nodeState, processTime);

            }
            catch (Exception ex)
            {
                throw new Exception("修改流程节点状态错误：" + ex.Message);
            }
        }

        /// <summary>
        /// 创建多个审批意见
        /// </summary>
        /// <param name="FlowPerformID">流程执行流水号</param>
        /// <param name="FlowNodeCode">流程节点代码</param>
        /// <param name="Users">操作人代码集合</param>
        /// <param name="UserType">人员类型（0执行人，1抄送人）</param>
        /// <param name="ProcessTime">上一步处理时候</param>
        /// <param name="PreNodeCode">上一步处理节点代码</param>
        /// <param name="reviewNodeCode">重审发起节点代码</param>
        private void CreatePerformOpinions(string FlowPerformID, int FlowNodeCode, DataTable Users, int UserType, DateTime ProcessTime, int PreNodeCode, int reviewNodeCode)
        {

            string sql = "";
            SqlParameter[] paras = null;


            try
            {
                if (reviewNodeCode > -1)//退回重审节点
                {
                    sql = "if not exists(select 1 from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode and UserCode=@UserCode) insert into TbFlowPerformOpinions(FlowPerformID,FlowNodeCode,PreNodeCompleteDate,UserCode,UserType,PerformState,ReviewNodeCode,ReviewWhy)"
              + " values(@FlowPerformID,@FlowNodeCode,@PreNodeCompleteDate,@UserCode,@UserType,-1,@ReviewNodeCode,@ReviewWhy)";

                    //获取上一步节点、当前节点名称

                    DataTable dtNodeInfo = transHelper.ExcuteDataTable(CommandType.Text, "select FlowNodeCode,FlowNodeName from TbFlowPerformNode where FlowPerformID=@FlowPerformID and (FlowNodeCode=@preNodeCode or FlowNodeCode=@FlowNodeCode)",
                        new SqlParameter[] { new SqlParameter("@PreNodeCode", PreNodeCode), new SqlParameter("@FlowNodeCode", FlowNodeCode), new SqlParameter("@FlowPerformID", FlowPerformID) });

                    if (dtNodeInfo == null || dtNodeInfo.Rows.Count <= 0) throw new Exception("获取上一步和当前处理节点名称失败");


                    paras = new SqlParameter[] {
                    new SqlParameter("@FlowPerformID",SqlDbType.VarChar),
                    new SqlParameter("@FlowNodeCode",SqlDbType.Int),
                    new SqlParameter("@PreNodeCompleteDate",SqlDbType.DateTime),
                    new SqlParameter("@UserCode",SqlDbType.VarChar),
                    new SqlParameter("@UserType",SqlDbType.Int),
                    new SqlParameter("@ReviewNodeCode",SqlDbType.Int),
                    new SqlParameter("@ReviewWhy",SqlDbType.VarChar)
                };
                    paras[0].Value = FlowPerformID;
                    paras[1].Value = FlowNodeCode;
                    paras[2].Value = ProcessTime;
                    paras[4].Value = UserType;
                    paras[5].Value = reviewNodeCode;

                    //上一步处理时间+上一步节点名称+“退回到”+退回到节点名称
                    paras[6].Value = string.Format("{0}-{1}-退回到-{2}", ProcessTime, dtNodeInfo.Select("FlowNodeCode=" + PreNodeCode)[0]["FlowNodeName"], dtNodeInfo.Select("FlowNodeCode=" + FlowNodeCode)[0]["FlowNodeName"]);

                    foreach (DataRow item in Users.Rows)
                    {
                        paras[3].Value = item["UserCode"];
                        transHelper.ExecuteNonQuery(CommandType.Text, sql, paras);
                    }
                }
                else
                {//正常流程审批节点
                    sql = "if not exists(select 1 from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode and UserCode=@UserCode) insert into TbFlowPerformOpinions(FlowPerformID,FlowNodeCode,PreNodeCompleteDate,UserCode,UserType,PerformState) values(@FlowPerformID,@FlowNodeCode,@PreNodeCompleteDate,@UserCode,@UserType,-1)";
                    paras = new SqlParameter[] {
                    new SqlParameter("@FlowPerformID",SqlDbType.VarChar),
                    new SqlParameter("@FlowNodeCode",SqlDbType.Int),
                    new SqlParameter("@PreNodeCompleteDate",SqlDbType.DateTime),
                    new SqlParameter("@UserCode",SqlDbType.VarChar),
                    new SqlParameter("@UserType",SqlDbType.Int)
                };
                    paras[0].Value = FlowPerformID;
                    paras[1].Value = FlowNodeCode;
                    paras[2].Value = ProcessTime;
                    paras[4].Value = UserType;

                    foreach (DataRow item in Users.Rows)
                    {
                        paras[3].Value = item["UserCode"];
                        transHelper.ExecuteNonQuery(CommandType.Text, sql, paras);

                    }
                }


            }
            catch (Exception ex)
            {

                throw new Exception("创建审批意见信息错误：" + ex.Message);
            }
        }

        /// <summary>
        /// 创建审批意见
        /// </summary>
        /// <param name="FlowPerformID">流程执行流水号</param>
        /// <param name="FlowNodeCode">流程节点代码</param>
        /// <param name="userCode">接收人代码</param>
        /// <param name="UserType">人员类型（0执行人，1抄送人）</param> 
        /// <param name="ProcessTime">上一步处理时间</param>
        /// <param name="PreNodeCode">上一步处理节点代码</param>
        /// <param name="reviewNodeCode">重审发起节点代码</param>
        private void CreateSinglePerformOpinion(string FlowPerformID, int FlowNodeCode, string userCode, int UserType, DateTime ProcessTime, int PreNodeCode, int reviewNodeCode)
        {

            string sql = "";
            SqlParameter[] paras = null;
            try
            {
                if (reviewNodeCode >= 0)//退回重审节点
                {
                    sql = "if not exists(select 1 from TbFlowPerformOpinions where UserCode=@UserCode and FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode) insert into TbFlowPerformOpinions(FlowPerformID,FlowNodeCode,PreNodeCompleteDate,UserCode,UserType,PerformState,ReviewNodeCode,ReviewWhy)"
                    + " values(@FlowPerformID,@FlowNodeCode,@PreNodeCompleteDate,@UserCode,@UserType,-1,@ReviewNodeCode,@ReviewWhy)";

                    //获取上一步节点、当前节点名称

                    DataTable dtNodeInfo = transHelper.ExcuteDataTable(CommandType.Text, "select FlowNodeCode,FlowNodeName from TbFlowPerformNode where FlowPerformID=@FlowPerformID and (FlowNodeCode=@preNodeCode or FlowNodeCode=@FlowNodeCode)",
                        new SqlParameter[] { new SqlParameter("@PreNodeCode", PreNodeCode), new SqlParameter("@FlowNodeCode", FlowNodeCode), new SqlParameter("@FlowPerformID", FlowPerformID) });

                    if (dtNodeInfo == null || dtNodeInfo.Rows.Count <= 0) throw new Exception("获取上一步和当前处理节点名称失败");


                    paras = new SqlParameter[] {
                        new SqlParameter("@FlowPerformID",SqlDbType.VarChar),
                        new SqlParameter("@FlowNodeCode",SqlDbType.Int),
                        new SqlParameter("@PreNodeCompleteDate",SqlDbType.DateTime),
                        new SqlParameter("@UserCode",SqlDbType.VarChar),
                        new SqlParameter("@UserType",SqlDbType.Int),
                        new SqlParameter("@ReviewNodeCode",SqlDbType.Int),
                        new SqlParameter("@ReviewWhy",SqlDbType.VarChar)
                    };
                    paras[0].Value = FlowPerformID;
                    paras[1].Value = FlowNodeCode;
                    paras[2].Value = ProcessTime;
                    paras[3].Value = userCode;
                    paras[4].Value = UserType;
                    paras[5].Value = reviewNodeCode;

                    //上一步处理时间+上一步节点名称+“退回到”+退回到节点名称
                    var lastFlowNodeName = dtNodeInfo.Select("FlowNodeCode='" + PreNodeCode + "'")[0]["FlowNodeName"];
                    var returnFlowNodeName = dtNodeInfo.Select("FlowNodeCode='" + FlowNodeCode + "'")[0]["FlowNodeName"];
                    paras[6].Value = string.Format("{0}-{1}-退回到-{2}", ProcessTime, Convert.ToString(lastFlowNodeName), Convert.ToString(returnFlowNodeName));

                }
                //else if (reviewNodeCode == 0)//退回到发起人
                //{


                //}
                else
                {//正常流程审批节点
                    sql = "if not exists(select 1 from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode and PreNodeCompleteDate=@PreNodeCompleteDate and UserCode=@UserCode) insert into TbFlowPerformOpinions(FlowPerformID,FlowNodeCode,PreNodeCompleteDate,UserCode,UserType,PerformState) values(@FlowPerformID,@FlowNodeCode,@PreNodeCompleteDate,@UserCode,@UserType,-1)";
                    paras = new SqlParameter[] {
                    new SqlParameter("@FlowPerformID",SqlDbType.VarChar),
                    new SqlParameter("@FlowNodeCode",SqlDbType.Int),
                    new SqlParameter("@PreNodeCompleteDate",SqlDbType.DateTime),
                    new SqlParameter("@UserCode",SqlDbType.VarChar),
                    new SqlParameter("@UserType",SqlDbType.Int)
                };
                    paras[0].Value = FlowPerformID;
                    paras[1].Value = FlowNodeCode;
                    paras[2].Value = ProcessTime;
                    paras[3].Value = userCode;
                    paras[4].Value = UserType;
                }

                if (sql != "")
                {

                    transHelper.ExecuteNonQuery(CommandType.Text, sql, paras);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("创建审批意见信息错误：" + ex.Message);
            }
        }


        /// <summary>
        /// 写入消息接收人为（部门、职位、角色）多个
        /// </summary>
        public void WritePersonnels(string messageID, DataTable dt, int userType)
        {
            string sql = "insert into TbFlowPerformMsgReceive(messageID,UserCode,UserType,messageState,ActionType) values(@messageID,@UserCode,@UserType,0,-1)";
            SqlParameter[] paras = new SqlParameter[] {
                    new SqlParameter("@messageID",SqlDbType.VarChar),
                    new SqlParameter("@UserCode",SqlDbType.VarChar),
                    new SqlParameter("@UserType",SqlDbType.Int)

                };
            paras[0].Value = messageID;
            paras[2].Value = userType;

            try
            {

                foreach (DataRow item in dt.Rows)
                {
                    paras[1].Value = item["UserCode"];

                    transHelper.ExecuteNonQuery(CommandType.Text, sql, paras);//写入消息接收人员
                }
            }
            catch (Exception ex)
            {
                throw new Exception("写消息接收人员错误：" + ex.Message);
            }
        }

        /// <summary>
        /// 写入消息接收人为部门主管、部门副主管、发起人、操作员
        /// </summary>
        public void WriteSinglePersonnel(string messageID, string userCode, int userType)
        {
            string sql = "insert into TbFlowPerformMsgReceive(messageID,UserCode,UserType,messageState,ActionType) values(@messageID,@UserCode,@UserType,0,-1)";
            SqlParameter[] paras = new SqlParameter[] {
                    new SqlParameter("@messageID",SqlDbType.VarChar),
                    new SqlParameter("@UserCode",SqlDbType.VarChar),
                    new SqlParameter("@UserType",SqlDbType.Int)

                };
            paras[0].Value = messageID;
            paras[1].Value = userCode;
            paras[2].Value = userType;
            try
            {
                transHelper.ExecuteNonQuery(CommandType.Text, sql, paras);//写入消息接收人员
            }
            catch (Exception ex)
            {
                throw new Exception("写消息接收人员错误：" + ex.Message);
            }

        }
        /// <summary>
        /// 获取消息ID
        /// </summary>
        /// <param name="messageID">流程执行消息ID</param>
        /// <returns></returns>
        public string GetMessageID()
        {
            string messageID = null;

            try
            {
                DataSet dsMessageID = (DataSet)AshxSql.AutomaticCollection("GetFlowMessageID", null, UserInfo.CONNECTION_STRING, true);//返回流程执行消息ID
                if (dsMessageID == null || dsMessageID.Tables[0].Rows.Count < 1) { return JsonString.ErrorInfo("获取消息ID失败"); }
                messageID = dsMessageID.Tables[0].Rows[0]["FlowMessageID"].ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("获取消息ID错误：" + ex.Message);
            }
            return messageID;
        }

        /// <summary>
        /// 获取上一步处理时间，审批意见ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="performID"></param>
        /// <param name="flowNodeCode"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public string GetPreDate(string performID, int flowNodeCode, string connStr)
        {
            #region 获取上一步处理时间，审批意见ID

            string strGetOpinion = "select convert(varchar(19),b.a,20) as PreNodeCompleteDate from (" +
            " select max(PreNodeCompleteDate) as a from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode and UserCode=@UserCode)b";

            DataTable tbOpinion = transHelper.ExcuteDataTable(CommandType.Text, strGetOpinion, new SqlParameter[] {
                    new SqlParameter("@UserCode", userCodeLogin),
                    new SqlParameter("@FlowPerformID",performID),
                    new SqlParameter("@FlowNodeCode",flowNodeCode)
                });

            if (tbOpinion == null || tbOpinion.Rows.Count == 0) throw new Exception("获取审批意见上一步处理时间错误");

            string preNodeCompleteDate = tbOpinion.Rows[0]["PreNodeCompleteDate"].ToString().Trim();

            return preNodeCompleteDate;

            #endregion
        }


        /// <summary>
        /// 处理审批消息
        /// </summary>
        /// <param name="performState">审批状态</param>
        /// <param name="performOpinions">审批意见</param>
        /// <param name="performID">流程执行流水号</param>
        /// <param name="flowNodeCode">流程节点代码</param>
        public void DisposeApprovalOpinion(int performState, string performOpinions, string performID, int flowNodeCode, string FreeNodeUser = "")
        {
            //string connStr = HttpContext.Current.Session[UserInfo.CONNECTION_STRING].ToString();//数据库连接字符串
            //string userCode = HttpContext.Current.Session[UserInfo.USER_CODE].ToString().Trim();//用户代码
            string userCode = userCodeLogin;//用户代码
            bool falg = false;
            try
            {
                string preNodeCompleteDate = GetPreDate(performID, flowNodeCode, connStr);

                bool mark = IsApproval(performID, preNodeCompleteDate, flowNodeCode);
                if (mark)
                {
                    throw new Exception("意见已经审批过，不能重复审批");
                }

                #region 提交审批意见
                DateTime processTime = DateTime.Now;//处理当前审批时间
                string strUpdateOpinion = "update TbFlowPerformOpinions set PerformState=@PerformState,PerformDate=@PerformDate,PerformOpinions=@PerformOpinions where " +
               " FlowPerformID=@FlowPerformID and convert(varchar(19),PreNodeCompleteDate,20)=@PreNodeCompleteDate and FlowNodeCode=@FlowNodeCode and UserCode=@UserCode";
                SqlParameter[] opinionParas = new SqlParameter[] { 
                    new SqlParameter("@UserCode",SqlDbType.VarChar),
                    new SqlParameter("@PerformOpinions",SqlDbType.Text),
                    new SqlParameter("@PerformState",SqlDbType.Int),
                    new SqlParameter("@FlowPerformID",SqlDbType.VarChar),
                    new SqlParameter("@PreNodeCompleteDate",SqlDbType.VarChar),
                    new SqlParameter("@FlowNodeCode",SqlDbType.Int),
                    new SqlParameter("@PerformDate",SqlDbType.DateTime)
                };

                opinionParas[0].Value = userCode;//用户代码
                opinionParas[1].Value = performOpinions;
                opinionParas[2].Value = performState;
                opinionParas[3].Value = performID;
                opinionParas[4].Value = preNodeCompleteDate;
                opinionParas[5].Value = flowNodeCode;
                opinionParas[6].Value = processTime;

                transHelper.ExecuteNonQuery(CommandType.Text, strUpdateOpinion, opinionParas);

                #endregion

                //返回节点汇签标识
                DataTable dt = transHelper.ExcuteDataTable(CommandType.Text, "select AllApproval from TbFlowPerformNode where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode", new SqlParameter[] { new SqlParameter("FlowNodeCode", flowNodeCode), new SqlParameter("@FlowPerformID", performID) });

                if (dt == null || dt.Rows.Count <= 0) { throw new Exception("获取节点汇签标识失败"); }

                #region 提交审批意见，推送流程

                if (dt.Rows[0]["AllApproval"].ToString().Trim() == "0")
                {
                    //0非汇签   convert(varchar(19),PreNodeCompleteDate,20)
                    string strSql = "update TbFlowPerformOpinions set PerformState=9 where FlowPerformID=@FlowPerformID and convert(varchar(19),PreNodeCompleteDate,20)=@PreNodeCompleteDate and FlowNodeCode=@FlowNodeCode and UserCode!=@UserCode and UserType=0";
                    SqlParameter[] paras = new SqlParameter[]{ 
                        new SqlParameter("@UserCode",SqlDbType.VarChar),
                        new SqlParameter("@FlowPerformID",SqlDbType.VarChar),
                        new SqlParameter("@FlowNodeCode",SqlDbType.Int),
                        new SqlParameter("@PreNodeCompleteDate",SqlDbType.VarChar)
                    };
                    paras[0].Value = userCode;
                    paras[1].Value = performID;
                    paras[2].Value = flowNodeCode;
                    paras[3].Value = preNodeCompleteDate;

                    transHelper.ExecuteNonQuery(CommandType.Text, strSql, paras);

                    ChangeFlowNodeState(performID, flowNodeCode, 3, DateTime.Now);//审批意见处理后将该流程节点状态改为 已完成（3）
                    ExecuteFlowEvent(performID, performState, DateTime.Now);
                    falg = true;
                }
                else
                {
                    //1 汇签
                    falg = IsCompleteApproval(connStr, performID, preNodeCompleteDate, flowNodeCode);

                    if (!falg)
                    {
                        transHelper.Commit();
                        return;//未经过所有人审批同意，不推送流程
                    }

                    ChangeFlowNodeState(performID, flowNodeCode, 3, DateTime.Now);//通过所有人审批后将该流程节点状态改为 已完成（3）
                    ExecuteFlowEvent(performID, performState, DateTime.Now);
                }
                //判断当前(并列)及之前的所有流程是否审批同意,未同意不推送流程
                string sqlstr = "select id from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID and PerformState!=1 and UserType != 1";
                SqlParameter[] para = new SqlParameter[]{
                    new SqlParameter("@FlowPerformID",SqlDbType.VarChar),
               };
                para[0].Value = performID;
                DataTable dtData = transHelper.ExcuteDataTable(CommandType.Text, sqlstr, para);
                if (dtData != null && dtData.Rows.Count > 0)
                {
                    falg = false;//审批不同意
                }

                //审批同意、推送流程
                if (performState == 1 && falg)
                {

                    //获取流程执行表单数据、需推送消息的流程节点
                    string sql = "select FlowCode,FormCode,FormDataCode from TbFlowPerform where FlowPerformID=@FlowPerformID;";

                    DataTable FlowPerform = transHelper.ExcuteDataTable(CommandType.Text, sql, new SqlParameter[] { new SqlParameter("@FlowPerformID", performID) });

                    if (FlowPerform != null && FlowPerform.Rows.Count > 0)
                    {
                        #region 从当前处理意见节点获取重审发出节点代码，正常流程值为 -1

                        string strReviewNodeCode = "select ReviewNodeCode from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID  "
                            + "and convert(varchar(19),PreNodeCompleteDate,20)=@PreNodeCompleteDate and FlowNodeCode=@FlowNodeCode and UserCode=@UserCode";

                        DataTable dtReviewNodeCode = transHelper.ExcuteDataTable(CommandType.Text, strReviewNodeCode,
                            new SqlParameter[] {
                                new SqlParameter("@FlowPerformID",performID),
                                new SqlParameter("@PreNodeCompleteDate",preNodeCompleteDate),
                                new SqlParameter("@FlowNodeCode",flowNodeCode),
                                new SqlParameter("@UserCode",userCode)
                            });

                        if (dtReviewNodeCode == null || dtReviewNodeCode.Rows.Count <= 0) throw new Exception("获取发出节点代码失败");
                        //wanglong
                        #endregion

                        LaunchFlow(performID, flowNodeCode, processTime, int.Parse(dtReviewNodeCode.Rows[0]["ReviewNodeCode"].ToString().Trim()), FreeNodeUser);
                    }
                    else
                    {
                        throw new Exception("获取流程执行主表信息失败");
                    }
                }
                else//此处应对不同意的情况进行处理（暂未处理）
                {
                    transHelper.Commit();
                }
                #endregion

            }
            catch (Exception ex)
            {
                transHelper.Rollback();
                throw new Exception("处理审批消息错误：" + ex.Message);
            }

        }
        /// <summary>
        /// 返回指定执行流水号对应的表单，和表单数据号,流程代码
        /// </summary>
        /// <param name="performID">流程执行流水号</param>
        /// <returns>formCode,formDataCode</returns>
        private Tuple<string, int, string> GetPerformFormCode(string performID)
        {

            //获取指定执行流水号对应的表单，和表单数据号
            string sql = "select FlowCode,FormCode,FormDataCode from TbFlowPerform where FlowPerformID=@FlowPerformID";
            try
            {

                DataTable FlowPerform = transHelper.ExcuteDataTable(CommandType.Text, sql, new SqlParameter[] { new SqlParameter("@FlowPerformID", performID) });

                if (FlowPerform != null && FlowPerform.Rows.Count > 0)
                {
                    return new Tuple<string, int, string>(FlowPerform.Rows[0]["FormCode"].ToString(), int.Parse(FlowPerform.Rows[0]["FormDataCode"].ToString()), FlowPerform.Rows[0]["FlowCode"].ToString());
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("获取执行表单单据信息失败:" + ex.Message);
            }
        }

        /// <summary>
        /// 退回流程
        /// </summary>
        /// <param name="oldNodeCode">当前处理流程节点</param>
        /// <param name="newNodeCode">退回后流程节点代码</param>
        /// <param name="performID">流程执行流水号</param>
        /// <param name="performOpinions">退回原因</param>
        public void FlowRollback(int oldNodeCode, int newNodeCode, string performID, string performOpinions)
        {
            try
            {
                //string connStr = HttpContext.Current.Session[UserInfo.CONNECTION_STRING].ToString();//数据库连接字符串
                string userCode = userCodeLogin;//用户代码
                string preNodeCompleteDate = GetPreDate(performID, oldNodeCode, connStr);
                DateTime processTime = DateTime.Now;//当前意见处理时间，下一步发起时间
                bool falg = IsApproval(performID, preNodeCompleteDate, oldNodeCode);
                if (falg)
                {
                    throw new Exception("意见已经审批过，不能重复审批");
                }
                #region 提交退回意见

                string strUpdateOpinion = "update TbFlowPerformOpinions set PerformState=3,PerformDate=@PerformDate,PerformOpinions=@PerformOpinions where " +
                    " FlowPerformID=@FlowPerformID and convert(varchar(19),PreNodeCompleteDate,20)=@PreNodeCompleteDate and FlowNodeCode=@FlowNodeCode and UserCode=@UserCode";//退回
                SqlParameter[] opinionParas = new SqlParameter[] { 
                    new SqlParameter("@UserCode",SqlDbType.VarChar),
                    new SqlParameter("@PerformOpinions",SqlDbType.Text),
                    new SqlParameter("@FlowPerformID",SqlDbType.VarChar),
                    new SqlParameter("@PreNodeCompleteDate",SqlDbType.VarChar),
                    new SqlParameter("@FlowNodeCode",SqlDbType.Int),
                    new SqlParameter("@PerformDate",SqlDbType.DateTime)
                };
                opinionParas[0].Value = userCode;
                opinionParas[1].Value = performOpinions;
                opinionParas[2].Value = performID;
                opinionParas[3].Value = preNodeCompleteDate;
                opinionParas[4].Value = oldNodeCode;
                opinionParas[5].Value = processTime;


                transHelper.ExecuteNonQuery(CommandType.Text, strUpdateOpinion, opinionParas);

                #endregion

                #region 更新当前节点其它未处理意见审批状态（9 其他用户已经处理 ）

                string strSql = "update TbFlowPerformOpinions set PerformState=9 where convert(varchar(19),PreNodeCompleteDate,20)=@PreNodeCompleteDate and FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode and UserCode!=@UserCode and UserType=0";
                SqlParameter[] paras = new SqlParameter[]{ 
                        new SqlParameter("@UserCode",SqlDbType.VarChar),
                        new SqlParameter("@FlowPerformID",SqlDbType.VarChar),
                        new SqlParameter("@FlowNodeCode",SqlDbType.Int),
                        new SqlParameter("@PreNodeCompleteDate",SqlDbType.VarChar)
                    };
                paras[0].Value = userCode;
                paras[1].Value = performID;
                paras[2].Value = oldNodeCode;
                paras[3].Value = preNodeCompleteDate;

                transHelper.ExecuteNonQuery(CommandType.Text, strSql, paras);

                #endregion

                #region 向退回节点写推送消息，和审批意见

                CreatePerformMessage(GetMessageTitle(performID), newNodeCode, performID, 2, oldNodeCode, processTime, newNodeCode);//消息类型2（退回）

                ChangeFlowState(performID, 2);//流程状态退回
                ChangeFlowNodeState(performID, oldNodeCode, 2, processTime);//当前流程节点状态退回
                if (newNodeCode == 0)//退回到发起人
                {
                    //LaunchEndFlow(performID, oldNodeCode, processTime, -1, "");
                }
                else
                {
                    ChangeFlowNodeState(performID, newNodeCode, 1, processTime);//退回后流程节点状态（执行中）
                }


                transHelper.Commit();

                #endregion
            }
            catch (Exception ex)
            {
                transHelper.Rollback();
                throw new Exception("退回流程错误：" + ex.Message);
            }
            finally
            {
                transHelper.Close();
            }

        }

        /// <summary>
        /// 撤消流程
        /// </summary>
        /// <param name="performID"></param>
        public void RevokeFlow(string performID)
        {
            try
            {
                //string connStr = HttpContext.Current.Session[UserInfo.CONNECTION_STRING].ToString();//数据库连接字符串
                //string userCode = HttpContext.Current.Session[UserInfo.USER_CODE].ToString().Trim();//用户代码               

                #region 更新流程状态为已撤消
                //将所有节点设置为已终止
                string strUpdatePerformNode = "UPDATE TbFlowPerformNode SET FlowNodeState=4 WHERE FlowPerformID=@flowPerformID";
                SqlParameter[] PerformNodeParas = new SqlParameter[] {                     
                    new SqlParameter("@FlowPerformID",SqlDbType.VarChar)
                };
                PerformNodeParas[0].Value = performID;
                transHelper.ExecuteNonQuery(CommandType.Text, strUpdatePerformNode, PerformNodeParas);

                //将流程状态改为已撤消
                ChangeFlowState(performID, 1);

                #endregion

                transHelper.Commit();
            }
            catch (Exception ex)
            {
                transHelper.Rollback();
                throw new Exception("撤消流程错误：" + ex.Message);
            }
            finally
            {
                transHelper.Close();
            }
        }

        /// <summary>
        /// 判断意见是否已审批
        /// </summary>
        /// <returns>已审批返回true,未审批返回false</returns>
        public bool IsApproval(string performId, string preDate, int flowNodeCode)
        {
            string sql = "select id from TbFlowPerformOpinions where PerformState in(-1,0) and FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode" +
            " and convert(varchar(19),PreNodeCompleteDate,20)=@PreNodeCompleteDate and UserCode=@UserCode";

            SqlParameter[] pars = new SqlParameter[] {
                new SqlParameter("@FlowPerformID",performId),
                new SqlParameter("@FlowNodeCode",flowNodeCode),
                new SqlParameter("@PreNodeCompleteDate",preDate),
                new SqlParameter("@UserCode",userCodeLogin)
            };

            DataTable dt = transHelper.ExcuteDataTable(CommandType.Text, sql, pars);

            if (dt != null && dt.Rows.Count > 0) return false;//未审批
            else return true;//已审批
        }

        /// <summary>
        /// 获取消息标题
        /// </summary>
        /// <param name="performID">流程执行流水号</param>
        /// <returns>消息标题</returns>
        public string GetMessageTitle(string performID)
        {
            //string formOrFlowName = "select a.FlowCode,b.FlowName,c.FormName from TbFlowPerform a left join TbFlowDefine b on a.FlowCode=b.FlowCode left join TbFormDefine c on c.FormCode=a.FormCode where a.FlowPerformID=@FlowPerformID";
            //DataTable dtName = SqlHelper.ExecuteDataTable(connStr, CommandType.Text, formOrFlowName, new SqlParameter[] { new SqlParameter("@FlowPerformID", performID) });
            //string msgTitle = dtName.Rows[0]["FlowName"].ToString() + "-" + dtName.Rows[0]["FormName"].ToString() + "-" + HttpContext.Current.Session[UserInfo.USER_NAME].ToString();
            //return msgTitle;
            string strFlowTitle = "select FlowTitle from TbFlowPerform where FlowPerformID=@FlowPerformID";


            DataTable FlowTitle = transHelper.ExcuteDataTable(CommandType.Text, strFlowTitle, new SqlParameter[] { new SqlParameter("@FlowPerformID", performID) });

            return FlowTitle.Rows[0]["FlowTitle"].ToString();

        }


        /// <summary>
        /// 判断汇签时，流程节点审批是否经过所有人同意
        /// </summary>
        /// <param name="connStr"></param>
        /// <param name="performID">流程执行流水号</param>
        /// <param name="preNodeCompleteDate">上一步处理时间</param>
        /// <param name="flowNodeCode">当前流程节点代码</param>
        /// <returns>bool</returns>
        public bool IsCompleteApproval(string connStr, string performID, string preNodeCompleteDate, int flowNodeCode)
        {
            bool falg = false;
            try
            {
                string sql = "select id from TbFlowPerformOpinions where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode and convert(varchar(19),PreNodeCompleteDate,20)=@PreNodeCompleteDate and PerformState!=1 and UserType != 1";
                SqlParameter[] paras = new SqlParameter[]{
                    new SqlParameter("@FlowPerformID",SqlDbType.VarChar),
                    new SqlParameter("@FlowNodeCode",SqlDbType.Int),
                    new SqlParameter("@PreNodeCompleteDate",SqlDbType.VarChar)
               };
                paras[0].Value = performID;
                paras[1].Value = flowNodeCode;
                paras[2].Value = preNodeCompleteDate;

                DataTable dt = transHelper.ExcuteDataTable(CommandType.Text, sql, paras);

                if (dt != null && dt.Rows.Count == 0)
                {
                    falg = true;//通过审批
                }

            }
            catch (Exception ex)
            {
                throw new Exception("检查节点是否通过所有人审批错误：" + ex.Message);
            }

            return falg;

        }

        /// <summary>
        /// 获取下级没有设置执行人的自由选择人节点
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowPerformID"></param>
        /// <param name="CurrNodeCode"></param>        
        /// <returns></returns>
        public DataTable GetChildNoUserFreeNode(string FlowCode, string FlowPerformID, int CurrNodeCode)
        {
            try
            {
                if (CurrNodeCode == 9999)//当前节点是结束节点
                {
                    return null;
                }
                string strSQL = string.Empty;
                if (CurrNodeCode == 0) //是发起节点，则当前流程还未发出
                {
                    //获取下级节点中的没有设置执行人的自由选人节点 
                    strSQL = "Select '' as PersonnelCode,'' as PersonnelName,a.FlowCode,a.FlowNodeCode,a.FlowNodeName From (Select * From TbFlowNode Where FlowCode='" + FlowCode + "' And FreeCandidates=1 And FlowNodeCode In (" +
                       "Select ChildNodeCode From TbFlowNodeRelation Where FlowCode='" + FlowCode + "' And ChildNodeCode<>9999 And ParentNodeCode=0) " +
                       ") as a Left Join (Select * From TbFlowNodePersonnel Where FlowCode='" + FlowCode + "' And ActionType=0) as b " +
                       " On a.FlowCode=b.FlowCode And a.FlowNodeCode=b.FlowNodeCode Where b.id is  null ";
                }
                else //是流程执行过程中
                {
                    //获取下级节点中的没有设置执行人的自由选人节点
                    strSQL = "Select '' as PersonnelCode,'' as PersonnelName,a.FlowCode,a.FlowNodeCode,a.FlowNodeName From (Select * From TbFlowPerformNode Where FlowPerformID='" + FlowPerformID + "' And FreeCandidates=1 And FlowNodeCode In (" +
                            "Select ChildNodeCode From TbFlowPerformNodeRelation Where FlowPerformID='" + FlowPerformID + "' And ChildNodeCode<>9999 And ParentNodeCode=" + CurrNodeCode.ToString() + ")" +
                            ") as a Left Join (Select * From TbFlowPerformNodePersonnel Where FlowPerformID='" + FlowPerformID + "' And ActionType=0) as b" +
                            " On a.FlowPerformID=b.FlowPerformID And a.FlowNodeCode=b.FlowNodeCode Where b.id is  null";
                }
                DataTable tbChildNode = transHelper.ExcuteDataTable(CommandType.Text, strSQL, null);//获取下级自由选人节点
                if (tbChildNode == null || tbChildNode.Rows.Count <= 0)//没有下级自由选人节点
                {
                    return null;
                }
                if (CurrNodeCode != 0)
                {
                    //判断下级自由选人节点的所有父节点是否审批完成
                    DataTable dtFreeNodes = tbChildNode.Clone();
                    string parentNodeStr = "select a.ParentNodeCode from TbFlowPerformNodeRelation a left join TbFlowPerformNode b on a.FlowPerformID=b.FlowPerformID and b.FlowNodeCode=a.ParentNodeCode " +
                                                        "where a.FlowPerformID=@FlowPerformID and a.ChildNodeCode=@ChildNodeCode and a.ParentNodeCode!=@ParentNodeCode and b.FlowNodeState not in (3,7,8)";
                    SqlParameter[] parentParams = new SqlParameter[] { 
                    new SqlParameter("@FlowPerformID", SqlDbType.VarChar),
                    new SqlParameter("@ChildNodeCode", SqlDbType.Int),
                    new SqlParameter("@ParentNodeCode", SqlDbType.Int) };
                    parentParams[0].Value = FlowPerformID;
                    parentParams[2].Value = CurrNodeCode;//发起节点
                    DataTable dtParentNode = null;//未完成审批的父节点
                    foreach (DataRow drChildNode in tbChildNode.Rows)
                    {
                        parentParams[1].Value = drChildNode["FlowNodeCode"];
                        dtParentNode = transHelper.ExcuteDataTable(CommandType.Text, parentNodeStr, parentParams);
                        if (dtParentNode != null && dtParentNode.Rows.Count == 0)//所有上级节点都已审批完成
                        {
                            dtFreeNodes.ImportRow(drChildNode);

                        }
                    }
                    return dtFreeNodes;
                }
                else
                {
                    return tbChildNode;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 获取未设置执行人的下级节点
        /// </summary>
        /// <param name="FlowPerformID">流程执行流水号</param>
        /// <param name="ParentNodeCode">发起（或当前）流程节点</param>       
        /// <param name="NoUserNode">未设置执行人的下级节点</param>
        /// <returns>false:没有未设置执行人的下级节点；true:有未设置执行人的下级节点</returns>
        public bool GetNoUserChileNode(string FlowPerformID, int ParentNodeCode, ref List<int> NoUserNode)
        {
            try
            {
                if (ParentNodeCode == 9999)//当前节点是结束节点
                {
                    return false;
                }
                string strChildNode = "";//流程将要推送消息节点
                SqlParameter[] childNodeParas = null;
                strChildNode = "select ChildNodeCode from TbFlowPerformNodeRelation where FlowPerformID=@FlowPerformID and ParentNodeCode=@ParentNodeCode ";//获取指定流程节点的子节点
                childNodeParas = new SqlParameter[] {
                            new SqlParameter("@FlowPerformID",FlowPerformID),
                            new SqlParameter("@ParentNodeCode",ParentNodeCode)
                        };
                //获取下一步审批节点
                DataTable tbChildNode = transHelper.ExcuteDataTable(CommandType.Text, strChildNode, childNodeParas);//获取下级节点

                List<int> flowNodeCode = new List<int>();//存放不满足判定条件的节点代码
                List<int> matchNodeCode = new List<int>();//存放满足判定条件的节点代码
                List<int> withoutCriteria = new List<int>();//存放无判定条件的节点代码

                if (tbChildNode == null || tbChildNode.Rows.Count <= 0) return false;//无子节点

                DataTable dtCriteria;//判定条件
                int childNodeCode = 0;
                Tuple<string, int, string> formInfo = GetPerformFormCode(FlowPerformID);
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@FlowPerformID", FlowPerformID),
                    new SqlParameter("@FormCode", formInfo.Item1),
                    new SqlParameter("@id",formInfo.Item2)
                };
                string strSelect = GetStrSql(sqlParams);//不包含判定条件的查询字符串
                if (strSelect == "") return false;

                foreach (DataRow drChildNode in tbChildNode.Rows)
                {
                    childNodeCode = int.Parse(drChildNode[0].ToString());

                    if (childNodeCode == 9999) //下级节点是结束节点
                    {
                        return false;
                    };
                    //获取判断条件Sql
                    string judgeCriteria = "select * from TbFlowPerformNodeJudgeCriteria where FlowPerformID=@FlowPerformID and FlowNodeCode=@FlowNodeCode";
                    dtCriteria = transHelper.ExcuteDataTable(CommandType.Text, judgeCriteria, new SqlParameter[] {
                         new SqlParameter("@FlowPerformID",FlowPerformID),
                         new SqlParameter("@FlowNodeCode",childNodeCode)});//返回指定节点的判定条件

                    if (dtCriteria == null || dtCriteria.Rows.Count <= 0)
                    {
                        //无判定条件
                        if (!withoutCriteria.Contains(childNodeCode)) withoutCriteria.Add(childNodeCode);
                    }
                    else
                    {
                        bool matchCriteria = IsMatchCriteria(dtCriteria, strSelect, sqlParams);//判断是否满足判定条件
                        if (!matchCriteria)
                        {
                            if (!flowNodeCode.Contains(childNodeCode))
                            {
                                flowNodeCode.Add(childNodeCode);//添加不满足判定条件的节点代码
                            }
                        }
                        else
                        {
                            if (!matchNodeCode.Contains(childNodeCode)) matchNodeCode.Add(childNodeCode);//添加满足判定条件的节点代码
                        }
                    }
                }

                #region  判断子节点的所有父节点是否审批完成，完成子节点可用；反之

                string parentNodeStr = "select a.ParentNodeCode from TbFlowPerformNodeRelation a left join TbFlowPerformNode b on a.FlowPerformID=b.FlowPerformID and b.FlowNodeCode=a.ParentNodeCode " +
                                    "where a.FlowPerformID=@FlowPerformID and a.ChildNodeCode=@ChildNodeCode and a.ParentNodeCode!=@ParentNodeCode and b.FlowNodeState not in (3,7,8)";
                List<int> disableNode = new List<int>();//需禁用的节点
                List<int> enableNode = new List<int>();//可以节点
                SqlParameter[] parentParams = new SqlParameter[] { 
                    new SqlParameter("@FlowPerformID", SqlDbType.VarChar),
                    new SqlParameter("@ChildNodeCode", SqlDbType.Int),
                    new SqlParameter("@ParentNodeCode", SqlDbType.Int) };
                parentParams[0].Value = FlowPerformID;
                parentParams[2].Value = ParentNodeCode;//发起节点
                DataTable dtParentNode = null;//未完成审批的父节点
                foreach (DataRow drChildNode in tbChildNode.Rows)
                {
                    childNodeCode = int.Parse(drChildNode[0].ToString());
                    parentParams[1].Value = childNodeCode;
                    dtParentNode = transHelper.ExcuteDataTable(CommandType.Text, parentNodeStr, parentParams);
                    if (dtParentNode != null && dtParentNode.Rows.Count == 0)
                    {
                        if (flowNodeCode.Contains(childNodeCode))//节点不满足判断条件
                        {
                            //添加到不可用节点
                            if (!disableNode.Contains(childNodeCode))
                                disableNode.Add(childNodeCode);
                        }
                        else
                        {
                            //所有父节点完成审核并且节点满足判断条件，当前子节点可用
                            //添加到可用节点
                            if (!enableNode.Contains(childNodeCode)) enableNode.Add(childNodeCode);
                        }
                    }
                    else
                    {
                        //添加到不可用节点
                        if (!disableNode.Contains(childNodeCode))
                            disableNode.Add(childNodeCode);
                    }
                }
                #endregion

                StringBuilder strIn = new StringBuilder();
                NoUserNode.Clear();
                Boolean blnNoUser = false;
                string strCheckUser = "Select * From TbFlowPerformNodePersonnel where FlowPerformID=@FlowPerformID And FlowNodeCode=@FlowNodeCode And ActionType=0";
                for (int i = 0; i < enableNode.Count; i++)
                {
                    object dtUser = transHelper.ExecuteScalar(CommandType.Text, strCheckUser, new SqlParameter[]{
                        new SqlParameter("@FlowPerformID", FlowPerformID ),
                        new SqlParameter("@FlowNodeCode",enableNode[i])   });
                    if (dtUser == null)
                    {
                        NoUserNode.Add(enableNode[i]);
                        blnNoUser = true;
                    }
                }
                return blnNoUser;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
