using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;

namespace MyUtil.TerminalDemo
{
    public class SmtpDemo
    {
        private string serverHost = "smtp.163.com";
        private int serverHostPort = 25;
        private string serverUserName = "qq610173623@163.com";
        private string serverUserPwd = "ming4699365";
        private string boundary = "wangshaoming";
        private IContentTypeProvider contentTypeProvider = new FileExtensionContentTypeProvider();
        public async Task Send(string fromUser, string[] toUsers, string[] ccUsers, string subject, string body,params Attachment[] attachs)
        {
            Console.WriteLine("Start");
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(serverHost, serverHostPort);
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream) { AutoFlush = true, NewLine = "\r\n" })
                {
                    Console.WriteLine(reader.ReadLine());
                    writer.WriteLine("HELO " + serverHost);
                    Console.WriteLine(reader.ReadLine());
                    writer.WriteLine("AUTH LOGIN");
                    Console.WriteLine(reader.ReadLine());
                    string base64UserName = Convert.ToBase64String(Encoding.UTF8.GetBytes(serverUserName));
                    writer.WriteLine(base64UserName);
                    Console.WriteLine(reader.ReadLine());
                    string base64UserPwd = Convert.ToBase64String(Encoding.UTF8.GetBytes(serverUserPwd));
                    writer.WriteLine(base64UserPwd);
                    Console.WriteLine(reader.ReadLine());
                    writer.WriteLine($"MAIL FROM :<{fromUser}>");
                    Console.WriteLine(reader.ReadLine());
                    foreach (var to in toUsers)
                    {
                        writer.WriteLine($"RCPT TO:<{to}>");
                        Console.WriteLine(reader.ReadLine());
                    }
                    foreach (var cc in ccUsers)
                    {
                        writer.WriteLine($"RCPT TO:<{cc}>");
                        Console.WriteLine(reader.ReadLine());
                    }
                    writer.WriteLine("DATA");
                    Console.WriteLine(reader.ReadLine());
                    writer.WriteLine($"From:{fromUser}");
                    writer.WriteLine($"To:{string.Join(",", toUsers)}");
                    if (ccUsers.Any())
                    {
                        writer.WriteLine($"Cc:{string.Join(",", ccUsers)}");
                    }
                    writer.WriteLine($"Subject:{subject}");
                    writer.WriteLine($"Date:{DateTime.Now.ToString()}");
                    writer.WriteLine($"Content-Type:multipart/mixed;charset=\"utf8\";boundary=\"{boundary}_EMAIL\"");
                    writer.WriteLine("");
                    writer.WriteLine($"--{boundary}_EMAIL");
                    writer.WriteLine($"Content-Type:multipart/alternative;charset=\"utf8\";boundary=\"{boundary}_CONTENT\"");
                    writer.WriteLine("");

                    writer.WriteLine($"--{boundary}_CONTENT");
                    writer.WriteLine("Content-Transfer-Encoding:base64;");
                    writer.WriteLine("Content-Type:text/plain;charset=\"utf8\";");
                    writer.WriteLine("");
                    writer.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(body)));
                    writer.WriteLine("");
                    writer.WriteLine($"--{boundary}_CONTENT--");

                    if (attachs != null && attachs.Any())
                    {
                        foreach (var attach in attachs)
                        {                            
                            string contentType;
                            contentType = contentTypeProvider.TryGetContentType(attach.FileName, out contentType) ? contentType : "application/octet-stream";
                            writer.WriteLine($"--{boundary}_EMAIL");
                            writer.WriteLine($"Content-Type:{contentType};name=\"{attach.FileName}\"");
                            writer.WriteLine($"Content-Disposition:attachment;filename={attach.FileName}");
                            writer.WriteLine("Content-Transfer-Encoding:base64");
                            writer.WriteLine("");
                            writer.WriteLine($"{Convert.ToBase64String(attach.File)}");
                            writer.WriteLine("");
                        }
                    }

                    writer.WriteLine($"--{boundary}_EMAIL--");

                    writer.WriteLine(".");

                    Console.WriteLine(reader.ReadLine());
                    writer.WriteLine("QUIT");
                    Console.WriteLine(reader.ReadLine());
                }
            }
            Console.WriteLine("OK");
        }
    }

    public class Attachment
    {
        public string FileName { get; set; }
        public byte[] File { get; set; }
    }


}
