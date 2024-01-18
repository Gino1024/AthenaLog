# ***AthenaLog***
## **1.開發原因**
    減少RDS使用空間, 轉移至S3, 也提供保持可以查詢的功能
## **2.功能說明**
    將指定table匯出成Athena的Json格式至S3, 提供給Athena查詢
---

# **2. 設定檔詳細註釋 (僅列出需注意的項目)**
 ### 2.1. **appSettings**
   Key | 說明
   --- | ---
   Site | 用於檔案路徑的判斷( **SIT**,**UAT**,**PRD** )
   perPageCount | 每次作業筆數 

---
# **3. DB Schema 說明**
<a title="另開新視窗" href="./AthenaLogSchema.html"><img src="./AthenaLogSchema.png" /></a>
---
# **4. AthenaLog 主流程說明**

```mermaid
flowchart TD;

  AthenaFectory-->|Query AthenaTaskRecord.IsCompleted=false|A{建立RetryAthenaTask};
  AthenaFectory-->AthenaTask(建立AthenaTask);
  A-->|無|結束;
  A-->|有|BaseAthenaTaskDictionary;
  AthenaTask-->BaseAthenaTaskDictionary;
  BaseAthenaTaskDictionary-.-Lines["`(key,value)
  (table1,tasks)
  (table2,tasks)
  (table3,tasks)
  ...`"]
  BaseAthenaTaskDictionary --> |Paralell| AthenaTask1 --> retryTask1 --> Task1
  BaseAthenaTaskDictionary --> |Paralell| AthenaTask2  --> retryTask2 --> Task2
  BaseAthenaTaskDictionary --> |Paralell| ...  --> retryTask... --> Task...

```
---
BaseAthenaTask 說明
---
```mermaid
  flowchart TD;
    BaseAthenaTask --> AthenaRetryTask
    BaseAthenaTask --> AthenaTask

    AthenaRetryTask --> AthenaRetryTask.Begin --> AthenaRetryTask.Verify --> AthenaRetryTask.Composition --> AthenaRetryTask.Excuting 
    AthenaRetryTask.Begin -.- updateTaskRecord2(更新taskRecord)
    AthenaRetryTask.Composition -.- 建立RetryTaskDetail一筆
    AthenaRetryTask.Excuting --> AthenaRetryTask.Record
    AthenaRetryTask.Excuting -.- 執行RetryTaskDetail
    AthenaRetryTask.Record -.- updateTaskRecord3(更新taskRecord)


    AthenaTask --> AthenaTask.Begin 

    AthenaTask.Begin -->  AthenaTask.Verify 
    AthenaTask.Begin  -.- 建立taskRecord 


    AthenaTask.Verify --> TaskVerify{是否驗證成功}

    TaskVerify --> |否| AthenaTask.Record
    TaskVerify --> |是|AthenaTask.Composition 

    AthenaTask.Composition --> TaskTotal{是否有資料} -->
    |有資料|建立AthenaTaskDetail多筆  --> AthenaTask.Excuting --> AthenaTask.Record 

    TaskTotal --> |否| AthenaTask.Record 

    建立AthenaTaskDetail多筆 -.- 批次處理大量資料
    AthenaTask.Excuting -.- 迴圈執行AthenaTaskDetailObject任務
    AthenaTask.Record -.- updateTaskRecord(更新taskRecord)
```
---
BaseAthenaDetailTask 說明
---
```mermaid
  flowchart TD;
    BaseAthenaDetailTask --> AthenaRetryTaskDetail
    BaseAthenaDetailTask --> AthenaTaskDetail

    AthenaRetryTaskDetail --> BaseTaskDetailRetry(BaseTaskDetail.Begin)
    BaseTaskDetailRetry --> AthenaRetryTaskDetail.GetSourceDataAndOrganize

      AthenaRetryTaskDetail.GetSourceDataAndOrganize -->  DetailOrganize2{是否成功} --> |是| BaseTaskDetail.SendToS3

      DetailOrganize2{是否成功} --> |否| 結束
    


    BaseTaskDetail.SendToS3 --> BaseTaskDetail.Record
    BaseTaskDetail.Record --> BaseTaskDetail.Dispose

    AthenaTaskDetail --> 
    BaseTaskDetail.Begin --> AthenaTaskDetail.GetSourceDataAndOrganize

    AthenaTaskDetail.GetSourceDataAndOrganize --> DetailOrganize{是否成功} --> |是| AthenaTaskDetailSendToS3(BaseTaskDetail.SendToS3)

    DetailOrganize --> |否| 更新TaskDetailRecord --> end2(結束)

    AthenaTaskDetailSendToS3 --> AthenaTaskDetailRecord(BaseTaskDetail.Record)
    AthenaTaskDetailRecord --> AthenaTaskDetailDispose(BaseTaskDetail.Dispose)


    BaseTaskDetail.Begin -.- 建立AthenaDetailRecord
```