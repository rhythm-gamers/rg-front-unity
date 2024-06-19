using System;
using UnityEngine;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Threading.Tasks;
using Amazon.Runtime;
using System.Collections;
using DotNetEnv;

public class S3Uploader : MonoBehaviour
{
    static S3Uploader instance;
    public static S3Uploader Instance
    {
        get
        {
            return instance;
        }
    }

    private readonly string bucketName = Env.GetString("BUCKET_NAME");
    private readonly string awsAccessKeyId = Env.GetString("AWS_ACCESS_KEY_ID");
    private readonly string awsSecretAccessKey = Env.GetString("AWS_SECRET_ACCESS_KEY");

    private string fileKey;
    private readonly RegionEndpoint bucketRegion = RegionEndpoint.APNortheast2;

    private static IAmazonS3 client;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void UploadFile(string contentBody)
    {
        Init();
        StartCoroutine(IEUploadFile(contentBody));
    }

    private void Init()
    {
        fileKey = $"Sheet/{GameManager.Instance.sheet.title}/{GameManager.Instance.sheet.title}.sheet";

        var credentials = new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey);
        client = new AmazonS3Client(credentials, bucketRegion);
    }

    private IEnumerator IEUploadFile(string contentBody)
    {
        Task uploadTask = UploadFileAsync(contentBody);
        yield return new WaitUntil(() => uploadTask.IsCompleted);

        Debug.Log("File uploaded successfully.");
    }

    private async Task UploadFileAsync(string contentBody)
    {
        try
        {
            Debug.Log("S3 Uploading");
            var putRequest1 = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = fileKey,
                ContentBody = contentBody,
                ContentType = "binary/octet-stream"
            };

            PutObjectResponse res = await client.PutObjectAsync(putRequest1);
        }
        catch (AmazonS3Exception e)
        {
            Debug.Log(
                $"Error encountered on server. Message:'{e.Message}' when uploading an object");
        }
        catch (Exception e)
        {
            Debug.Log(
                $"Unknown encountered on server. Message:'{e.Message}' when uploading an object");
        }
        finally
        {
            client.Dispose();
        }
    }
}