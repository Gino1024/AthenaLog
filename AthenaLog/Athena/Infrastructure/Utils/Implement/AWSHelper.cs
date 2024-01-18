using AthenaLog.Athena.Infrastructure.Utils.Interface;
using System;
using AthenaLog.Athena.Infrastructure.Utils.Dto;
using Amazon.S3.Model;
using System.Configuration;
using AthenaLog.Athena.Infrastructure.Utils.Enum;
using System.Collections.Generic;
using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace AthenaLog.Athena.Infrastructure.Utils.Implement
{
    public class AWSHelper : IAWSHelper
    {
        private readonly IFileHelper _fileHelper;
        public AWSHelper(IFileHelper fileHelper)
        {
            _fileHelper = fileHelper;
        }
        public ResultHelper<string> SendFileToS3(byte[] fileMs, string s3Path, S3DataType s3DataType, bool isAbsolutePath = true)
        {
            var result = new ResultHelper<string>();
            string environment = ConfigurationManager.AppSettings["Site"];
            string basePath = GetS3BasePath(environment, s3DataType);
            s3Path = s3Path.StartsWith("/") ? s3Path.Substring(1, s3Path.Length - 1) : s3Path;

            //地端
            if (string.IsNullOrWhiteSpace(environment) || (environment == nameof(Enviroment.SIT)))
            {
                try
                {

                    string modifiedS3Path = (isAbsolutePath)
                        ? Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "S3", s3Path)
                        : Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "S3", basePath, s3Path);


                    string fileDirectory = Path.GetDirectoryName(modifiedS3Path);
                    if (!Directory.Exists(fileDirectory))
                    {
                        Directory.CreateDirectory(fileDirectory);
                    }

                    result.Status = _fileHelper.ByteArrayToFile(modifiedS3Path, fileMs);
                    result.ResultMessage = result.Status ? string.Empty : "產檔失敗";
                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.ResultMessage = ex.ToString();
                }
                return result;
            }
            else
            {
                try
                {
                    // 判斷環境
                    string bucketName = GetBucketName(s3DataType);
                    string modifiedS3Path = (isAbsolutePath) ? s3Path : $"{basePath}/{s3Path}";

                    // S3的Server位置，目前都是ap-north-east1
                    RegionEndpoint bucketRegion = RegionEndpoint.APNortheast1;
                    IAmazonS3 s3client;
                    s3client = new AmazonS3Client(bucketRegion);
                    // 建立S3傳輸物件
                    var fileTransferUtility = new TransferUtility(s3client);
                    // 上傳單檔
                    fileTransferUtility.Upload(new TransferUtilityUploadRequest()
                    {
                        BucketName = bucketName,
                        Key = modifiedS3Path,
                        InputStream = new MemoryStream(fileMs)
                    });

                    result.Status = true;
                }
                catch (AmazonS3Exception e)
                {
                    result.Status = false;
                    result.ResultMessage = $"S3 Error Exception Message:'{e}' ";
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.ResultMessage = $"非 S3 Error Exception Message:'{e}' ";
                }
                return result;
            }
        }

        protected string GetBucketName(S3DataType s3dataType)
        {
            string environment = ConfigurationManager.AppSettings["Site"];
            // 判斷環境
            string bucketName =
                environment == nameof(Enviroment.PRD)
                ? s3dataType == S3DataType.Image
                    ? "image-prd"
                    : "prd"
                : s3dataType == S3DataType.Image
                    ? "image-test"
                    : "test";

            return bucketName;
        }
        protected string GetS3BasePath(string enviroment, S3DataType s3dataType)
        {
            try
            {
                string result = string.Empty;

                result = (enviroment == nameof(Enviroment.PRD)) ? s3dataType.ToString() : $"{s3dataType.ToString()}_test";
                return result;
            }
            catch (Exception) { throw; }
        }
    }
}
