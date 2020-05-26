using Dos.ORM;
using PM.Common;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Business.System
{
    public class TbProjectInfoLogic
    {

        /// <summary>
        /// 新增数据(单条)
        /// </summary>
        public AjaxResult InsertNew(List<TbProjectInfo> model, bool isApi = false)
        {
            if (model == null)
                return AjaxResult.Warning("参数错误");

            try
            {
                List<TbProjectInfo> data = Db.Context.From<TbProjectInfo>().Select(TbProjectInfo._.All).ToList();
                using (DbTrans trans = Db.Context.BeginTransaction())
                {
                    //先删除原来的表
                    Repository<TbProjectInfo>.Delete(trans, data, isApi);
                    //插入从BM那边取过来的数据
                    Repository<TbProjectInfo>.Insert(trans, model, isApi);
                    trans.Commit();//提交事务
                    return AjaxResult.Success();
                }
            }
            catch (Exception)
            {
                return AjaxResult.Error();
            }

        }
    }
}
