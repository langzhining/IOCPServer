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
        #region Properties

        /// <summary>
        /// 通信使用的编码
        /// </summary>
        public Encoding Encoding { get; set; }
        /// <summary>
        /// 服务器文件路径
        /// </summary>
        public string ContentPath { get; set; }

        public event ResponseEventHandler ResponseReady;

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

        #endregion

        public Util(Encoding encoding, string contentPath)
        {
            this.Encoding = encoding;
            this.ContentPath = contentPath;
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="e"></param>
        /// <param name="strReceived"></param>
        public void processRequest(SocketAsyncEventArgs e, string strReceived)
        {
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.SocketAsyncArg = e;
            requestInfo.RequestData = strReceived;

            ThreadPool.QueueUserWorkItem(new WaitCallback(genResponse), requestInfo);
        }

        /// <summary>
        /// 生成响应信息
        /// </summary>
        /// <param name="args">请求信息</param>
        private void genResponse(Object args)
        {
            RequestInfo requestInfo = args as RequestInfo;
            string strReceived = requestInfo.RequestData;
            ResponseEventArgs responseEventArgs = new ResponseEventArgs();
            responseEventArgs.ResponseAsyncEventArg = requestInfo.SocketAsyncArg;

            if ( strReceived == "")
            {
                responseEventArgs.ResponseData = notFound();
                ResponseReady(this, responseEventArgs);
                return ;
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
                responseEventArgs.ResponseData = notImplemented();
                ResponseReady(this, responseEventArgs);
                return ;
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
                        long fileLength = fs.Length;
                        byte[] context = new byte[fileLength];
                        fs.Read(context, 0, (int)fileLength);

                        responseEventArgs.ResponseData = genResponseHeader("200 OK", extensions[extension], fileLength).Concat(context).ToArray();
                        ResponseReady(this, responseEventArgs);
                        return ;
                    }
                    else
                    {
                        responseEventArgs.ResponseData = notFound();
                        ResponseReady(this, responseEventArgs);
                        return;
                    }
                }
                else
                {
                    // We don't support this extension.
                    responseEventArgs.ResponseData = notImplemented();
                    ResponseReady(this, responseEventArgs);
                    return;
                }
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
                    long fileLength = fs.Length;
                    byte[] context = new byte[fileLength];
                    fs.Read(context, 0, (int)fileLength);

                    responseEventArgs.ResponseData = genResponseHeader("200 OK", "text/htm", fileLength).Concat(context).ToArray();
                    ResponseReady(this, responseEventArgs);
                    return;
                }
                else if (File.Exists(ContentPath + requestedFile + "index.html"))
                {
                    FileStream fs = File.OpenRead(ContentPath + requestedFile + "index.html");
                    long fileLength = fs.Length;
                    byte[] context = new byte[fileLength];
                    fs.Read(context, 0, (int)fileLength);

                    responseEventArgs.ResponseData = genResponseHeader("200 OK", "text/html", fileLength).Concat(context).ToArray();
                    ResponseReady(this, responseEventArgs);
                    return;
                }
                else
                {
                    responseEventArgs.ResponseData = notFound();
                    ResponseReady(this, responseEventArgs);
                    return;
                }
            }
        }

        public byte[] notImplemented()
        {
            byte[] content = Encoding.GetBytes("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><h2>CGLZ Simple Web Server</h2><div>501 - Method Not Implemented</div></body></html>");
            return genResponseHeader("501 Not Implemented", "text/html", content.Length).Concat(content).ToArray();
        }

        public byte[] notFound()
        {
            byte[] content = Encoding.GetBytes("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\"></head><body><h2>CGLZ Simple Web Server</h2><div>404 - Not Found</div></body></html>");
            return genResponseHeader("404 Not Found", "text/html", content.Length).Concat(content).ToArray();
        }

        private byte[] genResponseHeader(string responseCode, string contentType, long contentLength)
        {
                return Encoding.GetBytes(
                                    "HTTP/1.1 " + responseCode + "\r\n"
                                  + "Server: CGLZ Simple Web Server\r\n"
                                  + "Content-Length: " + contentLength + "\r\n"
                                  + "Connection: close\r\n"
                                  + "Content-Type: " + contentType + "\r\n\r\n");
        }

    }

    class RequestInfo
    {
        public SocketAsyncEventArgs SocketAsyncArg { get; set; }

        public string RequestData { get; set; }
    }


}
