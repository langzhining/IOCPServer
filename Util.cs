using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace IOCPServer
{
    class Util
    {


        /// <summary>
        /// 通信使用的编码
        /// </summary>
        public Encoding Encoding { get; set; }
        /// <summary>
        /// 服务器文件路径
        /// </summary>
        public string ContentPath { get; set; }


        private Dictionary<string, string> extensions = new Dictionary<string, string>()
        { 
        //{ "extension", "content type" }
        { "htm", "text/html" },
        { "html", "text/html" },
        { "xml", "text/xml" },
        { "txt", "text/plain" },
        { "css", "text/css" },
        { "png", "image/png" },
        { "gif", "image/gif" },
        { "jpg", "image/jpg" },
        { "jpeg", "image/jpeg" },
        { "ico", "image/x-icon"},
        { "pdf", "application/pdf"},
        { "zip", "application/zip"}
    };
        
        public Util(Encoding encoding, string contentPath)
        {
            this.Encoding = encoding;
            this.ContentPath = contentPath;
        }


        public byte[] notImplemented()
        {

            byte[] data = getResponseData(Encoding.GetBytes("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><h2>CGLZ Simple Web Server</h2><div>501 - Method Not Implemented</div></body></html>"),
                "501 Not Implemented", "text/html");
            return data;
        }

        public byte[] notFound()
        {
            byte[] data = getResponseData(Encoding.GetBytes("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\"></head><body><h2>CGLZ Simple Web Server</h2><div>404 - Not Found</div></body></html>"),
                "404 Not Found", "text/html");
            return data;
        }

        

        // For byte arrays

        private byte[] getResponseData(byte[] bContent, string responseCode,
                                  string contentType)
        {
            
            try
            {
                byte[] data = Encoding.GetBytes(
                                    "HTTP/1.1 " + responseCode + "\r\n"
                                  + "Server: CGLZ Simple Web Server\r\n"
                                  + "Content-Length: " + bContent.Length + "\r\n"
                                  + "Connection: close\r\n"
                                  + "Content-Type: " + contentType + "\r\n\r\n");             
                //data.Add(header);
                //byte[] content = Encoding.GetBytes(bContent);
                //data.Add(content);
                return data.Concat(bContent).ToArray();
            }
            catch {
                return null;
            }
        }
        

        public byte[] getResData(string strReceived)
        {
            
            if (strReceived == "")
            {
                return notFound();
            }

            // Parse method of the request
            string httpMethod = strReceived.Substring(0, strReceived.IndexOf(" "));
            int start = strReceived.IndexOf(httpMethod) + httpMethod.Length + 1;
            int length = strReceived.LastIndexOf("HTTP") - start - 1;
            string requestedUrl = strReceived.Substring(start, length);
            string requestedFile;

            if (httpMethod.Equals("GET") || httpMethod.Equals("POST"))
                requestedFile = requestedUrl.Split('?')[0];
            else // You can implement other methods...
            {
                return notImplemented();
            }

            requestedFile = requestedFile.Replace("/", @"\").Replace("\\..", "");
            start = requestedFile.LastIndexOf('.') + 1;

            if (start > 0)
            {
                length = requestedFile.Length - start;
                string extension = requestedFile.Substring(start, length);
                if (extensions.ContainsKey(extension)) // Do we support this extension?
                {
                    if (File.Exists(ContentPath + requestedFile)) //If yes check existence of the file
                    {
                        // Everything is OK, send requested file with correct content type:
                        FileStream fs = File.OpenRead(ContentPath + requestedFile);
                        int fileLength = (int) fs.Length;
                        byte[] context = new byte[fileLength];
                        fs.Read(context, 0, fileLength);
                        return getResponseData(context, "200", extensions[extension]);
                    }                     
                    else
                    {
                        return notImplemented();
                    }
                }
                // We don't support this extension.
                return notImplemented();
                // We are assuming that it doesn't exist.
            }
            else
            {
                // If file is not specified try to send index.htm or index.html
                // You can add more (default.htm, default.html)
                if (requestedFile.Substring(length - 1, 1) != @"\")
                {
                    requestedFile += @"\";
                }
                if (File.Exists(ContentPath + requestedFile + "index.htm"))
                {
                    FileStream fs = File.OpenRead(ContentPath + requestedFile + "index.htm");
                    int fileLength = (int)fs.Length;
                    byte[] context = new byte[fileLength];
                    fs.Read(context, 0, fileLength);
                    return getResponseData(context, "200", "text/htm");
                }
                else if (File.Exists(ContentPath + requestedFile + "index.html"))
                {
                    FileStream fs = File.OpenRead(ContentPath + requestedFile + "index.html");
                    int fileLength = (int)fs.Length;
                    byte[] context = new byte[fileLength];
                    fs.Read(context, 0, fileLength);
                    return getResponseData(context, "200", "text/html");
                }
                else
                {
                    return notFound();
                }             
            }
        }

    }




}
