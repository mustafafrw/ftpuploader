using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTPUpload
{
    public static class Tool
    {
        public static string TrimStart(this string target, string trimString)
        {
            string result = target;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }
        public static string OpenPath(Site sit, string lokasyon)
        {
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + sit.SiteName + "/www/" + lokasyon);
            ftpRequest.Credentials = new NetworkCredential(sit.Username, sit.Pwd);
            ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            using (var resp = (FtpWebResponse)ftpRequest.GetResponse())
            {
                return resp.StatusCode.ToString();
            }
        }
        public static string UploadFile(Site sit, string dosya)
        {
            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(sit.Username, sit.Pwd);
                client.UploadFile("ftp://"+sit.SiteName+"/www/"+dosya, "STOR", "wordpress/"+dosya);
            }
            return "okey";
        }
        private static List<string> GetDirectories(string direct)
        {
            List<string> directories = new List<string>();
            foreach (string d in Directory.GetDirectories(direct))
            {
                directories.Add(d);
            }
            return directories;
        }
        public static List<string> GetAllDirectory(string dd)
        {
            List<string> DirectoryAll = new List<string>();

            String[] allfiles2 = System.IO.Directory.GetDirectories("wordpress", "*.*", System.IO.SearchOption.AllDirectories);
            foreach (string a in allfiles2)
                DirectoryAll.Add(Tool.TrimStart(a, @"wordpress\"));

            return DirectoryAll;
        } 
        public static List<Site> GetSiteler()
        {
            List<Site> siteler = new List<Site>();
            List<string> rowList = new List<string>();
            StreamReader read = new StreamReader("info.txt");
            string row = read.ReadLine();
            while (row != null)
            {
                rowList.Add(row);
                row = read.ReadLine();
            }
            foreach (string r in rowList)
            {
                int i = 0;
                Site sit = new Site();
                foreach (string bilgi in r.Split('&'))
                {
                    i++;
                    if (i == 1)
                        sit.SiteName = bilgi;
                    if (i == 2)
                    {
                        sit.Username = bilgi;
                        sit.Pwd = "c32a03418f";
                        sit.DB_Pw = "c32a03418f";

                        if (bilgi.Length <= 8)
                        {
                            sit.DB_name = bilgi+"_wp";
                            sit.DB_Username = bilgi+"_wpk";
                        }
                        else
                        {
                            sit.DB_name = bilgi.Substring(0,8) + "_wp";
                            sit.DB_Username = bilgi.Substring(0,8) + "_wpk";
                        }
                    }
                }
                siteler.Add(sit);
            }
            return siteler;
        }
        public static void ConfigSampleChange(Site site)
        {
            string content = Properties.Settings.Default.config_sample_php;
            content = content.Replace("database_name_here", site.DB_name);
            content = content.Replace("username_here", site.DB_Username);
            content = content.Replace("password_here", site.DB_Pw);
            File.WriteAllText("wordpress/wp-config-sample.php", content);
        }
    }
    public class Site
    {
        public string SiteName { get; set; }
        public string Username { get; set; }
        public string Pwd { get; set; }
        public string DB_name { get; set; }
        public string DB_Username { get; set; }
        public string DB_Pw { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<Site> siteler = Tool.GetSiteler();
            Console.WriteLine("Toplam {0} site bilgisi mevcut", siteler.Count);

            string wr = Console.ReadLine();
            if (wr.Equals("start"))
            {
                foreach (Site si in siteler)
                {
                    Console.WriteLine("{0} Sitesi için wordpress ayarları yapılıyor {1} - {2} - {3} - {4} - {5} ", si.SiteName,si.Username,si.Pwd,si.DB_name,si.DB_Username,si.DB_Pw);
                   Tool.ConfigSampleChange(siteler[0]);
                    Console.WriteLine("DB Bilgileri Girildi");

                    Console.WriteLine("Klasörler oluşturuluyor\n");
                    List<string> dr = Tool.GetAllDirectory("wordpress");
                    int jj = 0, ht = 0;
                    foreach (string dd in dr)
                    {
                        jj++;
                        try
                        {
                            Tool.OpenPath(si, dd.Trim().Replace(@"\", "/"));
                        }
                        catch { ht++; }
                        Console.Write("\r{0}/{1} Oluşturuldu - {2} hata tespiti", jj,dr.Count,ht);
                    }
                    Console.WriteLine("klasörler oluşturuldu");
                    

                    Console.WriteLine("Upload başlıyor");
                    String[] allfiles = System.IO.Directory.GetFiles("wordpress", "*.*", System.IO.SearchOption.AllDirectories);
                    int ju = 0, hatali = 0;
                    foreach (string fi in allfiles)
                    {
                        ju++;
                        try
                        {
                            Tool.UploadFile(si, Tool.TrimStart(fi, @"wordpress\"));
                        }
                        catch { Console.WriteLine(fi); hatali++; }
                        Console.Write("\r{0}/{1} upload edildi - {2} hatalı dosya", ju,allfiles.Length,hatali);
                    }
                    Console.WriteLine("Upload bitti");
                }
                Console.WriteLine("Tüm işlemler sona erdi");
            }


            //  Tool.ConfigSampleChange(siteler[0]); // wordpress/wp-config-sample.php {DB BİLGİLERİNİ GİR}



            String[] allfiles = System.IO.Directory.GetFiles("wordpress", "*.*", System.IO.SearchOption.AllDirectories);
            foreach (string fi in allfiles)
            {
                Console.WriteLine(fi.Replace(@"wordpress\",""));
            }
            
            Console.ReadLine();
        }
    }
}
