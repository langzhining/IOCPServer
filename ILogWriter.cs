using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace IOCPServer
{
    //定义日志类型的枚举值

    public enum logType { 
        //追踪程序流的日志条目
        Trace,

        //追踪状态改变信息
        Info,

        //帮助debug应用的日志条目
        Debug,

        //警告信息条目
        Warning,

        //错误条目
        Error,

        //严重错误条目
        Fatal
    }
    public interface ILogWriter{

        // 向日志文件写入一条日志信息
        void writeLog(object source , logType type , string messages);

    }

    public sealed class LogWriter:ILogWriter  {
        public static readonly LogWriter logWriter = new LogWriter();

        //实现接口函数
        public void writeLog(object source, logType type, string messages) {
            StringBuilder log = new StringBuilder();
            log.Append(DateTime.Now.ToString());
            log.Append(" ");
            log.Append(type.ToString().PadRight(10));
            log.Append(" | ");
#if DEBUG
            StackTrace stackTrace = new StackTrace();
            StackFrame[] stackFrames = stackTrace.GetFrames();
            int endFrame = stackFrames.Length > 4 ? 4 : stackFrames.Length;
            int startFrame = stackFrames.Length > 0 ? 1 : 0;
            for(int i = startFrame ; i < endFrame ; ++i){
                log.Append(stackFrames[i].GetMethod().Name);
                log.Append(" -> ");
            }
#else
            log.Append(System.Reflection.MethodBase.GetCurrentMethod().Name);
            log.Append（" | "）;
#endif
            log.Append(messages);

            Console.ForegroundColor = getColor(type);
            Console.WriteLine(log.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;

        }

        //将不同类型的日志异色显示
        public static ConsoleColor getColor(logType type){
            switch (type) { 
                case logType.Trace:
                    return ConsoleColor.Blue;
                case logType.Info:
                    return ConsoleColor.Cyan;
                case logType.Debug:
                    return ConsoleColor.DarkGray;
                case logType.Warning:
                    return ConsoleColor.Red;
                case logType.Error:
                    return ConsoleColor.Magenta;
                case logType.Fatal:
                    return ConsoleColor.DarkRed;
            }

            return ConsoleColor.Green;
        }

        //空日志Writer
        public sealed class NullLogWriter : ILogWriter {
            public static readonly NullLogWriter nullLogWriter = new NullLogWriter();

            public void writeLog(object source, logType type, string messages) { 
            }
        } 
    }
}
