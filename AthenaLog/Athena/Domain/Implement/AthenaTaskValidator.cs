using AthenaLog.Athena.Domain.Dto;
using AthenaLog.Athena.Domain.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Domain.Implement
{
    public class AthenaTaskValidator : IAthenaTaskValidator
    {
        public ResultDto Verify(AthenaTask instance)
        {
            ResultDto result = new ResultDto();

            try
            {
                List<string> errMsg = new List<string>();

                #region 驗證必填項目

                if (string.IsNullOrEmpty(instance._taskRecordEntity.target))
                {
                    errMsg.Add($"{instance._taskRecordEntity.target}(sys_athenatask) target欄位不可為空值");
                }
                if (string.IsNullOrEmpty(instance._taskRecordEntity.partition_by))
                {
                    errMsg.Add($"{instance._taskRecordEntity.target}(sys_athenatask) partition_by欄位不可為空值");
                }
                if (string.IsNullOrEmpty(instance._taskRecordEntity.log_scope_by))
                {
                    errMsg.Add($"{instance._taskRecordEntity.target}(sys_athenatask) log_scope_by欄位不可為空值");
                }

                if (errMsg.Count > 0)
                {
                    result.msg = "驗證失敗: "+string.Join(",", errMsg);
                    return result;
                }
                #endregion

                #region 驗證partition_by, log_scope_by 欄位格式

                // 定義正則表達式，表示只能包含數字、英文、底線和逗號
                Regex regex = new Regex(@"^[0-9a-zA-Z_,]+$");
                if (!regex.IsMatch(instance._taskRecordEntity.partition_by))
                {
                    errMsg.Add($"{instance._taskRecordEntity.target}(sys_athenatask) partition_by只能包含數字、英文、底線且使用逗號分隔");
                }
                // 定義正則表達式，表示只能包含數字、英文、底線
                Regex regex2 = new Regex(@"^[0-9a-zA-Z_]+$");
                if (!regex2.IsMatch(instance._taskRecordEntity.log_scope_by))
                {
                    errMsg.Add($"{instance._taskRecordEntity.target}(sys_athenatask) log_scope_by只能包含數字、英文、底線");
                }

                if (errMsg.Count > 0)
                {
                    result.msg = "驗證失敗: " + string.Join(",", errMsg);
                    return result;
                }
                #endregion

                #region 驗證Target Info
                var tempTargetTable = instance._targetTableSchema
                        .Where(m => m.table_name == instance._taskRecordEntity.target);

                //驗證target table 是否存在
                if (!tempTargetTable.Any())
                {
                    result.msg = $"{instance._taskRecordEntity.target} 該target資料表不存在";
                    return result;
                }

                //驗證partion_by中的欄位是否存在於target Table
                //partion_by可以用逗號分隔, 且多個欄位
                var partitionColumnDic = new Dictionary<string, string>();
                foreach (var partitionColumnName in instance._taskRecordEntity.partition_by.Split(','))
                {
                    var tempPartitionColumn = tempTargetTable
                                        .Where(m => m.column_name == partitionColumnName)
                                        .FirstOrDefault();
                    if (tempPartitionColumn != null)
                        if (!partitionColumnDic.ContainsKey(tempPartitionColumn.column_name))
                            partitionColumnDic.Add(tempPartitionColumn.column_name, tempPartitionColumn.data_type);
                        else
                            errMsg.Add($"{instance._taskRecordEntity.target}(sys_athenatask).{tempPartitionColumn.column_name} 已重複");
                    else
                        errMsg.Add($"{instance._taskRecordEntity.target}(sys_athenatask).{partitionColumnName} 不存在");
                }

                //if (!partitionColumnDic.Values.Contains("datetime") && !partitionColumnDic.Values.Contains("string"))
                //    errMsg.Add($"{instance._taskRecordEntity.target}(sys_athenatask).partitions 僅能使用datetime、string、int");

                if (partitionColumnDic.Values.Where(m => m == "datetime").GroupBy(m => m).Where(m => m.Count() > 1).Any())
                    errMsg.Add($"{instance._taskRecordEntity.target}(sys_athenatask).partitions datetime欄位僅能一個");

                //驗證log_scope_by中的欄位是否存在於target Table
                //log_scopr_by僅單一欄位
                var tempLogScopeByColumn = tempTargetTable
                                        .Where(m => m.column_name == instance._taskRecordEntity.log_scope_by)
                                        .FirstOrDefault();
                if (tempLogScopeByColumn == null)
                    errMsg.Add($"{instance._taskRecordEntity.target}(sys_athenatask).{instance._taskRecordEntity.log_scope_by} 不存在");

                if (errMsg.Count > 0)
                {
                    result.msg = "驗證失敗: " + string.Join(",", errMsg);
                    return result;
                }



                #endregion

                #region 若有任務有lastRecord, 需多檢查partition_by, log_scope_by 欄位是否相同
                if (instance._lastSysAthenaTaskRecordEntity != null)
                {
                    //驗證當前任務的partition_by 是否與原先執行過的紀錄相同
                    //必須相同, 因為在Athena上會建立相對應的Partition區塊,
                    //若不相同會上傳S3至錯誤路徑, 導致Partition, 詳細請了解 aws athena partition table
                    if (instance._taskRecordEntity.partition_by.ToLower() != instance._lastSysAthenaTaskRecordEntity.partition_by.ToLower())
                    {
                        errMsg.Add($"{instance._taskRecordEntity.target}(sys_athenatask) partition_by 與最後執行的記錄(sys_athenatask_record)不同");
                    }
                    //驗證當前任務的log_scope_by 是否與原先執行過的紀錄相同
                    if (instance._taskRecordEntity.log_scope_by.ToLower() != instance._lastSysAthenaTaskRecordEntity.log_scope_by.ToLower())
                    {
                        errMsg.Add($"{instance._taskRecordEntity.target}(sys_athenatask) log_scope_by 與最後執行的記錄(sys_athenatask_record)不同");
                    }

                    if (errMsg.Count > 0)
                    {
                        result.msg = "驗證失敗: " + string.Join(",", errMsg);
                        return result;
                    }
                }
                #endregion

                result.isSuccess = true;
                result.msg = $"驗證通過";
            }
            catch (Exception ex)
            {
                result.msg = ex.ToString();
            }

            return result;
        }
    }
}
