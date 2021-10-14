/***********************************************************************
 *            Project: CoreCms
 *        ProjectName: 核心内容管理系统                                
 *                Web: https://www.corecms.net                      
 *             Author: 大灰灰                                          
 *              Email: jianweie@163.com                                
 *         CreateTime: 2021/1/31 21:45:10
 *        Description: 暂无
 ***********************************************************************/

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CoreCms.Net.Configuration;
using CoreCms.Net.Model.Entities;
using CoreCms.Net.Model.Entities.Expression;
using CoreCms.Net.Model.FromBody;
using CoreCms.Net.Model.ViewModels.UI;
using CoreCms.Net.Filter;
using CoreCms.Net.Loging;
using CoreCms.Net.IServices;
using CoreCms.Net.Utility.Helper;
using CoreCms.Net.Utility.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.UserModel;
using SqlSugar;

namespace CoreCms.Net.Web.Admin.Controllers
{
    /// <summary>
    /// 服务券核验日志
    ///</summary>
    [Description("服务券核验日志")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [RequiredErrorForAdmin]
    [Authorize(Permissions.Name)]
    public class CoreCmsUserServicesTicketVerificationLogController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICoreCmsUserServicesTicketVerificationLogServices _CoreCmsUserServicesTicketVerificationLogServices;

        /// <summary>
        /// 构造函数
        ///</summary>
        public CoreCmsUserServicesTicketVerificationLogController(IWebHostEnvironment webHostEnvironment
            , ICoreCmsUserServicesTicketVerificationLogServices CoreCmsUserServicesTicketVerificationLogServices
            )
        {
            _webHostEnvironment = webHostEnvironment;
            _CoreCmsUserServicesTicketVerificationLogServices = CoreCmsUserServicesTicketVerificationLogServices;
        }

        #region 获取列表============================================================
        // POST: Api/CoreCmsUserServicesTicketVerificationLog/GetPageList
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Description("获取列表")]
        public async Task<AdminUiCallBack> GetPageList()
        {
            var jm = new AdminUiCallBack();
            var pageCurrent = Request.Form["page"].FirstOrDefault().ObjectToInt(1);
            var pageSize = Request.Form["limit"].FirstOrDefault().ObjectToInt(30);
            var where = PredicateBuilder.True<CoreCmsUserServicesTicketVerificationLog>();
            //获取排序字段
            var orderField = Request.Form["orderField"].FirstOrDefault();
            Expression<Func<CoreCmsUserServicesTicketVerificationLog, object>> orderEx;
            switch (orderField)
            {
                case "id":
                    orderEx = p => p.id;
                    break;
                case "storeId":
                    orderEx = p => p.storeId;
                    break;
                case "verificationUserId":
                    orderEx = p => p.verificationUserId;
                    break;
                case "ticketId":
                    orderEx = p => p.ticketId;
                    break;
                case "ticketRedeemCode":
                    orderEx = p => p.ticketRedeemCode;
                    break;
                case "verificationTime":
                    orderEx = p => p.verificationTime;
                    break;
                default:
                    orderEx = p => p.id;
                    break;
            }
            //设置排序方式
            var orderDirection = Request.Form["orderDirection"].FirstOrDefault();
            var orderBy = orderDirection switch
            {
                "asc" => OrderByType.Asc,
                "desc" => OrderByType.Desc,
                _ => OrderByType.Desc
            };
            //查询筛选

            //序列 int
            var id = Request.Form["id"].FirstOrDefault().ObjectToInt(0);
            if (id > 0)
            {
                where = where.And(p => p.id == id);
            }
            //核销门店id int
            var storeId = Request.Form["storeId"].FirstOrDefault().ObjectToInt(0);
            if (storeId > 0)
            {
                where = where.And(p => p.storeId == storeId);
            }
            //核验人 int
            var verificationUserId = Request.Form["verificationUserId"].FirstOrDefault().ObjectToInt(0);
            if (verificationUserId > 0)
            {
                where = where.And(p => p.verificationUserId == verificationUserId);
            }
            //服务券序列 int
            var ticketId = Request.Form["ticketId"].FirstOrDefault().ObjectToInt(0);
            if (ticketId > 0)
            {
                where = where.And(p => p.ticketId == ticketId);
            }
            //核验码 nvarchar
            var ticketRedeemCode = Request.Form["ticketRedeemCode"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ticketRedeemCode))
            {
                where = where.And(p => p.ticketRedeemCode.Contains(ticketRedeemCode));
            }
            //核验时间 datetime
            var verificationTime = Request.Form["verificationTime"].FirstOrDefault();
            if (!string.IsNullOrEmpty(verificationTime))
            {
                if (verificationTime.Contains("到"))
                {
                    var dts = verificationTime.Split("到");
                    var dtStart = dts[0].Trim().ObjectToDate();
                    where = where.And(p => p.verificationTime > dtStart);
                    var dtEnd = dts[1].Trim().ObjectToDate();
                    where = where.And(p => p.verificationTime < dtEnd);
                }
                else
                {
                    var dt = verificationTime.ObjectToDate();
                    where = where.And(p => p.verificationTime > dt);
                }
            }
            //获取数据
            var list = await _CoreCmsUserServicesTicketVerificationLogServices.QueryPageAsync(where, orderEx, orderBy, pageCurrent, pageSize);
            //返回数据
            jm.data = list;
            jm.code = 0;
            jm.count = list.TotalCount;
            jm.msg = "数据调用成功!";
            return jm;
        }
        #endregion

        #region 首页数据============================================================
        // POST: Api/CoreCmsUserServicesTicketVerificationLog/GetIndex
        /// <summary>
        /// 首页数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Description("首页数据")]
        public AdminUiCallBack GetIndex()
        {
            //返回数据
            var jm = new AdminUiCallBack { code = 0 };
            return jm;
        }
        #endregion

        #region 创建数据============================================================
        // POST: Api/CoreCmsUserServicesTicketVerificationLog/GetCreate
        /// <summary>
        /// 创建数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Description("创建数据")]
        public AdminUiCallBack GetCreate()
        {
            //返回数据
            var jm = new AdminUiCallBack { code = 0 };
            return jm;
        }
        #endregion

        #region 创建提交============================================================
        // POST: Api/CoreCmsUserServicesTicketVerificationLog/DoCreate
        /// <summary>
        /// 创建提交
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Description("创建提交")]
        public async Task<AdminUiCallBack> DoCreate([FromBody] CoreCmsUserServicesTicketVerificationLog entity)
        {
            var jm = new AdminUiCallBack();

            var bl = await _CoreCmsUserServicesTicketVerificationLogServices.InsertAsync(entity) > 0;
            jm.code = bl ? 0 : 1;
            jm.msg = (bl ? GlobalConstVars.CreateSuccess : GlobalConstVars.CreateFailure);

            return jm;
        }
        #endregion

        #region 编辑数据============================================================
        // POST: Api/CoreCmsUserServicesTicketVerificationLog/GetEdit
        /// <summary>
        /// 编辑数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Description("编辑数据")]
        public async Task<AdminUiCallBack> GetEdit([FromBody] FMIntId entity)
        {
            var jm = new AdminUiCallBack();

            var model = await _CoreCmsUserServicesTicketVerificationLogServices.QueryByIdAsync(entity.id);
            if (model == null)
            {
                jm.msg = "不存在此信息";
                return jm;
            }
            jm.code = 0;
            jm.data = model;

            return jm;
        }
        #endregion

        #region 编辑提交============================================================
        // POST: Api/CoreCmsUserServicesTicketVerificationLog/Edit
        /// <summary>
        /// 编辑提交
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Description("编辑提交")]
        public async Task<AdminUiCallBack> DoEdit([FromBody] CoreCmsUserServicesTicketVerificationLog entity)
        {
            var jm = new AdminUiCallBack();

            var oldModel = await _CoreCmsUserServicesTicketVerificationLogServices.QueryByIdAsync(entity.id);
            if (oldModel == null)
            {
                jm.msg = "不存在此信息";
                return jm;
            }
            //事物处理过程开始
            oldModel.id = entity.id;
            oldModel.storeId = entity.storeId;
            oldModel.verificationUserId = entity.verificationUserId;
            oldModel.ticketId = entity.ticketId;
            oldModel.ticketRedeemCode = entity.ticketRedeemCode;
            oldModel.verificationTime = entity.verificationTime;

            //事物处理过程结束
            var bl = await _CoreCmsUserServicesTicketVerificationLogServices.UpdateAsync(oldModel);
            jm.code = bl ? 0 : 1;
            jm.msg = bl ? GlobalConstVars.EditSuccess : GlobalConstVars.EditFailure;

            return jm;
        }
        #endregion

        #region 删除数据============================================================
        // POST: Api/CoreCmsUserServicesTicketVerificationLog/DoDelete/10
        /// <summary>
        /// 单选删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Description("单选删除")]
        public async Task<AdminUiCallBack> DoDelete([FromBody] FMIntId entity)
        {
            var jm = new AdminUiCallBack();

            var model = await _CoreCmsUserServicesTicketVerificationLogServices.QueryByIdAsync(entity.id);
            if (model == null)
            {
                jm.msg = GlobalConstVars.DataisNo;
                return jm;
            }
            var bl = await _CoreCmsUserServicesTicketVerificationLogServices.DeleteByIdAsync(entity.id);
            jm.code = bl ? 0 : 1;
            jm.msg = bl ? GlobalConstVars.DeleteSuccess : GlobalConstVars.DeleteFailure;
            return jm;

        }
        #endregion

        #region 批量删除============================================================
        // POST: Api/CoreCmsUserServicesTicketVerificationLog/DoBatchDelete/10,11,20
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Description("批量删除")]
        public async Task<AdminUiCallBack> DoBatchDelete([FromBody] FMArrayIntIds entity)
        {
            var jm = new AdminUiCallBack();

            var bl = await _CoreCmsUserServicesTicketVerificationLogServices.DeleteByIdsAsync(entity.id);
            jm.code = bl ? 0 : 1;
            jm.msg = bl ? GlobalConstVars.DeleteSuccess : GlobalConstVars.DeleteFailure;

            return jm;
        }

        #endregion

        #region 预览数据============================================================
        // POST: Api/CoreCmsUserServicesTicketVerificationLog/GetDetails/10
        /// <summary>
        /// 预览数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Description("预览数据")]
        public async Task<AdminUiCallBack> GetDetails([FromBody] FMIntId entity)
        {
            var jm = new AdminUiCallBack();

            var model = await _CoreCmsUserServicesTicketVerificationLogServices.QueryByIdAsync(entity.id);
            if (model == null)
            {
                jm.msg = "不存在此信息";
                return jm;
            }
            jm.code = 0;
            jm.data = model;

            return jm;
        }
        #endregion

        #region 选择导出============================================================
        // POST: Api/CoreCmsUserServicesTicketVerificationLog/SelectExportExcel/10
        /// <summary>
        /// 选择导出
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Description("选择导出")]
        public async Task<AdminUiCallBack> SelectExportExcel([FromBody] FMArrayIntIds entity)
        {
            var jm = new AdminUiCallBack();

            //创建Excel文件的对象
            var book = new HSSFWorkbook();
            //添加一个sheet
            var mySheet = book.CreateSheet("Sheet1");
            //获取list数据
            var listModel = await _CoreCmsUserServicesTicketVerificationLogServices.QueryListByClauseAsync(p => entity.id.Contains(p.id), p => p.id, OrderByType.Asc);
            //给sheet1添加第一行的头部标题
            var headerRow = mySheet.CreateRow(0);
            var headerStyle = ExcelHelper.GetHeaderStyle(book);

            var cell0 = headerRow.CreateCell(0);
            cell0.SetCellValue("序列");
            cell0.CellStyle = headerStyle;
            mySheet.SetColumnWidth(0, 10 * 256);

            var cell1 = headerRow.CreateCell(1);
            cell1.SetCellValue("核销门店id");
            cell1.CellStyle = headerStyle;
            mySheet.SetColumnWidth(1, 10 * 256);

            var cell2 = headerRow.CreateCell(2);
            cell2.SetCellValue("核验人");
            cell2.CellStyle = headerStyle;
            mySheet.SetColumnWidth(2, 10 * 256);

            var cell3 = headerRow.CreateCell(3);
            cell3.SetCellValue("服务券序列");
            cell3.CellStyle = headerStyle;
            mySheet.SetColumnWidth(3, 10 * 256);

            var cell4 = headerRow.CreateCell(4);
            cell4.SetCellValue("核验码");
            cell4.CellStyle = headerStyle;
            mySheet.SetColumnWidth(4, 10 * 256);

            var cell5 = headerRow.CreateCell(5);
            cell5.SetCellValue("核验时间");
            cell5.CellStyle = headerStyle;
            mySheet.SetColumnWidth(5, 10 * 256);

            headerRow.Height = 30 * 20;
            var commonCellStyle = ExcelHelper.GetCommonStyle(book);

            //将数据逐步写入sheet1各个行
            for (var i = 0; i < listModel.Count; i++)
            {
                var rowTemp = mySheet.CreateRow(i + 1);

                var rowTemp0 = rowTemp.CreateCell(0);
                rowTemp0.SetCellValue(listModel[i].id.ToString());
                rowTemp0.CellStyle = commonCellStyle;

                var rowTemp1 = rowTemp.CreateCell(1);
                rowTemp1.SetCellValue(listModel[i].storeId.ToString());
                rowTemp1.CellStyle = commonCellStyle;

                var rowTemp2 = rowTemp.CreateCell(2);
                rowTemp2.SetCellValue(listModel[i].verificationUserId.ToString());
                rowTemp2.CellStyle = commonCellStyle;

                var rowTemp3 = rowTemp.CreateCell(3);
                rowTemp3.SetCellValue(listModel[i].ticketId.ToString());
                rowTemp3.CellStyle = commonCellStyle;

                var rowTemp4 = rowTemp.CreateCell(4);
                rowTemp4.SetCellValue(listModel[i].ticketRedeemCode.ToString());
                rowTemp4.CellStyle = commonCellStyle;

                var rowTemp5 = rowTemp.CreateCell(5);
                rowTemp5.SetCellValue(listModel[i].verificationTime.ToString());
                rowTemp5.CellStyle = commonCellStyle;

            }
            // 导出excel
            string webRootPath = _webHostEnvironment.WebRootPath;
            string tpath = "/files/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "-CoreCmsUserServicesTicketVerificationLog导出(选择结果).xls";
            string filePath = webRootPath + tpath;
            DirectoryInfo di = new DirectoryInfo(filePath);
            if (!di.Exists)
            {
                di.Create();
            }
            FileStream fileHssf = new FileStream(filePath + fileName, FileMode.Create);
            book.Write(fileHssf);
            fileHssf.Close();

            jm.code = 0;
            jm.msg = GlobalConstVars.ExcelExportSuccess;
            jm.data = tpath + fileName;

            return jm;
        }
        #endregion

        #region 查询导出============================================================
        // POST: Api/CoreCmsUserServicesTicketVerificationLog/QueryExportExcel/10
        /// <summary>
        /// 查询导出
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Description("查询导出")]
        public async Task<AdminUiCallBack> QueryExportExcel()
        {
            var jm = new AdminUiCallBack();

            var where = PredicateBuilder.True<CoreCmsUserServicesTicketVerificationLog>();
            //查询筛选

            //序列 int
            var id = Request.Form["id"].FirstOrDefault().ObjectToInt(0);
            if (id > 0)
            {
                where = where.And(p => p.id == id);
            }
            //核销门店id int
            var storeId = Request.Form["storeId"].FirstOrDefault().ObjectToInt(0);
            if (storeId > 0)
            {
                where = where.And(p => p.storeId == storeId);
            }
            //核验人 int
            var verificationUserId = Request.Form["verificationUserId"].FirstOrDefault().ObjectToInt(0);
            if (verificationUserId > 0)
            {
                where = where.And(p => p.verificationUserId == verificationUserId);
            }
            //服务券序列 int
            var ticketId = Request.Form["ticketId"].FirstOrDefault().ObjectToInt(0);
            if (ticketId > 0)
            {
                where = where.And(p => p.ticketId == ticketId);
            }
            //核验码 nvarchar
            var ticketRedeemCode = Request.Form["ticketRedeemCode"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ticketRedeemCode))
            {
                where = where.And(p => p.ticketRedeemCode.Contains(ticketRedeemCode));
            }
            //核验时间 datetime
            var verificationTime = Request.Form["verificationTime"].FirstOrDefault();
            if (!string.IsNullOrEmpty(verificationTime))
            {
                var dt = verificationTime.ObjectToDate();
                where = where.And(p => p.verificationTime > dt);
            }
            //获取数据
            //创建Excel文件的对象
            var book = new HSSFWorkbook();
            //添加一个sheet
            var mySheet = book.CreateSheet("Sheet1");
            //获取list数据
            var listModel = await _CoreCmsUserServicesTicketVerificationLogServices.QueryListByClauseAsync(where, p => p.id, OrderByType.Asc);
            //给sheet1添加第一行的头部标题
            var headerRow = mySheet.CreateRow(0);
            var headerStyle = ExcelHelper.GetHeaderStyle(book);

            var cell0 = headerRow.CreateCell(0);
            cell0.SetCellValue("序列");
            cell0.CellStyle = headerStyle;
            mySheet.SetColumnWidth(0, 10 * 256);

            var cell1 = headerRow.CreateCell(1);
            cell1.SetCellValue("核销门店id");
            cell1.CellStyle = headerStyle;
            mySheet.SetColumnWidth(1, 10 * 256);

            var cell2 = headerRow.CreateCell(2);
            cell2.SetCellValue("核验人");
            cell2.CellStyle = headerStyle;
            mySheet.SetColumnWidth(2, 10 * 256);

            var cell3 = headerRow.CreateCell(3);
            cell3.SetCellValue("服务券序列");
            cell3.CellStyle = headerStyle;
            mySheet.SetColumnWidth(3, 10 * 256);

            var cell4 = headerRow.CreateCell(4);
            cell4.SetCellValue("核验码");
            cell4.CellStyle = headerStyle;
            mySheet.SetColumnWidth(4, 10 * 256);

            var cell5 = headerRow.CreateCell(5);
            cell5.SetCellValue("核验时间");
            cell5.CellStyle = headerStyle;
            mySheet.SetColumnWidth(5, 10 * 256);


            headerRow.Height = 30 * 20;
            var commonCellStyle = ExcelHelper.GetCommonStyle(book);

            //将数据逐步写入sheet1各个行
            for (var i = 0; i < listModel.Count; i++)
            {
                var rowTemp = mySheet.CreateRow(i + 1);


                var rowTemp0 = rowTemp.CreateCell(0);
                rowTemp0.SetCellValue(listModel[i].id.ToString());
                rowTemp0.CellStyle = commonCellStyle;



                var rowTemp1 = rowTemp.CreateCell(1);
                rowTemp1.SetCellValue(listModel[i].storeId.ToString());
                rowTemp1.CellStyle = commonCellStyle;



                var rowTemp2 = rowTemp.CreateCell(2);
                rowTemp2.SetCellValue(listModel[i].verificationUserId.ToString());
                rowTemp2.CellStyle = commonCellStyle;



                var rowTemp3 = rowTemp.CreateCell(3);
                rowTemp3.SetCellValue(listModel[i].ticketId.ToString());
                rowTemp3.CellStyle = commonCellStyle;



                var rowTemp4 = rowTemp.CreateCell(4);
                rowTemp4.SetCellValue(listModel[i].ticketRedeemCode.ToString());
                rowTemp4.CellStyle = commonCellStyle;



                var rowTemp5 = rowTemp.CreateCell(5);
                rowTemp5.SetCellValue(listModel[i].verificationTime.ToString());
                rowTemp5.CellStyle = commonCellStyle;


            }
            // 写入到excel
            string webRootPath = _webHostEnvironment.WebRootPath;
            string tpath = "/files/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "-CoreCmsUserServicesTicketVerificationLog导出(查询结果).xls";
            string filePath = webRootPath + tpath;
            DirectoryInfo di = new DirectoryInfo(filePath);
            if (!di.Exists)
            {
                di.Create();
            }
            FileStream fileHssf = new FileStream(filePath + fileName, FileMode.Create);
            book.Write(fileHssf);
            fileHssf.Close();

            jm.code = 0;
            jm.msg = GlobalConstVars.ExcelExportSuccess;
            jm.data = tpath + fileName;

            return jm;
        }
        #endregion



    }
}
