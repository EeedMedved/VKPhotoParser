using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VKPhotoParser
{
    public class VKHelper
    {
        private int userId;
        public VKHelper(int userId)
        {
            this.userId = userId;
        }

        public List<Model.VKAlbum> GetVKAlbums()
        {
            String albumsUri = GetVKApiAlbumsUri();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(albumsUri);

            HttpWebResponse response;
            
            // Здесь будут хранится метаданные альбомов
            List<Model.VKAlbum> vkAlbums = new List<Model.VKAlbum>();

            using (response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    String jsonString = reader.ReadToEnd();

                    JObject JResponse = JObject.Parse(jsonString);
                    JArray array = (JArray)JResponse["response"];

                    if (array.Count == 0)
                    {
                        return vkAlbums;
                    }

                    vkAlbums = array.Select(album => new Model.VKAlbum
                    {
                        Id = (int)album["aid"],
                        Name = (String)album["title"],
                        Count = (int)album["size"]
                    }).ToList();
                }
            }

            return vkAlbums;
        }

        private String GetVKApiAlbumsUri()
        {
            String albumsListVKApi = "https://api.vk.com/method/photos.getAlbums?owner_id=" + userId;
            return albumsListVKApi;
        }

        public String GetUserAlbumUri(int albumId)
        {
            return String.Format("https://api.vk.com/method/photos.get?owner_id={0}&album_id={1}", userId, albumId);
        }
    }
}
