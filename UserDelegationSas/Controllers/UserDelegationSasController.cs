using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;

namespace UserDelegationSas.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserDelegationSasController : ControllerBase
    {

        [HttpGet(Name = "GetSas")]
        public async Task<string> Get()
        {
            var accountName = "attachmentsademo";
            // Construct the blob endpoint from the account name.
            string blobEndpoint = string.Format("https://{0}.blob.core.windows.net", accountName);

            // Create a new Blob service client with Azure AD credentials.
            BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(blobEndpoint),
                                                                 new DefaultAzureCredential());

            UserDelegationKey key = await blobServiceClient.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow,
                                                                   DateTimeOffset.UtcNow.AddDays(7));

            var containerClient = blobServiceClient.GetBlobContainerClient("attachments");
            var blobClient = containerClient.GetBlobClient("cat-enjoying-the-view.jpg");

            // Create a SAS token that's also valid for 7 days.
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Specify read permissions for the SAS.
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Add the SAS token to the blob URI.
            BlobUriBuilder blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
            {
                // Specify the user delegation key.
                Sas = sasBuilder.ToSasQueryParameters(key,
                                                      blobServiceClient.AccountName)
            };
            
            var uri = blobUriBuilder.ToUri();

            return uri.AbsoluteUri;
        }
    }
}
