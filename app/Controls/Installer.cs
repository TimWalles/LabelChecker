/*
LabelChecker - A tool for checking and correcting image labels
Copyright (C) 2025 Tim Johannes Wim Walles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


// using System;
// using System.Linq;
// using System.Net;
// using System.IO;
// using System.Collections.Generic;
// using System.Collections.Concurrent;
// using System.Threading.Tasks;
// using System.Diagnostics;
// using System.Net.Http;

// namespace LabelChecker
// {
//     public class Installer(Application app)
//     {
//         public const string CURRENT_VERSION = "1.0.1";

//         public long FileLength
//         {
//             get; set;
//         }
//         public float Progress
//         {
//             get; set;
//         }
//         public long ProgessBytes
//         {
//             get; set;
//         }
//         public string NewVersion
//         {
//             get; set;
//         }
//         public string WhatsNew
//         {
//             get; set;
//         }
//         readonly Application _app = app;

//         //4711schmecktGut
//         public bool CheckForNewVersion()
//         {

//             string uriVersion = "http://195.90.200.35/installer/version";

//             CookieContainer cookieJar = new CookieContainer();
//             HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriVersion);
//             request.CookieContainer = cookieJar;
//             request.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
//             request.Accept = @"text/html, application/xhtml+xml, */*";
//             request.Referer = @"http://www.google.com/";
//             request.Headers.Add("Accept-Language", "en-GB");
//             request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
//             String username = "installer";
//             String password = "4711schmecktGut";
//             String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
//             request.Headers.Add("Authorization", "Basic " + encoded);
//             // incase of no internet connection
//             try
//             {
//                 using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
//                 {
//                     using (var sr = new StreamReader(response.GetResponseStream()))
//                     {
//                         NewVersion = sr.ReadLine();
//                         FileLength = long.Parse(sr.ReadLine());
//                         string sLine = "";
//                         while ((sLine = sr.ReadLine()) != null)
//                         {
//                             WhatsNew += sLine + "\n";
//                         }
//                     }

//                 }
//                 return NewVersion != CURRENT_VERSION;
//             }
//             catch (Exception)
//             {
//                 return NewVersion == CURRENT_VERSION;
//             }

//         }

//         public void Reinstall()
//         {
//             string uri = "http://195.90.200.35/installer/market-installer.exe";
//             //string uri = " https://nimbus.igb-berlin.de/index.php/s/6sW7mMWk736M3Jz/download";
//             CookieContainer cookieJar = new CookieContainer();
//             HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
//             request.CookieContainer = cookieJar;
//             request.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
//             request.Accept = @"text/html, application/xhtml+xml, */*";
//             request.Referer = @"http://www.google.com/";
//             request.Headers.Add("Accept-Language", "en-GB");
//             request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
//             String username = "installer";
//             String password = "4711schmecktGut";
//             String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
//             request.Headers.Add("Authorization", "Basic " + encoded);
//             var tmpPath = Path.Combine(Path.GetTempPath(), "market-installer.exe");
//             using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
//             {

//                 using (var stream = response.GetResponseStream())
//                 {
//                     //  long length = stream.Length;
//                     var path = Path.GetTempFileName();
//                     List<byte> data = new List<byte>();
//                     long length = FileLength;
//                     int current = 0;
//                     long index = 0;
//                     while (current != -1)
//                     {
//                         current = stream.ReadByte();
//                         if (current != -1)
//                         {
//                             data.Add((byte)current);
//                             Progress = ++index / (float)length * 100f;
//                             ProgessBytes = index;
//                         }
//                     }
//                     //stream.Read(data);
//                     File.WriteAllBytes(tmpPath, data.ToArray());
//                 }

//                 //Start the BT Setup Process
//                 var pi = Process.Start(tmpPath);
//                 System.Threading.Thread.Sleep(1000);
//                 Process.GetCurrentProcess().Kill();

//             }
//         }


