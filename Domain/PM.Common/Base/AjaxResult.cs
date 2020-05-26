using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Common
{
    /// <summary>
    /// 数据处理结果
    /// </summary>
    public class AjaxResult
    {
        /// <summary>
        /// 操作结果类型
        /// </summary>
        public object state { get; set; }
        /// <summary>
        /// 获取 消息内容
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 获取 返回数据
        /// </summary>
        public object data { get; set; }

        /// <summary>
        /// 成功
        /// </summary>
        public static AjaxResult Success(string msg="操作成功")
        {
            return new AjaxResult()
            {
                state = ResultType.success.ToString(),
                message = msg,
                data = null
            };
        }
        /// <summary>
        /// 成功
        /// </summary>
        public static AjaxResult Success(object data)
        {
            return new AjaxResult()
            {
                state = ResultType.success.ToString(),
                data = data,
                message = "操作成功"
            };
        }
        
        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="errorCode"></param>
        public static AjaxResult Error(string msg = "操作失败")
        {
            return new AjaxResult()
            {
                state = ResultType.error.ToString(),
                message = msg,
                data = null
            };
        }
        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="errorCode"></param>
        public static AjaxResult Warning(string msg)
        {
            return new AjaxResult()
            {
                state = ResultType.warning.ToString(),
                message = msg,
                data = null
            };
        }      
    }


    /// <summary>
    /// 数据处理结果
    /// </summary>
    public class AjaxResult<T> : AjaxResult
    {
        public AjaxResult()
        {
            data = default(T);
        }

        /// <summary>
        /// 返回对象
        /// </summary>
        public T data { get; set; }

        public static AjaxResult<T> Success(T data)
        {
            return new AjaxResult<T>()
            {
                state = ResultType.success.ToString(),
                data = data
            };
        }
    }

    /// <summary>
    /// 表示 ajax 操作结果类型的枚举
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// 消息结果类型
        /// </summary>
        info,
        /// <summary>
        /// 成功结果类型
        /// </summary>
        success,
        /// <summary>
        /// 警告结果类型
        /// </summary>
        warning,
        /// <summary>
        /// 异常结果类型
        /// </summary>
        error
    }
}
