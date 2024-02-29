using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ModernRecrut.Documents.API.Interfaces;
using ModernRecrut.Documents.API.Models;

namespace ModernRecrut.Documents.API.Services
{
	public class StorageServiceHelper : IStorageServiceHelper
	{
		private readonly BlobServiceClient _blobServiceClient;
		private readonly IConfiguration _configuration;
		private string _containerName;

		public StorageServiceHelper(BlobServiceClient blobServiceClient, IConfiguration configuration)
		{
			_blobServiceClient = blobServiceClient;
			_configuration = configuration;
			_containerName = _configuration.GetValue<string>("StorageAccountContainer");
		}

		public Task EnregistrerFichier(Document document, string utilisateurId)
		{
			int randomNum = new Random().Next(1000, 9999);  // Nombre aléatoire
			string extension = Path.GetExtension(document.DocumentDetails.FileName);  // Extension du fichier
			string fichierNom = $"{utilisateurId}_{document.DocumentType}_{randomNum}{extension}";
			//Obtenir le conteneur
			var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

			containerClient.CreateIfNotExists();
			//Obtenir le blob et verifier si le fichier existe
			var blobClient = containerClient.GetBlobClient(fichierNom);
			//Enregistrer le fichier
			return blobClient.UploadAsync(document.DocumentDetails.OpenReadStream(), true);

		}

		public async Task<IEnumerable<string>> ObtenirSelonUtilisateurId(string utilisateurId)
		{
			List<string> storageAccountDatas = new();

			//Obtenir le conteneur
			var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

			//Obtenir les blobs
			await foreach (var blobItem in containerClient.GetBlobsAsync())
			{
				if (blobItem.Name.StartsWith($"{utilisateurId}_"))
				{
					storageAccountDatas.Add(blobItem.Name);
				}
			}

			return storageAccountDatas;
		}

		public async Task<bool> SupprimerFichier(string fichierNom)
		{
			string utilisateurId = fichierNom.Split("_")[0];
			//Obtenir le conteneur
			var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

			//Obtenir le blob

			var blobClient = containerClient.GetBlobClient(fichierNom);

			//Supprimer le blob
			return await blobClient.DeleteIfExistsAsync();
		}
	}
}