//     }//end class installer
//     public class DownloadResult
//     {
//         public long Size { get; set; }
//         public String FilePath { get; set; }
//         public TimeSpan TimeTaken { get; set; }
//         public int ParallelDownloads { get; set; }
//     }
//     public static class Downloader
//     {
//         internal class Range
//         {
//             public long Start { get; set; }
//             public long End { get; set; }
//         }

//         public static long Progress
//         {
//             get; set;
//         }
//         static Downloader()
//         {
//             ServicePointManager.Expect100Continue = false;
//             ServicePointManager.DefaultConnectionLimit = 100;
//             ServicePointManager.MaxServicePointIdleTime = 1000;

//         }
//         public static DownloadResult Download(String fileUrl, String destinationFolderPath, int numberOfParallelDownloads = 0, bool validateSSL = false)
//         {
//             if (!validateSSL)
//             {
//                 ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
//             }

//             Uri uri = new Uri(fileUrl);

//             //Calculate destination path  
//             String destinationFilePath = Path.Combine(destinationFolderPath, uri.Segments.Last());

//             DownloadResult result = new DownloadResult() { FilePath = destinationFilePath };

//             //Handle number of parallel downloads  
//             if (numberOfParallelDownloads <= 0)
//             {
//                 numberOfParallelDownloads = Environment.ProcessorCount;
//             }

//             #region Get file size  
//             WebRequest webRequest = HttpWebRequest.Create(fileUrl);
//             webRequest.Method = "HEAD";
//             long responseLength;
//             using (WebResponse webResponse = webRequest.GetResponse())
//             {
//                 responseLength = long.Parse(webResponse.Headers.Get("Content-Length"));
//                 result.Size = responseLength;
//             }
//             #endregion

//             if (File.Exists(destinationFilePath))
//             {
//                 File.Delete(destinationFilePath);
//             }

//             using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Append))
//             {
//                 ConcurrentDictionary<int, String> tempFilesDictionary = new ConcurrentDictionary<int, String>();

//                 #region Calculate ranges  
//                 List<Range> readRanges = new List<Range>();
//                 for (int chunk = 0; chunk < numberOfParallelDownloads - 1; chunk++)
//                 {
//                     var range = new Range()
//                     {
//                         Start = chunk * (responseLength / numberOfParallelDownloads),
//                         End = ((chunk + 1) * (responseLength / numberOfParallelDownloads)) - 1
//                     };
//                     readRanges.Add(range);
//                 }


//                 readRanges.Add(new Range()
//                 {
//                     Start = readRanges.Any() ? readRanges.Last().End + 1 : 0,
//                     End = responseLength - 1
//                 });

//                 #endregion

//                 DateTime startTime = DateTime.Now;

//                 #region Parallel download  

//                 int index = 0;
//                 Parallel.ForEach(readRanges, new ParallelOptions() { MaxDegreeOfParallelism = numberOfParallelDownloads }, readRange =>
//                 {
//                     HttpWebRequest httpWebRequest = HttpWebRequest.Create(fileUrl) as HttpWebRequest;
//                     httpWebRequest.Method = "GET";
//                     httpWebRequest.AddRange(readRange.Start, readRange.End);
//                     using (HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse)
//                     {
//                         String tempFilePath = Path.GetTempFileName();
//                         using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Write))
//                         {
//                             httpWebResponse.GetResponseStream().CopyTo(fileStream);
//                             tempFilesDictionary.TryAdd(index, tempFilePath);
//                         }
//                         Progress += readRange.Start - readRange.End;
//                     }
//                     index++;

//                 });

//                 result.ParallelDownloads = index;

//                 #endregion

//                 result.TimeTaken = DateTime.Now.Subtract(startTime);

//                 #region Merge to single file  
//                 foreach (var tempFile in tempFilesDictionary.OrderBy(b => b.Key))
//                 {
//                     byte[] tempFileBytes = File.ReadAllBytes(tempFile.Value);
//                     destinationStream.Write(tempFileBytes, 0, tempFileBytes.Length);
//                     File.Delete(tempFile.Value);
//                 }
//                 #endregion


//                 return result;
//             }


//         }
//     }
// }
