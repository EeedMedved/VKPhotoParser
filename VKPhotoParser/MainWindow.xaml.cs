using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace VKPhotoParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Путь к директории, в которой будут сохраняться фотографии
        String userPath = String.Empty;

        // Идентификатор пользователя
        int inputUserID;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnParse_Click(object sender, RoutedEventArgs e)
        {
            txtBlockMsgProfile.Text = "Фотографии профиля ";
            txtBlockMsgWall.Text = "Фотографии стены ";
            txtBoxMsgSaved.Text = "Сохранённые фотографии ";

            try
            {
                inputUserID = Convert.ToInt32(txtBoxUserID.Text);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка ID пользователя. Должно быть только число!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



            VKHelper vkHelper = new VKHelper(inputUserID);
            List<Model.VKAlbum> vkAlbums = vkHelper.GetVKAlbums();

            userPath = String.Format("f:\\yum\\{0}", inputUserID);

            SaveAlbum(GetStandardAlbumUri(AlbumType.Profile, inputUserID));
            //MessageBox.Show("Фотографии профиля сохранены!");
            SaveAlbum(GetStandardAlbumUri(AlbumType.Saved, inputUserID));
            //MessageBox.Show("Фотографии сохраненные сохранены!");
            SaveAlbum(GetStandardAlbumUri(AlbumType.Wall, inputUserID));
            //MessageBox.Show("Фотографии со стены сохранены!");

            foreach (Model.VKAlbum vkAlbum in vkAlbums)
            {
                SaveAlbum(vkHelper.GetUserAlbumUri(vkAlbum.Id));
            }


            MessageBox.Show("Все фотографии сохранены");
        }


        enum AlbumType
        {
            Wall,
            Profile,
            Saved
        };

        private String GetStandardAlbumUri(AlbumType albumType, Int32 userID)
        {
            string albumTypeString = String.Empty;

            switch (albumType)
            {
                case AlbumType.Wall:
                    albumTypeString = "wall";
                    break;
                case AlbumType.Profile:
                    albumTypeString = "profile";
                    break;
                case AlbumType.Saved:
                    albumTypeString = "saved";
                    break;
                default:
                    break;
            }

            return String.Format("https://api.vk.com/method/photos.get?owner_id={0}&album_id={1}", userID, albumTypeString);
        }

        private void SaveAlbum(string albumUri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(albumUri);
            HttpWebResponse response;
            string str;

            using (response = (HttpWebResponse)request.GetResponse())
            {

                String albumType = String.Empty;

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    str = reader.ReadToEnd();
                    JObject JResponse = JObject.Parse(str);
                    JArray array = (JArray)JResponse["response"];

                    if (array.Count == 0)
                    {
                        return;
                    }

                    List<Photo> photos;


                    if (albumUri.Contains("profile"))
                    {
                        albumType = "profile";
                        photos = array.Select(x => new Photo
                        {
                            pid = (int)x["pid"],
                            aid = (int)x["aid"],
                            created = (int)x["created"],
                            owner_id = (int)x["owner_id"],
                            post_id = (int)x["post_id"],
                            src = (string)x["src"],
                            src_small = (string)x["src_small"],
                            src_big = (string)x["src_big"],
                            src_xbig = (string)x["src_xbig"],
                            src_xxbig = (string)x["src_xxbig"],
                            src_xxxbig = (string)x["src_xxxbig"]
                        }).ToList();
                    }
                    else if (albumUri.Contains("wall"))
                    {
                        albumType = "wall";
                        photos = array.Select(x => new Photo
                        {
                            pid = (int)x["pid"],
                            aid = (int)x["aid"],
                            src = (string)x["src"],
                            src_small = (string)x["src_small"],
                            src_big = (string)x["src_big"],
                            src_xbig = (string)x["src_xbig"],
                            src_xxbig = (string)x["src_xxbig"],
                            src_xxxbig = (string)x["src_xxxbig"]
                        }).ToList();
                    }
                    else if (albumUri.Contains("saved"))
                    {
                        albumType = "saved";
                        photos = array.Select(x => new Photo
                        {
                            pid = (int)x["pid"],
                            aid = (int)x["aid"],
                            src = (string)x["src"],
                            src_small = (string)x["src_small"],
                            src_big = (string)x["src_big"],
                            src_xbig = (string)x["src_xbig"]
                        }).ToList();
                    }
                    else
                    {
                        albumType = "userAlbum";
                        photos = array.Select(x => new Photo
                        {
                            pid = (int)x["pid"],
                            aid = (int)x["aid"],
                            created = (int)x["created"],
                            //owner_id = (int)x["owner_id"],
                            //post_id = (int)x["post_id"],
                            //src = (string)x["src"],
                            src_small = (string)x["src_small"],
                            src_big = (string)x["src_big"],
                            src_xbig = (string)x["src_xbig"],
                            src_xxbig = (string)x["src_xxbig"],
                            src_xxxbig = (string)x["src_xxxbig"]
                        }).ToList();
                    }

                    if (albumUri.Contains("profile"))
                    {
                        txtBlockMsgProfile.Text += photos.Count.ToString();
                    }
                    else if (albumUri.Contains("wall"))
                    {
                        txtBlockMsgWall.Text += photos.Count.ToString();
                    }
                    else if (albumUri.Contains("saved"))
                    {
                        txtBoxMsgSaved.Text += photos.Count.ToString();
                    }



                    foreach (Photo p in photos)
                    {
                        string imgSrc = String.Empty;

                        if (p.src_xxxbig != null)
                        {
                            imgSrc = p.src_xxxbig;
                        }
                        else if (p.src_xxbig != null)
                        {
                            imgSrc = p.src_xxbig;
                        }
                        else if (p.src_xbig != null)
                        {
                            imgSrc = p.src_xbig;
                        }
                        else if (p.src_big != null)
                        {
                            imgSrc = p.src_big;
                        }
                        else if (p.src_small != null)
                        {
                            imgSrc = p.src_small;
                        }
                        else if (p.src != null)
                        {
                            imgSrc = p.src;
                        }
                        else
                        {
                            MessageBox.Show("Нет активных ссылок!");
                        }

                        SaveImage(imgSrc, p.pid, albumType, p.created);
                    }
                }
            }
        }


        private bool SaveImage(String url, int pid, String albumType, int created)
        {
            HttpWebRequest request;

            String dirPath = String.Format("{0}\\{1}", userPath, albumType);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            DateTime createdDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(created);

            String filePath = String.Format("{0}\\{1}-{2}-{3}-{4}.jpg", dirPath, pid, createdDate.Year, createdDate.Month, createdDate.Day);

            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
            }
            catch (Exception e)
            {
                return false;
            }

            String lsResponse = string.Empty;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
                    {
                        Byte[] lnByte = reader.ReadBytes(1 * 1024 * 1024 * 10);
                        using (FileStream lxFs = new FileStream(filePath, FileMode.Create))
                        {
                            lxFs.Write(lnByte, 0, lnByte.Length);
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void btnAlbums_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                inputUserID = Convert.ToInt32(txtBoxUserID.Text);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка ID пользователя. Должно быть только число!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



            VKHelper vkHelper = new VKHelper(inputUserID);

            lstViewAlbums.ItemsSource = vkHelper.GetVKAlbums();
        }
    }

    public class Photo
    {
        public int pid { get; set; }
        public int aid { get; set; }
        public int owner_id { get; set; }
        public String src { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String src_big { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String src_small { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String src_xbig { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String src_xxbig { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String src_xxxbig { get; set; }
        public String text { get; set; }
        public int created { get; set; }
        public int post_id { get; set; }
    }
}
